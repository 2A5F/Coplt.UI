using System.Collections.Frozen;
using System.Globalization;
using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Layouts.Native;

namespace Coplt.UI.TextLayout;

[Dropping]
public sealed unsafe partial class FontCollection
{
    #region Fields

    [Drop]
    internal Rc<IFontCollection> m_inner;
    internal readonly FontFamily[] m_families;
    internal readonly uint m_default_family;
    internal readonly FrozenDictionary<string, uint> m_all_in_one_name_to_family;
    internal readonly FrozenDictionary<CultureInfo, FrozenDictionary<string, uint>> m_name_to_family;

    #endregion

    #region Properties

    public ref readonly Rc<IFontCollection> Inner => ref m_inner;
    public ReadOnlySpan<FontFamily> Families => m_families;

    public FontFamily DefaultFamily => m_families[m_default_family];

    public FrozenDictionary<string, uint> NameToFamily => m_all_in_one_name_to_family;
    public FrozenDictionary<CultureInfo, FrozenDictionary<string, uint>> CulturedNameToFamily => m_name_to_family;

    #endregion

    #region Ctor

    internal FontCollection(Rc<IFontCollection> inner)
    {
        m_inner = inner;
        uint len;
        var p_families = inner.GetFamilies(&len);
        m_families = new FontFamily[len];
        for (var i = 0u; i < len; i++)
        {
            var p = p_families[i];
            p->AddRef();
            m_families[i] = new(new(p), this, i);
        }
        inner.ClearNativeFamiliesCache();

        m_default_family = inner.FindDefaultFamily();

        Dictionary<string, uint> all_in_one_name_to_family = new();
        Dictionary<CultureInfo, Dictionary<string, uint>> name_to_family = new();
        for (var i = 0u; i < m_families.Length; i++)
        {
            var family = m_families[i];
            foreach (var name in family.Names)
            {
                var lower = name.Value.ToLower();
                all_in_one_name_to_family.TryAdd(lower, i);
                ref var map = ref CollectionsMarshal.GetValueRefOrAddDefault(name_to_family, name.Key, out var exists);
                if (!exists) map = new();
                map!.TryAdd(lower, i);
            }
        }
        m_all_in_one_name_to_family = all_in_one_name_to_family.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        m_name_to_family = name_to_family
            .ToFrozenDictionary(a => a.Key, a => a.Value.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));
    }

    #endregion

    #region Find

    public FontFamily? Find(string name) =>
        m_all_in_one_name_to_family.TryGetValue(name, out var value) ? m_families[value] : null;
    public FontFamily? Find(CultureInfo culture, string name) =>
        m_name_to_family.TryGetValue(culture, out var map) && map.TryGetValue(name, out var value) ? m_families[value] : null;

    #endregion
}
