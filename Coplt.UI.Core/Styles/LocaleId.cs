using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Coplt.UI.Styles;

public unsafe struct LocaleId() : IEquatable<LocaleId>
{
    // layout [utf16] 0 0 [ascii] 0
    public byte* Name = null;
    public nuint Length = 0;

    private static readonly ConcurrentDictionary<string, LocaleId> m_map = new();
    public static LocaleId Null => default;

    public static LocaleId Default { get; } = Of(CultureInfo.CurrentCulture);
    public static LocaleId DefaultUI { get; } = Of(CultureInfo.CurrentUICulture);

    public static LocaleId Of(string name)
    {
        return m_map.GetOrAdd(name, static name =>
        {
            var arr_name = GC.AllocateArray<byte>(name.Length * 3 + 3, true);
            MemoryMarshal.Cast<char, byte>(name.AsSpan()).CopyTo(arr_name);
            Encoding.ASCII.GetBytes(name, arr_name.AsSpan(name.Length * 2 + 2));
            var h_name = GCHandle.Alloc(arr_name, GCHandleType.Pinned);
            var p_name = (byte*)h_name.AddrOfPinnedObject();
            return new LocaleId
            {
                Name = p_name,
                Length = (uint)name.Length,
            };
        });
    }

    public static LocaleId Of(CultureInfo info) => Of(info.Name);

    public bool Equals(LocaleId other) => Name == other.Name;
    public override bool Equals(object? obj) => obj is LocaleId other && Equals(other);
    public override int GetHashCode() => ((nint)Name).GetHashCode();
    public static bool operator ==(LocaleId left, LocaleId right) => left.Equals(right);
    public static bool operator !=(LocaleId left, LocaleId right) => !left.Equals(right);

    public bool IsNull => Name == null;
    public static bool operator true(LocaleId self) => self.Name != null;
    public static bool operator false(LocaleId self) => self.Name == null;
    public static bool operator !(LocaleId self) => self.Name != null;

    public override string ToString() => new ReadOnlySpan<char>((char*)Name, (int)Length).ToString();
}
