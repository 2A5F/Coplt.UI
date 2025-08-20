using System.Collections;
using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.Collections;
using Coplt.UI.Layouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

public sealed class UIDocument
{
    #region Fields

    internal UIElement? m_root;

    #endregion

    #region Props

    public UIPanel? Panel { get; internal set; }

    #endregion

    #region Root

    public void SetRoot(UIElement root)
    {
        if (root.Document == this) return;
        if (root.Document != null) root.Document.m_root = null;
        root.Parent?.Remove(root);
        m_root = root;
    }

    #endregion

    #region Compute

    public void ComputeLayout(Size<AvailableSpace> available_space, bool use_rounding)
    {
        if (m_root == null) return;
        LayoutTree tree = default;
        BoxLayout.ComputeRootLayout<LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator, RefCoreStyle<StyleSet>>(
            ref tree, m_root, available_space
        );
        // todo rounding
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        if (m_root == null) return "Empty Tree";
        LayoutTree tree = default;
        return PrintTree.Print<LayoutTree, UIElement, OrderedSet<UIElement>.Enumerator>(ref tree, m_root);
    }

    #endregion
}
