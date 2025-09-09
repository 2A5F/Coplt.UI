using System.Text;
using Coplt.Dropping;
using Coplt.UI.BoxLayouts;
using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.Collections;
using Coplt.UI.Document.Interfaces;
using Coplt.UI.Layouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

[Dropping(Unmanaged = true)]
public sealed partial class UIDocument<TRd, TEd>
    where TRd : IRenderData, new() where TEd : new()
{
    #region Fields

    [Drop]
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

    public bool ComputeLayout(Size<AvailableSpace> available_space, bool round = true)
    {
        var root = Root;
        if (m_last_available_size == available_space && m_last_root_version == root.LayoutVersion) return false;
        m_last_available_size = available_space;
        m_last_root_version = root.LayoutVersion;
        LayoutTree<TRd, TEd> tree = new(this);
        BoxLayout.ComputeRootLayout<LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator, StyleRef>(
            ref tree, root, available_space
        );
        if (round)
        {
            BoxLayout.RoundLayout<LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator>(
                ref tree, root
            );
        }
        else
        {
            BoxLayout.NoRoundLayout<LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator>(
                ref tree, root
            );
        }
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
            , StyleRef, StyleRef, StyleRef>,
        IRoundTree<UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator>,
        IPrintTree<UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator>,
        ICacheTree<UIElement<TRd, TEd>>
    where TRd : IRenderData, new() where TEd : new()
{
    public OrderedSet<UIElement<TRd, TEd>>.Enumerator ChildIds(UIElement<TRd, TEd> parent_node_id) =>
        parent_node_id.m_childs.GetEnumerator();

    public int ChildCount(UIElement<TRd, TEd> parent_node_id) => parent_node_id.m_childs.Count;

    public float Calc(CalcId id, float basis) => 0; // todo

    public StyleRef GetCoreContainerStyle(UIElement<TRd, TEd> node_id) => new(ref node_id.m_common_style);
    public StyleRef GetFlexBoxContainerStyle(UIElement<TRd, TEd> node_id) => new(ref node_id.m_common_style);
    public StyleRef GetFlexboxChildStyle(UIElement<TRd, TEd> child_node_id) => new(ref child_node_id.m_common_style);

    public void SetUnroundedLayout(UIElement<TRd, TEd> node_id, in UnroundedLayout layout) => node_id.m_unrounded_layout = layout;
    public LayoutOutput ComputeChildLayout(UIElement<TRd, TEd> node_id, LayoutInput inputs)
    {
        if (inputs.RunMode == RunMode.PerformHiddenLayout)
            return BoxLayout.ComputeHiddenLayout
                <LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator, StyleRef>
                (ref this, node_id);
        return BoxLayout.ComputeCachedLayout(
            ref this, node_id, inputs,
            static (ref tree, node_id, inputs) => (node_id.m_common_style.Display, node_id.Count) switch
            {
                (Display.None, _) => BoxLayout.ComputeHiddenLayout
                    <LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator, StyleRef>
                    (ref tree, node_id),
                (Display.Flex, > 0) => BoxLayout.ComputeFlexBoxLayout<
                    LayoutTree<TRd, TEd>, UIElement<TRd, TEd>, OrderedSet<UIElement<TRd, TEd>>.Enumerator,
                    StyleRef, StyleRef, StyleRef
                >(ref tree, node_id, inputs),
                (Display.Grid, > 0) => throw new NotImplementedException(),
                (Display.Block, > 0) => throw new NotImplementedException(),
                (_, <= 0) => BoxLayout.ComputeLeafLayout(
                    inputs, new StyleRef(ref node_id.m_common_style), ref tree, node_id,
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

internal ref struct StyleRef(ref readonly CommonStyle Target) : IBlockContainerStyle, IFlexContainerStyle, IFlexItemStyle
{
    public ref readonly CommonStyle Target = ref Target;

    public BoxGenerationMode BoxGenerationMode => Target.Display == Display.None ? BoxGenerationMode.None : BoxGenerationMode.Normal;
    public bool IsBlock => Target.Display == Display.Block;
    public bool IsCompressibleReplaced => false;
    public BoxSizing BoxSizing => Target.BoxSizing;
    public Point<Overflow> Overflow => Target.Overflow;
    public float ScrollbarWidth => 0;
    public Position Position => Target.Position;
    public Rect<LengthPercentageAuto> Inset => Target.Inset;
    public Size<Dimension> Size => Target.Size;
    public Size<Dimension> MinSize => Target.MinSize;
    public Size<Dimension> MaxSize => Target.MaxSize;
    public float? AspectRatio => Target.AspectRatio;
    public Rect<LengthPercentageAuto> Margin => Target.Margin;
    public Rect<LengthPercentage> Padding => Target.Padding;
    public Rect<LengthPercentage> Border => Target.Border;
    public TextAlign TextAlign => Target.TextAlign;
    public FlexDirection FlexDirection => Target.FlexDirection;
    public FlexWrap FlexWrap => Target.FlexWrap;
    public Size<LengthPercentage> Gap => Target.Gap;
    public AlignContent? AlignContent => Target.AlignContent;
    public AlignItems? AlignItems => Target.AlignItems;
    public JustifyContent? JustifyContent => Target.JustifyContent;
    public Dimension FlexBias => Target.FlexBias;
    public float FlexGrow => Target.FlexGrow;
    public float FlexShrink => Target.FlexShrink;
    public AlignSelf? AlignSelf => Target.AlignSelf;
}
