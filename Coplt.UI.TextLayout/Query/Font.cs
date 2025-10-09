using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Layouts.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.TextLayout;

[Dropping]
public sealed unsafe partial class Font
{
    #region Fields

    [Drop]
    internal Rc<IFont> m_inner;
    internal NFontInfo* m_info;

    #endregion

    #region Properties

    public ref readonly Rc<IFont> Inner => ref m_inner;
    public ref readonly FontMetrics Metrics => ref m_info->Metrics;
    public FontWidth Width => m_info->Width;
    public FontWeight Weight => m_info->Weight;
    public FontStyle Style => m_info->Style;

    #endregion

    #region Drop

    [Drop]
    private void ClearInfo()
    {
        m_info = null;
    }

    #endregion

    #region Ctor

    internal Font(Rc<IFont> inner, NFontInfo* info)
    {
        m_inner = inner;
        m_info = info;
    }

    #endregion
}
