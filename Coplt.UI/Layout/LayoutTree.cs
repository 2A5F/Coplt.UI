using System.Text;
using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.BoxLayouts;
using Coplt.UI.Collections;
using Coplt.UI.Elements;
using Coplt.UI.Layouts;
using Coplt.UI.Styles;

namespace Coplt.UI;

internal struct LayoutTree
    : ILayoutFlexboxContainer<UIElement, OrderedSet<UIElement>.Enumerator
            , RefCoreStyle<StyleSet>, RefFlexContainerStyle<StyleSet>, RefFlexItemStyle<StyleSet>>,
        IPrintTree<UIElement, OrderedSet<UIElement>.Enumerator>
{
    public OrderedSet<UIElement>.Enumerator ChildIds(UIElement parent_node_id) =>
        parent_node_id.m_childs.GetEnumerator();

    public int ChildCount(UIElement parent_node_id) => parent_node_id.m_childs.Count;

    public float Calc(CalcId id, float basis) => 0; // todo

    public RefCoreStyle<StyleSet> GetCoreContainerStyle(UIElement node_id) => new(ref node_id.m_computed_style);
    public RefFlexContainerStyle<StyleSet> GetFlexBoxContainerStyle(UIElement node_id) => new(ref node_id.m_computed_style);
    public RefFlexItemStyle<StyleSet> GetFlexboxChildStyle(UIElement child_node_id) => new(ref child_node_id.m_computed_style);

    public void SetUnroundedLayout(UIElement node_id, in Layout layout) => node_id.m_unrounded_layout = layout;
    public LayoutOutput ComputeChildLayout(UIElement node_id, LayoutInput inputs) => BoxLayout
        .ComputeFlexBoxLayout<LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator,
                RefCoreStyle<StyleSet>, RefFlexContainerStyle<StyleSet>, RefFlexItemStyle<StyleSet>>
            (ref this, node_id, inputs);

    public void FormatDebugLabel(UIElement node_id, StringBuilder builder) => builder.Append($"{node_id}");
    public ref readonly Layout GetFinalLayout(UIElement node_id) => ref node_id.UnroundedLayout; // todo round
}
