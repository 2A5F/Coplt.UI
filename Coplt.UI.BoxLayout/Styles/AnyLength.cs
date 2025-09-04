using Coplt.Union;

namespace Coplt.UI.Styles;

[Union2]
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

    public static AnyLength Zero => AnyLength.Fixed(0);
}
