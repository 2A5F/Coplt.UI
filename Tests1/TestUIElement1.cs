using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        Unsafe.AsRef(in child.ComputedStyle).Size
            = new(50.Fx(), 100.Fx());
        doc.SetRoot(root);
        doc.ComputeLayout(new(1920, 1080));
        Console.WriteLine(doc);
    }
}
