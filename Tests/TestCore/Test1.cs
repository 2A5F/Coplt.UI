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
        ref var node1_style = ref doc.At<CommonStyleData>(node1);
        node1_style.Width = LengthType.Fixed;
        node1_style.Height = LengthType.Fixed;
        node1_style.WidthValue = 100;
        node1_style.HeightValue = 100;
        doc.Update();
        ref var node1_layout = ref doc.At<CommonLayoutData>(node1);
        Console.WriteLine(node1_layout.FinalLayout);
    }
}
