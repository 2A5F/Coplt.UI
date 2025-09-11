using Coplt.Dropping;

namespace Coplt.UI.Rendering.Gpu.Graphics;

[Dropping(Unmanaged = true)]
public abstract partial class GpuCommandRecorder
{
    #region Lifecycle

    /// <summary>
    /// Start a new record
    /// </summary>
    public abstract void Renew();

    /// <summary>
    /// Finish record
    /// </summary>
    public abstract void Finish();

    #endregion

    #region Commands

    public abstract void SetScissorRect(uint Left, uint Top, uint Width, uint Height);

    #endregion
}
