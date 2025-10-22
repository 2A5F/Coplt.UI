using Coplt.UI.Core.Styles;
using Coplt.Union;

namespace Coplt.UI.Styles;

[Union2]
public partial struct Length
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        float Fixed();
        float Percent();
    }

    public LengthType Type => Tag switch
    {
        Tags.Auto => LengthType.Auto,
        Tags.Fixed => LengthType.Fixed,
        Tags.Percent => LengthType.Percent,
        _ => throw new ArgumentOutOfRangeException()
    };

    public float Value => Tag switch
    {
        Tags.Auto => 0,
        Tags.Fixed => Fixed,
        Tags.Percent => Percent,
        _ => throw new ArgumentOutOfRangeException()
    };

    public Length(LengthType type, float value)
    {
        this = type switch
        {
            LengthType.Fixed => Length.Fixed(value),
            LengthType.Percent => Length.Percent(value),
            LengthType.Auto => Length.Auto,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static implicit operator Length(float value) => Length.Fixed(value);
    public static implicit operator Length(Fixed value) => Length.Fixed(value.Value);
    public static implicit operator Length(Percent value) => Length.Percent(value.Value);
}

public record struct Fixed(float Value)
{
    public static implicit operator Fixed(float value) => new(value);
}

public record struct Percent(float Value)
{
    public static implicit operator Percent(float value) => new(value);
}

public static partial class StyleExtensions
{
    extension(float v)
    {
        public Fixed fx => new(v);
        public Percent pc => new(v);
    }
}
