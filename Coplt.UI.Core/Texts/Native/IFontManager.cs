using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Miscellaneous;

namespace Coplt.UI.Native;

[Interface(typeof(IWeak)), Guid("15a9651e-4fa2-48f3-9291-df0f9681a7d1")]
public unsafe partial struct IFontManager
{
    public partial ulong SetAssocUpdate(
        void* Data,
        delegate* unmanaged[Cdecl]<void*, void> OnDrop,
        delegate* unmanaged[Cdecl]<void*, IFontFace*, ulong, void> OnAdd,
        delegate* unmanaged[Cdecl]<void*, IFontFace*, ulong, void> OnExpired
    );
    public partial void RemoveAssocUpdate(ulong AssocUpdateId);

    /// <returns>AddRef will be called</returns>
    public partial IFrameSource* GetFrameSource();

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
    /// Collecting expired fonts that are no longer in use. Can be executed concurrently in the background
    /// </summary>
    public partial void Collect();

    public partial void Add(IFontFace* Face);
    /// <returns>AddRef will be called</returns>
    public partial IFontFace* GetOrAdd(ulong Id, void* Data, delegate* unmanaged[Cdecl]<void*, ulong, IFontFace*> OnAdd);
    /// <returns>null if not exists; AddRef will be called</returns>
    public partial IFontFace* Get(ulong Id);
}
