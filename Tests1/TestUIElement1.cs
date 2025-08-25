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
        var doc = new UIDocument<object, object>();
        var root = new UIElement<object, object> { Name = "Root" };
        Unsafe.AsRef(in root.CommonStyle).FlexDirection = FlexDirection.Row;
        Unsafe.AsRef(in root.CommonStyle).FlexWrap = FlexWrap.Wrap;

        for (var i = 0; i < 3; i++)
        {
            var child = new UIElement<object, object> { Name = $"Child{i}" };
            root.Add(child);
            Unsafe.AsRef(in child.CommonStyle).Size
                = new(5.Fx(), 1.Fx());
        }

        doc.SetRoot(root);
        doc.ComputeLayout(new(10, AvailableSpace.MinContent));
        Console.WriteLine(doc);
    }
}
