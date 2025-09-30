using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Layouts.Native;

namespace Coplt.UI.TextLayout;

[Dropping]
public sealed unsafe partial class FontCollection
{
    #region Fields

    internal Rc<IFontCollection> m_inner;
    internal readonly FontFamily[] m_families;
    internal readonly uint m_default_family;

    #endregion

    #region Properties

    public ref readonly Rc<IFontCollection> Inner => ref m_inner;
    public ReadOnlySpan<FontFamily> Families => m_families;
    
    public FontFamily DefaultFamily => m_families[m_default_family];

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
            m_families[i] = new(new(p));
        }
        inner.ClearNativeFamiliesCache();

        m_default_family = inner.FindDefaultFamily();
    }

    #endregion
}
