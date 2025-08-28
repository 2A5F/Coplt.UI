namespace Coplt.UI.Styles;

public static class StyleSynEx
{
    extension(int value)
    {
        public Length Fx => Length.MakeFixed(value);
        public LengthPercentage Pc => LengthPercentage.MakePercent(value);
    }

    extension(float value)
    {
        public Length Fx => Length.MakeFixed(value);
        public LengthPercentage Pc => LengthPercentage.MakePercent(value);
    }
}
