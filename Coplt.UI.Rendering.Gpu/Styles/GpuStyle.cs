using System.Runtime.InteropServices;
using Coplt.Mathematics;

namespace Coplt.UI.Styles;

[StructLayout(LayoutKind.Auto)]
public record struct GpuStyle()
{
    #region Default

    public static readonly GpuStyle Default = new();

    #endregion

    #region Styles

    public int ZIndex { get; set; } = 0;
    public float Opaque { get; set; } = 1f;

    public BoxShadow BoxShadow { get; set; } = new(0.Fx(), 0.Fx(), Color.Transparent);

    public Color BackgroundColor { get; set; } = Color.Transparent;
    public UIImage BackgroundImage { get; set; } = UIImage.None;
    public Color BackgroundImageTint { get; set; } = Color.White;
    public SamplerType BackgroundImageSampler { get; set; } = SamplerType.LinearClamp;

    public Rect<Color> BorderColor { get; set; } = new(Color.Transparent);
    public Corner<float> BorderRadius { get; set; } = new(0);
    public BorderRadiusMode BorderRadiusMode = BorderRadiusMode.Circle;

    public Color TextColor { get; set; } = Color.Black;
    public Length TextSize { get; set; } = 16.Fx();

    public FilterFunc BackDrop { get; set; } = FilterFunc.None;
    public FilterFunc Filter { get; set; } = FilterFunc.None;

    #endregion

    #region Props

    public bool IsVisible =>
        Opaque > 0 && (
            (BackgroundImage != UIImage.None && BackgroundImageTint.a > 0)
            || BackgroundColor.a > 0
            || BorderColor.Top.a > 0
            || BorderColor.Right.a > 0
            || BorderColor.Bottom.a > 0
            || BorderColor.Left.a > 0
        );

    #endregion
}
