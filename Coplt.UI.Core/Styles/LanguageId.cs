using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Coplt.UI.Styles;

public unsafe struct LanguageId : IEquatable<LanguageId>
{
    public char* Name;

    private static readonly ConcurrentDictionary<string, LanguageId> m_map = new();

    public static LanguageId Default { get; } = Of(CultureInfo.CurrentCulture);

    public static LanguageId Of(string name)
    {
        return m_map.GetOrAdd(name, static name =>
        {
            var h_name = GCHandle.Alloc($"{name}", GCHandleType.Pinned);
            var p_name = (char*)h_name.AddrOfPinnedObject();
            return new LanguageId
            {
                Name = p_name
            };
        });
    }

    public static LanguageId Of(CultureInfo info) => Of(info.TwoLetterISOLanguageName);

    public bool Equals(LanguageId other) => Name == other.Name;
    public override bool Equals(object? obj) => obj is LanguageId other && Equals(other);
    public override int GetHashCode() => ((nint)Name).GetHashCode();
    public static bool operator ==(LanguageId left, LanguageId right) => left.Equals(right);
    public static bool operator !=(LanguageId left, LanguageId right) => !left.Equals(right);

    public override string ToString() => new(Name);
}
