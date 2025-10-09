using System.Collections.Frozen;
using System.Globalization;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Layouts.Native;

namespace Coplt.UI.TextLayout;

[Dropping]
public sealed unsafe partial class FontFamily
{
    #region Fields

    [Drop]
    internal Rc<IFontFamily> m_inner;
    internal readonly FrozenDictionary<CultureInfo, string> m_names;
    internal readonly FontCollection m_collection;
    internal readonly uint m_index_in_collection;

    #endregion

    #region Properties

    public ref readonly Rc<IFontFamily> Inner => ref m_inner;
    public FrozenDictionary<CultureInfo, string> Names => m_names;
    public FontCollection Collection => m_collection;
    public uint Index => m_index_in_collection;

    #endregion

    #region Ctor

    internal FontFamily(Rc<IFontFamily> inner, FontCollection collection, uint index)
    {
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
}
