using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Layouts.Native;

namespace Coplt.UI.TextLayout;

[Dropping]
public sealed partial class FontCollection
{
    #region Fields

    internal Rc<IFontCollection> m_inner;

    #endregion

    #region Properties

    public ref readonly Rc<IFontCollection> Inner => ref m_inner;

    #endregion

    #region Ctor

    internal FontCollection(Rc<IFontCollection> inner) => m_inner = inner;

    #endregion
}
