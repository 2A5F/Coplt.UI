using System.Globalization;
using System.Runtime.InteropServices;
using Coplt.UI.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.Utilities;

public static class Utils
{
    [StructLayout(LayoutKind.Sequential)]
    private struct AlignOfHelper<T>
    {
        public byte dummy;
        public T data;
    }

    public static unsafe int AlignOf<T>() => sizeof(AlignOfHelper<T>) - sizeof(T);

    public static unsafe LocaleId GetUserUiDefaultLocale()
    {
        nuint len = 0;
        var locale = coplt_ui_get_user_ui_default_locale(&len);

        var str = new string((sbyte*)locale, 0, (int)len);

        NativeLib.Free(locale);

        return LocaleId.Of(str);

        [DllImport("Coplt.UI.Native")]
        static extern unsafe byte* coplt_ui_get_user_ui_default_locale(nuint* len);
    }
}
