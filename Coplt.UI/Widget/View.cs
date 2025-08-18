using System.Diagnostics.CodeAnalysis;

namespace Coplt.UI.Widgets;

public ref struct View<Self>
{
    public void Dispose() { }

    [UnscopedRef]
    public ref View<Self> On<E>(Action<Self, E> Callback)
    {
        return ref this;
    }

    [UnscopedRef]
    public ref View<Self> Text(params ReadOnlySpan<string> StaticText)
    {
        return ref this;
    }

    [UnscopedRef]
    public ref View<Self> Text(Func<Self, string> DynamicText)
    {
        return ref this;
    }

    [UnscopedRef]
    public ref View<Self> Text<V>(Func<Self, V> DynamicText) =>
        ref Text(s => $"{DynamicText(s)}");
}

public ref struct View<Self, T> where T : AWidget
{
    public void Dispose() { }

    [UnscopedRef]
    public ref View<Self, T> On<E>(Action<Self, E> Callback)
    {
        return ref this;
    }

    [UnscopedRef]
    public ref View<Self, T> Pass<V>(string Name, Func<Self, V> Data)
    {
        return ref this;
    }
}

public ref struct Scope<Self>
{
    public void Dispose() { }

    [UnscopedRef]
    public ref Scope<Self> Key(Func<Self, int> Key)
    {
        return ref this;
    }
}

public struct LoopContext<T>
{
    public int Index => throw new NotImplementedException();
    public T Current => throw new NotImplementedException();
}
