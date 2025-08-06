using Coplt.SoftGraphics;

namespace Tests1;

public class ZOrder
{
    [Test]
    public static void Test1()
    {
        var arr = new uint[256];
        for (var i = 0u; i < 256; i++)
        {
            arr[i] = Encode(i);
        }
        Console.WriteLine(string.Join(", ", arr));
    }

    private static uint Encode(uint n)
    {
        n &= 0x0000FFFF;
        n = (n | (n << 8)) & 0x00FF00FF;
        n = (n | (n << 4)) & 0x0F0F0F0F;
        n = (n | (n << 2)) & 0x33333333;
        n = (n | (n << 1)) & 0x55555555;
        return n;
    }

    [Test]
    public static void Test2()
    {
        var r = Utils.EncodeZOrderGather(new(31, 101));
        Console.WriteLine(r);
    }
}
