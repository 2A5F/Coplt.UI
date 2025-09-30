using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Layouts.Native;

namespace Coplt.UI.TextLayout;

[Dropping]
public sealed unsafe partial class FontFamily
{
    #region Fields

    internal Rc<IFontFamily> m_inner;
    internal readonly string[] m_names;

    #endregion

    #region Properties

    public ref readonly Rc<IFontFamily> Inner => ref m_inner;
    public ReadOnlySpan<string> Names => m_names;

    #endregion

    #region Ctor

    internal FontFamily(Rc<IFontFamily> inner)
    {
        m_inner = inner;
        {
            uint len;
            var p_names = inner.GetNames(&len);
            m_names = new string[len];
            for (var i = 0; i < len; i++)
            {
                m_names[i] = p_names[i].ToString();
            }
        }
        inner.ClearNativeNamesCache();
    }

    #endregion

    #region ToString

    public override string ToString() => $"FontFamily {{ {string.Join(", ", m_names)} }}";

    #endregion
}
