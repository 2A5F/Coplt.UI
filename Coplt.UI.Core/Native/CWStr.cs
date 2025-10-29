using Coplt.Com;

namespace Coplt.UI.Native;

public unsafe struct CWStr : IEquatable<CWStr>
{
    [ComType<ConstPtr<char>>]
    public char* Locale;

    public override string ToString() => new(Locale);

    public bool Equals(CWStr other) => Locale == other.Locale;
    public override bool Equals(object? obj) => obj is CWStr other && Equals(other);
    public override int GetHashCode() => unchecked((int)(long)Locale);
    public static bool operator ==(CWStr left, CWStr right) => left.Equals(right);
    public static bool operator !=(CWStr left, CWStr right) => !left.Equals(right);
}
