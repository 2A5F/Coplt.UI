using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Texts;

namespace Coplt.UI.Native;

[Interface, Guid("09c443bc-9736-4aac-8117-6890555005ff")]
public unsafe partial struct IFontFace
{
    public partial void SetManagedHandle(void* Handle, delegate* unmanaged[Cdecl]<void*, void> OnDrop);
    public partial void* GetManagedHandle();

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

    public FontFace? Manager
    {
        get
        {
            var handle = GetManagedHandle();
            if (handle == null) return null;
            var gc_handle = GCHandle.FromIntPtr((nint)handle);
            return Unsafe.As<FontFace?>(gc_handle.Target);
        }
    }
}

public static unsafe partial class IFontFaceExtensions
{
    extension(Rc<IFontFace> manager)
    {
        public FontFace? Manager => manager.Handle->Manager;
    }
}
