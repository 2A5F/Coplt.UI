using System.Diagnostics;
using Coplt.UI.Texts;

namespace TestCore;

public class TestFont
{
    [Test]
    public void Test1()
    {
        var start = Stopwatch.GetTimestamp();
        var b = FontCollection.SystemCollection;
        var end = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(start, end);
        Console.WriteLine($"{elapsed}");
        Console.WriteLine(b.DefaultFamily);
        Console.WriteLine();
        foreach (var family in b.Families) 
        {
            Console.WriteLine(family);
        }
    }
    
    [Test]
    public void Test2()
    {
        var fm = new FontManager();
        var start = Stopwatch.GetTimestamp();
        var b = FontCollection.SystemCollection;
        var end = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(start, end);
        var d = b.DefaultFamily;
        var fonts = d.GetFonts();
        Console.WriteLine($"{elapsed}");
        foreach (var font in fonts)
        {
            Console.WriteLine(font.CreateFace(fm));
        }
    }
}
