using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

// Benchmarking is needed; it's unclear whether gchandle or native allocation performs better.

public unsafe struct NString : IDisposable, IEquatable<NString>
{
    [ComType<ConstPtr<char>>]
    private char* m_str;
    private void* m_handle;

    private NString(string str)
    {
        var handle = GCHandle.Alloc(str, GCHandleType.Pinned);
        m_str = (char*)handle.AddrOfPinnedObject();
        m_handle = (void*)GCHandle.ToIntPtr(handle);
    }

    public static NString Create(string str) => new(str);

    public void Dispose()
    {
        if (m_handle == null) return;
        var handle = GCHandle.FromIntPtr((nint)m_handle);
        handle.Free();
        m_handle = null;
        m_str = null;
    }

    public override string ToString()
    {
        if (m_handle == null) return "";
        var handle = GCHandle.FromIntPtr((nint)m_handle);
        return Unsafe.As<string>(handle.Target)!;
    }

    public bool Equals(NString other) => m_str == other.m_str;
    public override bool Equals(object? obj) => obj is NString other && Equals(other);
    public override int GetHashCode() => m_str == null ? 0 : ((nuint)m_str).GetHashCode();
    public static bool operator ==(NString left, NString right) => left.Equals(right);
    public static bool operator !=(NString left, NString right) => !left.Equals(right);

    public NString Move()
    {
        var self = this;
        this = default;
        return self;
    }
}
