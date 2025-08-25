using Coplt.Dropping;
using Coplt.UI.Elements;

namespace Coplt.UI.Rending.Gpu;

[Dropping]
public sealed partial class GpuRenderer<TEd>(GpuRendererBackend Backend, UIDocument<GpuRd, TEd> Document)
    where TEd : new()
{
    [Drop]
    public GpuRendererBackend Backend { get; } = Backend;
    public UIDocument<GpuRd, TEd> Document { get; } = Document;

    #region Update

    /// <summary>
    /// Calculates the data required for rendering, which can be executed in any thread, but concurrency is not allowed
    /// </summary>
    public void Update() { }

    #endregion

    #region Render

    /// <summary>
    /// Actual rendering may need to be performed on the rendering thread, which is limited by the rendering backend
    /// </summary>
    public void Render() { }

    #endregion
}
