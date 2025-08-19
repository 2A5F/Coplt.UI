using Coplt.UI.BoxLayouts;
using Coplt.UI.Widgets;

namespace Coplt.UI.Elements;

public sealed class UIElement
{
    #region Fields

    private HashSet<string>? m_tags;
    internal List<UIElement>? m_childs;

    internal StyleSet m_computed_style = new();
    internal Layout m_unrounded_layout;

    internal bool m_dirty;

    #endregion

    #region Props

    public ulong Version { get; internal set; }

    public UIPanel? Panel { get; internal set; }
    public AWidget? Widget { get; internal set; }

    public UIElement? Parent { get; internal set; }

    public string? Name { get; set; }
    public HashSet<string> Tags =>
        m_tags ?? Interlocked.CompareExchange(ref m_tags, new(), null) ?? m_tags;

    public ref readonly Layout UnroundedLayout => ref m_unrounded_layout;

    #endregion

    #region Dirty

    public void MarkDirty()
    {
        if (Panel == null) return;
        if (m_dirty) return;
        m_dirty = true;
        // todo
    }

    #endregion

    #region Childs

    internal List<UIElement> EnsureChildsList => m_childs ??= new();
    internal List<UIElement> AssertChildsList => m_childs ?? throw new IndexOutOfRangeException();

    public int Count => m_childs?.Count ?? 0;

    public UIElement this[int index]
    {
        get => AssertChildsList[index];
        set => AssertChildsList[index] = value;
    }

    private void CheckCirRef(UIElement new_child)
    {
        for (var cur = Parent; cur != null; cur = cur.Parent)
        {
            if (cur == new_child) throw new IndexOutOfRangeException("Adding will create a circular reference");
        }
    }

    public void Add(UIElement child, bool no_check = false)
    {
        if (child == this) throw new InvalidOperationException("Cannot add self as child.");
        if (child.Parent == this) return;
        if (!no_check) CheckCirRef(child);
        var list = EnsureChildsList;
        child.Parent?.Remove(child);
        list.Add(child);
        child.Parent = this;
        MarkDirty();
    }

    public void RemoveAt(int index)
    {
        var list = AssertChildsList[index];
        list.RemoveAt(index);
        list.Parent = null;
        MarkDirty();
    }

    public bool Remove(UIElement child)
    {
        var list = m_childs;
        if (list == null) return false;
        var index = list.IndexOf(child);
        if (index < 0) return false;
        list.RemoveAt(index);
        return true;
    }

    public bool Contains(UIElement child) => m_childs?.Contains(child) ?? false;

    #endregion
}
