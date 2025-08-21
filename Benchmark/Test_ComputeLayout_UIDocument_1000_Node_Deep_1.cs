using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
using Coplt.UI.Elements;
using Coplt.UI.Styles;

namespace Benchmark;

[DisassemblyDiagnoser(maxDepth: 1000, syntax: DisassemblySyntax.Intel)]
public class Test_ComputeLayout_UIDocument_1000_Node_Deep_1
{
    private UIDocument document;

    [GlobalSetup]
    public void SetUp()
    {
        var doc = new UIDocument();
        var root = new UIElement { Name = "Root" };
        Unsafe.AsRef(in root.ComputedStyle).FlexDirection = FlexDirection.Row;
        Unsafe.AsRef(in root.ComputedStyle).FlexWrap = FlexWrap.Wrap;
        for (int i = 0; i < 1000; i++)
        {
            var child = new UIElement { Name = $"Child{i}" };
            Unsafe.AsRef(in child.ComputedStyle).Size
                = new(Random.Shared.Next(10, 100).Fx(), 100.Fx());
            root.Add(child);
        }
        doc.SetRoot(root);
        document = doc;
    }

    [Benchmark]
    public void ComputeLayout()
    {
        document.ComputeLayout(new(1920, 1080));
    }
}
