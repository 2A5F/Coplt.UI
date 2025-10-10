using Coplt.Com;
using Coplt.Com.OpaqueTypes;
using Coplt.Dropping;
using Coplt.UI.Styles;

namespace Coplt.UI.Native;

[Dropping]
public partial struct UiNodeData()
{
    [ComType<Ptr<ComVoid>>]
    public object? Object = null;
    
    public StyleData Style;
    public LayoutData Layout;
    public LayoutData FinalLayout;
    public LayoutCache LayoutCache;
}
