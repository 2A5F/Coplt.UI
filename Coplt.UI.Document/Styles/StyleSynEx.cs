namespace Coplt.UI.Styles;

public static class StyleSynEx
{
    extension(int value)
    {
        public Length Fx => Length.Fixed(value);
        public LengthPercentage Pc => LengthPercentage.Percent(value);
    }

    extension(float value)
    {
        public Length Fx => Length.Fixed(value);
        public LengthPercentage Pc => LengthPercentage.Percent(value);
    }
}
