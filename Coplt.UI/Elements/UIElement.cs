using Coplt.UI.BoxLayouts;
using Coplt.UI.Collections;
using Coplt.UI.Utilities;
using Coplt.UI.Widgets;

namespace Coplt.UI.Elements;

public sealed class UIElement
{
    #region Fields

    internal EmbedSet<(object, ulong)> m_tags;
    internal OrderedSet<UIElement> m_childs;

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

    public int Count => m_childs.Count;

    public bool Contains(UIElement child) => m_childs.Contains(child);

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
        child.Parent?.Remove(child);
        m_childs.Add(child);
        child.Parent = this;
        MarkDirty();
    }

    public bool Remove(UIElement child)
    {
        if (!m_childs.Remove(child)) return false;
        child.Parent = null;
        child.Panel = null;
        MarkDirty();
        return true;
    }

    public void Clear()
    {
        if (m_childs.Count == 0) return;
        foreach (ref var child in m_childs)
        {
            child.Panel = null;
            child.Parent = null;
        }
        m_childs.Clear();
        MarkDirty();
    }

    #endregion
}
