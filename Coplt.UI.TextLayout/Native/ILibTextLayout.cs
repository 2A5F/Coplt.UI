using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;

namespace Coplt.UI.Layouts.Native;

[Interface, Guid("778be1fe-18f2-4aa5-8d1f-52d83b132cff")]
public unsafe partial struct ILibTextLayout
{
    public partial IFace* CreateFace();
}

[Dropping]
public sealed unsafe partial class LibTextLayout
{
    #region Fields

    [Drop]
    internal Rc<ILibTextLayout> m_inner;

    #endregion

    #region Properties

    public ref readonly Rc<ILibTextLayout> Inner => ref m_inner;

    #endregion

    #region Ctor

    private LibTextLayout()
    {
        [DllImport("Coplt.UI.TextLayout.Native")]
        static extern ILibTextLayout* Coplt_CreateLibTextLayout();

        m_inner = new(Coplt_CreateLibTextLayout());
        if (!m_inner) throw new Exception("Failed to load LibTextLayout");
    }

    #endregion

    #region Instance

    public static LibTextLayout Instance { get; } = new();

    #endregion
}
