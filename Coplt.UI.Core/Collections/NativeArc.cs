using System.Runtime.CompilerServices;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Utilities;

namespace Coplt.UI.Collections;

internal struct NativeArcInner<T>
{
    public ulong m_count;
    public T m_data;
}

[Dropping]
public unsafe partial struct NativeArc<T> : IEquatable<NativeArc<T>>
{
    #region Static Check

    static NativeArc()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            throw new NotSupportedException("Only unmanaged types are supported");
    }

    #endregion

    #region Fields

    internal NativeArcInner<T>* m_ptr;

    #endregion

    #region Drop

    private void Drop()
    {
        if (m_ptr == null) return;
        if (Interlocked.Decrement(ref m_ptr->m_count) == 0)
        {
            if (DisposeProxy<T>.IsDisposable)
            {
                DisposeProxy<T>.Dispose(ref m_ptr->m_data);
            }
            NativeLib.Instance.Free(m_ptr);
        }
        m_ptr = null;
    }

    #endregion

    #region Ctor

    public NativeArc(T value)
    {
        this = New();
        m_ptr->m_data = value;
    }
    
    private NativeArc(NativeArcInner<T>* ptr)
    {
        m_ptr = ptr;
    }

    public static NativeArc<T> New()
    {
        var ptr = NativeLib.Instance.Alloc<NativeArcInner<T>>();
        ptr->m_count = 1;
        return new(ptr);
    }

    #endregion

    #region Props

    public ref T Value => ref m_ptr->m_data;
    public T* Ptr => (&m_ptr->m_data)!;

    #endregion

    #region Equals

    public bool Equals(NativeArc<T> other) => m_ptr == other.m_ptr || EqualityComparer<T>.Default.Equals(Value, other.Value);
    public override bool Equals(object? obj) => obj is NativeArc<T> other && Equals(other);
    public override int GetHashCode() => m_ptr == null ? 0 : EqualityComparer<T>.Default.GetHashCode(Value!);
    public static bool operator ==(NativeArc<T> left, NativeArc<T> right) => left.Equals(right);
    public static bool operator !=(NativeArc<T> left, NativeArc<T> right) => !left.Equals(right);

    #endregion

    #region ToString

    public override string ToString() => m_ptr == null ? "null" : m_ptr->ToString()!;

    #endregion

    #region GetPinnableReference

    public ref T GetPinnableReference() => ref Value;

    #endregion

    #region Move

    public NativeArc<T> Move() => Swap(default);

    public NativeArc<T> Swap(NativeArc<T> other)
    {
        var self = this;
        this = other;
        return self;
    }

    #endregion

    #region Clone

    public NativeArc<T> Clone()
    {
        if (m_ptr == null) return default;
        Interlocked.Increment(ref m_ptr->m_count);
        return this;
    }

    #endregion
}
