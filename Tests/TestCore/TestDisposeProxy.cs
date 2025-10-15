using System.Runtime.CompilerServices;
using Coplt.UI.Utilities;

namespace TestCore;

public struct C : IDisposable
{
    public int a;

    public void Dispose()
    {
        a = 456;
        Console.WriteLine("123");
    }
}

public class D : IDisposable
{
    public int a;

    public void Dispose()
    {
        a = 456;
        Console.WriteLine("123");
    }
}

public class Tests2
{
    [Test]
    public void Test1()
    {
        var c = new C();
        DisposeProxy.TryDispose(ref c);
        Console.WriteLine(c.a);
        Assert.That(c.a, Is.EqualTo(456));
    }

    [Test]
    public void Test2()
    {
        var c = new D();
        DisposeProxy.TryDispose(ref c);
        Console.WriteLine(c.a);
        Assert.That(c.a, Is.EqualTo(456));
    }

    public void BadBox<T>(ref T a)
    {
       if (a is IDisposable b) b.Dispose();
    }

    [Test]
    public void Test3()
    {
        var c = new C();
        BadBox(ref c);
        Console.WriteLine(c.a);
        Assert.That(c.a, Is.EqualTo(0));
    }
}
