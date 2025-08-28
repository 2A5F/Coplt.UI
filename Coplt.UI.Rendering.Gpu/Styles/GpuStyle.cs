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
    extension<TEd>(ref StyleAccess<GpuRd, TEd> access) where TEd : new()
    {
        private ref GpuStyle Style => ref Unsafe.AsRef(in access.Element.RData.m_gpu_style);

        // todo set mark dirty

        public int ZIndex
        {
            get => access.Style.ZIndex;
            set => access.Style.ZIndex = value;
        }

        public float Opaque
        {
            get => access.Style.Opaque;
            set => access.Style.Opaque = value;
        }

        public BoxShadow BoxShadow
        {
            get => access.Style.BoxShadow;
            set => access.Style.BoxShadow = value;
        }

        public Color BackgroundColor
        {
            get => access.Style.BackgroundColor;
            set => access.Style.BackgroundColor = value;
        }

        public UIImage BackgroundImage
        {
            get => access.Style.BackgroundImage;
            set => access.Style.BackgroundImage = value;
        }

        public Color BackgroundImageTint
        {
            get => access.Style.BackgroundImageTint;
            set => access.Style.BackgroundImageTint = value;
        }

        public SamplerType BackgroundImageSampler
        {
            get => access.Style.BackgroundImageSampler;
            set => access.Style.BackgroundImageSampler = value;
        }

        public Rect<Color> BorderColor
        {
            get => access.Style.BorderColor;
            set => access.Style.BorderColor = value;
        }

        public Corner<float> BorderRadius
        {
            get => access.Style.BorderRadius;
            set => access.Style.BorderRadius = value;
        }

        public BorderRadiusMode BorderRadiusMode
        {
            get => access.Style.BorderRadiusMode;
            set => access.Style.BorderRadiusMode = value;
        }

        public Color TextColor
        {
            get => access.Style.TextColor;
            set => access.Style.TextColor = value;
        }

        public Length TextSize
        {
            get => access.Style.TextSize;
            set => access.Style.TextSize = value;
        }

        public FilterFunc BackDrop
        {
            get => access.Style.BackDrop;
            set => access.Style.BackDrop = value;
        }

        public FilterFunc Filter
        {
            get => access.Style.Filter;
            set => access.Style.Filter = value;
        }
    }
}
