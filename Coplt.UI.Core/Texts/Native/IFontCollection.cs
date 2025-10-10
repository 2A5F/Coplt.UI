using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

[Interface, Guid("e56d9271-e6fd-4def-b03a-570380e0d560")]
public unsafe partial struct IFontCollection
{
    [return: ComType<ConstPtr<Ptr<IFontFamily>>>]
    public readonly partial IFontFamily** GetFamilies([Out] uint* count);
    
    public partial void ClearNativeFamiliesCache();

    public partial uint FindDefaultFamily();
}
