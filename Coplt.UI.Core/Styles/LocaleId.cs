using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Coplt.UI.Styles;

public unsafe struct LocaleId : IEquatable<LocaleId>
{
    public char* Name;

    private static readonly ConcurrentDictionary<string, LocaleId> m_map = new();
    public static LocaleId Null => default;

    public static LocaleId Default { get; } = Of(CultureInfo.CurrentCulture);

    public static LocaleId Of(string name)
    {
        return m_map.GetOrAdd(name, static name =>
        {
            var h_name = GCHandle.Alloc($"{name}", GCHandleType.Pinned);
            var p_name = (char*)h_name.AddrOfPinnedObject();
            return new LocaleId
            {
                Name = p_name
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

    public override string ToString() => new(Name);
}
