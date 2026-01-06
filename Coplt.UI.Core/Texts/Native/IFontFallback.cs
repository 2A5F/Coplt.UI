using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Styles;

namespace Coplt.UI.Native;

[Interface, Guid("b0dbb428-eca1-4784-b27f-629bddf93ea4")]
public unsafe partial struct IFontFallback { }

public struct FontFallbackBuilderCreateInfo
{
    public bool DisableSystemFallback;
}

[Interface, Guid("9b4e9893-0ea4-456b-bf54-9563db70eff0")]
public unsafe partial struct IFontFallbackBuilder
{
    public partial HResult Build(IFontFallback** ff);

    public partial HResult Add([ComType<ConstPtr<char>>] char* name, int length, bool* exists);
    public partial HResult AddLocaled(
        [ComType<ConstPtr<LocaleId>>] LocaleId* locale,
        [ComType<ConstPtr<char>>] char* name, int name_length,
        bool* exists
    );
}
