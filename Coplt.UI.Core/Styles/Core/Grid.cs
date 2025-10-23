using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Collections;

namespace Coplt.UI.Core.Styles;

public enum GridNameType : byte
{
    Name,
    Start,
    End,
}

public record struct GridName
{
    public int Id;
    public GridNameType Type;
}

public enum SizingType : byte
{
    Auto,
    Fixed,
    Percent,
    Fraction,
    MinContent,
    MaxContent,
    FitContent,
}

public record struct SizingValue
{
    /// <code>Fixed | Percent | Fraction | FitContent</code>
    public float Value;
    /// <code>FitContent</code>
    public LengthType Type;
}

public enum RepetitionType : byte
{
    Count,
    AutoFill,
    AutoFit,
}

public record struct Repetition
{
    public ushort Value;
    public RepetitionType Type;

    public static implicit operator Repetition(ushort value) => Count(value);
    public static implicit operator Repetition(int value) => Count(value);

    public static Repetition Count(int value) => new() { Value = (ushort)value, Type = RepetitionType.Count };
    public static Repetition Count(ushort value) => new() { Value = value, Type = RepetitionType.Count };
    public static Repetition AutoFill => new() { Type = RepetitionType.AutoFill };
    public static Repetition AutoFit => new() { Type = RepetitionType.AutoFit };
}

public record struct TrackSizing
{
    public SizingValue Value;
    public SizingType Type;

    public static TrackSizing Auto => new()
    {
        Type = SizingType.Auto,
    };

    public static TrackSizing MinContent => new()
    {
        Type = SizingType.MinContent,
    };

    public static TrackSizing MaxContent => new()
    {
        Type = SizingType.MaxContent,
    };

    public static TrackSizing Fraction(float value)
    {
        return new()
        {
            Value = new() { Value = value },
            Type = SizingType.Fraction,
        };
    }

    public static TrackSizing Length(float value, LengthType type)
    {
        var t = type switch
        {
            LengthType.Fixed => SizingType.Fixed,
            LengthType.Percent => SizingType.Percent,
            LengthType.Auto => SizingType.Auto,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        return new()
        {
            Value = new() { Value = value },
            Type = t,
        };
    }

    public static TrackSizing FitContent(float value, LengthType type)
    {
        return new()
        {
            Value = new() { Value = value, Type = type },
            Type = SizingType.FitContent,
        };
    }
}

public record struct TrackSizingFunction
{
    public SizingValue MinValue;
    public SizingValue MaxValue;
    public SizingType Min;
    public SizingType Max;

    public static implicit operator TrackSizingFunction(TrackSizing Function) => new()
    {
        MinValue = Function.Value,
        MaxValue = Function.Value,
        Min = Function.Type,
        Max = Function.Type,
    };

    public static TrackSizingFunction MinMax(TrackSizing Min, TrackSizing Max) => new()
    {
        MinValue = Min.Value,
        MaxValue = Max.Value,
        Min = Min.Type,
        Max = Max.Type,
    };

    public static TrackSizingFunction Auto => TrackSizing.Auto;

    public static TrackSizingFunction MinContent => TrackSizing.MinContent;

    public static TrackSizingFunction MaxContent => TrackSizing.MaxContent;

    public static TrackSizingFunction Fraction(float value) => TrackSizing.Fraction(value);

    public static TrackSizingFunction Length(float value, LengthType type) => TrackSizing.Length(value, type);

    public static TrackSizingFunction FitContent(float value, LengthType type) => TrackSizing.FitContent(value, type);
}

[Dropping]
public partial record struct GridTemplateRepetition
{
    [Drop]
    public NativeList<TrackSizingFunction> Tracks;
    [Drop]
    public NativeList<NativeList<GridName>> LineIds;
    public ushort RepetitionValue;
    public RepetitionType Repetition;

    public static GridTemplateRepetition Create(
        Repetition Repetition, NativeList<TrackSizingFunction> Tracks, NativeList<NativeList<GridName>> LineIds
    ) => new()
    {
        Tracks = Tracks,
        LineIds = LineIds,
        RepetitionValue = Repetition.Value,
        Repetition = Repetition.Type,
    };

    public GridTemplateRepetition Move() => Swap(default);

    public GridTemplateRepetition Swap(GridTemplateRepetition other)
    {
        var self = this;
        this = other;
        return self;
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct GridTemplateComponentUnion
{
    [FieldOffset(0)]
    public TrackSizingFunction Single;
    [FieldOffset(0)]
    public GridTemplateRepetition Repeat;
}

public enum GridTemplateComponentType : byte
{
    Single,
    Repeat,
}

public record struct GridTemplateComponent : IDisposable
{
    public GridTemplateComponentUnion Union;
    public GridTemplateComponentType Type;

    public void Dispose()
    {
        if (Type is GridTemplateComponentType.Repeat)
        {
            Union.Repeat.Dispose();
        }
        Type = GridTemplateComponentType.Single;
    }

    public readonly bool Equals(GridTemplateComponent other) => Type == other.Type && Type switch
    {
        GridTemplateComponentType.Single => Union.Single == other.Union.Single,
        GridTemplateComponentType.Repeat => Union.Repeat == other.Union.Repeat,
        _ => throw new ArgumentOutOfRangeException()
    };

    public readonly override int GetHashCode() => (int)Type ^ Type switch
    {
        GridTemplateComponentType.Single => Union.Single.GetHashCode(),
        GridTemplateComponentType.Repeat => Union.Repeat.GetHashCode(),
        _ => throw new ArgumentOutOfRangeException()
    };

    public override string ToString() => Type switch
    {
        GridTemplateComponentType.Single => Union.Single.ToString(),
        GridTemplateComponentType.Repeat => Union.Repeat.ToString(),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator GridTemplateComponent(TrackSizingFunction single) => Single(single);

    public static GridTemplateComponent Single(TrackSizingFunction single) => new()
    {
        Union = new() { Single = single },
        Type = GridTemplateComponentType.Single
    };

    public static GridTemplateComponent Repeat(GridTemplateRepetition repeat) => new()
    {
        Union = new() { Repeat = repeat },
        Type = GridTemplateComponentType.Repeat
    };

    public GridTemplateComponent Move() => Swap(default);

    public GridTemplateComponent Swap(GridTemplateComponent other)
    {
        var self = this;
        this = other;
        return self;
    }

    public bool IsAutoRepetition => this is
    {
        Type: GridTemplateComponentType.Repeat,
        Union.Repeat.Repetition: RepetitionType.AutoFill or RepetitionType.AutoFit,
    };
}

public record struct GridTemplateArea
{
    public GridName Id;
    public ushort RowStart;
    public ushort RowEnd;
    public ushort ColumnStart;
    public ushort ColumnEnd;
}

public enum GridPlacementType : byte
{
    Auto,
    Line,
    NamedLine,
    Span,
    NamedSpan,
}

public record struct GridPlacement
{
    public int Name;
    public short Value1;
    public GridNameType NameType;
    public GridPlacementType Type;

    public static GridPlacement Auto => new() { Type = GridPlacementType.Auto };
    public static GridPlacement Line(int value) => new() { Value1 = (short)value, Type = GridPlacementType.Line };
    public static GridPlacement NamedLine(GridName id, int value) =>
        new() { Name = id.Id, NameType = id.Type, Value1 = (short)value, Type = GridPlacementType.NamedLine };
    public static GridPlacement Span(int value) => new() { Value1 = (short)value, Type = GridPlacementType.Span };
    public static GridPlacement NamedSpan(GridName id, int value) =>
        new() { Name = id.Id, NameType = id.Type, Value1 = (short)value, Type = GridPlacementType.NamedSpan };
}
