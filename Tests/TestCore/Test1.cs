using System.Diagnostics;
using Coplt.UI.Collections;
using Coplt.UI.Core.Styles;
using Coplt.UI.Native;
using Coplt.UI.Styles;
using Coplt.UI.Texts;
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
        var node = new Access.View(doc)
        {
            Width = 456, Height = 123,
            Container = Container.Grid,
        };
        doc.AddRoot(node.Id);
        node.StyleData.Grid = new(new()
        {
            GridTemplateRows = [1.fr, 2.fr, 20.px],
            GridTemplateColumns = [25.pc, 1.fr, 3.fr],
        });
        var child = new Access.View(doc)
        {
            GridRow = 2, GridColumn = 2,
        };
        node.Add(child);
        var start = Stopwatch.GetTimestamp();
        doc.Update();
        var end = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(start, end);
        Console.WriteLine($"{elapsed}; {elapsed.TotalMilliseconds}ms");
        Console.WriteLine(node.Layout.ToString());
        Console.WriteLine(child.Layout.ToString());
    }

    [Test]
    public void Test2()
    {
        
        using var doc = new Document.Builder()
            .Create();
        var node = new Access.View(doc)
        {
            Width = 100, Height = 100,
            Container = Container.Text,
        };
        doc.AddRoot(node.Id);
        node.Add("123 阿斯顿 asd ياخشىمۇسىز 😊😅ひらがな");
        var start = Stopwatch.GetTimestamp();
        doc.Update();
        var end = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(start, end);
        Console.WriteLine($"{elapsed}; {elapsed.TotalMilliseconds}ms");
        Console.WriteLine(node.Layout.ToString());
    }
}
