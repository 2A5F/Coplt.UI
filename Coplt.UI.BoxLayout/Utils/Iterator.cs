using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Coplt.UI.BoxLayout.Utilities;

public interface IIterator<out T>
    where T : allows ref struct
{
    public T Current { get; }

    public bool MoveNext();
}

public readonly ref struct IteratorEnumerable<TIterator, T>(TIterator Value)
    where TIterator : IIterator<T>, allows ref struct
    where T : allows ref struct
{
    public readonly TIterator Value = Value;

    public IteratorIEnumerator<TIterator, T> GetEnumerator() => new(Value);
}

public ref struct IteratorIEnumerator<TIterator, T>(TIterator Value)
    where TIterator : IIterator<T>, allows ref struct
    where T : allows ref struct
{
    public TIterator Value = Value;

    public T Current => Value.Current;
    public bool MoveNext() => Value.MoveNext();
}

public static class IteratorEx
{
    public static IteratorEnumerable<TIterator, T> AsEnumerable<TIterator, T>(this TIterator Value)
        where TIterator : IIterator<T>, allows ref struct
        where T : allows ref struct => new(Value);

    public static SpanIter<T> AsIter<T>(this Span<T> self) => new(self);

    public static RoSpanIter<T> AsIter<T>(this ReadOnlySpan<T> self) => new(self);

    public static SpanIter<T> AsIter<T>(this T[] self) => new(self.AsSpan());

    public static SpanIter<T> AsIter<T>(this List<T> self) => new(CollectionsMarshal.AsSpan(self));
}

public ref struct SpanIter<T>(Span<T> self) : IIterator<T>
{
    private Span<T>.Enumerator enumerator = self.GetEnumerator();

    public T Current => enumerator.Current;
    public bool MoveNext() => enumerator.MoveNext();
}

public ref struct RoSpanIter<T>(ReadOnlySpan<T> self) : IIterator<T>
{
    private ReadOnlySpan<T>.Enumerator enumerator = self.GetEnumerator();

    public T Current => enumerator.Current;
    public bool MoveNext() => enumerator.MoveNext();
}
