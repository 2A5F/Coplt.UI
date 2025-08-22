using Coplt.UI.Elements;

namespace Coplt.UI.Styles.Rules;

internal class InlineStyle
{
    internal StyleSheet m_sheet;
}

public readonly ref struct InlineStyleAccess(UIElement Element)
{
    #region Fields

    public readonly UIElement Element = Element;

    #endregion

    #region Props

    internal InlineStyle? TryStyle => Element.m_inline_style;
    internal InlineStyle Style => Element.m_inline_style ??= new();

    #endregion

    #region Items

    public Display Display
    {
        get => TryStyle == null || !TryStyle.m_sheet.TryGetByteEnum<Display>(StylePropertyId.Display, out var v)
            ? ComputedStyle.Default.Display
            : v;
        set
        {
            Style.m_sheet.SetByteEnum(StylePropertyId.Display, value);
            Element.MarkStylesDirty();
        }
    }

    #endregion
}
