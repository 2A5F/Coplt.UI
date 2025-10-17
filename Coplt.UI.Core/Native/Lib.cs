using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Utilities;

namespace Coplt.UI.Native;

[Dropping]
public sealed unsafe partial class NativeLib
{
    #region Fields

    [Drop]
    internal Rc<ILib> m_lib;
    [Drop]
    internal Rc<ILayout> m_layout;

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

        m_layout = new(m_lib.CreateLayout());
        if (!m_layout) throw new Exception("Failed to create native layout object");
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
    public void* ZAlloc(int size, int align) => m_lib.ZAlloc(size, align);
    public void* ReAlloc(void* ptr, int size, int align) => m_lib.ReAlloc(ptr, size, align);
    public void Free(void* ptr, int align) => m_lib.Free(ptr, align);

    public T* Alloc<T>(int size = 1) => (T*)m_lib.Alloc(size * sizeof(T), Utils.AlignOf<T>());
    public T* ZAlloc<T>(int size = 1) => (T*)m_lib.ZAlloc(size * sizeof(T), Utils.AlignOf<T>());
    public T* ReAlloc<T>(T* ptr, int size) => (T*)m_lib.ReAlloc(ptr, size * sizeof(T), Utils.AlignOf<T>());
    public void Free<T>(T* ptr) => m_lib.Free(ptr, Utils.AlignOf<T>());

    #endregion
}
