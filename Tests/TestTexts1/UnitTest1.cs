using Coplt.UI.TextLayout;

namespace TestTexts1;

public class Tests
{
    [Test]
    public void Test1()
    {
        var a = TextLayout.Instance;
        var b = a.SystemFontCollection;
        foreach (var family in b.Families) 
        {
            Console.WriteLine(family);
        }
    }
}
