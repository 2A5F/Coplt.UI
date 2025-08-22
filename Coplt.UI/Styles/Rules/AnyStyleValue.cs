using Coplt.Union;

namespace Coplt.UI.Styles.Rules;

[Union]
public partial struct AnyStyleValue
{
    [UnionTemplate]
    private interface Template
    {
        void None();
        void Auto();
        bool Bool();
        byte Byte();
        int Int();
        float Fixed();
        float Percent();
        CalcId Calc();

        // external index
        int Color();
    }

    public static implicit operator AnyStyleValue(float value) => MakeFixed(value);

    public static implicit operator AnyStyleValue(float? value) => value.HasValue ? MakeFixed(value.Value) : MakeNone();

    public static implicit operator AnyStyleValue(Length value) => value.Tag switch
    {
        Length.Tags.Fixed => MakeFixed(value.Fixed),
        Length.Tags.Calc => MakeCalc(value.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator AnyStyleValue(LengthPercentage value) => value.Tag switch
    {
        LengthPercentage.Tags.Fixed => MakeFixed(value.Fixed),
        LengthPercentage.Tags.Percent => MakePercent(value.Percent),
        LengthPercentage.Tags.Calc => MakeCalc(value.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator AnyStyleValue(LengthPercentageAuto value) => value.Tag switch
    {
        LengthPercentageAuto.Tags.Auto => MakeAuto(),
        LengthPercentageAuto.Tags.Fixed => MakeFixed(value.Fixed),
        LengthPercentageAuto.Tags.Percent => MakePercent(value.Percent),
        LengthPercentageAuto.Tags.Calc => MakeCalc(value.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator AnyStyleValue(Dimension value) => value.Tag switch
    {
        Dimension.Tags.Auto => MakeAuto(),
        Dimension.Tags.Fixed => MakeFixed(value.Fixed),
        Dimension.Tags.Percent => MakePercent(value.Percent),
        Dimension.Tags.Calc => MakeCalc(value.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };
}
