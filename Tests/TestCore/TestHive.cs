using Coplt.UI.Collections;

namespace TestCore;

public class TestHive
{
    [Test]
    public void Test_Locate_Index_1()
    {
        Assert.Multiple(() =>
        {
            for (var i = 0; i < 1024; i++)
            {
                var locate = Hive.Locate(i);
                var index = Hive.Index(locate.chunk, locate.index);
                Console.WriteLine($"{i} => {locate} => {index}");
                Assert.That(index, Is.EqualTo(i));
            }
        });
    }
    [Test]
    public void Test_Locate_Index_2()
    {
        Assert.Multiple(() =>
        {
            var index = 0;
            for (var c = 0; c < 8; c++)
            {
                var size = Hive.ChunkSize(c);
                for (int i = 0; i < size; i++)
                {
                    var nth = index++;
                    var a = Hive.Locate(nth);
                    var b = Hive.Index(c, i);
                    Console.WriteLine($"{nth} => {a} => {b}");
                    Assert.That(b, Is.EqualTo(nth));
                    Assert.That(a, Is.EqualTo((c, i)));
                }
            }
        });
    }

    [Test]
    public void Test1()
    {
       using var hive = new NativeHive<int>();
       hive.Add(123);
       hive.Add(456);
       // hive.Remove(0, 0);
    }
}
