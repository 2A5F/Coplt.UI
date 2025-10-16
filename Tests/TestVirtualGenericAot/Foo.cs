using System.Runtime.CompilerServices;

internal abstract class Of
{
    public abstract Type Type { get; }
    public abstract C Create();
    public abstract C Chain(C c);
}

internal sealed class Of<T> : Of
{
    public override Type Type => typeof(T);
    public override C Create() => new C<T>();
    public override C Chain(C c) => c.Add<C<T>>();
}

internal interface IC
{
    public static abstract int i_Index { get; }
    public static abstract string i_Name { get; }

    public static abstract bool i_Is<T>();
}

internal abstract class C
{
    public abstract C? Parent { get; }
    public abstract int Index { get; }
    public abstract C Add<T>() where T : C, IC;
    public abstract bool Is<T>();
}

internal sealed class C<T0> : C, IC
{
    public override C? Parent => null;
    public override int Index => i_Index;
    public override C Add<T>() => new C<C<T0>, T>(this);
    public override bool Is<T>() => i_Is<T>();
    public override string ToString() => i_Name;
    
    public static int i_Index => 0;
    public static string i_Name => typeof(T0).FullName ?? typeof(T0).Name;
    public static bool i_Is<T>() => typeof(T) == typeof(T0);
}

internal sealed class C<T0, T1>(T0 parent) : C, IC
    where T0 : C, IC where T1 : C, IC
{
    public override C Parent => parent;
    public override int Index => i_Index;
    public override C Add<T>() => new C<C<T0, T1>, T>(this);
    public override bool Is<T>() => i_Is<T>();
    public override string ToString() => i_Name;

    // ReSharper disable once StaticMemberInGenericType
    public static int i_Index { get; } = T0.i_Index + 1;
    public static string i_Name => $"{T0.i_Name}, {T1.i_Name}";
    public static bool i_Is<T>() => T1.i_Is<T>();
}
