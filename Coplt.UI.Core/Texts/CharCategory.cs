using System.Globalization;

namespace Coplt.UI.Texts;

// copy from unicode
public enum CharCategory : byte
{
    /** Non-category for unassigned and non-character code points. @stable ICU 2.0 */
    Unassigned = 0,
    /** Cn "Other, Not Assigned (no characters in [UnicodeData.txt] have this property)" (same as UNASSIGNED!) @stable ICU 2.0 */
    GeneralOtherTypes = 0,
    /** Lu @stable ICU 2.0 */
    UppercaseLetter = 1,
    /** Ll @stable ICU 2.0 */
    LowercaseLetter = 2,
    /** Lt @stable ICU 2.0 */
    TitlecaseLetter = 3,
    /** Lm @stable ICU 2.0 */
    ModifierLetter = 4,
    /** Lo @stable ICU 2.0 */
    OtherLetter = 5,
    /** Mn @stable ICU 2.0 */
    NonSpacingMark = 6,
    /** Me @stable ICU 2.0 */
    EnclosingMark = 7,
    /** Mc @stable ICU 2.0 */
    CombiningSpacingMark = 8,
    /** Nd @stable ICU 2.0 */
    DecimalDigitNumber = 9,
    /** Nl @stable ICU 2.0 */
    LetterNumber = 10,
    /** No @stable ICU 2.0 */
    OtherNumber = 11,
    /** Zs @stable ICU 2.0 */
    SpaceSeparator = 12,
    /** Zl @stable ICU 2.0 */
    LineSeparator = 13,
    /** Zp @stable ICU 2.0 */
    ParagraphSeparator = 14,
    /** Cc @stable ICU 2.0 */
    ControlChar = 15,
    /** Cf @stable ICU 2.0 */
    FormatChar = 16,
    /** Co @stable ICU 2.0 */
    PrivateUseChar = 17,
    /** Cs @stable ICU 2.0 */
    Surrogate = 18,
    /** Pd @stable ICU 2.0 */
    DashPunctuation = 19,
    /** Ps @stable ICU 2.0 */
    StartPunctuation = 20,
    /** Pe @stable ICU 2.0 */
    EndPunctuation = 21,
    /** Pc @stable ICU 2.0 */
    ConnectorPunctuation = 22,
    /** Po @stable ICU 2.0 */
    OtherPunctuation = 23,
    /** Sm @stable ICU 2.0 */
    MathSymbol = 24,
    /** Sc @stable ICU 2.0 */
    CurrencySymbol = 25,
    /** Sk @stable ICU 2.0 */
    ModifierSymbol = 26,
    /** So @stable ICU 2.0 */
    OtherSymbol = 27,
    /** Pi @stable ICU 2.0 */
    InitialPunctuation = 28,
    /** Pf @stable ICU 2.0 */
    FinalPunctuation = 29,
}

public static class CharCategoryExtensions
{
    extension(CharCategory value)
    {
        public UnicodeCategory ToUnicodeCategory => value switch
        {
            CharCategory.UppercaseLetter => UnicodeCategory.UppercaseLetter,
            CharCategory.LowercaseLetter => UnicodeCategory.LowercaseLetter,
            CharCategory.TitlecaseLetter => UnicodeCategory.TitlecaseLetter,
            CharCategory.ModifierLetter => UnicodeCategory.ModifierLetter,
            CharCategory.OtherLetter => UnicodeCategory.OtherLetter,
            CharCategory.NonSpacingMark => UnicodeCategory.NonSpacingMark,
            CharCategory.EnclosingMark => UnicodeCategory.EnclosingMark,
            CharCategory.CombiningSpacingMark => UnicodeCategory.SpacingCombiningMark,
            CharCategory.DecimalDigitNumber => UnicodeCategory.DecimalDigitNumber,
            CharCategory.LetterNumber => UnicodeCategory.LetterNumber,
            CharCategory.OtherNumber => UnicodeCategory.OtherNumber,
            CharCategory.SpaceSeparator => UnicodeCategory.SpaceSeparator,
            CharCategory.LineSeparator => UnicodeCategory.LineSeparator,
            CharCategory.ParagraphSeparator => UnicodeCategory.ParagraphSeparator,
            CharCategory.ControlChar => UnicodeCategory.Control,
            CharCategory.FormatChar => UnicodeCategory.Format,
            CharCategory.PrivateUseChar => UnicodeCategory.PrivateUse,
            CharCategory.Surrogate => UnicodeCategory.Surrogate,
            CharCategory.DashPunctuation => UnicodeCategory.DashPunctuation,
            CharCategory.StartPunctuation => UnicodeCategory.OpenPunctuation,
            CharCategory.EndPunctuation => UnicodeCategory.ClosePunctuation,
            CharCategory.ConnectorPunctuation => UnicodeCategory.ConnectorPunctuation,
            CharCategory.OtherPunctuation => UnicodeCategory.OtherPunctuation,
            CharCategory.MathSymbol => UnicodeCategory.MathSymbol,
            CharCategory.CurrencySymbol => UnicodeCategory.CurrencySymbol,
            CharCategory.ModifierSymbol => UnicodeCategory.ModifierSymbol,
            CharCategory.OtherSymbol => UnicodeCategory.OtherSymbol,
            CharCategory.InitialPunctuation => UnicodeCategory.InitialQuotePunctuation,
            CharCategory.FinalPunctuation => UnicodeCategory.FinalQuotePunctuation,
            _ => UnicodeCategory.OtherNotAssigned,
        };
    }

    extension(UnicodeCategory value)
    {
        public CharCategory ToCharCategory => value switch
        {
            UnicodeCategory.UppercaseLetter => CharCategory.UppercaseLetter,
            UnicodeCategory.LowercaseLetter => CharCategory.LowercaseLetter,
            UnicodeCategory.TitlecaseLetter => CharCategory.TitlecaseLetter,
            UnicodeCategory.ModifierLetter => CharCategory.ModifierLetter,
            UnicodeCategory.OtherLetter => CharCategory.OtherLetter,
            UnicodeCategory.NonSpacingMark => CharCategory.NonSpacingMark,
            UnicodeCategory.SpacingCombiningMark => CharCategory.CombiningSpacingMark,
            UnicodeCategory.EnclosingMark => CharCategory.EnclosingMark,
            UnicodeCategory.DecimalDigitNumber => CharCategory.DecimalDigitNumber,
            UnicodeCategory.LetterNumber => CharCategory.LetterNumber,
            UnicodeCategory.OtherNumber => CharCategory.OtherNumber,
            UnicodeCategory.SpaceSeparator => CharCategory.SpaceSeparator,
            UnicodeCategory.LineSeparator => CharCategory.LineSeparator,
            UnicodeCategory.ParagraphSeparator => CharCategory.ParagraphSeparator,
            UnicodeCategory.Control => CharCategory.ControlChar,
            UnicodeCategory.Format => CharCategory.FormatChar,
            UnicodeCategory.Surrogate => CharCategory.Surrogate,
            UnicodeCategory.PrivateUse => CharCategory.PrivateUseChar,
            UnicodeCategory.ConnectorPunctuation => CharCategory.ConnectorPunctuation,
            UnicodeCategory.DashPunctuation => CharCategory.DashPunctuation,
            UnicodeCategory.OpenPunctuation => CharCategory.StartPunctuation,
            UnicodeCategory.ClosePunctuation => CharCategory.EndPunctuation,
            UnicodeCategory.InitialQuotePunctuation => CharCategory.InitialPunctuation,
            UnicodeCategory.FinalQuotePunctuation => CharCategory.FinalPunctuation,
            UnicodeCategory.OtherPunctuation => CharCategory.OtherPunctuation,
            UnicodeCategory.MathSymbol => CharCategory.MathSymbol,
            UnicodeCategory.CurrencySymbol => CharCategory.CurrencySymbol,
            UnicodeCategory.ModifierSymbol => CharCategory.ModifierSymbol,
            UnicodeCategory.OtherSymbol => CharCategory.OtherSymbol,
            _ => CharCategory.Unassigned,
        };
    }
}
