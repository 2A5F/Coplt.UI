using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public unsafe partial struct TextData
{
    [Drop]
    internal OpaqueObject m_native_data;
    [Drop]
    public NativeList<TextData_FontRange> m_font_ranges;
    // todo
}

[Dropping]
public unsafe partial struct TextData_FontRange
{
    public uint Start;
    public uint Length;
    [ComType<Ptr<IFontFace>>]
    public Rc<IFontFace> m_font_face;
}
