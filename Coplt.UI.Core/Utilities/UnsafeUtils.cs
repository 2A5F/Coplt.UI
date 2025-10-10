using System.Runtime.CompilerServices;

namespace Coplt.UI.Utilities;

public static class UnsafeUtils
{
    public static ulong EnumToULong<E>(E value) where E : struct, Enum => Unsafe.SizeOf<E>() switch
    {
        1 => Unsafe.BitCast<E, byte>(value),
        2 => Unsafe.BitCast<E, ushort>(value),
        4 => Unsafe.BitCast<E, uint>(value),
        8 => Unsafe.BitCast<E, ulong>(value),
        _ => throw new NotSupportedException()
    };
}
