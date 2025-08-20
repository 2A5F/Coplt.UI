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
            = new (50.Fx(), 100.Fx());
        doc.SetRoot(root);
        doc.ComputeLayout(new(1920, 1080));
        Console.WriteLine(doc);
    }
    
    // [Test, Repeat(100)]
    // public void Test2()
    // {
    //     var doc = new UIDocument();
    //     var root = new UIElement { Name = "Root" };
    //     Unsafe.AsRef(in root.ComputedStyle).FlexDirection = FlexDirection.Row;
    //     Unsafe.AsRef(in root.ComputedStyle).FlexWrap = FlexWrap.Wrap;
    //     for (int i = 0; i < 1000; i++)
    //     {
    //         var child = new UIElement { Name = $"Child{i}" };
    //         Unsafe.AsRef(in child.ComputedStyle).Size 
    //             = new (Random.Shared.Next(10, 100).Fx(), 100.Fx());
    //         root.Add(child);
    //     }
    //     doc.SetRoot(root);
    //     var start = Stopwatch.GetTimestamp();
    //     doc.ComputeLayout(new(1920, 1080));
    //     var end = Stopwatch.GetTimestamp();
    //     var time = Stopwatch.GetElapsedTime(start, end);
    //     Console.WriteLine($"{time}; {time.TotalMilliseconds}; {time.TotalMicroseconds}");
    //     Console.WriteLine(doc);
    // }
}
