using System;

namespace Coplt.UI.BoxLayouts.Utilities;

public interface IAsReadOnlySpan<T>
{
    public ReadOnlySpan<T> AsReadOnlySpan { get; }
}
