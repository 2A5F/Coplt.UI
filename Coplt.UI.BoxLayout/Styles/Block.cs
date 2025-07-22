namespace Coplt.UI.Styles;

public interface IBlockContainerStyle : ICoreStyle
{
    public TextAlign TextAlign => BoxStyle.Default.TextAlign;
}

public enum TextAlign : byte
{
    Auto,
    LegacyLeft,
    LegacyRight,
    LegacyCenter,
}
