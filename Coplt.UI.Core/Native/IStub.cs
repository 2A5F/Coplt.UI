using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

[Interface, Guid("a998ec87-868d-4320-a30a-638c291f5562")]
public unsafe partial struct IStub
{
    public partial void Some(LayoutData* layout);
}
