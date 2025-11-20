using System.Runtime.CompilerServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.Texts;

[Dropping]
public sealed unsafe partial class FontFallback
{
    #region Fields

    [Drop]
    internal Rc<IFontFallback> m_inner;

    #endregion

    #region Props

    public ref readonly Rc<IFontFallback> Inner => ref m_inner;

    #endregion

    #region Ctor

    internal FontFallback(IFontFallback* inner)
    {
        m_inner = new(inner);
    }

    #endregion

    #region Builder

    public static FontFallback Create(params ReadOnlySpan<string> FamilyNames)
        => Create(false, FamilyNames);

    public static FontFallback Create(bool DisableSystemFallback = false, params ReadOnlySpan<string> FamilyNames)
    {
        var builder = CreateBuilder();
        foreach (var name in FamilyNames)
        {
            builder.Add(name);
        }
        return builder.Build();
    }

    public static Builder CreateBuilder(bool DisableSystemFallback = false) => new(DisableSystemFallback);

    [Dropping]
    public sealed partial class Builder
    {
        #region Fields

        [Drop]
        internal Rc<IFontFallbackBuilder> m_inner;

        #endregion

        #region Props

        public ref readonly Rc<IFontFallbackBuilder> Inner => ref m_inner;

        #endregion

        #region Ctor

        internal Builder(bool DisableSystemFallback)
        {
            var lib = NativeLib.Instance;
            IFontFallbackBuilder* ptr;
            FontFallbackBuilderCreateInfo info = new()
            {
                DisableSystemFallback = DisableSystemFallback
            };
            lib.m_lib.CreateFontFallbackBuilder(&ptr, &info).TryThrowWithMsg();
            m_inner = new(ptr);
        }

        #endregion

        #region Build

        public FontFallback Build()
        {
            IFontFallback* ptr;
            m_inner.Build(&ptr).TryThrowWithMsg();
            return new(ptr);
        }

        #endregion

        #region Add

        public Builder Add(string FontFamilyName) => Add(FontFamilyName, out _);
        public Builder Add(string FontFamilyName, out bool exists)
        {
            Unsafe.SkipInit(out exists);
            fixed (bool* p_exists = &exists)
            fixed (char* p_family_name = FontFamilyName)
            {
                m_inner.Add(p_family_name, FontFamilyName.Length, p_exists).TryThrowWithMsg();
            }
            return this;
        }

        public Builder Add(LocaleId Locale, string FontFamilyName) => Add(Locale, FontFamilyName, out _);
        public Builder Add(LocaleId Locale, string FontFamilyName, out bool exists)
        {
            Unsafe.SkipInit(out exists);
            fixed (bool* p_exists = &exists)
            fixed (char* p_family_name = FontFamilyName)
            {
                m_inner.AddLocaled(Locale.Name, p_family_name, FontFamilyName.Length, p_exists).TryThrowWithMsg();
            }
            return this;
        }

        #endregion
    }

    #endregion
}
