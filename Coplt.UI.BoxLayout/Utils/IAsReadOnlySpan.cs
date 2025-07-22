using System;

namespace Coplt.UI.BoxLayout.Utilities;

public interface IAsReadOnlySpan<T>
{
    public ReadOnlySpan<T> AsReadOnlySpan { get; }
}
