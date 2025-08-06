using System.Text;
using Coplt.UI.BoxLayouts;

namespace Coplt.UI.BoxLayout.Utilities;

public interface IPrintTree<TNodeId, out TChildIter> : ITraverseTree<TNodeId, TChildIter>
    where TNodeId : allows ref struct
    where TChildIter : IIterator<TNodeId>, allows ref struct
{
    public void FormatDebugLabel(TNodeId node_id, StringBuilder builder);

    public ref readonly Layout GetFinalLayout(TNodeId node_id);
}

public static class PrintTree
{
    public static string Print<TTree, TNodeId, TChildIter>(ref TTree tree, TNodeId root)
        where TTree : IPrintTree<TNodeId, TChildIter>
        where TNodeId : allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
    {
        var sb = new StringBuilder();
        Print<TTree, TNodeId, TChildIter>(sb, ref tree, root);
        return sb.ToString();
    }

    public static void Print<TTree, TNodeId, TChildIter>(StringBuilder sb, ref TTree tree, TNodeId root)
        where TTree : IPrintTree<TNodeId, TChildIter>
        where TNodeId : allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
    {
        sb.AppendLine($"Tree");
        PrintNode<TTree, TNodeId, TChildIter>(sb, ref tree, root, false, "");
    }

    private static void PrintNode<TTree, TNodeId, TChildIter>(
        StringBuilder sb, ref TTree tree, TNodeId node_id, bool has_sibling, string lines
    )
        where TTree : IPrintTree<TNodeId, TChildIter>
        where TNodeId : allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
    {
        ref readonly var layout = ref tree.GetFinalLayout(node_id);
        var num_children = tree.ChildCount(node_id);

        var fork = has_sibling ? "├── " : "└── ";
        sb.Append($"{lines}{fork} ");
        tree.FormatDebugLabel(node_id, sb);
        sb.Append($" {{ x: {layout.Location.X:0.####}, y: {layout.Location.Y:0.####}, w: {layout.Size.Width:0.####}, h: {layout.Size.Height:0.####}");
        sb.Append($", content: (w: {layout.ContentSize.Width:0.####}, h: {layout.ContentSize.Height:0.####})");
        sb.Append($", border: (l: {layout.Border.Left}, r: {layout.Border.Right}, t: {layout.Border.Top}, b: {layout.Border.Bottom})");
        sb.Append($", padding: (l: {layout.Padding.Left}, r: {layout.Padding.Right}, t: {layout.Padding.Top}, b: {layout.Padding.Bottom}) }}");
        sb.AppendLine();
        
        var bar = has_sibling ? "│   " : "    ";
        var new_string = lines + bar;

        var i = 0;
        foreach (var child in tree.ChildIds(node_id).AsEnumerable<TChildIter, TNodeId>())
        {
            var index = i++;
            var child_has_sibling = index < num_children - 1;
            PrintNode<TTree, TNodeId, TChildIter>(sb, ref tree, child, child_has_sibling, new_string);
        }
    }
}
