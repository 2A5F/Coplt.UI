using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

public interface ICalc
{
    float Calc(CalcId id, float basis);
}

public interface ITryResolve<in TCtx>
{
    public float? TryResolve<TCalc>(TCtx ctx, ref TCalc calc) where TCalc : ICalc, allows ref struct;
}
