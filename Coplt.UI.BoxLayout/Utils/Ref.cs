using System.Runtime.CompilerServices;

namespace Coplt.UI.BoxLayouts.Utilities;

public ref struct Ref<T>(ref T Value)
{
    public ref T Value = ref Value;
}

public ref struct RoRef<T>(in T Value)
{
    public ref readonly T Value = ref Value;
}

public static class Ref
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Ref<T> Of<T>(ref T value) => new(ref value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RoRef<T> In<T>(in T value) => new(in value);
}
