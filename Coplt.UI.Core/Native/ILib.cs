using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Miscellaneous;

namespace Coplt.UI.Native;

[Interface, Guid("778be1fe-18f2-4aa5-8d1f-52d83b132cff")]
public unsafe partial struct ILib
{
    public partial void SetLogger(void* obj, delegate*<LogLevel, int, char*, void> logger, delegate*<void*, void> drop);

    public partial Str8 GetCurrentErrorMessage();

    public readonly partial void* Alloc(int size, int align);
    public readonly partial void Free(void* ptr, int align);
    public readonly partial void* ZAlloc(int size, int align);
    public readonly partial void* ReAlloc(void* ptr, int size, int align);

    public partial HResult GetSystemFontCollection(IFontCollection** fc);
}
