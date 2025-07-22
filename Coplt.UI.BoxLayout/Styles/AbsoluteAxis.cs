using System;
using Coplt.UI.BoxLayouts;

namespace Coplt.UI.Styles;

public enum AbsoluteAxis : byte
{
    Horizontal,
    Vertical,
}

public enum AbstractAxis : byte
{
    Inline,
    Block,
}

public static partial class BoxStyleExtensions
{
    public static AbsoluteAxis Other(this AbsoluteAxis self) => self switch
    {
        AbsoluteAxis.Horizontal => AbsoluteAxis.Vertical,
        AbsoluteAxis.Vertical => AbsoluteAxis.Horizontal,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
    };
    public static AbstractAxis Other(this AbstractAxis self) => self switch
    {
        AbstractAxis.Inline => AbstractAxis.Block,
        AbstractAxis.Block => AbstractAxis.Inline,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
    };

    public static RequestedAxis ToRequestedAxis(this AbsoluteAxis self) => self switch
    {
        AbsoluteAxis.Horizontal => RequestedAxis.Horizontal,
        AbsoluteAxis.Vertical => RequestedAxis.Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
    };
}
