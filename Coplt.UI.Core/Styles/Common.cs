namespace Coplt.UI.Styles;

public enum Visible : byte
{
    Visible,
    /// <summary>
    /// Acts like it's transparent, participates in layout, but doesn't display
    /// </summary>
    Hidden,
    /// <summary>
    /// Does not participate in layout, equivalent to <c>display: none</c>
    /// </summary>
    Remove,
}

public enum Container : byte
{
    Flex,
    Grid,
    Text,
}

// /// <summary>
// /// Similar to CSS float, but behaves differently.
// /// </summary>
// public enum FloatInText : byte
// {
//     None,
//     Start,
//     End,
// }

public enum Position : byte
{
    Relative,
    Absolute,
}

public enum BoxSizing : byte
{
    BorderBox,
    ContentBox,
}

public enum Overflow : byte
{
    Visible,
    Clip,
    Hidden,
}
