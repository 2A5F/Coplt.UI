using Coplt.UI.Collections;
using NUnit.Framework.Legacy;

namespace TestCore;

public class TestSplitMap
{
    [Test]
    public void Test1()
    {
        using var a = new NSplitMap<int, int>();
        a.TryAdd(1, 123);
        a.TryAdd(2, 456);
        a.TryAdd(3, 789);
        a.Remove(2);
        a.TryAdd(111, 222);
        Console.WriteLine(string.Join(", ", a));
        Assert.That(a, Is.EqualTo(new KeyValuePair<int, int>[]
        {
            new(1, 123), new(111, 222), new(3, 789),
        }));
    }
}
