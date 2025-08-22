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
        Unsafe.AsRef(in root.RawStyle).FlexDirection = FlexDirection.Row;
        Unsafe.AsRef(in root.RawStyle).FlexWrap = FlexWrap.Wrap;

        for (var i = 0; i < 3; i++)
        {
            var child = new UIElement { Name = $"Child{i}" };
            root.Add(child);
            Unsafe.AsRef(in child.RawStyle).Size
                = new(5.Fx(), 1.Fx());
        }

        doc.SetRoot(root);
        doc.ComputeLayout(new(10, AvailableSpace.MinContent));
        Console.WriteLine(doc);
    }
}
