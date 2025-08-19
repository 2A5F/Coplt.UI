namespace Coplt.UI.Styles;

public static class StyleSynEx
{
    public static LengthPercentage Fx(this int value) => LengthPercentage.MakeFixed(value);
    public static LengthPercentage Fx(this float value) => LengthPercentage.MakeFixed(value);
    
    public static LengthPercentage Pc(this int value) => LengthPercentage.MakePercent(value);
    public static LengthPercentage Pc(this float value) => LengthPercentage.MakePercent(value);
}
