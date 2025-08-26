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

    internal EmbedList<Batch> m_batches;

    #endregion

    #region Update

    /// <summary>
    /// Calculate and upload rendering data.
    /// <para><b>May need to be performed on the rendering thread, which is limited by the rendering backend</b></para>
    /// </summary>
    /// <returns>Is re-rendering required</returns>
    public bool Update(uint Width, uint Height)
    {
        ref var batch = ref m_batches.UnsafeAdd();
        batch = new()
        {
            Left = 0,
            Top = 0,
            Width = Width,
            Height = Height,
        };
        var ctx = new UpdateCtx
        {
            Batch = ref batch,
        };
        return Update(Document.Root, ref ctx);
    }

    private ref struct UpdateCtx
    {
        public ref Batch Batch;
    }

    private bool Update(UIElement<GpuRd, TEd> element, ref UpdateCtx ctx)
    {
        ref var rd = ref Unsafe.AsRef(in element.RData);
        ref readonly var fl = ref element.FinalLayout;
        ref readonly var cs = ref element.CommonStyle;
        ref readonly var rs = ref rd.GpuStyle;
        // todo overflow hide
        var changed = true;
        if (rd.m_last_version != element.VisualVersion)
        {
            rd.m_last_version = element.VisualVersion;
            changed = true;
            // todo gpu only
            rd.m_box_data = new()
            {
                TransformMatrix = float4x4.Identity,
                LeftTopWidthHeight = new(fl.RootLocation.X, fl.RootLocation.Y, fl.Size.Width, fl.Size.Height),
                BorderSize_TopRightBottomLeft = new(fl.Border.Top, fl.Border.Right, fl.Border.Bottom, fl.Border.Left),
                BackgroundColor = rs.BackgroundColor,
                BackgroundImageTint = rs.BackgroundImageTint,
                BorderColor_Top = rs.BorderColor.Top,
                BorderColor_Right = rs.BorderColor.Right,
                BorderColor_Bottom = rs.BorderColor.Bottom,
                BorderColor_Left = rs.BorderColor.Left,
                Opaque = rs.Opaque,
                Z = fl.Order, // todo calc z rs.ZIndex
                BorderRadiusMode = BorderRadiusMode.Circle,
                BoxSizing = cs.BoxSizing,
                BackgroundImage = 0
            };
        }
        var sub_ctx = new UpdateCtx
        {
            Batch = ref ctx.Batch,
        };
        foreach (var child in element)
        {
            Update(child, ref sub_ctx);
        }
        return changed;
    }

    #endregion

    #region Render

    /// <summary>
    /// Actual rendering.
    /// <para><b>May need to be performed on the rendering thread, which is limited by the rendering backend</b></para>
    /// </summary>
    public void Render() { }

    #endregion
}
