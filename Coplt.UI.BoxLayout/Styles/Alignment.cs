namespace Coplt.UI.Styles;

public enum AlignItems : byte
{
    Start,
    End,
    FlexStart,
    FlexEnd,
    Center,
    Baseline,
    Stretch,
}

public enum JustifyItems : byte
{
    Start,
    End,
    FlexStart,
    FlexEnd,
    Center,
    Baseline,
    Stretch,
}

public enum AlignSelf : byte
{
    Start,
    End,
    FlexStart,
    FlexEnd,
    Center,
    Baseline,
    Stretch,
}

public enum JustifySelf : byte
{
    Start,
    End,
    FlexStart,
    FlexEnd,
    Center,
    Baseline,
    Stretch,
}

public enum AlignContent : byte
{
    Start,
    End,
    FlexStart,
    FlexEnd,
    Center,
    Stretch,
    SpaceBetween,
    SpaceEvenly,
    SpaceAround,
}

public enum JustifyContent : byte
{
    Start,
    End,
    FlexStart,
    FlexEnd,
    Center,
    Stretch,
    SpaceBetween,
    SpaceEvenly,
    SpaceAround,
}

public static partial class BoxStyleExtensions
{
    public static JustifyContent ToJustifyContent(this AlignContent self) => (JustifyContent)self;
    public static AlignContent ToAlignContent(this JustifyContent self) => (AlignContent)self;
}
