using System.Runtime.InteropServices;

namespace Coplt.UI.Events;

[StructLayout(LayoutKind.Auto)]
public record struct EventConfig()
{
    public bool Bubbles { get; set; } = true;
    public bool Cancelable { get; set; } = true;
}
