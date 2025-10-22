
using Coplt.UI.Core.Styles;
using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

public record struct TextStyleData()
{
    public float TextColorR = 1;
    public float TextColorG = 1;
    public float TextColorB = 1;
    public float TextColorA = 1;

    public float TextSizeValue = 16;

    public LengthType TextSize = LengthType.Fixed;
}
