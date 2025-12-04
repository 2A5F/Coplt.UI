using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Collections;
using Coplt.UI.Core.Geometry;
using Coplt.UI.Core.Geometry.Native;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Texts;

namespace Coplt.UI.Native;

[Interface, Guid("778be1fe-18f2-4aa5-8d1f-52d83b132cff")]
public unsafe partial struct ILib
{
    public partial void SetLogger(
        void* obj,
        delegate* unmanaged[Cdecl]<void*, LogLevel, StrKind, int, void*, void> logger,
        delegate* unmanaged[Cdecl]<void*, LogLevel, byte> is_enabled,
        delegate* unmanaged[Cdecl]<void*, void> drop
    );
    public partial void ClearLogger();

    public partial Str8 GetCurrentErrorMessage();

    public partial HResult CreateAtlasAllocator(AtlasAllocatorType Type, int Width, int Height, IAtlasAllocator** aa);

    public partial HResult CreateFrameSource(IFrameSource** fs);
    public partial HResult CreateFontManager(IFrameSource* fs, IFontManager** fm);

    public partial HResult GetSystemFontCollection(IFontCollection** fc);
    public partial HResult GetSystemFontFallback(IFontFallback** ff);

    public partial HResult CreateFontFallbackBuilder(
        IFontFallbackBuilder** ffb,
        [ComType<ConstPtr<FontFallbackBuilderCreateInfo>>]
        FontFallbackBuilderCreateInfo* info
    );

    public partial HResult CreateLayout(ILayout** layout);

    public partial HResult SplitTexts(NativeList<TextRange>* ranges, [ComType<ConstPtr<char>>] char* chars, int len);
}
