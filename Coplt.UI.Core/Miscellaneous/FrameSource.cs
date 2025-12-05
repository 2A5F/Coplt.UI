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

    #endregion

    #region Props

    public ref readonly Rc<IFrameSource> Inner => ref m_inner;

    #endregion

    #region Ctor

    public FrameSource()
    {
        IFrameSource* ptr;
        NativeLib.Instance.m_lib.CreateFrameSource(&ptr).TryThrowWithMsg();
        m_inner = new(ptr);
    }

    #endregion

    #region Data

    public FrameTime Data
    {
        get
        {
            FrameTime ft;
            m_inner.Get(&ft);
            return ft;
        }
        set => m_inner.Set(&value);
    }

    #endregion
}
