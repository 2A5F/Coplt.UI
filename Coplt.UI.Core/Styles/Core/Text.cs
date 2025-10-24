using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;

namespace Coplt.UI.Core.Styles;

[Dropping]
public partial struct FontSet
{
    [Drop]
    [ComType<NativeList<Ptr<IFontFace>>>]
    public NativeList<Rc<IFontFace>> m_fallbacks;
}
