using System.Diagnostics;
using Coplt.UI.Collections;
using Coplt.UI.Core.Styles;
using Coplt.UI.Styles;
using Coplt.UI.Texts;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees;

/// <summary>
/// A tool class for accessing nodes.<br/>
/// <b>Accessing through this tool class is inefficient and should only be used for testing and debugging purposes.</b>
/// </summary>
public static unsafe partial class Access
{
    /// <inheritdoc cref="Access"/>
    public readonly struct View(Document document)
    {
        public Document Document { get; } = document;
        public NodeId Id { get; } = document.CreateView();

        public ref StyleData StyleData => ref Document.At<StyleData>(Id);
        public ref CommonData CommonData => ref Document.At<CommonData>(Id);
        public LayoutView Layout => CommonData.Layout;
        public ref ChildsData ChildsData => ref Document.At<ChildsData>(Id);

        public void Add(string text)
        {
            ChildsData.UnsafeAddText(text);
            Document.DirtyTextLayout(Id);
        }

        public void Add(View node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            ref var parent = ref Document.At<ParentData>(node.Id);
            if (parent.Parent.HasValue) throw new ArgumentException("Target node already has a parent");
            ChildsData.UnsafeAdd(node.Id);
            parent.UnsafeSetParent(Id);
            Document.DirtyLayout(Id);
        }

        public void Remove(View node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            ref var parent = ref Document.At<ParentData>(node.Id);
            if (parent.Parent != Id) throw new ArgumentException("Target node is not a child of this node");
            var r = ChildsData.UnsafeRemove(node.Id);
            Debug.Assert(r);
            parent.UnsafeRemoveParent();
            Document.DirtyLayout(Id);
        }
    }

    extension(View node)
    {
        public Container Container
        {
            get => node.StyleData.Container;
            set => node.StyleData.Container = value;
        }

        public TextMode TextMode
        {
            get => node.StyleData.TextMode;
            set => node.StyleData.TextMode = value;
        }

        public Visible Visible
        {
            get => node.StyleData.Visible;
            set => node.StyleData.Visible = value;
        }

        public FontFallback FontFallback
        {
            set => node.StyleData.SetFontFallback(value);
        }

        public Length Width
        {
            get
            {
                ref var style = ref node.StyleData;
                return new(style.Width, style.WidthValue);
            }
            set
            {
                ref var style = ref node.StyleData;
                var type = value.Type;
                var val = value.Value;
                style.Width = type;
                style.WidthValue = val;
            }
        }
        public Length Height
        {
            get
            {
                ref var style = ref node.StyleData;
                return new(style.Height, style.HeightValue);
            }
            set
            {
                ref var style = ref node.StyleData;
                var type = value.Type;
                var val = value.Value;
                style.Height = type;
                style.HeightValue = val;
            }
        }

        public GridPlacement GridColumn
        {
            set
            {
                ref var style = ref node.StyleData;
                style.GridColumnStart = value;
                style.GridColumnEnd = value;
            }
        }
        public GridPlacement GridRow
        {
            set
            {
                ref var style = ref node.StyleData;
                style.GridRowStart = value;
                style.GridRowEnd = value;
            }
        }
    }
}
