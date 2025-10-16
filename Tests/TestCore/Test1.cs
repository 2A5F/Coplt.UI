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
        doc.CreateNode(NodeType.View);
        foreach (ref var a in doc.Query<ViewStyleData>())
        {
            Console.WriteLine(a);
        }
    }
}
