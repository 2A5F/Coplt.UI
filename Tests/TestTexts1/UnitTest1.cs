using System.Diagnostics;
using Coplt.UI.Texts;

namespace TestTexts1;

public class Tests
{
    [Test]
    public void Test1()
    {
        var start = Stopwatch.GetTimestamp();
        var a = TextLayout.Instance;
        var b = a.SystemFontCollection;
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
        var start = Stopwatch.GetTimestamp();
        var a = TextLayout.Instance;
        var b = a.SystemFontCollection;
        var end = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(start, end);
        var d = b.DefaultFamily;
        var fonts = d.GetFonts();
        foreach (var font in fonts) font.GetFace();
        Console.WriteLine($"{elapsed}");
        foreach (var font in fonts)
        {
            Console.WriteLine(font.GetFace());
        }
    }
}
