using System.Diagnostics;
using System.Runtime.CompilerServices;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Collections;
using Coplt.UI.Elements;
using Coplt.UI.Rendering.Gpu.Graphics;
using Coplt.UI.Styles;

namespace Coplt.UI.Rendering.Gpu;

[Dropping]
public sealed partial class GpuRenderer<TEd>(GpuRendererBackend Backend, UIDocument<GpuRd, TEd> Document)
    where TEd : new()
{
    #region Fields

    [Drop(Order = 10)]
    public GpuRendererBackend Backend { get; } = Backend;
    public UIDocument<GpuRd, TEd> Document { get; } = Document;
    [Drop]
    internal BoxDataSource BoxDataSource { get; } = new(Backend);

    public Color? ClearBackgroundColor
    {
        get => Backend.ClearBackgroundColor;
        set => Backend.ClearBackgroundColor = value;
    }

    private float m_max_z;

    private uint m_width;
    private uint m_height;

    #endregion

    #region Update

    /// <summary>
    /// Calculate and upload rendering data.
    /// <para><b>May need to be performed on the rendering thread, which is limited by the rendering backend</b></para>
    /// </summary>
    /// <returns>Is re-rendering required</returns>
    public bool Update(uint Width, uint Height, bool LayoutChanged)
    {
        Debug.Assert((m_width == Width && m_height == Height) || LayoutChanged,
            "When the view size changes, the layout must also change");
        m_width = Width;
        m_height = Height;
        m_max_z = 1;
        var changed = UpdateOn(Document.Root) || LayoutChanged;
        if (LayoutChanged) Record();
        return changed;
    }

    private bool UpdateOn(UIElement<GpuRd, TEd> element)
    {
        ref var rd = ref Unsafe.AsRef(in element.RData);
        ref readonly var fl = ref element.FinalLayout;
        ref readonly var cs = ref element.CommonStyle;
        ref readonly var rs = ref rd.GpuStyle;
        var changed = false;
        // todo if not visible , return box data to pool
        if (rd.m_last_version != element.VisualVersion || rd.m_box_data.IsNull)
        {
            rd.m_last_version = element.VisualVersion;
            element.LayoutVisualTouch();
            changed = true;
            if (rd.m_box_data.IsNull) rd.MakeBoxData(BoxDataSource);
            var flags = RenderFlags.None;
            if (cs.BoxSizing is BoxSizing.ContentBox) flags |= RenderFlags.ContentBox;
            rd.m_box_data.Update(new()
            {
                TransformMatrix = float4x4.Identity,
                LeftTopWidthHeight = new(fl.RootLocation.X, fl.RootLocation.Y, fl.Size.Width, fl.Size.Height),
                BorderSize_TopRightBottomLeft = new(fl.Border.Top, fl.Border.Right, fl.Border.Bottom, fl.Border.Left),
                BorderRound = new(rs.BorderRadius.BottomRight, rs.BorderRadius.TopRight, rs.BorderRadius.BottomLeft, rs.BorderRadius.TopLeft),
                BackgroundColor = rs.BackgroundColor,
                BackgroundImageTint = rs.BackgroundImageTint,
                BorderColor_Top = rs.BorderColor.Top,
                BorderColor_Right = rs.BorderColor.Right,
                BorderColor_Bottom = rs.BorderColor.Bottom,
                BorderColor_Left = rs.BorderColor.Left,
                Opaque = rs.Opaque,
                Flags = flags,
                BackgroundImageSampler = rs.BackgroundImageSampler,
                BorderRadiusMode = rs.BorderRadiusMode,
                BackgroundImage = 0, // todo image
            });
        }
        foreach (var child in element)
        {
            if (UpdateOn(child)) changed = true;
        }
        return changed;
    }

    #endregion

    #region Record

    [Drop]
    private RecordContext<TEd> m_record_context = new(Backend);

    private void Record()
    {
        if (m_width == 0 || m_height == 0) return;
        m_record_context.Record(Document.Root, m_width, m_height, m_max_z);
    }

    #endregion

    #region Render

    /// <summary>
    /// Actual rendering.
    /// <para><b>May need to be performed on the rendering thread, which is limited by the rendering backend</b></para>
    /// </summary>
    public void Render()
    {
        if (m_width == 0 || m_height == 0) return;
        if (ClearBackgroundColor.HasValue) Backend.ClearBackground(ClearBackgroundColor.Value);

        Backend.SetViewPort(0, 0, m_width, m_height, m_max_z);
        m_record_context.Render();
    }

    #endregion

    #region Frame

    public void BeginFrame() => Backend.BeginFrame();

    public void EndFrame() => Backend.EndFrame();

    #endregion
}

[Dropping(Unmanaged = true)]
internal partial struct RecordContext<TEd>(GpuRendererBackend Backend) where TEd : new()
{
    #region Fields

    [Drop]
    private readonly GpuRenderLayerManager m_layer_manager = Backend.CreateRenderLayerPool();
    private EmbedList<GpuRenderLayer> m_layers;

    private EmbedList<UIElement<GpuRd, TEd>> m_cur_elements;
    private EmbedList<UIElement<GpuRd, TEd>> m_next_elements;

    private GpuRenderLayer? m_cur_layer_opaque;
    private GpuRenderLayer? m_cur_layer_alpha;

    private int m_clip_layer_start = 0;

    #endregion

    #region Swap

    private void SwapElements() => (m_cur_elements, m_next_elements) = (m_next_elements, m_cur_elements);

    #endregion

    #region AllocLayer

    private GpuRenderLayer GetOrAllocLayer(GpuRenderLayerType type) => type switch
    {
        GpuRenderLayerType.Opaque => m_cur_layer_opaque ??= AllocLayer(type, false),
        GpuRenderLayerType.Alpha => m_cur_layer_alpha ??= AllocLayer(type, false),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private GpuRenderLayer AllocLayer(GpuRenderLayerType type, bool clip)
    {
        var layer = m_layer_manager.RentLayer();
        layer.Data.Type = type;
        layer.Data.ScissorRect = default;
        layer.Data.Clip = clip;
        if (type is GpuRenderLayerType.Opaque) m_layers.Insert(m_clip_layer_start, layer);
        else m_layers.Add(layer);
        return layer;
    }

    #endregion

    #region Reset

    private void ClearLayer()
    {
        foreach (ref var layer in m_layers)
        {
            m_layer_manager.ReturnLayer(ref layer!);
        }
        m_layers.UnsafeClear();
    }

    private void Reset(uint Width, uint Height, float MaxZ)
    {
        m_cur_layer_opaque = null!;
        m_cur_layer_alpha = null!;
        m_clip_layer_start = 0;
        ClearLayer();
        m_cur_elements.Clear();
        m_next_elements.Clear();
        m_layer_manager.Reset(Width, Height, MaxZ);
    }

    #endregion

    #region Record

    // Breadth-first traversal batching
    public void Record(UIElement<GpuRd, TEd> root, uint Width, uint Height, float MaxZ)
    {
        Reset(Width, Height, MaxZ);
        RecordOn(root);
        SwapElements();
        for (; m_cur_elements.Count > 0; m_cur_elements.Clear(), SwapElements())
        {
            // Each alpha layer needs to be blended
            m_cur_layer_alpha = null;
            foreach (var parent in m_cur_elements)
            {
                foreach (var element in parent)
                {
                    RecordOn(element);
                }
            }
        }
        m_layer_manager.Record(m_layers.AsSpan);
    }

    #endregion

    #region RecordOn

    private void RecordOn(UIElement<GpuRd, TEd> element)
    {
        ref var rd = ref Unsafe.AsRef(in element.RData);
        ref readonly var fl = ref element.FinalLayout;
        ref readonly var cs = ref element.CommonStyle;
        ref readonly var rs = ref rd.GpuStyle;

        // todo clip; 需要裁剪时，不直接添加到 m_next_elements，单独维护裁剪层
        if (element.Count > 0) m_next_elements.Add(element);

        if (!rs.IsVisible) return;
        var layer = GetOrAllocLayer(rs.IsOpaque ? GpuRenderLayerType.Opaque : GpuRenderLayerType.Alpha);
        layer.AddItem(in rd.m_box_data);
    }

    #endregion

    #region Render

    public void Render()
    {
        m_layer_manager.Render(m_layers.AsSpan);
    }

    #endregion
}
