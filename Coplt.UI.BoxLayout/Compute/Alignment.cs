using System;
using System.Runtime.CompilerServices;
using Coplt.UI.Styles;

namespace Coplt.UI.Layouts;

public static partial class BoxLayout
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static AlignContent ApplyAlignmentFallback(
        float free_space,
        int num_items,
        AlignContent alignment_mode,
        bool is_safe
    )
    {
        // Fallback occurs in two cases:

        // 1. If there is only a single item being aligned and alignment is a distributed alignment keyword
        //    https://www.w3.org/TR/css-align-3/#distribution-values
        if (num_items <= 1 || free_space <= 0)
        {
            (alignment_mode, is_safe) = alignment_mode switch
            {
                AlignContent.Stretch => (AlignContent.FlexStart, true),
                AlignContent.SpaceBetween => (AlignContent.FlexStart, true),
                AlignContent.SpaceAround => (AlignContent.Center, true),
                AlignContent.SpaceEvenly => (AlignContent.Center, true),
                _ => (alignment_mode, is_safe),
            };
        }

        // 2. If free space is negative the "safe" alignment variants all fallback to Start alignment
        if (free_space <= 0 && is_safe)
        {
            alignment_mode = AlignContent.Start;
        }

        return alignment_mode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float ComputeAlignmentOffset(
        float free_space,
        int num_items,
        float gap,
        AlignContent alignment_mode,
        bool layout_is_flex_reversed,
        bool is_first
    )
    {
        if (is_first)
        {
            return alignment_mode switch
            {
                AlignContent.Start => 0,
                AlignContent.End => free_space,
                AlignContent.FlexStart => layout_is_flex_reversed ? free_space : 0,
                AlignContent.FlexEnd => layout_is_flex_reversed ? 0 : free_space,
                AlignContent.Center => free_space / 2,
                AlignContent.Stretch => 0,
                AlignContent.SpaceBetween => 0,
                AlignContent.SpaceEvenly => free_space >= 0 ? free_space / (num_items + 1) : free_space / 2,
                AlignContent.SpaceAround => free_space >= 0 ? free_space / num_items / 2 : free_space / 2,
                _ => throw new ArgumentOutOfRangeException(nameof(alignment_mode), alignment_mode, null)
            };
        }
        else
        {
            free_space = free_space.Max(0);
            return gap + alignment_mode switch
            {
                AlignContent.Start => 0,
                AlignContent.End => 0,
                AlignContent.FlexStart => 0,
                AlignContent.FlexEnd => 0,
                AlignContent.Center => 0,
                AlignContent.Stretch => 0,
                AlignContent.SpaceBetween => free_space / (num_items - 1),
                AlignContent.SpaceEvenly => free_space / (num_items + 1),
                AlignContent.SpaceAround => free_space / num_items,
                _ => throw new ArgumentOutOfRangeException(nameof(alignment_mode), alignment_mode, null)
            };
        }
    }
}
