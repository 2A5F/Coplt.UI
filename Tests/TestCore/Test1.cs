using Coplt.UI.Collections;
using Coplt.UI.Core.Styles;
using Coplt.UI.Styles;
using Coplt.UI.Trees;
using Coplt.UI.Trees.Datas;

namespace TestCore;

public class Tests1
{
    [Test]
    public void Test1()
    {
        using var doc = new Document.Builder()
            .Create();
        var node = new Access.Node(doc, NodeType.Root)
        {
            Width = 456, Height = 123,
            Container = Container.Grid,
        };
        node.ContainerStyle.Grid = new(new()
        {
            GridTemplateRows = [1.fr, 2.fr, 20.px],
            GridTemplateColumns = [25.pc, 1.fr, 3.fr],
        });
        var child = new Access.Node(doc, NodeType.View)
        {
            GridRow = 2, GridColumn = 2,
        };
        node.Add(child);
        doc.Update();
        Console.WriteLine(node.ContainerLayout.FinalLayout);
        Console.WriteLine();
        Console.WriteLine(child.ContainerLayout.FinalLayout);
    }

    [Test]
    public void Test2()
    {
        using var doc = new Document.Builder()
            .Create();
        var node = new Access.Node(doc, NodeType.Root)
        {
            Width = 100, Height = 100,
            Container = Container.Text,
        };
        var child = new Access.Node(doc, NodeType.Text)
        {
            Text = "123",
        };
        node.Add(child);
        doc.Update();
        Console.WriteLine(node.ContainerLayout.FinalLayout);
    }
}
