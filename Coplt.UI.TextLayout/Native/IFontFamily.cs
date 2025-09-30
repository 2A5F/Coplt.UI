using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Layouts.Native;

[Interface, Guid("f8009d34-9417-4b87-b23b-b7885d27aeab")]
public unsafe partial struct IFontFamily
{
    [return: ComType<ConstPtr<Str16>>]
    public readonly partial Str16* GetLocalNames([Out] uint* length);
    
    [return: ComType<ConstPtr<FontFamilyNameInfo>>]
    public readonly partial FontFamilyNameInfo* GetNames([Out] uint* length);

    public partial void ClearNativeNamesCache();
}

public struct FontFamilyNameInfo
{
    public Str16 Name;
    public uint Local;
}
