namespace Coplt.UI.Styles;

public enum Display : byte
{
    Flex,
    Grid,
    Block,
    Inline,
}

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
    Scroll,
}

public enum LengthType : byte
{
    Fixed,
    Percent,
    Auto,
}
