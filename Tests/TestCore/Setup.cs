using Coplt.UI.Native;

namespace TestCore;

[SetUpFixture]
public class _
{
    [OneTimeSetUp]
    public void Setup()
    {
        NativeLib.Instance.SetLogger((l, msg) => Console.WriteLine($"[{l}] {msg}"));
    }
}
