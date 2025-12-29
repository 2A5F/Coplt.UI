using System.Diagnostics;
using Coplt.UI.Collections;
using Coplt.UI.Core.Styles;
using Coplt.UI.Native;
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
    public readonly struct TextParagraph(Document document)
    {
        public Document Document { get; } = document;
        public NodeId Id { get; } = document.CreateTextParagraph();

        public TextParagraph(View parent) : this(parent.Document)
        {
            parent.Add(this);
        }

        public ref TextParagraphData Data => ref Document.UnsafeAt<TextParagraphData>(Id);
        public ref TextStyleData StyleData => ref Document.UnsafeAt<TextStyleData>(Id);
        public ref CommonData CommonData => ref Document.UnsafeAt<CommonData>(Id);
        public ref ChildsData ChildsData => ref Document.UnsafeAt<ChildsData>(Id);
        public ref ManagedData ManagedData => ref Document.UnsafeAt<ManagedData>(Id);

        public void Add(View node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            node.Document.AddChild(Id, node.Id);
        }

        public void Remove(View node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            node.Document.RemoveChild(Id, node.Id);
        }
    }

    /// <inheritdoc cref="Access"/>
    public readonly struct View(Document document)
    {
        public Document Document { get; } = document;
        public NodeId Id { get; } = document.CreateView();

        public View(View parent) : this(parent.Document)
        {
            parent.Add(this);
        }

        public ref StyleData StyleData => ref Document.UnsafeAt<StyleData>(Id);
        public ref CommonData CommonData => ref Document.UnsafeAt<CommonData>(Id);
        public ref LayoutData LayoutData => ref Document.UnsafeAt<LayoutData>(Id);
        public LayoutView Layout => LayoutData.Layout;
        public ref ChildsData ChildsData => ref Document.UnsafeAt<ChildsData>(Id);
        public ref ManagedData ManagedData => ref Document.UnsafeAt<ManagedData>(Id);

        public void Add(View node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            node.Document.AddChild(Id, node.Id);
        }

        public void Remove(View node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            node.Document.RemoveChild(Id, node.Id);
        }
        public void Add(TextParagraph node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            node.Document.AddChild(Id, node.Id);
        }

        public void Remove(TextParagraph node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            node.Document.RemoveChild(Id, node.Id);
        }

        public TextParagraph Add(string text)
        {
            return new TextParagraph(this)
            {
                Text = text
            };
        }
    }

    extension(View node)
    {
        public Container Container
        {
            get => node.StyleData.Container;
            set => node.StyleData.Container = value;
        }

        public Visible Visible
        {
            get => node.StyleData.Visible;
            set => node.StyleData.Visible = value;
        }

        public FontFallback? FontFallback
        {
            get => node.ManagedData.FontFallback;
            set
            {
                node.StyleData.SetFontFallback(value);
                node.ManagedData.FontFallback = value;
            }
        }

        public TextWrap TextWrap
        {
            get => node.StyleData.TextWrap;
            set => node.StyleData.TextWrap = value;
        }

        public WrapFlags WrapFlags
        {
            get => node.StyleData.WrapFlags;
            set => node.StyleData.WrapFlags = value;
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

        public Length MinWidth
        {
            get
            {
                ref var style = ref node.StyleData;
                return new(style.MinWidth, style.MinWidthValue);
            }
            set
            {
                ref var style = ref node.StyleData;
                var type = value.Type;
                var val = value.Value;
                style.MinWidth = type;
                style.MinWidthValue = val;
            }
        }
        public Length MinHeight
        {
            get
            {
                ref var style = ref node.StyleData;
                return new(style.MinHeight, style.MinHeightValue);
            }
            set
            {
                ref var style = ref node.StyleData;
                var type = value.Type;
                var val = value.Value;
                style.MinHeight = type;
                style.MinHeightValue = val;
            }
        }

        public Length MaxWidth
        {
            get
            {
                ref var style = ref node.StyleData;
                return new(style.MaxWidth, style.MaxWidthValue);
            }
            set
            {
                ref var style = ref node.StyleData;
                var type = value.Type;
                var val = value.Value;
                style.MaxWidth = type;
                style.MaxWidthValue = val;
            }
        }
        public Length MaxHeight
        {
            get
            {
                ref var style = ref node.StyleData;
                return new(style.MaxHeight, style.MaxHeightValue);
            }
            set
            {
                ref var style = ref node.StyleData;
                var type = value.Type;
                var val = value.Value;
                style.MaxHeight = type;
                style.MaxHeightValue = val;
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

    extension(TextParagraph node)
    {
        public string Text
        {
            get => node.Data.Text;
            set => node.Data.SetText(node.Document, value);
        }
    }
}
