using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native.Collections;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct ChildsData
{
    [Drop]
    [ComType<FFIOrderedSet>]
    internal NOrderedSet<NodeId> m_childs;
    [Drop]
    internal NativeBox<TextData> m_text_data;

    [UnscopedRef]
    public NOrderedSet<NodeId>.Enumerator GetEnumerator() => m_childs.GetEnumerator();
}

public record struct HierarchyData
{
    public NodeId? Parent { get; set; }

    internal EmbedMap<uint, string> m_texts;
    internal uint m_text_id_inc;

    internal Stack<ViewNode>? m_scope_stack;
}

[Dropping]
public partial record struct TextData
{
    public ViewNode m_text_root;

    [Drop]
    public NativeList<TextItem> m_items;
    [Drop]
    public NativeList<TextParagraph> m_paragraph;

    public void Clear(ref HierarchyData Hierarchies)
    {
        m_items.Clear();
        m_paragraph.Clear();
        ref var root_hierarchy = ref Unsafe.Add(ref Hierarchies, m_text_root.Index);
        var scope_stack = root_hierarchy.m_scope_stack ??= new();
        scope_stack.Clear();
        scope_stack.Push(m_text_root);
    }

    public void AddText(ref HierarchyData Hierarchies, ViewNode Parent, TextNode Node, uint Length)
    {
        if (m_paragraph.Count == 0 || m_paragraph[^1].Type != TextParagraphType.Inline)
        {
            m_paragraph.Add(new TextParagraph
            {
                ItemStart = (uint)m_items.Count,
                ItemLength = 0,
                LogicTextLength = 0,
                Type = TextParagraphType.Inline
            });
        }
        ref var root_hierarchy = ref Unsafe.Add(ref Hierarchies, m_text_root.Index);
        if (!root_hierarchy.m_scope_stack!.TryPeek(out var scope)) scope = new(uint.MaxValue);
        ref var paragraph = ref m_paragraph[^1];
        m_items.Add(new TextItem
        {
            LogicTextStart = paragraph.LogicTextLength,
            LogicTextLength = Length,
            Node = Node,
            Parent = Parent,
            Container = scope,
            Type = TextItemType.Text
        });
        paragraph.ItemLength++;
        paragraph.LogicTextLength += Length;
    }

    public void AddInlineBlock(ref HierarchyData Hierarchies, ViewNode Parent, ViewNode Node)
    {
        if (m_paragraph.Count == 0 || m_paragraph[^1].Type != TextParagraphType.Inline)
        {
            m_paragraph.Add(new TextParagraph
            {
                ItemStart = (uint)m_items.Count,
                ItemLength = 0,
                LogicTextLength = 0,
                Type = TextParagraphType.Inline
            });
        }
        ref var root_hierarchy = ref Unsafe.Add(ref Hierarchies, m_text_root.Index);
        if (!root_hierarchy.m_scope_stack!.TryPeek(out var scope)) scope = new(uint.MaxValue);
        ref var paragraph = ref m_paragraph[^1];
        m_items.Add(new TextItem
        {
            LogicTextStart = paragraph.LogicTextLength,
            LogicTextLength = 1,
            Node = Node,
            Parent = Parent,
            Container = scope,
            Type = TextItemType.InlineBlock
        });
        paragraph.ItemLength++;
        paragraph.LogicTextLength += 1;
    }

    public void AddBlock(ref HierarchyData Hierarchies, ViewNode Parent, ViewNode Node)
    {
        if (m_paragraph.Count == 0 || m_paragraph[^1].Type != TextParagraphType.Block)
        {
            m_paragraph.Add(new TextParagraph
            {
                ItemStart = (uint)m_items.Count,
                ItemLength = 0,
                LogicTextLength = 0,
                Type = TextParagraphType.Block
            });
        }
        ref var root_hierarchy = ref Unsafe.Add(ref Hierarchies, m_text_root.Index);
        if (!root_hierarchy.m_scope_stack!.TryPeek(out var scope)) scope = new(uint.MaxValue);
        ref var paragraph = ref m_paragraph[^1];
        m_items.Add(new TextItem
        {
            LogicTextStart = 0,
            LogicTextLength = 0,
            Node = Node,
            Parent = Parent,
            Container = scope,
            Type = TextItemType.Block
        });
        paragraph.ItemLength++;
    }

    public void AddAbsoluteBlock(ref HierarchyData Hierarchies, ViewNode Parent, ViewNode Node)
    {
        m_paragraph.Add(new TextParagraph
        {
            ItemStart = (uint)m_items.Count,
            ItemLength = 1,
            LogicTextLength = 0,
            Type = TextParagraphType.AbsoluteBlock
        });
        ref var root_hierarchy = ref Unsafe.Add(ref Hierarchies, m_text_root.Index);
        if (!root_hierarchy.m_scope_stack!.TryPeek(out var scope)) scope = new(uint.MaxValue);
        m_items.Add(new TextItem
        {
            LogicTextStart = 0,
            LogicTextLength = 0,
            Node = Node,
            Parent = Parent,
            Container = scope,
            Type = TextItemType.Block
        });
    }

    public void StartScope(ref HierarchyData Hierarchies, ViewNode Scope)
    {
        ref var root_hierarchy = ref Unsafe.Add(ref Hierarchies, m_text_root.Index);
        root_hierarchy.m_scope_stack!.Push(Scope);
    }

    public void EndScope(ref HierarchyData Hierarchies)
    {
        ref var root_hierarchy = ref Unsafe.Add(ref Hierarchies, m_text_root.Index);
        root_hierarchy.m_scope_stack!.Pop();
    }

    public void FinishBuild(ref HierarchyData Hierarchies)
    {
        foreach (ref var paragraph in m_paragraph)
        {
            if (paragraph.Type != TextParagraphType.Inline) continue;
            paragraph.RawCharMap.Clear();
            paragraph.RawCharMap = NativeList<RawCharType>.CreateZeroed((int)paragraph.LogicTextLength);
            paragraph.CollectedText.Clear();
            paragraph.CollectedText.Capacity = (int)paragraph.LogicTextLength;
            var items = m_items.AsSpan.Slice((int)paragraph.ItemStart, (int)paragraph.ItemLength);
            foreach (ref var item in items)
            {
                if (item.Type != TextItemType.Text)
                {
                    paragraph.CollectedText.Add('￼');
                }
                else
                {
                    ref var hierarchies = ref Unsafe.Add(ref Hierarchies, item.Parent.Index);
                    var text = hierarchies.m_texts[item.Node.Index];
                    var start = paragraph.CollectedText.Count;
                    paragraph.CollectedText.AddRange(text);
                    var span = paragraph.CollectedText.AsSpan[start..];
                    for (var i = 0; i < span.Length;)
                    {
                        var index = span[i..].IndexOfAny(['\t', '\v', '\r', '\n']);
                        if (index < 0) break;
                        var ci = i + index;
                        paragraph.RawCharMap[ci] = span[ci] switch
                        {
                            '\t' => RawCharType.HT,
                            '\v' => RawCharType.VT,
                            '\n' => RawCharType.LF,
                            '\r' => RawCharType.CR,
                            _ => throw new UnreachableException(),
                        };
                        span[ci] = ' ';
                        i = ci + 1;
                    }
                }
            }
        }
    }
}

public enum TextItemType : byte
{
    Text,
    InlineBlock,
    Block,
}

public struct TextItem
{
    public uint LogicTextStart;
    public uint LogicTextLength;
    public ViewOrTextNode Node;
    public ViewNode Parent;
    public ViewNode Container;
    public TextItemType Type;
}

public enum TextParagraphType : byte
{
    Inline,
    Block,
    AbsoluteBlock,
}

[Dropping]
public partial struct TextParagraph
{
    public NativeList<char> CollectedText;
    public NativeList<RawCharType> RawCharMap;
    public uint ItemStart;
    public uint ItemLength;
    public uint LogicTextLength;
    public TextParagraphType Type;
}

public enum RawCharType : byte
{
    AsIs = 0,
    LF = (byte)'\n',
    CR = (byte)'\r',
    HT = (byte)'\t',
    VT = (byte)'\v',
}
