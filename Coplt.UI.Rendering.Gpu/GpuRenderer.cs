using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Collections;
using Coplt.UI.Elements;
using Coplt.UI.Rendering.Gpu.Graphics;

namespace Coplt.UI.Rendering.Gpu;

[Dropping]
public sealed partial class GpuRenderer<TEd>(GpuRendererBackend Backend, UIDocument<GpuRd, TEd> Document)
    where TEd : new()
{
    #region Fields

    [Drop]
    public GpuRendererBackend Backend { get; } = Backend;
    public UIDocument<GpuRd, TEd> Document { get; } = Document;

    private int m_version;

    #endregion

    #region Update

    /// <summary>
    /// Calculates the data required for rendering.
    /// <para><b>Which can be executed in any thread, but concurrency is not allowed</b></para>
    /// </summary>
    public void Update()
    {
        
    }

    #endregion

    #region Upload

    /// <summary>
    /// Upload rendering data.
    /// <para><b>May need to be performed on the rendering thread, which is limited by the rendering backend</b></para>
    /// </summary>
    public void Upload()
    {
        
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
