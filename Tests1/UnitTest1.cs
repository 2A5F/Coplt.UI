using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Coplt.UI.BoxLayout.Utilities;
using Coplt.UI.BoxLayouts;
using Coplt.UI.Layouts;
using Coplt.UI.Styles;

namespace Tests1;

public class Tests
{
    public enum NodeKind
    {
        Flexbox,
        Image,
    }

    public class Node
    {
        public readonly List<Node> Childs = new();
        public NodeKind Kind;
        public BoxStyle Style;
        public Size<float> LeafSize;
        public Layout UnroundedLayout;

        #region Ctor

        public static Node NewRow(BoxStyle style) => new()
        {
            Kind = NodeKind.Flexbox,
            Style = style with { Display = Display.Flex, FlexDirection = FlexDirection.Row },
            LeafSize = default,
            UnroundedLayout = default
        };

        public static Node NewColumn(BoxStyle style) => new()
        {
            Kind = NodeKind.Flexbox,
            Style = style with { Display = Display.Flex, FlexDirection = FlexDirection.Column },
            LeafSize = default,
            UnroundedLayout = default
        };

        public static Node NewImage(BoxStyle style, Size<float> size) => new()
        {
            Kind = NodeKind.Image,
            Style = style,
            LeafSize = size,
            UnroundedLayout = default
        };

        #endregion

        #region Child

        public void Add(Node child) => Childs.Add(child);

        #endregion

        #region Compute

        public void ComputeLayout(Size<AvailableSpace> available_space, bool use_rounding)
        {
            StatelessLayoutTree tree = default;
            BoxLayout.ComputeRootLayout<StatelessLayoutTree, Node, SpanIter<Node>, RefCoreStyle<BoxStyle>>(ref tree, this, available_space);
            // todo rounding
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            StatelessLayoutTree tree = default;
            return PrintTree.Print<StatelessLayoutTree, Node, SpanIter<Node>>(ref tree, this);
        }

        #endregion
    }

    public struct StatelessLayoutTree :
        ILayoutFlexboxContainer<Node, SpanIter<Node>, RefCoreStyle<BoxStyle>, RefFlexContainerStyle<BoxStyle>, RefFlexItemStyle<BoxStyle>>,
        IPrintTree<Node, SpanIter<Node>>
    {
        public SpanIter<Node> ChildIds(Node parent_node_id) => new(CollectionsMarshal.AsSpan(parent_node_id.Childs));
        public int ChildCount(Node parent_node_id) => parent_node_id.Childs.Count;
        public Node GetChildId(Node parent_node_id, int index) => parent_node_id.Childs[index];
        public float Calc(CalcId id, float basis) => 0;
        public RefCoreStyle<BoxStyle> GetCoreContainerStyle(Node node_id) => new(ref node_id.Style);
        public RefFlexContainerStyle<BoxStyle> GetFlexBoxContainerStyle(Node node_id) => new(ref node_id.Style);
        public RefFlexItemStyle<BoxStyle> GetFlexboxChildStyle(Node child_node_id) => new(ref child_node_id.Style);
        public void SetUnroundedLayout(Node node_id, in Layout layout) => node_id.UnroundedLayout = layout;
        public LayoutOutput ComputeChildLayout(Node node_id, LayoutInput inputs) => node_id.Kind switch
        {
            NodeKind.Flexbox => BoxLayout
                .ComputeFlexBoxLayout<StatelessLayoutTree, Node, SpanIter<Node>, RefCoreStyle<BoxStyle>, RefFlexContainerStyle<BoxStyle>,
                    RefFlexItemStyle<BoxStyle>>(ref this, node_id, inputs),
            NodeKind.Image => BoxLayout.ComputeLeafLayout(inputs, new RefCoreStyle<BoxStyle>(ref node_id.Style), ref this, node_id, ImageMeasureFunction),
            _ => throw new ArgumentOutOfRangeException()
        };

        private static Size<float> ImageMeasureFunction(Node node_id, Size<float?> known_dimensions, Size<AvailableSpace> _available_space)
            => (known_dimensions.Width, known_dimensions.Height) switch
            {
                ({ } width, { } height) => new(width, height),
                ({ } width, null) => new(width, (width / node_id.LeafSize.Width) * node_id.LeafSize.Height),
                (null, { } height) => new((height / node_id.LeafSize.Height) * node_id.LeafSize.Width, height),
                (null, null) => node_id.LeafSize,
            };

        public void FormatDebugLabel(Node node_id, StringBuilder builder) => builder.Append($"{node_id.Kind}");

        public ref readonly Layout GetFinalLayout(Node node_id) => ref node_id.UnroundedLayout; // todo round
    }

    [Test]
    public void Test1()
    {
        var root = Node.NewColumn(BoxStyle.Default with { AlignItems = AlignItems.Center });

        var image_node1 = Node.NewImage(BoxStyle.Default, new(400, 300));
        root.Add(image_node1);

        var image_node2 = Node.NewImage(BoxStyle.Default, new(300, 600));
        root.Add(image_node2);

        root.ComputeLayout(new(AvailableSpace.MaxContent), true);
        Console.WriteLine(root.ToString());
    }
}
