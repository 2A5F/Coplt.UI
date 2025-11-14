using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.Texts;

[Dropping]
public sealed unsafe partial class FontFace : IEquatable<FontFace>
{
    #region Fields

    [Drop]
    internal Rc<IFontFace> m_inner;

    #endregion

    #region Properties

    public ref readonly Rc<IFontFace> Inner => ref m_inner;

    #endregion

    #region Ctor

    internal FontFace(Rc<IFontFace> inner)
    {
        m_inner = inner;
    }

    #endregion

    #region Equals

    public bool Equals(FontFace? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (m_inner.Equals(other.m_inner)) return true;
        return m_inner.Handle->Equals(other.m_inner.Handle);
    }
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FontFace other && Equals(other);
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => m_inner.HashCode();
    public static bool operator ==(FontFace? left, FontFace? right) => Equals(left, right);
    public static bool operator !=(FontFace? left, FontFace? right) => !Equals(left, right);

    #endregion
}
