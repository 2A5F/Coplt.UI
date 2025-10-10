using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

public unsafe struct NFontPair
{
    public IFont* Font;
    public NFontInfo* Info;
}

[Interface, Guid("f8009d34-9417-4b87-b23b-b7885d27aeab")]
public unsafe partial struct IFontFamily
{
    [return: ComType<ConstPtr<Str16>>]
    public readonly partial Str16* GetLocalNames([Out] uint* length);
    
    [return: ComType<ConstPtr<FontFamilyNameInfo>>]
    public readonly partial FontFamilyNameInfo* GetNames([Out] uint* length);

    public partial void ClearNativeNamesCache();

    [return: ComType<ConstPtr<NFontPair>>]
    public partial NFontPair* GetFonts([Out] uint* length);
    public partial void ClearNativeFontsCache();
}

public struct FontFamilyNameInfo
{
    public Str16 Name;
    public uint Local;
}
