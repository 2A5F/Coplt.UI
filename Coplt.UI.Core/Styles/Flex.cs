namespace Coplt.UI.Styles;

public enum FlexDirection : byte
{
    Column,
    Row,
    ColumnReverse,
    RowReverse,
}

public enum FlexWrap : byte
{
    NoWrap,
    Wrap,
    WrapReverse,
}

public enum AlignType : byte
{
    None,
    Start,
    End,
    FlexStart,
    FlexEnd,
    Center,
    Baseline,
    Stretch,
    SpaceBetween,
    SpaceEvenly,
    SpaceAround,
}
