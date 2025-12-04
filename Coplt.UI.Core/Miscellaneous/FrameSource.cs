using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;

namespace Coplt.UI.Miscellaneous;

public record struct FrameTime
{
    public ulong NthFrame;
    public ulong TimeTicks;
}

[Dropping(Unmanaged = true)]
public sealed unsafe partial class FrameSource
{
    #region Fields

    [Drop]
    internal Rc<IFrameSource> m_inner;
    internal readonly FrameTime* m_data;

    #endregion

    #region Props

    public ref readonly Rc<IFrameSource> Inner => ref m_inner;
    public ref FrameTime Data => ref *m_data;

    #endregion

    #region Ctor

    public FrameSource()
    {
        IFrameSource* ptr;
        NativeLib.Instance.m_lib.CreateFrameSource(&ptr).TryThrowWithMsg();
        m_inner = new(ptr);
        m_data = m_inner.Data;
    }

    #endregion
}
