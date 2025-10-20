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
        var node1 = doc.CreateNode(NodeType.Root);
        ref var node1_style = ref doc.At<ViewStyleData>(node1);
        node1_style.Width = LengthType.Percent;
        node1_style.Height = LengthType.Percent;
        node1_style.WidthValue = 1;
        node1_style.HeightValue = 1;
        doc.Update();
    }
}
