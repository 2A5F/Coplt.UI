using System.Collections;
using System.Text;
using Coplt.UI.BoxLayouts;
using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.Collections;
using Coplt.UI.Layouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

public sealed class UIDocument
{
    #region Fields

    internal UIElement? m_root;

    #endregion

    #region Props

    public UIPanel? Panel { get; internal set; }

    #endregion

    #region Root

    public void SetRoot(UIElement root)
    {
        if (root.Document == this) return;
        if (root.Document != null) root.Document.m_root = null;
        root.Parent?.Remove(root);
        m_root = root;
    }

    #endregion

    #region Compute

    public void ComputeLayout(Size<AvailableSpace> available_space, bool use_rounding = true)
    {
        if (m_root == null) return;
        LayoutTree tree = default;
        BoxLayout.ComputeRootLayout<LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator, RefCoreStyle<StyleSet>>(
            ref tree, m_root, available_space
        );
        if (use_rounding)
        {
            BoxLayout.RoundLayout<LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator>(
                ref tree, m_root
            );
        }
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        if (m_root == null) return "Empty Tree";
        LayoutTree tree = default;
        return PrintTree.Print<LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator>(ref tree, m_root);
    }

    #endregion
}

internal struct LayoutTree
    : ILayoutFlexboxContainer<UIElement, OrderedSet<UIElement>.Enumerator
            , RefCoreStyle<StyleSet>, RefFlexContainerStyle<StyleSet>, RefFlexItemStyle<StyleSet>>,
        IRoundTree<UIElement, OrderedSet<UIElement>.Enumerator>,
        IPrintTree<UIElement, OrderedSet<UIElement>.Enumerator>,
        ICacheTree<UIElement>
{
    public OrderedSet<UIElement>.Enumerator ChildIds(UIElement parent_node_id) =>
        parent_node_id.m_childs.GetEnumerator();

    public int ChildCount(UIElement parent_node_id) => parent_node_id.m_childs.Count;

    public float Calc(CalcId id, float basis) => 0; // todo

    public RefCoreStyle<StyleSet> GetCoreContainerStyle(UIElement node_id) => new(ref node_id.m_computed_style);
    public RefFlexContainerStyle<StyleSet> GetFlexBoxContainerStyle(UIElement node_id) => new(ref node_id.m_computed_style);
    public RefFlexItemStyle<StyleSet> GetFlexboxChildStyle(UIElement child_node_id) => new(ref child_node_id.m_computed_style);

    public void SetUnroundedLayout(UIElement node_id, in Layout layout) => node_id.m_unrounded_layout = layout;
    public LayoutOutput ComputeChildLayout(UIElement node_id, LayoutInput inputs)
    {
        if (inputs.RunMode == RunMode.PerformHiddenLayout)
            return BoxLayout.ComputeHiddenLayout
                <LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator, RefCoreStyle<StyleSet>>
                (ref this, node_id);
        return BoxLayout.ComputeCachedLayout(
            ref this, node_id, inputs,
            static (ref LayoutTree tree, UIElement node_id, LayoutInput inputs) => (node_id.m_computed_style.Display, node_id.Count) switch
            {
                (Display.None, _) => BoxLayout.ComputeHiddenLayout
                    <LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator, RefCoreStyle<StyleSet>>
                    (ref tree, node_id),
                (Display.Flex, > 0) => BoxLayout.ComputeFlexBoxLayout<LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator,
                        RefCoreStyle<StyleSet>, RefFlexContainerStyle<StyleSet>, RefFlexItemStyle<StyleSet>>
                    (ref tree, node_id, inputs),
                (Display.Grid, > 0) => throw new NotImplementedException(),
                (Display.Block, > 0) => throw new NotImplementedException(),
                (_, <= 0) => BoxLayout.ComputeLeafLayout(
                    inputs, new RefCoreStyle<StyleSet>(ref node_id.m_computed_style), ref tree, node_id,
                    static (node, known_dimensions, available_space) =>
                        known_dimensions.Or(new Size<float>(0f)) // todo
                ),
                _ => throw new ArgumentOutOfRangeException()
            }
        );
    }

    public ref readonly Layout GetUnroundedLayout(UIElement node_id) => ref node_id.m_unrounded_layout;
    public void SetFinalLayout(UIElement node_id, in Layout layout) => node_id.m_final_layout = layout;

    public void FormatDebugLabel(UIElement node_id, StringBuilder builder) => builder.Append($"{node_id}");
    public ref readonly Layout GetFinalLayout(UIElement node_id) => ref node_id.m_final_layout;

    public LayoutOutput? CacheGet(
        UIElement node_id, Size<float?> known_dimensions, Size<AvailableSpace> available_space, RunMode run_mode
    ) => node_id.m_cache.Get(known_dimensions, available_space, run_mode);
    public void CacheStore(
        UIElement node_id, Size<float?> known_dimensions, Size<AvailableSpace> available_space, RunMode run_mode,
        LayoutOutput layout_output
    ) => node_id.m_cache.Store(known_dimensions, available_space, run_mode, layout_output);
    public void CacheClear(UIElement node_id)
        => node_id.m_cache.Clear();
}
