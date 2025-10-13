using System.Runtime.InteropServices;

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
}
