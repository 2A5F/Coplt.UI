namespace Coplt.UI.Styles;

public static class StyleSynEx
{
    public static Length Fx(this int value) => Length.MakeFixed(value);
    public static Length Fx(this float value) => Length.MakeFixed(value);
    
    public static LengthPercentage Pc(this int value) => LengthPercentage.MakePercent(value);
    public static LengthPercentage Pc(this float value) => LengthPercentage.MakePercent(value);
}
