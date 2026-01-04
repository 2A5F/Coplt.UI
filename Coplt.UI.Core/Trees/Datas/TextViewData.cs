using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Core.Geometry;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public unsafe partial struct TextViewData
{
    [Drop]
    internal NativeList<LineSpanData> m_line_spans;
    [Drop]
    internal NativeList<LineData> m_lines;

    public ReadOnlySpan<LineSpanData> LineSpans => m_line_spans.AsSpan;
    public ReadOnlySpan<LineData> Lines => m_lines.AsSpan;
}

public record struct LineSpanData
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
    public float BaseLine;
    public uint NthLine;
    public uint NodeIndex;
    public uint RunRange;
    public uint Start;
    public uint End;
    public LineSpanType Type;
}

public enum LineSpanType : byte
{
    Text,
    Space,
    Tab,
    NewLine,
    Object,
}

public record struct LineData
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
    public float BaseLine;
    public uint NthLine;
    public uint SpanStart;
    public uint SpanEnd;
}
