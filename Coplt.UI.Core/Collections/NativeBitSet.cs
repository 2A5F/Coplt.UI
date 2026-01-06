using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Utilities;

namespace Coplt.UI.Collections;

[Dropping]
public unsafe partial struct NativeBitSet
{
    #region Fields

    private ulong* m_items;
    private int m_size;

    #endregion

    #region Ctor

    public NativeBitSet(int size)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

        var cap = (size + 63) / 64;
        m_items = cap == 0 ? null : ZAlloc(cap);
        m_size = size;
    }

    #endregion

    #region Private

    private static ulong* Alloc(int size) => (ulong*)NativeLib.Alloc(sizeof(ulong) * size, sizeof(ulong));
    private static ulong* ZAlloc(int size) => (ulong*)NativeLib.ZAlloc(sizeof(ulong) * size, sizeof(ulong));

    #endregion

    #region Drop

    [Drop]
    private void Drop()
    {
        if (m_items == null) return;
        NativeLib.Free(m_items);
        m_items = null;
        m_size = 0;
    }

    #endregion

    #region Props

    public int Length => m_size;

    public Span<ulong> RawData => new(m_items, (m_size + 63) / 64);

    #endregion

    #region Get Set

    public bool this[int index]
    {
        get
        {
            if ((uint)index >= (uint)m_size) return false;
            var (q, r) = Math.DivRem(index, 64);
            return (m_items[q] & (1UL << r)) != 0;
        }
        set
        {
            if ((uint)index >= (uint)m_size) return;
            var (q, r) = Math.DivRem(index, 64);
            if (value) m_items[q] |= 1UL << r;
            else m_items[q] &= ~(1UL << r);
        }
    }

    #endregion

    #region ReCtor

    public void ReCtor(int size)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        var old_cap = (m_size + 63) / 64;
        var cap = (size + 63) / 64;
        if (old_cap == cap)
        {
            if (cap == 0) return;
            new Span<ulong>(m_items, cap).Clear();
        }
        else
        {
            Drop();
            m_items = cap == 0 ? null : ZAlloc(size);
            m_size = size;
        }
    }

    public void ReCtorNoClear(int size)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        var old_cap = (m_size + 63) / 64;
        var cap = (size + 63) / 64;
        if (old_cap == cap) return;
        else
        {
            Drop();
            m_items = cap == 0 ? null : Alloc(size);
            m_size = size;
        }
    }

    #endregion

    #region Enumerator

    public struct Enumerator(NativeBitSet set) : IEnumerator<bool>
    {
        private int _index = -1;

        public bool MoveNext()
        {
            int index = _index + 1;
            if (index < set.Length)
            {
                _index = index;
                return true;
            }

            return false;
        }
        public bool Current => set[_index];

        object? IEnumerator.Current => Current;
        void IDisposable.Dispose() { }
        public void Reset() => throw new NotSupportedException();
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        for (var i = 0; i < m_size; i++)
        {
            if (i != 0) sb.Append(", ");
            sb.Append($"{(this[i] ? "true" : "false")}");
        }
        sb.Append(']');
        return sb.ToString();
    }

    #endregion
}
