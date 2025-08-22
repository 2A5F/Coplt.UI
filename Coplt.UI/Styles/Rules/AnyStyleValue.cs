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
        int Image();
        int BoxShadow();
        int FilterFunc();
    }
}

public record struct StyleValuePair(StylePropertyId Id, AnyStyleValue Value)
{
    public StylePropertyId Id = Id;
    public AnyStyleValue Value = Value;
}
