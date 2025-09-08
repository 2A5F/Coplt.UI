using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Mathematics;
using Coplt.UI.Elements;
using Coplt.UI.Rendering;

namespace Coplt.UI.Styles;

[StructLayout(LayoutKind.Auto)]
public record struct GpuStyle()
{
    #region Default

    public static readonly GpuStyle Default = new();

    #endregion

    #region Styles

    public int ZIndex = 0;
    public float Opaque = 1f;

    public BoxShadow BoxShadow = new(0.Fx, 0.Fx, Color.Transparent);

    public Color BackgroundColor = Color.Transparent;
    public UIImage BackgroundImage = UIImage.None;
    public Color BackgroundImageTint = Color.White;
    public SamplerType BackgroundImageSampler = SamplerType.LinearClamp;

    public Rect<Color> BorderColor = new(Color.Transparent);
    public Corner<float> BorderRadius = new(0);
    public BorderRadiusMode BorderRadiusMode = BorderRadiusMode.Circle;

    public Color TextColor = Color.Black;
    public Length TextSize = 16.Fx;

    public FilterFunc BackDrop = FilterFunc.None;
    public FilterFunc Filter = FilterFunc.None;

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

public static class GpuStyleExtensions
{
    extension<TEd>(UIElement<GpuRd, TEd> element) where TEd : new()
    {
        private ref GpuStyle RefStyle => ref Unsafe.AsRef(in element.RData.m_gpu_style);

        // todo set mark dirty

        public int ZIndex
        {
            get => element.RefStyle.ZIndex;
            set => element.RefStyle.ZIndex = value;
        }

        public float Opaque
        {
            get => element.RefStyle.Opaque;
            set => element.RefStyle.Opaque = value;
        }

        public BoxShadow BoxShadow
        {
            get => element.RefStyle.BoxShadow;
            set => element.RefStyle.BoxShadow = value;
        }

        public Color BackgroundColor
        {
            get => element.RefStyle.BackgroundColor;
            set => element.RefStyle.BackgroundColor = value;
        }

        public UIImage BackgroundImage
        {
            get => element.RefStyle.BackgroundImage;
            set => element.RefStyle.BackgroundImage = value;
        }

        public Color BackgroundImageTint
        {
            get => element.RefStyle.BackgroundImageTint;
            set => element.RefStyle.BackgroundImageTint = value;
        }

        public SamplerType BackgroundImageSampler
        {
            get => element.RefStyle.BackgroundImageSampler;
            set => element.RefStyle.BackgroundImageSampler = value;
        }

        public Rect<Color> BorderColor
        {
            get => element.RefStyle.BorderColor;
            set => element.RefStyle.BorderColor = value;
        }

        public Corner<float> BorderRadius
        {
            get => element.RefStyle.BorderRadius;
            set => element.RefStyle.BorderRadius = value;
        }

        public BorderRadiusMode BorderRadiusMode
        {
            get => element.RefStyle.BorderRadiusMode;
            set => element.RefStyle.BorderRadiusMode = value;
        }

        public Color TextColor
        {
            get => element.RefStyle.TextColor;
            set => element.RefStyle.TextColor = value;
        }

        public Length TextSize
        {
            get => element.RefStyle.TextSize;
            set => element.RefStyle.TextSize = value;
        }

        public FilterFunc BackDrop
        {
            get => element.RefStyle.BackDrop;
            set => element.RefStyle.BackDrop = value;
        }

        public FilterFunc Filter
        {
            get => element.RefStyle.Filter;
            set => element.RefStyle.Filter = value;
        }
    }
}
