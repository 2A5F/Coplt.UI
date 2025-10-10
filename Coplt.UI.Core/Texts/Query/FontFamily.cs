using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;

namespace Coplt.UI.Texts;

[Dropping]
public sealed unsafe partial class FontFamily
{
    #region CultureInfo

    internal static readonly CultureInfo s_culture_en_US = CultureInfo.GetCultureInfo("en-US");

    #endregion

    #region Fields

    internal readonly TextLayout m_lib;
    [Drop]
    internal Rc<IFontFamily> m_inner;
    internal readonly FrozenDictionary<CultureInfo, string> m_names;
    internal readonly FontCollection? m_collection;
    internal readonly uint m_index_in_collection;

    internal volatile Font[]? m_fonts;
    
    [field: AllowNull, MaybeNull]
    internal Lock m_load_fonts_lock =>
        field ?? Interlocked.CompareExchange(ref field, new Lock(), null) ?? field;

    #endregion

    #region Properties

    public TextLayout Lib => m_lib;
    public ref readonly Rc<IFontFamily> Inner => ref m_inner;
    public FrozenDictionary<CultureInfo, string> Names => m_names;
    public FontCollection? Collection => m_collection;
    public uint Index => m_index_in_collection;

    public string Name =>
        m_names.TryGetValue(s_culture_en_US, out var name) ? name : m_names.FirstOrDefault().Value ?? "";

    public string LocalName =>
        m_names.TryGetValue(CultureInfo.CurrentCulture, out var name) ? name : Name;

    #endregion

    #region Ctor

    internal FontFamily(Rc<IFontFamily> inner, FontCollection collection, uint index)
    {
        m_lib = collection.m_lib;
        m_inner = inner;
        m_collection = collection;
        m_index_in_collection = index;
        CultureInfo[] cultures;
        {
            uint len;
            var p_names = inner.GetLocalNames(&len);
            cultures = new CultureInfo[len];
            for (var i = 0; i < len; i++)
            {
                cultures[i] = CultureInfo.GetCultureInfo(p_names[i].ToString());
            }
        }
        Dictionary<CultureInfo, string> names = new();
        {
            uint len;
            var p_names = inner.GetNames(&len);
            for (var i = 0; i < len; i++)
            {
                var name = p_names[i].Name.ToString();
                var culture = cultures[p_names[i].Local];
                names.Add(culture, name);
            }
        }
        m_names = names.ToFrozenDictionary();
        inner.ClearNativeNamesCache();
    }

    #endregion

    #region ToString

    public override string ToString() => $"FontFamily {{ {string.Join(", ", m_names.Select(a => $"{a.Key}: {a.Value}"))} }}";

    #endregion

    #region Fonts

    public ReadOnlySpan<Font> GetFonts() => GetFontsInternal();

    internal Font[] GetFontsInternal()
    {
        var fonts = m_fonts;
        if (fonts != null) return fonts;
        lock (m_load_fonts_lock)
        {
            if (m_fonts != null) return m_fonts;
            uint num_fonts;
            var pp_fp = m_inner.GetFonts(&num_fonts);
            fonts = new Font[num_fonts];
            for (var i = 0; i < num_fonts; i++)
            {
                ref readonly var fp = ref pp_fp[i];
                fp.Font->AddRef();
                fonts[i] = new Font(new(fp.Font), fp.Info, this, i);
            }
            m_inner.ClearNativeFontsCache();
            m_fonts = fonts;
            return fonts;
        }
    }

    #endregion
}
