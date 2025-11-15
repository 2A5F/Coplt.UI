using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

[Interface, Guid("09c443bc-9736-4aac-8117-6890555005ff")]
public unsafe partial struct IFontFace
{
    public readonly partial ulong Id { get; }
    
    [ComType<ConstPtr<NFontInfo>>]
    public readonly partial NFontInfo* Info { get; }

    public readonly partial bool Equals(IFontFace* other);
    public readonly partial int HashCode();

    public readonly partial HResult GetFamilyNames(
        void* ctx,
        delegate* unmanaged[Cdecl]<void* /* ctx */, char* /* lang */, int, char* /* string */, int, void> add
    );
    public readonly partial HResult GetFaceNames(
        void* ctx,
        delegate* unmanaged[Cdecl]<void* /* ctx */, char* /* lang */, int, char* /* string */, int, void> add
    );
}
