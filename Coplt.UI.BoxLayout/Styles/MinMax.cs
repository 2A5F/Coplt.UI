namespace Coplt.UI.Styles;

public record struct MinMax<T>(T Min, T Max)
{
    public T Min = Min;
    public T Max = Max;

    public MinMax(T All) : this(All, All) { }
}

public record struct MinMax<TMin, TMax>(TMin Min, TMax Max)
{
    public TMin Min = Min;
    public TMax Max = Max;
}
