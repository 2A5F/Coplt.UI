namespace Coplt.UI.Styles;

public record struct Corner<T>(T TopRight, T TopLeft, T BottomLeft, T BottomRight)
{
    public T TopRight = TopRight;
    public T TopLeft = TopLeft;
    public T BottomLeft = BottomLeft;
    public T BottomRight = BottomRight;

    public Corner(T All) : this(All, All, All, All) { }

    public static implicit operator Corner<T>(T value) => new(value);
}
