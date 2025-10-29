using Coplt.UI.Collections;
using Coplt.UI.Native;
using Coplt.UI.Texts;

namespace TestCore;

public unsafe class TestText
{
    [Test]
    public void TestSplit()
    {
        using var list = new NativeList<TextRange>();
        var str = "123 阿斯顿 asd ياخشىمۇسىز 😊😅"; 
        fixed (char* p_str = str)
        {
            NativeLib.Instance.Lib.Handle->SplitTexts(&list, p_str, str.Length);
        }
        foreach (var range in list)
        {
            Console.WriteLine($"{range} ; {str.Substring(range.Start, range.Length)}");
        }
    }
}
