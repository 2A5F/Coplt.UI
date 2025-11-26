// var ff = FontFallback.Create("JetBrains Mono");
// Console.WriteLine(ff);

using System.Diagnostics;
using Coplt.UI.Native;
using Coplt.UI.Styles;
using Coplt.UI.Texts;
using Coplt.UI.Trees;

NativeLib.Instance.SetLogger((l, msg) => Console.WriteLine($"[{l}] {msg}"));

var ff = FontFallback.Create("Calibri");
Console.WriteLine(ff);

using var doc = new Document.Builder().Create();
var node = new Access.View(doc)
{
    Width = 100, Height = Length.Auto,
    Container = Container.Text,
    FontFallback = ff,
    WrapFlags = WrapFlags.AllowNewLine,
};
doc.AddRoot(node.Id);
// node.Add("123 阿斯顿 asd ياخشىمۇسىز 😊😅ひらがな");
// node.Add("有朋自远方来");
// var inline_box = new Access.View(doc) { TextMode = TextMode.Inline, Width = 10, Height = 10 };
// node.Add(inline_box);
// node.Add("不亦乐乎");
node.Add("Never Gonna Give You Up");
// node.Add("fia");
// node.Add("!=");
// node.Add("😀");
// node.Add("a c");
var start = Stopwatch.GetTimestamp();
doc.Update();
var end = Stopwatch.GetTimestamp();
var elapsed = Stopwatch.GetElapsedTime(start, end);
Console.WriteLine($"{elapsed}; {elapsed.TotalMilliseconds}ms; {elapsed.TotalMicroseconds}us");
Console.WriteLine(node.Layout.ToString());
