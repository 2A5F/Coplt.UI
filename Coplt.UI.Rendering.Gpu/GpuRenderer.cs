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

    [Drop]
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

    #endregion

    #region Update

    /// <summary>
    /// Calculate and upload rendering data.
    /// <para><b>May need to be performed on the rendering thread, which is limited by the rendering backend</b></para>
    /// </summary>
    /// <returns>Is re-rendering required</returns>
    public bool Update()
    {
        m_max_z = 0;
        return Update(Document.Root);
    }

    private bool Update(UIElement<GpuRd, TEd> element)
    {
        ref var rd = ref Unsafe.AsRef(in element.RData);
        ref readonly var fl = ref element.FinalLayout;
        ref readonly var cs = ref element.CommonStyle;
        ref readonly var rs = ref rd.GpuStyle;
        // todo overflow hide
        var changed = true;
        if (rd.m_last_version != element.VisualVersion || rd.m_box_data.IsNull)
        {
            rd.m_last_version = element.VisualVersion;
            changed = true;
            if (rd.m_box_data.IsNull) rd.MakeBoxData(BoxDataSource);
            var flags = RenderFlags.None;
            if (cs.BoxSizing is BoxSizing.ContentBox) flags |= RenderFlags.ContentBox;
            var z = fl.Order; // todo calc z rs.ZIndex
            m_max_z = Math.Max(m_max_z, z);
            rd.m_box_data.Ref = new()
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
                Z = z,
                Flags = flags,
                BackgroundImageSampler = rs.BackgroundImageSampler,
                BorderRadiusMode = rs.BorderRadiusMode,
                BackgroundImage = 0, // todo image
            };
            rd.m_box_data.MarkItemChanged();
        }
        foreach (var child in element)
        {
            if (Update(child)) changed = true;
        }
        return changed;
    }

    #endregion

    #region Render

    /// <summary>
    /// Actual rendering.
    /// <para><b>May need to be performed on the rendering thread, which is limited by the rendering backend</b></para>
    /// </summary>
    public void Render(uint Width, uint Height)
    {
        if (ClearBackgroundColor.HasValue) Backend.ClearBackground(ClearBackgroundColor.Value);

        Backend.SetViewPort(0, 0, Width, Height, m_max_z);
        Render(Document.Root);
    }

    // todo batch
    private void Render(UIElement<GpuRd, TEd> element)
    {
        ref var rd = ref Unsafe.AsRef(in element.RData);
        ref readonly var fl = ref element.FinalLayout;
        ref readonly var cs = ref element.CommonStyle;
        ref readonly var rs = ref rd.GpuStyle;

        if (rs.IsVisible)
        {
            Backend.DrawBox([
                new BatchData
                {
                    Buffer = rd.m_box_data.GpuBuffer!.GpuDescId,
                    Index = rd.m_box_data.Index,
                }
            ]);
        }

        foreach (var child in element)
        {
            Render(child);
        }
    }

    #endregion

    #region Frame

    public void BeginFrame() => Backend.BeginFrame();

    public void EndFrame() => Backend.EndFrame();

    #endregion
}

[Dropping]
internal sealed partial class BoxDataSource(GpuRendererBackend Backend)
{
    internal const uint BoxDataBufferSize = 1024;
    internal EmbedList<GpuUploadList> m_buffers;
    internal uint m_current_buffer_offset = 0;
    internal readonly Queue<BoxDataHandle> m_freed_queue = new();

    [Drop]
    private void DropBuffers()
    {
        foreach (ref var item in m_buffers)
        {
            item.Dispose();
        }
    }

    internal unsafe BoxDataHandle RentBoxData()
    {
        if (m_freed_queue.TryDequeue(out var r)) return r;
        if (m_buffers.Count == 0 || m_current_buffer_offset >= BoxDataBufferSize)
        {
            m_buffers.Add(Backend.AllocUploadList((uint)sizeof(BoxData), BoxDataBufferSize));
            m_current_buffer_offset = 0;
        }
        var buffer = m_buffers.Count - 1;
        var index = m_current_buffer_offset++;
        var ptr = &((BoxData*)m_buffers[buffer].MappedPtr)[index];
        return new((uint)buffer, index, ptr, this);
    }

    internal void ReturnBoxData(BoxDataHandle handle) => m_freed_queue.Enqueue(handle);
}

internal readonly unsafe struct BoxDataHandle(uint Buffer, uint Index, BoxData* Ptr, BoxDataSource? Source)
{
    public readonly uint Buffer = Buffer;
    public readonly uint Index = Index;
    public readonly BoxData* Ptr = Ptr;
    public readonly BoxDataSource? Source = Source;

    public ref BoxData Ref => ref *Ptr;

    public GpuUploadList? GpuBuffer => Source?.m_buffers[(int)Buffer];

    public bool IsNull => Ptr == null || Source == null;

    public void MarkItemChanged()
    {
        GpuBuffer?.MarkItemChanged(Index);
    }
}
