using Coplt.Com;
using Coplt.Com.OpaqueTypes;
using Coplt.Dropping;
using Coplt.UI.Native.Collections;

namespace Coplt.UI.Native;

public unsafe partial struct OpaqueObject : IDisposable
{
    public void* Ptr;
    [ComType<Ptr<ComVoid>>]
    public delegate* unmanaged[Cdecl]<void*, void> Drop;

    public void Dispose()
    {
        if (Drop == null || Ptr == null) return;
        Drop(Ptr);
        this = default;
    }
}
