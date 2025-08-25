// using Coplt.UI.Elements;
// using Coplt.UI.Events;
// using Coplt.UI.Widgets;
//
// namespace Coplt.UI;
//
// public class UIPanel
// {
//     public UIDocument Document { get; } = new();
//
//     public uint Width { get; private set; }
//     public uint Height { get; private set; }
//
//     public void SetSize(uint Width, uint Height)
//     {
//         this.Width = Width;
//         this.Height = Height;
//     }
//
//     public void SetRoot<W>() where W : AWidget<W>, IWidget<W>, new()
//     {
//         var tt = TemplateTree.Get<W>();
//         // todo
//     }
//
//     public void Provide<D>(D data)
//     {
//         // todo
//     }
//
//     /// <summary>
//     /// Needs to be called every frame, not just when data is updated
//     /// </summary>
//     public void Update()
//     {
//         // todo
//     }
//
//     public void DispatchEvent<E>(UIElement Target, E Event, EventConfig config)
//     {
//         // todo
//     }
// }
