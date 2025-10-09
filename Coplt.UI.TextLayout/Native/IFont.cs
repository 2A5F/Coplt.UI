using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Styles;
using Coplt.UI.Texts;

namespace Coplt.UI.Layouts.Native;

public struct NFontInfo
{
    public FontMetrics Metrics;
    public FontWidth Width;
    public FontWeight Weight;
    public FontStyle Style;
    public FontFlags Flags;
}


[Interface, Guid("09c443bc-9736-4aac-8117-6890555005ff")]
public unsafe partial struct IFont
{
    [ComType<ConstPtr<NFontInfo>>]
    public readonly partial NFontInfo* Info { get; }
}
