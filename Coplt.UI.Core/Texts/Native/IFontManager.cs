using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

[Interface, Guid("15a9651e-4fa2-48f3-9291-df0f9681a7d1")]
public unsafe partial struct IFontManager
{
    public readonly partial ulong SetAssocUpdate(
        void* Data,
        delegate* unmanaged[Cdecl]<void*, IFontFace*, ulong, void> OnAdd,
        delegate* unmanaged[Cdecl]<void*, IFontFace*, ulong, void> OnExpired
    );
    public readonly partial void* RemoveAssocUpdate(ulong AssocUpdateId);

    /// <summary>
    /// Sets the number of frames after which the font expires if no font is used.
    /// </summary>
    public readonly partial void SetExpiredFrame(ulong FrameCount);
    /// <summary>
    /// Get current frame
    /// </summary>
    /// <returns></returns>
    public readonly partial ulong GetCurrentFrame();
    /// <summary>
    /// Update frames. This will release all font faces that have exceeded their expiration frames.
    /// </summary>
    public readonly partial void Update();
    /// <summary>
    /// This will cause the last use of the font face to be updated to the current frame.
    /// </summary>
    public readonly partial ulong FontFaceToId(IFontFace* Face);
    /// <summary>
    /// This will cause the last use of the font face to be updated to the current frame.
    /// </summary>
    /// <returns>null if not exists</returns>
    public readonly partial IFontFace* IdToFontFace(ulong Id);
}
