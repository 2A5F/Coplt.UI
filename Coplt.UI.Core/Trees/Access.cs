using System.Diagnostics;
using Coplt.UI.Collections;
using Coplt.UI.Core.Styles;
using Coplt.UI.Styles;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees;

/// <summary>
/// A tool class for accessing nodes.<br/>
/// <b>Accessing through this tool class is inefficient and should only be used for testing and debugging purposes.</b>
/// </summary>
public static unsafe partial class Access
{
    /// <inheritdoc cref="Access"/>
    public readonly struct Node(Document document, NodeType type)
    {
        public Document Document { get; } = document;
        public NodeLocate Locate { get; } = document.CreateNode(type);

        public ref CommonStyleData CommonStyle => ref Document.At<CommonStyleData>(Locate);
        public ref ContainerStyleData ContainerStyle => ref Document.At<ContainerStyleData>(Locate);
        public ref ContainerLayoutData ContainerLayout => ref Document.At<ContainerLayoutData>(Locate);
        public ref ChildsData Childs => ref Document.At<ChildsData>(Locate);
        public ref TextData TextData => ref Document.At<TextData>(Locate);

        public void Add(Node node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            if (node.Locate.Id.Type is NodeType.Root) throw new ArgumentException("The root node cannot be a child");
            ref var parent = ref Document.At<ParentData>(node.Locate);
            if (parent.m_has_parent) throw new ArgumentException("Target node already has a parent");
            Childs.m_childs.Add(node.Locate);
            parent = new ParentData
            {
                m_parent = Locate,
                m_has_parent = true,
            };
        }

        public void Remove(Node node)
        {
            if (node.Document != Document) throw new InvalidOperationException();
            ref var parent = ref Document.At<ParentData>(node.Locate);
            if (!parent.m_has_parent || parent.m_parent != Locate) throw new ArgumentException("Target node is not a child of this node");
            var r = Childs.m_childs.Remove(node.Locate);
            Debug.Assert(r);
            parent.m_has_parent = false;
        }
    }

    extension(Node node)
    {
        public Container Container
        {
            get => node.ContainerStyle.Container;
            set => node.ContainerStyle.Container = value;
        }

        public Visible Visible
        {
            get => node.CommonStyle.Visible;
            set => node.CommonStyle.Visible = value;
        }

        public Length Width
        {
            get
            {
                ref var style = ref node.ContainerStyle;
                return new(style.Width, style.WidthValue);
            }
            set
            {
                ref var style = ref node.ContainerStyle;
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
                ref var style = ref node.ContainerStyle;
                return new(style.Height, style.HeightValue);
            }
            set
            {
                ref var style = ref node.ContainerStyle;
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
                ref var style = ref node.CommonStyle;
                style.GridColumnStart = value;
                style.GridColumnEnd = value;
            }
        }
        public GridPlacement GridRow
        {
            set
            {
                ref var style = ref node.CommonStyle;
                style.GridRowStart = value;
                style.GridRowEnd = value;
            }
        }

        public string Text
        {
            get
            {
                ref var data = ref node.TextData;
                return data.m_text.ToString();
            }
            set
            {
                ref var data = ref node.TextData;
                data.SetText(value);
            }
        }
    }
}
