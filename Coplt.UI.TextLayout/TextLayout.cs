using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Layouts.Native;

namespace Coplt.UI.TextLayout;

[Dropping]
public sealed unsafe partial class TextLayout
{
    #region Fields

    [Drop]
    internal Rc<ILibTextLayout> m_lib;

    #endregion

    #region Properties

    public ref readonly Rc<ILibTextLayout> Lib => ref m_lib;

    #endregion

    #region Ctor

    private TextLayout()
    {
        [DllImport("Coplt.UI.TextLayout.Native")]
        static extern ILibTextLayout* Coplt_CreateLibTextLayout();

        m_lib = new(Coplt_CreateLibTextLayout());
        if (!m_lib) throw new Exception("Failed to create LibTextLayout");
    }

    #endregion

    #region Instance

    public static TextLayout Instance { get; } = new();

    #endregion

    #region CurrentErrorMessage

    public string CurrentErrorMessage
    {
        get
        {
            var str = m_lib.GetCurrentErrorMessage();
            return str.ToString();
        }
    }

    #endregion

    #region SystemFontCollection

    [Drop(Order = -1)]
    private FontCollection? m_system_font_collection;

    private FontCollection GetSystemFontCollection()
    {
        IFontCollection* fc;
        m_lib.GetSystemFontCollection(&fc).TryThrowWithMsg(this);
        return new(new(fc));
    }

    public FontCollection SystemFontCollection => m_system_font_collection ??= GetSystemFontCollection();

    #endregion
}
