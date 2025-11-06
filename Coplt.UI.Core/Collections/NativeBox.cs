using System.Runtime.CompilerServices;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Utilities;

namespace Coplt.UI.Collections;

[Dropping]
public unsafe partial struct NativeBox<T>(T* ptr) : IEquatable<NativeBox<T>>
{
    #region Static Check

    static NativeBox()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            throw new NotSupportedException("Only unmanaged types are supported");
    }

    #endregion

    #region Fields

    internal T* m_ptr = ptr;

    #endregion

    #region Drop

    private void Drop()
    {
        if (m_ptr == null) return;
        if (DisposeProxy<T>.IsDisposable)
        {
            DisposeProxy<T>.Dispose(ref *m_ptr);
        }
        NativeLib.Free(m_ptr);
        m_ptr = null;
    }

    #endregion

    #region Ctor

    public NativeBox(T value) : this(NativeLib.Alloc<T>())
    {
        *m_ptr = value;
    }

    public static NativeBox<T> New() => new(NativeLib.Alloc<T>());

    #endregion

    #region Props

    public ref T Value => ref *m_ptr;
    public T* Ptr => m_ptr;

    #endregion

    #region Equals

    public bool Equals(NativeBox<T> other) => m_ptr == other.m_ptr || EqualityComparer<T>.Default.Equals(Value, other.Value);
    public override bool Equals(object? obj) => obj is NativeBox<T> other && Equals(other);
    public override int GetHashCode() => m_ptr == null ? 0 : EqualityComparer<T>.Default.GetHashCode(Value!);
    public static bool operator ==(NativeBox<T> left, NativeBox<T> right) => left.Equals(right);
    public static bool operator !=(NativeBox<T> left, NativeBox<T> right) => !left.Equals(right);

    #endregion

    #region ToString

    public override string ToString() => m_ptr == null ? "null" : m_ptr->ToString()!;

    #endregion

    #region GetPinnableReference

    public ref T GetPinnableReference() => ref Value;

    #endregion

    #region Move

    public NativeBox<T> Move() => Swap(default);

    public NativeBox<T> Swap(NativeBox<T> other)
    {
        var self = this;
        this = other;
        return self;
    }

    #endregion
}
