using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Miscellaneous;

[Interface, Guid("92a81f7e-98b1-4c83-b6ac-161fca9469d6")]
public unsafe partial struct IFrameSource
{
    public partial void Get(FrameTime* ft);
    public partial void Set([ComType<ConstPtr<FrameTime>>] FrameTime* ft);
}
