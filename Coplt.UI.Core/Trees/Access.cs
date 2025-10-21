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
        public ref CommonLayoutData CommonLayout => ref Document.At<CommonLayoutData>(Locate);
    }

    extension(Node node)
    {
        public Container Container
        {
            get => node.CommonStyle.Container;
            set => node.CommonStyle.Container = value;
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
                ref var style = ref node.CommonStyle;
                return new(style.Width, style.WidthValue);
            }
            set
            {
                ref var style = ref node.CommonStyle;
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
                ref var style = ref node.CommonStyle;
                return new(style.Height, style.HeightValue);
            }
            set
            {
                ref var style = ref node.CommonStyle;
                var type = value.Type;
                var val = value.Value;
                style.Height = type;
                style.HeightValue = val;
            }
        }
    }
}
