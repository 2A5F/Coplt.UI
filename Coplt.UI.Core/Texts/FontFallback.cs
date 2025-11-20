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
    internal readonly string[] m_family_names;
    internal readonly bool m_has_system_fallback;

    #endregion

    #region Props

    public ref readonly Rc<IFontFallback> Inner => ref m_inner;
    public ReadOnlySpan<string> FamilyNames => m_family_names;
    public bool HasSystemFallback => m_has_system_fallback;

    #endregion

    #region Ctor

    internal FontFallback(IFontFallback* inner, string[] family_names, bool has_system_fallback)
    {
        m_inner = new(inner);
        m_family_names = family_names;
        m_has_system_fallback = has_system_fallback;
    }

    #endregion

    #region Builder

    public static FontFallback Create(params ReadOnlySpan<string> FamilyNames)
        => Create(false, FamilyNames);

    public static FontFallback Create(bool DisableSystemFallback = false, params ReadOnlySpan<string> FamilyNames)
    {
        var builder = new Builder(DisableSystemFallback);
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
        internal readonly List<string> m_family_names = new();
        internal bool m_disable_system_fallback;

        #endregion

        #region Props

        public ref readonly Rc<IFontFallbackBuilder> Inner => ref m_inner;

        #endregion

        #region Ctor

        internal Builder(bool DisableSystemFallback)
        {
            m_disable_system_fallback = DisableSystemFallback;
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
            return new(ptr, m_family_names.ToArray(), !m_disable_system_fallback);
        }

        #endregion

        #region Add

        public Builder Add(string FontFamilyName) => Add(FontFamilyName, out _);
        public Builder Add(string FontFamilyName, out bool exists)
        {
            m_family_names.Add(FontFamilyName);
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
            m_family_names.Add(FontFamilyName);
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

    #region ToString

    public override string ToString() =>
        $"[{string.Join(", ", m_family_names)}{(m_has_system_fallback ? $"{(m_family_names.Length == 0 ? "" : ", ")}..." : "")}]";

    #endregion
}
