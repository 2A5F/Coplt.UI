using System.Collections;
using System.Text;
using Coplt.UI.BoxLayouts;
using Coplt.UI.Collections;
using Coplt.UI.Utilities;
using Coplt.UI.Widgets;

namespace Coplt.UI.Elements;

public sealed class UIElement : IEnumerable<UIElement>
{
    #region Fields

    internal EmbedSet<(object, ulong)> m_tags;
    internal OrderedSet<UIElement> m_childs;

    internal StyleSet m_computed_style = new();
    internal Layout m_unrounded_layout;
    internal Layout m_final_layout;

    internal bool m_dirty;

    #endregion

    #region Props

    public ulong Version { get; internal set; }

    public UIPanel? Panel { get; internal set; }
    public UIDocument? Document { get; internal set; }
    public AWidget? Widget { get; internal set; }

    public UIElement? Parent { get; internal set; }

    public string? Name { get; set; }

    public ref readonly StyleSet ComputedStyle => ref m_computed_style;
    public ref readonly Layout UnroundedLayout => ref m_unrounded_layout;
    public ref readonly Layout FinalLayout => ref m_final_layout;

    #endregion

    #region Dirty

    public void MarkDirty()
    {
        if (Document == null) return;
        if (m_dirty) return;
        m_dirty = true;
        Version++;
        // todo
    }

    #endregion

    #region Tags

    public bool HasTag(string Tag) => m_tags.Contains((Tag, 0));
    public bool HasTag<E>(E Tag) where E : struct, Enum => m_tags.Contains((typeof(E), UnsafeUtils.EnumToULong(Tag)));
    public bool AddTag(string Tag)
    {
        var r = m_tags.Add((Tag, 0));
        if (r) MarkDirty();
        return r;
    }
    public bool AddTag<E>(E Tag) where E : struct, Enum
    {
        var r = m_tags.Add((typeof(E), UnsafeUtils.EnumToULong(Tag)));
        if (r) MarkDirty();
        return r;
    }
    public bool RemoveTag(string Tag)
    {
        var r = m_tags.Remove((Tag, 0));
        if (r) MarkDirty();
        return r;
    }
    public bool RemoveTag<E>(E Tag) where E : struct, Enum
    {
        var r = m_tags.Remove((typeof(E), UnsafeUtils.EnumToULong(Tag)));
        if (r) MarkDirty();
        return r;
    }

    public void ClearTags()
    {
        if (m_tags.Count == 0) return;
        m_tags.Clear();
        MarkDirty();
    }

    #endregion

    #region Childs

    #region Count

    public int Count => m_childs.Count;

    #endregion

    #region Contains

    public bool Contains(UIElement child) => m_childs.Contains(child);

    #endregion

    #region Private

    private void CheckCirRef(UIElement new_child)
    {
        for (var cur = Parent; cur != null; cur = cur.Parent)
        {
            if (cur == new_child) throw new IndexOutOfRangeException("Adding will create a circular reference");
        }
    }

    private void EnsureChildNoAdd(UIElement node, bool no_check = false)
    {
        if (node.Parent != this)
        {
            if (!no_check) CheckCirRef(node);
            node.Parent?.Remove(node);
            node.Parent = this;
        }
    }

    #endregion

    #region Add

    public void Add(UIElement child, bool no_check = false)
    {
        if (child == this) throw new InvalidOperationException("Cannot add self as child.");
        if (child.Parent == this) return;
        if (!no_check) CheckCirRef(child);
        child.Parent?.Remove(child);
        m_childs.Add(child);
        child.Parent = this;
        MarkDirty();
    }

    public void Add(params ReadOnlySpan<UIElement> childs)
    {
        foreach (var child in childs)
        {
            Add(child);
        }
    }

    public void Add(bool no_check, params ReadOnlySpan<UIElement> childs)
    {
        foreach (var child in childs)
        {
            Add(child, no_check);
        }
    }

    #endregion

    #region Prepend

    public void Prepend(UIElement child, bool no_check = false)
    {
        if (child == this) throw new InvalidOperationException("Cannot add self as child.");
        if (child.Parent == this) return;
        if (!no_check) CheckCirRef(child);
        child.Parent?.Remove(child);
        m_childs.AddFirst(child);
        child.Parent = this;
        MarkDirty();
    }

    public void Prepend(params ReadOnlySpan<UIElement> childs)
    {
        foreach (var child in childs)
        {
            Prepend(child);
        }
    }

    public void Prepend(bool no_check, params ReadOnlySpan<UIElement> childs)
    {
        foreach (var child in childs)
        {
            Prepend(child, no_check);
        }
    }

    #endregion

    #region Remove

    public bool Remove(UIElement child)
    {
        if (!m_childs.Remove(child)) return false;
        child.Parent = null;
        child.Document = null;
        child.Panel = null;
        MarkDirty();
        return true;
    }

    #endregion

    #region Clear

    public void Clear()
    {
        if (m_childs.Count == 0) return;
        foreach (ref var child in m_childs)
        {
            child.Parent = null;
            child.Document = null;
            child.Panel = null;
        }
        m_childs.Clear();
        MarkDirty();
    }

    #endregion

    #region SetNext

    public void SetNext(UIElement child, UIElement next_child, bool no_check = false)
    {
        EnsureChildNoAdd(child);
        EnsureChildNoAdd(next_child);
        m_childs.SetNext(child, next_child);
    }

    #endregion

    #region SetPrev

    public void SetPrev(UIElement child, UIElement prev_child, bool no_check = false)
    {
        EnsureChildNoAdd(child);
        EnsureChildNoAdd(prev_child);
        m_childs.SetPrev(child, prev_child);
    }

    #endregion

    #region Enumerator

    public OrderedSet<UIElement>.Enumerator GetEnumerator() => m_childs.GetEnumerator();
    IEnumerator<UIElement> IEnumerable<UIElement>.GetEnumerator() => new OrderedSet<UIElement>.ClassEnumerator(ref m_childs);
    IEnumerator IEnumerable.GetEnumerator() => new OrderedSet<UIElement>.ClassEnumerator(ref m_childs);

    #endregion

    #endregion

    #region Tree

    public UIElement? After => Parent == null ? null : Parent.m_childs.TryGetNext(this, out var next) ? next : null;
    public UIElement? Before => Parent == null ? null : Parent.m_childs.TryGetPrev(this, out var prev) ? prev : null;

    #endregion

    #region ToString

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"<view");
        if (Name != null) sb.Append($" name=\"{Name.Replace("\"", "\\\"")}\"");
        sb.Append('>');
        return sb.ToString();
    }

    #endregion
}
