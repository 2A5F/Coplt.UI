using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.Texts;

[Dropping]
public sealed unsafe partial class Font
{
    #region Fields

    internal readonly FontFamily m_family;
    [Drop]
    internal Rc<IFont> m_inner;
    internal NFontInfo* m_info;
    internal readonly int m_index;

    internal FontFace? m_face;

    #endregion

    #region Properties

    public ref readonly Rc<IFont> Inner => ref m_inner;
    public ref readonly FontMetrics Metrics => ref m_info->Metrics;
    public FontFamily Family => m_family;
    public int Index => m_index;
    public FontWidth Width => m_info->Width;
    public FontWeight Weight => m_info->Weight;
    public FontStyle Style => m_info->Style;

    public FontFlags Flags => m_info->Flags;

    public bool IsColor => (m_info->Flags & FontFlags.Color) != 0;
    public bool IsMonospaced => (m_info->Flags & FontFlags.Monospaced) != 0;

    #endregion

    #region Drop

    [Drop]
    private void ClearInfo()
    {
        m_info = null;
    }

    #endregion

    #region Ctor

    internal Font(Rc<IFont> inner, NFontInfo* info, FontFamily family, int index)
    {
        m_family = family;
        m_inner = inner;
        m_info = info;
        m_index = index;
    }

    #endregion

    #region ToString

    public override string ToString() =>
        $"Font({m_family.LocalName}, {m_index}) {{ Width = {Width}, Weight = {Weight}, Style = {Style}, Flags = {Flags} }}";

    #endregion

    #region Face

    private FontFace CreateFace()
    {
        IFontFace* face;
        m_inner.CreateFace(&face).TryThrowWithMsg();
        return new(new(face), this);
    }
    
    public FontFace GetFace() => 
        m_face ?? Interlocked.CompareExchange(ref m_face, m_face ?? CreateFace(), null) ?? m_face;

    #endregion
}
