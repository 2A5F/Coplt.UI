using Coplt.Union;

namespace Coplt.UI.Layouts;

public enum AvailableSpaceType : byte
{
    Definite,
    MinContent,
    MaxContent,
}

[Union2]
public partial struct AvailableSpace
{
    [UnionTemplate]
    private interface Template
    {
        float Definite();
        void MinContent();
        void MaxContent();
    }

    public float Value => Tag switch
    {
        Tags.Definite => Definite,
        Tags.MinContent => 0,
        Tags.MaxContent => 0,
        _ => throw new ArgumentOutOfRangeException()
    };

    public AvailableSpaceType Type => Tag switch
    {
        Tags.Definite => AvailableSpaceType.Definite,
        Tags.MinContent => AvailableSpaceType.MinContent,
        Tags.MaxContent => AvailableSpaceType.MaxContent,
        _ => throw new ArgumentOutOfRangeException()
    };
}
