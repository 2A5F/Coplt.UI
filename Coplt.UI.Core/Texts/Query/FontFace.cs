using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.Texts;

[Dropping]
public sealed unsafe partial class FontFace
{
    #region Fields

    internal readonly Font m_font;
    [Drop]
    internal Rc<IFontFace> m_inner;

    #endregion

    #region Properties

    public ref readonly Rc<IFontFace> Inner => ref m_inner;
    public Font Font => m_font;
    public ref readonly FontMetrics Metrics => ref m_font.Metrics;
    public FontFamily Family => m_font.Family;
    public int Index => m_font.m_index;
    public FontWidth Width => m_font.Width;
    public FontWeight Weight => m_font.Weight;
    public FontStyle Style => m_font.Style;

    public FontFlags Flags => m_font.Flags;

    public bool IsColor => m_font.IsColor;
    public bool IsMonospaced => m_font.IsMonospaced;

    #endregion

    #region Ctor

    internal FontFace(Rc<IFontFace> inner, Font font)
    {
        m_inner = inner;
        m_font = font;
    }

    #endregion

    #region ToString

    public override string ToString() =>
        $"FontFace({m_font.m_family.LocalName}, {m_font.m_index}) {{ Width = {Width}, Weight = {Weight}, Style = {Style}, Flags = {Flags} }}";

    #endregion
}
