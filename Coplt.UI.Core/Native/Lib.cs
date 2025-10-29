using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Texts;
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

        ILayout* p_layout;
        m_lib.CreateLayout(&p_layout).TryThrowWithMsg();
        m_layout = new(p_layout);
    }

    #endregion

    #region Instance

    public static NativeLib Instance { get; } = new();

    #endregion

    #region SetLog

    public void SetLogger(
        void* obj,
        delegate* unmanaged[Cdecl]<void*, LogLevel, int, char*, void> logger,
        delegate* unmanaged[Cdecl]<void*, void> drop
    )
    {
        m_lib.SetLogger(obj, logger, drop);
    }

    public void SetLogger(Action<LogLevel, string> logger)
    {
        var gch = GCHandle.Alloc(logger);
        m_lib.SetLogger((void*)GCHandle.ToIntPtr(gch), &ActionLoggerProxyLogger, &ActionLoggerProxyDrop);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void ActionLoggerProxyLogger(void* obj, LogLevel level, int len, char* msg)
    {
        var gch = GCHandle.FromIntPtr((nint)obj);
        ((Action<LogLevel, string>)gch.Target!).Invoke(level, new string(msg, 0, len));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void ActionLoggerProxyDrop(void* obj)
    {
        var gch = GCHandle.FromIntPtr((nint)obj);
        gch.Free();
    }

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

    public void* Alloc(int count, int align) => m_lib.Alloc(count, align);
    public void* ZAlloc(int count, int align) => m_lib.ZAlloc(count, align);
    public void* ReAlloc(void* ptr, int count, int align) => m_lib.ReAlloc(ptr, count, align);
    public void Free(void* ptr, int align) => m_lib.Free(ptr, align);

    public T* Alloc<T>(int count = 1) => (T*)m_lib.Alloc(count * sizeof(T), Utils.AlignOf<T>());
    public T* ZAlloc<T>(int count = 1) => (T*)m_lib.ZAlloc(count * sizeof(T), Utils.AlignOf<T>());
    public T* ReAlloc<T>(T* ptr, int count) => (T*)m_lib.ReAlloc(ptr, count * sizeof(T), Utils.AlignOf<T>());
    public void Free<T>(T* ptr) => m_lib.Free(ptr, Utils.AlignOf<T>());

    #endregion

    #region SplitTexts

    public void SplitTexts(NativeList<TextRange>* ranges, char* chars, int len)
    {
        m_lib.SplitTexts(ranges, chars, len).TryThrowWithMsg();
    }

    #endregion
}
