using Coplt.Union;

namespace Coplt.UI.Styles;

[Union]
public readonly partial struct AnyLength
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        float Fixed();
        float Percent();
        float Fr();
        void MinContent();
        void MaxContent();
        float FitContentFixed();
        float FitContentPercent();
        CalcId Calc();
    }

    public static AnyLength Zero => MakeFixed(0);

    public static AnyLength Auto = MakeAuto();
    public static AnyLength MinContent = MakeMinContent();
    public static AnyLength MaxContent = MakeMaxContent();
}
