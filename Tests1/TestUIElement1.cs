using Coplt.UI.Elements;
using Coplt.UI.Styles;

namespace Tests1;

public class TestUIElement1
{
    [Test]
    public void Test1()
    {
        var doc = new UIDocument();
        var root = new UIElement { Name = "Root" };
        var child = new UIElement { Name = "Child" };
        root.Add(child);
        doc.SetRoot(root);
        doc.ComputeLayout(new(1920, 1080), false);
        Console.WriteLine(doc);
    }
}
