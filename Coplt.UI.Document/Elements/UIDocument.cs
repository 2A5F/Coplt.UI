using System.Collections;
using System.Text;
using Coplt.UI.BoxLayouts;
using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.Collections;
using Coplt.UI.Layouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

public class UIDocument<TRd, TEd>
    where TRd : new() where TEd : new()
{
    #region Fields

    internal UIElement<TRd, TEd>? m_root;
    
    internal Size<AvailableSpace>? m_last_available_size;
    internal ulong m_last_root_version;

    #endregion

    #region Root

    public UIElement<TRd, TEd> Root
    {
        get => m_root ??= new();
        set
        {
            if (value.Document == this) return;
            if (value.Document != null) value.Document.m_root = null;
            value.Parent?.Remove(value);
            m_root = value;
            m_last_available_size = null;
            m_last_root_version = 0;
        }
    }

    #endregion

    #region Compute

    public bool ComputeLayout(Size<AvailableSpace> available_space)
    {
        var root = Root;
        if (m_last_available_size == available_space && m_last_root_version == root.LayoutVersion) return false;
        m_last_available_size = available_space;
        m_last_root_version = root.LayoutVersion;
        LayoutTree<TRd, TEd> tree = new(this);
        BoxLayout.ComputeRootLayout<LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator, RefCoreStyle<CommonStyle>>(
            ref tree, root, available_space
        );
        BoxLayout.RoundLayout<LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator>(
            ref tree, root
        );
        return true;
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        if (m_root == null) return "Empty Tree";
        LayoutTree<TRd, TEd> tree = default;
        return PrintTree.Print<LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator>(ref tree, m_root);
    }

    #endregion
}

internal struct LayoutTree<TRd, TEd>(UIDocument<TRd, TEd> document)
    : ILayoutFlexboxContainer<UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator
            , RefCoreStyle<CommonStyle>, RefFlexContainerStyle<CommonStyle>, RefFlexItemStyle<CommonStyle>>,
        IRoundTree<UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator>,
        IPrintTree<UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator>,
        ICacheTree<UIElement<TRd, TEd>>
    where TRd : new() where TEd : new()
{
    public OrderedSet<UIElement<TRd, TEd>>.Enumerator ChildIds(UIElement<TRd, TEd> parent_node_id) =>
        parent_node_id.m_childs.GetEnumerator();

    public int ChildCount(UIElement<TRd, TEd> parent_node_id) => parent_node_id.m_childs.Count;

    public float Calc(CalcId id, float basis) => 0; // todo

    public RefCoreStyle<CommonStyle> GetCoreContainerStyle(UIElement<TRd, TEd> node_id) => new(ref node_id.m_common_style);
    public RefFlexContainerStyle<CommonStyle> GetFlexBoxContainerStyle(UIElement<TRd, TEd> node_id) => new(ref node_id.m_common_style);
    public RefFlexItemStyle<CommonStyle> GetFlexboxChildStyle(UIElement<TRd, TEd> child_node_id) => new(ref child_node_id.m_common_style);

    public void SetUnroundedLayout(UIElement<TRd, TEd> node_id, in UnroundedLayout layout) => node_id.m_unrounded_layout = layout;
    public LayoutOutput ComputeChildLayout(UIElement<TRd, TEd> node_id, LayoutInput inputs)
    {
        if (inputs.RunMode == RunMode.PerformHiddenLayout)
            return BoxLayout.ComputeHiddenLayout
                <LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator, RefCoreStyle<CommonStyle>>
                (ref this, node_id);
        return BoxLayout.ComputeCachedLayout(
            ref this, node_id, inputs,
            static (ref LayoutTree<TRd, TEd> tree, UIElement<TRd, TEd> node_id, LayoutInput inputs) => (node_id.m_common_style.Display, node_id.Count) switch
            {
                (Display.None, _) => BoxLayout.ComputeHiddenLayout
                    <LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator, RefCoreStyle<CommonStyle>>
                    (ref tree, node_id),
                (Display.Flex, > 0) => BoxLayout.ComputeFlexBoxLayout<LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator,
                        RefCoreStyle<CommonStyle>, RefFlexContainerStyle<CommonStyle>, RefFlexItemStyle<CommonStyle>>
                    (ref tree, node_id, inputs),
                (Display.Grid, > 0) => throw new NotImplementedException(),
                (Display.Block, > 0) => throw new NotImplementedException(),
                (_, <= 0) => BoxLayout.ComputeLeafLayout(
                    inputs, new RefCoreStyle<CommonStyle>(ref node_id.m_common_style), ref tree, node_id,
                    static (node, known_dimensions, available_space) =>
                        known_dimensions.Or(new Size<float>(0f)) // todo
                ),
                _ => throw new ArgumentOutOfRangeException()
            }
        );
    }

    public ref readonly UnroundedLayout GetUnroundedLayout(UIElement<TRd, TEd> node_id) => ref node_id.m_unrounded_layout;
    public void SetFinalLayout(UIElement<TRd, TEd> node_id, in Layout layout)
    {
        node_id.m_final_layout = layout;
        node_id.LayoutDirtyTouch(document);
    }

    public void FormatDebugLabel(UIElement<TRd, TEd> node_id, StringBuilder builder) => builder.Append($"{node_id}");
    public ref readonly Layout GetFinalLayout(UIElement<TRd, TEd> node_id) => ref node_id.m_final_layout;

    public LayoutOutput? CacheGet(
        UIElement<TRd, TEd> node_id, Size<float?> known_dimensions, Size<AvailableSpace> available_space, RunMode run_mode
    ) => node_id.m_cache.Get(known_dimensions, available_space, run_mode);
    public void CacheStore(
        UIElement<TRd, TEd> node_id, Size<float?> known_dimensions, Size<AvailableSpace> available_space, RunMode run_mode,
        LayoutOutput layout_output
    ) => node_id.m_cache.Store(known_dimensions, available_space, run_mode, layout_output);
    public void CacheClear(UIElement<TRd, TEd> node_id)
        => node_id.m_cache.Clear();
}
