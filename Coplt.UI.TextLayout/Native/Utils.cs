using Coplt.Com;

namespace Coplt.UI.Layouts.Native;

internal static unsafe class Utils
{
    public static void TryThrowWithMsg(this HResult hr, Texts.TextLayout lib)
    {
        if (hr.IsSuccess) return;
        var msg = lib.CurrentErrorMessage;
        if (string.IsNullOrWhiteSpace(msg)) hr.TryThrow();
        throw new NativeException(msg, hr.ToException());
    }
}
