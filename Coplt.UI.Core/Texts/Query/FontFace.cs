using System.Collections.Frozen;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.Texts;

[Dropping]
public sealed unsafe partial class FontFace : IEquatable<FontFace>
{
    #region CultureInfo

    internal static readonly CultureInfo s_culture_en_US = CultureInfo.GetCultureInfo("en-US");

    #endregion

    #region Fields

    [Drop]
    internal Rc<IFontFace> m_inner;
    internal NFontInfo* m_info;
    internal FrozenDictionary<CultureInfo, string>? m_family_names;
    internal FrozenDictionary<CultureInfo, string>? m_face_names;

    #endregion

    #region Properties

    public ref readonly Rc<IFontFace> Inner => ref m_inner;
    public ref readonly FontMetrics Metrics => ref m_info->Metrics;
    public FontWidth Width => m_info->Width;
    public FontWeight Weight => m_info->Weight;

    public FontFlags Flags => m_info->Flags;

    public bool IsColor => (m_info->Flags & FontFlags.Color) != 0;
    public bool IsMonospaced => (m_info->Flags & FontFlags.Monospaced) != 0;

    public FrozenDictionary<CultureInfo, string> FamilyNames =>
        m_family_names ?? Interlocked.CompareExchange(ref m_family_names, GetFamilyNames(), null) ?? m_family_names;

    public FrozenDictionary<CultureInfo, string> FaceNames =>
        m_face_names ?? Interlocked.CompareExchange(ref m_face_names, GetFaceNames(), null) ?? m_face_names;

    public string FamilyName =>
        FamilyNames.TryGetValue(s_culture_en_US, out var name) ? name : FamilyNames.FirstOrDefault().Value ?? "";

    public string LocalFamilyName =>
        FamilyNames.TryGetValue(CultureInfo.CurrentCulture, out var name) ? name : FamilyName;

    public string FaceName =>
        FaceNames.TryGetValue(s_culture_en_US, out var name) ? name : FaceNames.FirstOrDefault().Value ?? "";

    public string LocalFaceName =>
        FaceNames.TryGetValue(CultureInfo.CurrentCulture, out var name) ? name : FaceName;

    #endregion

    #region Ctor

    internal FontFace(Rc<IFontFace> inner)
    {
        m_inner = inner;
        m_info = m_inner.Info;
    }

    #endregion

    #region Drop

    [Drop]
    private void ClearInfo()
    {
        m_info = null;
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

    #region Names

    private FrozenDictionary<CultureInfo, string> GetFamilyNames()
    {
        Dictionary<CultureInfo, string> names = new();
        m_inner.GetFamilyNames(&names, &BuildNames).TryThrowWithMsg();
        return names.ToFrozenDictionary();
    }

    private FrozenDictionary<CultureInfo, string> GetFaceNames()
    {
        Dictionary<CultureInfo, string> names = new();
        m_inner.GetFaceNames(&names, &BuildNames).TryThrowWithMsg();
        return names.ToFrozenDictionary();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void BuildNames(void* ctx, char* p_lang, int lang_len, char* p_str, int str_len)
    {
        var names = *(Dictionary<CultureInfo, string>*)ctx;
        var lang = new Span<char>(p_lang, lang_len).ToString();
        var str = new Span<char>(p_str, str_len).ToString();
        names[CultureInfo.GetCultureInfo(lang)] = str;
    }

    #endregion

    #region ToString

    public override string ToString() =>
        $"FontFace({LocalFamilyName}/{LocalFaceName}) {{ Width = {Width}, Weight = {Weight}, Flags = {Flags} }}";

    #endregion
}
