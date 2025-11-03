using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Trees;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Native;

[Interface, Guid("a998ec87-868d-4320-a30a-638c291f5562")]
public unsafe partial struct IStub
{
    public partial void Some(
        NodeType a,
        RootData* b,
        NString* c 
    );
}
