using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Miscellaneous;

namespace Coplt.UI.Native;

[Interface, Guid("09c443bc-9736-4aac-8117-6890555005ff")]
public unsafe partial struct IFontFace
{
    public readonly partial ulong Id { get; }
    public readonly partial uint RefCount { get; }
    [ComType<ConstPtr<FrameTime>>]
    public readonly partial FrameTime* FrameTime { get; }

    /// <returns>AddRef will be called</returns>
    public readonly partial IFrameSource* GetFrameSource();
    /// <returns>It may be null, AddRef will be called</returns>
    public readonly partial IFontManager* GetFontManager();

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
