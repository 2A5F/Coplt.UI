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
            Width = 100,
            Height = 100
        };
        doc.Update();
        Console.WriteLine(node.CommonLayout.FinalLayout);
    }
}
