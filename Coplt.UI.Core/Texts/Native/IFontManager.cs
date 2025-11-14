using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

[Interface, Guid("15a9651e-4fa2-48f3-9291-df0f9681a7d1")]
public unsafe partial struct IFontManager
{
    public partial ulong SetAssocUpdate(
        void* Data,
        delegate* unmanaged[Cdecl]<void*, void> OnDrop,
        delegate* unmanaged[Cdecl]<void*, IFontFace*, ulong, void> OnAdd,
        delegate* unmanaged[Cdecl]<void*, IFontFace*, ulong, void> OnExpired
    );
    public partial void RemoveAssocUpdate(ulong AssocUpdateId);

    /// <summary>
    /// Sets the number of frames after which the font expires if no font is used. default is 180 frames, min is 4 frames
    /// <para>A font face will only actually expire if both frame and time requirements are met.</para>
    /// </summary>
    public partial void SetExpireFrame(ulong FrameCount);
    /// <summary>
    /// Sets the number of frames after which the font expires if no font is used. default is 3 second (30000000, use c# timespan ticks)
    /// <para>A font face will only actually expire if both frame and time requirements are met.</para>
    /// </summary>
    public partial void SetExpireTime(ulong TimeTicks);
    /// <summary>
    /// Get current frame
    /// </summary>
    /// <returns></returns>
    public readonly partial ulong GetCurrentFrame();
    /// <summary>
    /// Update frames. This will release all font faces that have exceeded their expiration frames and times.
    /// </summary>
    public partial void Update(ulong CurrentTime);
    /// <summary>
    /// This will cause the last use of the font face to be updated to the current frame.
    /// </summary>
    public partial ulong FontFaceToId(IFontFace* Face);
    /// <summary>
    /// This will cause the last use of the font face to be updated to the current frame.
    /// </summary>
    /// <returns>null if not exists</returns>
    public partial IFontFace* IdToFontFace(ulong Id);
}
