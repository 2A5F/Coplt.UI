using System;

namespace Coplt.UI.Styles;

public enum Display : byte
{
    Flex,
    Grid,
    Block,
    None,
}

public enum BoxGenerationMode : byte
{
    Normal,
    None,
}

public enum Position
{
    Relative,
    Absolute,
}

public enum BoxSizing
{
    BorderBox,
    ContentBox,
}

public enum Overflow
{
    Visible,
    Clip,
    Hidden,
    Scroll,
}

public static partial class BoxStyleExtensions
{
    public static bool IsScrollContainer(this Overflow self) => self switch
    {
        Overflow.Visible or Overflow.Clip => false,
        Overflow.Hidden or Overflow.Scroll => true,
        _ => false
    };

    public static float? TryAutoMinSize(this Overflow self) => self.IsScrollContainer() ? 0 : null;

    public static float? TryIntoAutomaticMinSize(this Overflow self) => self.IsScrollContainer() ? 0 : null;
}
