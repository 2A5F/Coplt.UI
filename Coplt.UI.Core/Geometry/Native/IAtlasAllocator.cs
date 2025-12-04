using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Core.Geometry.Native;

[Interface, Guid("32b30623-411e-4fd5-a009-ae7e9ed88e78")]
public unsafe partial struct IAtlasAllocator
{
    public partial void Clear();
    public partial bool IsEmpty { get; }
    public partial void GetSize(int* out_width, int* out_height);
    public partial bool Allocate(int width, int height, uint* out_id, AABB2DI* out_rect);
    public partial void Deallocate(uint id);
}
