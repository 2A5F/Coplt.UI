using Coplt.UI.Elements;

namespace Tests1;

public class TestUIElement1
{
    [Test]
    public void Test1()
    {
        var root = new UIElement { Name = "Root" };
        var child = new UIElement { Name = "Child" };
        root.Add(child);
    }
}
