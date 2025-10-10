using Coplt.UI.Collections;

namespace Tests1;

public class TestOrderedSet
{
    [Test]
    public void Test1()
    {
        var set = new OrderedSet<int>();
        set.Add(123);
        set.Add(456);
        set.Add(789);
        set.Add(111);
        set.Remove(456);
        Assert.That(set, Is.EqualTo(new[] { 123, 789, 111 }).AsCollection);
    }

    [Test]
    public void Test2()
    {
        var set = new OrderedSet<int>();
        set.Add(123);
        set.Add(456);
        set.Add(789);
        set.SetNext(789, 123);
        Assert.That(set, Is.EqualTo(new[] { 456, 789, 123 }).AsCollection);
    }

    [Test]
    public void Test3()
    {
        var set = new OrderedSet<int>();
        set.Add(123);
        set.Add(456);
        set.Add(789);
        set.SetPrev(789, 123);
        Assert.That(set, Is.EqualTo(new[] { 456, 123, 789 }).AsCollection);
    }
}
