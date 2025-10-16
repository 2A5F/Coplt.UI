using System.Runtime.CompilerServices;

namespace Coplt.UI.Utilities.TypeChains;

internal abstract class Of
{
    public abstract Type Type { get; }
    public abstract C Create();
    public abstract C Chain(C c);
}

internal sealed class Of<T> : Of
{
    public override Type Type => typeof(T);
    public override C Create() => C<T>.Instance;
    public override C Chain(C c) => c.Add<C<T>>();
}

internal interface IC
{
    public static abstract C Instance { get; }

    public static abstract int i_Index { get; }
    public static abstract string i_Name { get; }
    public static abstract bool i_Is<T>();
    public static abstract int i_IndexOf<T>();
}

internal abstract class C
{
    public abstract C? Parent { get; }
    public abstract int Index { get; }
    public abstract C Add<T>() where T : C, IC;
    public abstract bool Is<T>();

    public abstract int IndexOf<T>();
}

internal sealed class C<T0> : C, IC
{
    private C() { }

    public override C? Parent => null;
    public override int Index => i_Index;
    public override C Add<T>() => C<C<T0>, T>.Instance;
    public override bool Is<T>() => i_Is<T>();
    public override int IndexOf<T>() => i_IndexOf<T>();
    public override string ToString() => i_Name;

    public static C Instance { get; } = new C<T0>();
    public static int i_Index
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => 0;
    }
    public static string i_Name => typeof(T0).FullName ?? typeof(T0).Name;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool i_Is<T>() => typeof(T) == typeof(T0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int i_IndexOf<T>() => i_Is<T>() ? 0 : -1;
}

internal sealed class C<T0, T1> : C, IC
    where T0 : C, IC where T1 : C, IC
{
    private C() { }

    public override C Parent => T0.Instance;
    public override int Index => i_Index;
    public override C Add<T>() => C<C<T0, T1>, T>.Instance;
    public override bool Is<T>() => i_Is<T>();
    public override int IndexOf<T>()
    {
        if (i_Is<T>()) return i_Index;
        return CQuery<T0, T>.IndexOf;
    }
    public override string ToString() => i_Name;

    public static C Instance { get; } = new C<T0, T1>();
    // ReSharper disable once StaticMemberInGenericType
    public static int i_Index
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    } = T0.i_Index + 1;
    public static string i_Name => $"{T0.i_Name}, {T1.i_Name}";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool i_Is<T>() => T1.i_Is<T>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int i_IndexOf<T>()
    {
        if (i_Is<T>()) return i_Index;
        return T0.i_IndexOf<T>();
    }
}

internal static class CQuery<TC, T> where TC : C, IC
{
    public static int IndexOf
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    } = TC.i_IndexOf<T>();
}
