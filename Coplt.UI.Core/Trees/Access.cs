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
    public readonly struct TextSpan
    {
        public Document Document { get; }
        public NodeId Id { get; }

        public TextSpan(View view, string text)
        {
            Document = view.Document;
            Id = Document.CreateTextSpan();

            ref var childs = ref view.ChildsData;
            var index = childs.m_texts.Count;
            childs.m_texts.Add(NString.Create(text));

            ref var span_data = ref TextSpanData;
            span_data.TextIndex = (uint)index;
            span_data.TextStart = 0;
            span_data.TextLength = (uint)text.Length;
        }

        public ref CommonData CommonData => ref Document.UnsafeAt<CommonData>(Id);
        public LayoutView Layout => CommonData.Layout;
        public ref ManagedData ManagedData => ref Document.UnsafeAt<ManagedData>(Id);
        public ref TextSpanData TextSpanData => ref Document.UnsafeAt<TextSpanData>(Id);
        public ref TextSpanStyleData TextSpanStyleData => ref Document.UnsafeAt<TextSpanStyleData>(Id);
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
        public LayoutView Layout => CommonData.Layout;
        public ref ChildsData ChildsData => ref Document.UnsafeAt<ChildsData>(Id);
        public ref ManagedData ManagedData => ref Document.UnsafeAt<ManagedData>(Id);

        public TextSpan Add(string text) => new(this, text);

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
}
