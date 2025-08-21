using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

[StructLayout(LayoutKind.Auto)]
public struct LayoutCache
{
    private const int CACHE_SIZE = 9;

    [StructLayout(LayoutKind.Auto)]
    private struct CacheEntry<T>
    {
        /// The initial cached size of the node itself
        public Size<float?> known_dimensions;
        /// The initial cached size of the parent's node
        public Size<AvailableSpace> available_space;
        /// The cached size and baselines of the item
        public T content;
    }

    [InlineArray(CACHE_SIZE)]
    private struct MeasureEntryArray
    {
        private CacheEntry<Size<float>>? _;
    }

    /// The cache entry for the node's final layout
    private CacheEntry<LayoutOutput>? m_final_layout_entry;
    /// The cache entries for the node's preliminary size measurements
    private MeasureEntryArray m_measure_entries;
    /// Tracks if all cache entries are empty
    private bool m_is_empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeCacheSlot(
        Size<float?> known_dimensions,
        Size<AvailableSpace> available_space
    )
    {
        var has_known_width = known_dimensions.Width.HasValue;
        var has_known_height = known_dimensions.Height.HasValue;

        // Slot 0: Both known_dimensions were set
        if (has_known_width && has_known_height)
        {
            return 0;
        }

        // Slot 1: width but not height known_dimension was set and the other dimension was either a MaxContent or Definite available space constraint
        // Slot 2: width but not height known_dimension was set and the other dimension was a MinContent constraint
        if (has_known_width && !has_known_height)
        {
            return 1 + (available_space.Height.IsMinContent ? 1 : 0);
        }

        // Slot 3: height but not width known_dimension was set and the other dimension was either a MaxContent or Definite available space constraint
        // Slot 4: height but not width known_dimension was set and the other dimension was a MinContent constraint
        if (has_known_height && !has_known_width)
        {
            return 3 + (available_space.Width.IsMinContent ? 1 : 0);
        }

        // Slots 5-8: Neither known_dimensions were set and:
        return (available_space.Width, available_space.Height) switch
        {
            // Slot 5: x-axis available space is MaxContent or Definite and y-axis available space is MaxContent or Definite
            ({ IsMaxContent: true } or { IsDefinite: true }, { IsMaxContent: true } or { IsDefinite: true }) => 5,
            // Slot 6: x-axis available space is MaxContent or Definite and y-axis available space is MinContent
            ({ IsMaxContent: true } or { IsDefinite: true }, { IsMinContent: true }) => 6,
            // Slot 7: x-axis available space is MinContent and y-axis available space is MaxContent or Definite
            ({ IsMinContent: true }, { IsMaxContent: true } or { IsDefinite: true }) => 7,
            // Slot 8: x-axis available space is MinContent and y-axis available space is MinContent
            ({ IsMinContent: true }, { IsMinContent: true }) => 8,
            _ => throw new UnreachableException()
        };
    }

    public LayoutOutput? Get(
        Size<float?> known_dimensions,
        Size<AvailableSpace> available_space,
        RunMode run_mode
    )
    {
        switch (run_mode)
        {
            case RunMode.PerformLayout:
            {
                if (m_final_layout_entry is { } entry)
                {
                    var cached_size = entry.content.Size;
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if ((known_dimensions.Width == entry.known_dimensions.Width
                         // ReSharper disable once CompareOfFloatsByEqualityOperator
                         || known_dimensions.Width == cached_size.Width)
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        && (known_dimensions.Height == entry.known_dimensions.Height
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            || known_dimensions.Height == cached_size.Height)
                        && (known_dimensions.Width.HasValue
                            || entry.available_space.Width.IsRoughlyEqual(available_space.Width))
                        && (known_dimensions.Height.HasValue
                            || entry.available_space.Height.IsRoughlyEqual(available_space.Height)))
                        return entry.content;
                }
                break;
            }
            case RunMode.ComputeSize:
            {
                foreach (ref var may_entry in m_measure_entries)
                {
                    if (!may_entry.HasValue) continue;
                    var entry = may_entry.Value;
                    var cached_size = entry.content;

                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if ((known_dimensions.Width == entry.known_dimensions.Width
                         // ReSharper disable once CompareOfFloatsByEqualityOperator
                         || known_dimensions.Width == cached_size.Width)
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        && (known_dimensions.Height == entry.known_dimensions.Height
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            || known_dimensions.Height == cached_size.Height)
                        && (known_dimensions.Width.HasValue
                            || entry.available_space.Width.IsRoughlyEqual(available_space.Width))
                        && (known_dimensions.Height.HasValue
                            || entry.available_space.Height.IsRoughlyEqual(available_space.Height)))
                        return LayoutOutput.FromOuterSize(cached_size);
                }
                break;
            }
            case RunMode.PerformHiddenLayout:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(run_mode), run_mode, null);
        }
        return null;
    }

    public void Store(
        Size<float?> known_dimensions,
        Size<AvailableSpace> available_space,
        RunMode run_mode,
        LayoutOutput layout_output
    )
    {
        switch (run_mode)
        {
            case RunMode.PerformLayout:
            {
                m_is_empty = false;
                m_final_layout_entry = new()
                {
                    known_dimensions = known_dimensions,
                    available_space = available_space,
                    content = layout_output,
                };
                break;
            }
            case RunMode.ComputeSize:
            {
                m_is_empty = false;
                var cache_slot = ComputeCacheSlot(known_dimensions, available_space);
                m_measure_entries[cache_slot] = new()
                {
                    known_dimensions = known_dimensions,
                    available_space = available_space,
                    content = layout_output.Size,
                };
                break;
            }
            case RunMode.PerformHiddenLayout:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(run_mode), run_mode, null);
        }
    }

    public ClearState Clear()
    {
        if (m_is_empty) return ClearState.AlreadyEmpty;
        m_is_empty = true;
        m_final_layout_entry = null;
        m_measure_entries = default;
        return ClearState.Cleared;
    }

    public bool IsEmpty
    {
        get
        {
            if (m_final_layout_entry.HasValue) return false;
            foreach (var entry in m_measure_entries)
            {
                if (entry.HasValue) return false;
            }
            return true;
        }
    }
}

/// Clear operation outcome.
public enum ClearState
{
    /// Cleared some values
    Cleared,
    /// Everything was already cleared
    AlreadyEmpty,
}
