using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;

namespace Coplt.UI.Native;

[Dropping]
public sealed unsafe partial class NativeLib
{
    #region Fields

    [Drop]
    internal Rc<ILib> m_lib;

    #endregion

    #region Properties

    public ref readonly Rc<ILib> Lib => ref m_lib;

    #endregion

    #region Ctor

    private NativeLib()
    {
        [DllImport("Coplt.UI.Native")]
        static extern ILib* Coplt_CreateLibUi();

        m_lib = new(Coplt_CreateLibUi());
        if (!m_lib) throw new Exception("Failed to create native lib");
    }

    #endregion

    #region Instance

    public static NativeLib Instance { get; } = new();

    #endregion

    #region CurrentErrorMessage

    public string CurrentErrorMessage
    {
        get
        {
            var str = m_lib.GetCurrentErrorMessage();
            return str.ToString();
        }
    }

    #endregion

    #region Alloc

    public void* Alloc(int size, int align) => m_lib.Alloc(size, align);
    public void Free(void* ptr, int align) => m_lib.Free(ptr, align);

    #endregion
}
