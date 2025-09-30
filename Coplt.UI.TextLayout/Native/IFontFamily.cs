using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Layouts.Native;

[Interface, Guid("f8009d34-9417-4b87-b23b-b7885d27aeab")]
public unsafe partial struct IFontFamily
{
    [return: ComType<ConstPtr<Str16>>]
    public readonly partial Str16* GetNames([Out] uint* length);

    public partial void ClearNativeNamesCache();
}
