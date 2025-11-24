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
        LibLoadInfo info = new()
        {
            p_dwrite = DWrite.Load(),
        };
        ILib* p_lib;
        new HResult(coplt_ui_create_lib(&info, &p_lib)).TryThrow();
        m_lib = new(p_lib);

        ILayout* p_layout;
        m_lib.CreateLayout(&p_layout).TryThrowWithMsg();
        m_layout = new(p_layout);

        return;

        [DllImport("Coplt.UI.Native")]
        static extern HRESULT coplt_ui_create_lib(LibLoadInfo* info, ILib** lib);
    }

    #endregion

    #region Instance

    public static NativeLib Instance { get; } = new();

    #endregion

    #region MyRegion

    public event Action<Exception>? UnhandledException;

    public void EmitUnhandledExceptionEvent(Exception ex) => UnhandledException?.Invoke(ex);

    #endregion

    #region SetLog

    public void ClearLogger() => m_lib.ClearLogger();

    public void SetLogger(
        void* obj,
        delegate* unmanaged[Cdecl]<void*, LogLevel, StrKind, int, void*, void> logger,
        delegate* unmanaged[Cdecl]<void*, LogLevel, byte> is_enabled,
        delegate* unmanaged[Cdecl]<void*, void> drop
    )
    {
        m_lib.SetLogger(obj, logger, is_enabled, drop);
    }

    public void SetLogger(Action<LogLevel, string> logger, Func<LogLevel, bool>? is_enable = null)
    {
        SetLogger(new ActionLogger(logger, is_enable));
    }

    public void SetLogger(ILogger logger)
    {
        var gch = GCHandle.Alloc(logger);
        m_lib.SetLogger((void*)GCHandle.ToIntPtr(gch), &Logger, &IsEnabled, &Drop);
        return;

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Logger(void* obj, LogLevel level, StrKind kind, int len, void* msg)
        {
            var gch = GCHandle.FromIntPtr((nint)obj);
            Unsafe.As<ILogger>(gch.Target)!.Log(level, kind.GetString(msg, len));
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static byte IsEnabled(void* obj, LogLevel level)
        {
            var gch = GCHandle.FromIntPtr((nint)obj);
            return Unsafe.As<ILogger>(gch.Target)!.IsEnabled(level) ? (byte)1 : (byte)0;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Drop(void* obj)
        {
            var gch = GCHandle.FromIntPtr((nint)obj);
            gch.Free();
        }
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

    [DllImport("Coplt.UI.Native", EntryPoint = "coplt_ui_malloc")]
    public static extern void* Alloc(nuint size, nuint align);
    [DllImport("Coplt.UI.Native", EntryPoint = "coplt_ui_zalloc")]
    public static extern void* ZAlloc(nuint size, nuint align);
    [DllImport("Coplt.UI.Native", EntryPoint = "coplt_ui_realloc")]
    public static extern void* ReAlloc(void* ptr, nuint new_size, nuint align);
    [DllImport("Coplt.UI.Native", EntryPoint = "coplt_ui_free")]
    public static extern void Free(void* ptr, nuint align);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* Alloc(int count, int align) => Alloc((nuint)count, (nuint)align);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ZAlloc(int count, int align) => ZAlloc((nuint)count, (nuint)align);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ReAlloc(void* ptr, int count, int align) => ReAlloc(ptr, (nuint)count, (nuint)align);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free(void* ptr, int align) => Free(ptr, (nuint)align);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Alloc<T>(int count = 1) => (T*)Alloc(count * sizeof(T), Utils.AlignOf<T>());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* ZAlloc<T>(int count = 1) => (T*)ZAlloc(count * sizeof(T), Utils.AlignOf<T>());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* ReAlloc<T>(T* ptr, int count) => (T*)ReAlloc(ptr, count * sizeof(T), Utils.AlignOf<T>());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free<T>(T* ptr) => Free(ptr, Utils.AlignOf<T>());

    #endregion

    #region SplitTexts

    public void SplitTexts(NativeList<TextRange>* ranges, char* chars, int len)
    {
        m_lib.SplitTexts(ranges, chars, len).TryThrowWithMsg();
    }

    #endregion
}
