using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct ContainerLayoutData
{
    [Drop]
    [ComType<Ptr<ITextLayout>>]
    public Rc<ITextLayout> TextLayoutObject;
    public LayoutData FinalLayout;
    public LayoutData Layout;
    public LayoutCache LayoutCache;
}
