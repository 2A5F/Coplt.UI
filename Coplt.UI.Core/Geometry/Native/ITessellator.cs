using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Core.Geometry.Native;

[Interface, Guid("acf5d52e-a656-4c00-a528-09aa4d86b2b2")]
public unsafe partial struct ITessellator
{
    public partial HResult Fill(IPath* path, TessFillOptions* options); // todo output
    public partial HResult Stroke(IPath* path, TessStrokeOptions* options); // todo output
}
