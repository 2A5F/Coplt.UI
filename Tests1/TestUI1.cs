using Coplt.UI;
using Coplt.UI.Events;
using Coplt.UI.Widgets;

namespace Tests1;

public class TestUI1
{
    [Widget]
    public partial class Widget1 : AWidget<Widget1>, IWidget<Widget1>
    {
        public int Count { get; set; }

        public static void ElementTemplate()
        {
            using (View("Root"))
            {
                View("Button").On<ClickEvent>((s, _) => s.Count++);
                View("Text").Text("Value: ").Text(s => s.Count);

                using (If(s => s.Count > 3))
                {
                    View().Text("> 3");
                }
                using (ElseIf(s => s.Count > 1))
                {
                    View().Text("> 1");
                }
                using (Else())
                {
                    View().Text("<= 1");
                }

                using (For(out var loop, s => Enumerable.Range(0, s.Count)))
                {
                    View().Text(_ => loop.Current);
                }
                using (Empty())
                {
                    View().Text("none");
                }
            }
        }

        public static void StyleTemplate() { }
    }

    [Test]
    public void Test1()
    {
        var panel = new UIPanel();
        panel.SetSize(1024, 1024);
        panel.SetRoot<Widget1>();
        panel.Update();
    }
}
