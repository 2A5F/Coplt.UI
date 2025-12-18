using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

internal static unsafe class NativeUtils
{
    public static void TryThrowWithMsg(this HResult hr)
    {
        if (hr.IsSuccess) return;
        var msg = NativeLib.Instance.CurrentErrorMessage;
        if (string.IsNullOrWhiteSpace(msg)) hr.TryThrow();
        throw new NativeException(msg, hr.ToException());
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void FreeGcHandle(void* handle) => GCHandle.FromIntPtr((nint)handle).Free();
}
