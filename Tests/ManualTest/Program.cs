// var ff = FontFallback.Create("JetBrains Mono");
// Console.WriteLine(ff);

using System.Diagnostics;
using Coplt.Mathematics;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Native;
using Coplt.UI.Styles;
using Coplt.UI.Texts;
using Coplt.UI.Trees;
using Coplt.UI.Trees.Datas;
using Coplt.UI.Utilities;

NativeLib.Instance.SetLogger((l, msg) => Console.WriteLine($"[{l}] {msg}"));

// var fm = new FontManager(new FrameSource());
// var start = Stopwatch.GetTimestamp();
// var b = FontCollection.SystemCollection;
// var end = Stopwatch.GetTimestamp();
// var elapsed = Stopwatch.GetElapsedTime(start, end);
// var d = b.DefaultFamily;
// var fonts = d.GetFonts();
// Console.WriteLine($"{elapsed}; {elapsed.TotalMilliseconds}ms; {elapsed.TotalMicroseconds}μs");
// foreach (var font in fonts)
// {
//     start = Stopwatch.GetTimestamp();
//     var face = font.CreateFace(fm);
//     end = Stopwatch.GetTimestamp();
//     elapsed = Stopwatch.GetElapsedTime(start, end);
//     Console.WriteLine($"{elapsed}; {elapsed.TotalMilliseconds}ms; {elapsed.TotalMicroseconds}μs; {face}");
// }

var locale = Utils.GetUserUiDefaultLocale();
Console.WriteLine($"{locale}");

// var ff = FontFallback.Create("Calibri");
// // var ff = FontFallback.Create("Microsoft YaHei UI");
// Console.WriteLine(ff);

using var doc = new Document.Builder().Create();
var node = new Access.View(doc)
{
    MaxWidth = 100, Height = Length.Auto,
    Container = Container.Text,
    // FontFallback = ff,
    WrapFlags = WrapFlags.AllowNewLine,
};
doc.AddRoot(node.Id);
// node.Add("123 阿斯顿 asd ياخشىمۇسىز 😊😅ひらがな");
// node.Add("有朋自远方来");
// var inline_box = new Access.View(doc) { Width = 30, Height = 30 };
// node.Add(inline_box);
// node.Add("不亦乐乎");
// node.Add("Never Gonna Give You Up");
// node.Add("fia");
// node.Add("!=");
// node.Add("😀");
// node.Add("a c");
// node.Add("123 阿斯顿 asd");
var paragraph = node.Add("Never Gonna Give 有朋自远方来不亦乐乎");
var start = Stopwatch.GetTimestamp();
doc.Update();
var end = Stopwatch.GetTimestamp();
var elapsed = Stopwatch.GetElapsedTime(start, end);
Console.WriteLine($"{elapsed}; {elapsed.TotalMilliseconds}ms; {elapsed.TotalMicroseconds}us");
Console.WriteLine(node.Layout.ToString());
ref readonly var layout = ref doc.At<LayoutData>(node.Id).TextViewData;
ref readonly var data = ref doc.At<TextParagraphData>(paragraph.Id);
var text = data.Text;
foreach (ref readonly var line in layout.Lines)
{
    Console.WriteLine($"  <line({line.NthLine}) pos=({line.X}, {line.Y}) size=({line.Width}, {line.Height}) baseline=({line.BaseLine}) />");
    foreach (var line_span in layout.LineSpans[(int)line.SpanStart..(int)line.SpanEnd])
    {
        Console.WriteLine($"    <span pos=({line_span.X}, {line_span.Y}) size=({line_span.Width}, {line_span.Height}) baseline=({line_span.BaseLine}) text=\"{text[(int)line_span.Start..(int)line_span.End]}\" />");
    }
}
Console.WriteLine($"{text}");
