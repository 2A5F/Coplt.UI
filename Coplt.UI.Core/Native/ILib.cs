using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Collections;
using Coplt.UI.Core.Utilities;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Texts;

namespace Coplt.UI.Native;

[Interface, Guid("778be1fe-18f2-4aa5-8d1f-52d83b132cff")]
public unsafe partial struct ILib
{
    public partial void SetLogger(
        void* obj,
        delegate* unmanaged[Cdecl]<void*, LogLevel, int, char*, void> logger,
        delegate* unmanaged[Cdecl]<void*, void> drop
    );

    public partial Str8 GetCurrentErrorMessage();

    public partial HResult CreateFontManager(IFontManager** fm);

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
