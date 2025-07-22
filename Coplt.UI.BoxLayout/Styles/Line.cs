namespace Coplt.UI.Styles;

public record struct Line<T>(T Start, T End)
{
    public T Start = Start;
    public T End = End;

    public Line(T All) : this(All, All) { }
}
