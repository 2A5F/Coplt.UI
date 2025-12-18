// var ff = FontFallback.Create("JetBrains Mono");
// Console.WriteLine(ff);

using System.Diagnostics;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Native;
using Coplt.UI.Styles;
using Coplt.UI.Texts;
using Coplt.UI.Trees;
using Coplt.UI.Trees.Datas;

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



var ff = FontFallback.Create("Calibri");
// var ff = FontFallback.Create("Microsoft YaHei UI");
Console.WriteLine(ff);

using var doc = new Document.Builder().Create();
var node = new Access.View(doc)
{
    MaxWidth = 100, Height = Length.Auto,
    Container = Container.Text,
    FontFallback = ff,
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
var paragraph = node.Add("不亦乐乎");
var start = Stopwatch.GetTimestamp();
doc.Update();
var end = Stopwatch.GetTimestamp();
var elapsed = Stopwatch.GetElapsedTime(start, end);
Console.WriteLine($"{elapsed}; {elapsed.TotalMilliseconds}ms; {elapsed.TotalMicroseconds}us");
Console.WriteLine(node.Layout.ToString());
var data = doc.At<TextParagraphData>(paragraph.Id);
Console.WriteLine($"{data.Text}");
