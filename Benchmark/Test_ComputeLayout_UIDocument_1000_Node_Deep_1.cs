using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
using Coplt.UI.Elements;
using Coplt.UI.Styles;

namespace Benchmark;

[DisassemblyDiagnoser(maxDepth: 1000, syntax: DisassemblySyntax.Intel)]
[WarmupCount(100)]
[IterationCount(100)]
public class Test_ComputeLayout_UIDocument_1000_Node_Deep_1
{
    private UIDocument[] document = new UIDocument[100];
    private int i;

    [GlobalSetup]
    public void SetUp()
    {
        for (int j = 0; j < 100; j++)
        {
            var doc = new UIDocument();
            var root = new UIElement { Name = "Root" };
            Unsafe.AsRef(in root.RawStyle).FlexDirection = FlexDirection.Row;
            Unsafe.AsRef(in root.RawStyle).FlexWrap = FlexWrap.Wrap;
            for (int i = 0; i < 1000; i++)
            {
                var child = new UIElement { Name = $"Child{i}" };
                Unsafe.AsRef(in child.RawStyle).Size
                    = new(Random.Shared.Next(10, 100).Fx(), 100.Fx());
                root.Add(child);
            }
            doc.SetRoot(root);
            document[j] = doc;
        }
    }

    [GlobalCleanup]
    public void Clean()
    {
        Console.WriteLine($"Version: {document[0].Root!.Version}");
        Console.WriteLine(document[0]);
    }

    [IterationSetup]
    public void IterSetup()
    {
        i++;
        if (i >= 100)
        {
            i = 0;
            for (int j = 0; j < 100; j++)
            {
                foreach (var child in document[j].Root!)
                {
                    child.MarkLayoutDirty();
                }
            }
        }
    }

    [Benchmark]
    public void ComputeLayout()
    {
        document[i].ComputeLayout(new(1920, 1080));
    }
}
