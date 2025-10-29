namespace Coplt.UI.Texts;

// copy from icu
public enum ScriptCode
{
    /** @STABLE ICU 2.2 */
    InvalidCode = -1,
    /** @STABLE ICU 2.2 */
    Common = 0, /* ZYYY */
    /** @STABLE ICU 2.2 */
    Inherited = 1, /* ZINH */ /* "CODE FOR INHERITED SCRIPT", FOR NON-SPACING COMBINING MARKS; ALSO QAAI */
    /** @STABLE ICU 2.2 */
    Arabic = 2, /* ARAB */
    /** @STABLE ICU 2.2 */
    Armenian = 3, /* ARMN */
    /** @STABLE ICU 2.2 */
    Bengali = 4, /* BENG */
    /** @STABLE ICU 2.2 */
    Bopomofo = 5, /* BOPO */
    /** @STABLE ICU 2.2 */
    Cherokee = 6, /* CHER */
    /** @STABLE ICU 2.2 */
    Coptic = 7, /* COPT */
    /** @STABLE ICU 2.2 */
    Cyrillic = 8, /* CYRL */
    /** @STABLE ICU 2.2 */
    Deseret = 9, /* DSRT */
    /** @STABLE ICU 2.2 */
    Devanagari = 10, /* DEVA */
    /** @STABLE ICU 2.2 */
    Ethiopic = 11, /* ETHI */
    /** @STABLE ICU 2.2 */
    Georgian = 12, /* GEOR */
    /** @STABLE ICU 2.2 */
    Gothic = 13, /* GOTH */
    /** @STABLE ICU 2.2 */
    Greek = 14, /* GREK */
    /** @STABLE ICU 2.2 */
    Gujarati = 15, /* GUJR */
    /** @STABLE ICU 2.2 */
    Gurmukhi = 16, /* GURU */
    /** @STABLE ICU 2.2 */
    Han = 17, /* HANI */
    /** @STABLE ICU 2.2 */
    Hangul = 18, /* HANG */
    /** @STABLE ICU 2.2 */
    Hebrew = 19, /* HEBR */
    /** @STABLE ICU 2.2 */
    Hiragana = 20, /* HIRA */
    /** @STABLE ICU 2.2 */
    Kannada = 21, /* KNDA */
    /** @STABLE ICU 2.2 */
    Katakana = 22, /* KANA */
    /** @STABLE ICU 2.2 */
    Khmer = 23, /* KHMR */
    /** @STABLE ICU 2.2 */
    Lao = 24, /* LAOO */
    /** @STABLE ICU 2.2 */
    Latin = 25, /* LATN */
    /** @STABLE ICU 2.2 */
    Malayalam = 26, /* MLYM */
    /** @STABLE ICU 2.2 */
    Mongolian = 27, /* MONG */
    /** @STABLE ICU 2.2 */
    Myanmar = 28, /* MYMR */
    /** @STABLE ICU 2.2 */
    Ogham = 29, /* OGAM */
    /** @STABLE ICU 2.2 */
    OldItalic = 30, /* ITAL */
    /** @STABLE ICU 2.2 */
    Oriya = 31, /* ORYA */
    /** @STABLE ICU 2.2 */
    Runic = 32, /* RUNR */
    /** @STABLE ICU 2.2 */
    Sinhala = 33, /* SINH */
    /** @STABLE ICU 2.2 */
    Syriac = 34, /* SYRC */
    /** @STABLE ICU 2.2 */
    Tamil = 35, /* TAML */
    /** @STABLE ICU 2.2 */
    Telugu = 36, /* TELU */
    /** @STABLE ICU 2.2 */
    Thaana = 37, /* THAA */
    /** @STABLE ICU 2.2 */
    Thai = 38, /* THAI */
    /** @STABLE ICU 2.2 */
    Tibetan = 39, /* TIBT */
    /** CANADIAN_ABORIGINAL SCRIPT. @STABLE ICU 2.6 */
    CanadianAboriginal = 40, /* CANS */
    /** CANADIAN_ABORIGINAL SCRIPT (ALIAS). @STABLE ICU 2.2 */
    Ucas = CanadianAboriginal,
    /** @STABLE ICU 2.2 */
    Yi = 41, /* YIII */
    /* NEW SCRIPTS IN UNICODE 3.2 */
    /** @STABLE ICU 2.2 */
    Tagalog = 42, /* TGLG */
    /** @STABLE ICU 2.2 */
    Hanunoo = 43, /* HANO */
    /** @STABLE ICU 2.2 */
    Buhid = 44, /* BUHD */
    /** @STABLE ICU 2.2 */
    Tagbanwa = 45, /* TAGB */

    /* NEW SCRIPTS IN UNICODE 4 */
    /** @STABLE ICU 2.6 */
    Braille = 46, /* BRAI */
    /** @STABLE ICU 2.6 */
    Cypriot = 47, /* CPRT */
    /** @STABLE ICU 2.6 */
    Limbu = 48, /* LIMB */
    /** @STABLE ICU 2.6 */
    LinearB = 49, /* LINB */
    /** @STABLE ICU 2.6 */
    Osmanya = 50, /* OSMA */
    /** @STABLE ICU 2.6 */
    Shavian = 51, /* SHAW */
    /** @STABLE ICU 2.6 */
    TaiLe = 52, /* TALE */
    /** @STABLE ICU 2.6 */
    Ugaritic = 53, /* UGAR */

    /** NEW SCRIPT CODE IN UNICODE 4.0.1 @STABLE ICU 3.0 */
    KatakanaOrHiragana = 54, /*HRKT */

    /* NEW SCRIPTS IN UNICODE 4.1 */
    /** @STABLE ICU 3.4 */
    Buginese = 55, /* BUGI */
    /** @STABLE ICU 3.4 */
    Glagolitic = 56, /* GLAG */
    /** @STABLE ICU 3.4 */
    Kharoshthi = 57, /* KHAR */
    /** @STABLE ICU 3.4 */
    SylotiNagri = 58, /* SYLO */
    /** @STABLE ICU 3.4 */
    NewTaiLue = 59, /* TALU */
    /** @STABLE ICU 3.4 */
    Tifinagh = 60, /* TFNG */
    /** @STABLE ICU 3.4 */
    OldPersian = 61, /* XPEO */

    /* NEW SCRIPT CODES FROM UNICODE AND ISO 15924 */
    /** @STABLE ICU 3.6 */
    Balinese = 62, /* BALI */
    /** @STABLE ICU 3.6 */
    Batak = 63, /* BATK */
    /** @STABLE ICU 3.6 */
    Blissymbols = 64, /* BLIS */
    /** @STABLE ICU 3.6 */
    Brahmi = 65, /* BRAH */
    /** @STABLE ICU 3.6 */
    Cham = 66, /* CHAM */
    /** @STABLE ICU 3.6 */
    Cirth = 67, /* CIRT */
    /** @STABLE ICU 3.6 */
    OldChurchSlavonicCyrillic = 68, /* CYRS */
    /** @STABLE ICU 3.6 */
    DemoticEgyptian = 69, /* EGYD */
    /** @STABLE ICU 3.6 */
    HieraticEgyptian = 70, /* EGYH */
    /** @STABLE ICU 3.6 */
    EgyptianHieroglyphs = 71, /* EGYP */
    /** @STABLE ICU 3.6 */
    Khutsuri = 72, /* GEOK */
    /** @STABLE ICU 3.6 */
    SimplifiedHan = 73, /* HANS */
    /** @STABLE ICU 3.6 */
    TraditionalHan = 74, /* HANT */
    /** @STABLE ICU 3.6 */
    PahawhHmong = 75, /* HMNG */
    /** @STABLE ICU 3.6 */
    OldHungarian = 76, /* HUNG */
    /** @STABLE ICU 3.6 */
    HarappanIndus = 77, /* INDS */
    /** @STABLE ICU 3.6 */
    Javanese = 78, /* JAVA */
    /** @STABLE ICU 3.6 */
    KayahLi = 79, /* KALI */
    /** @STABLE ICU 3.6 */
    LatinFraktur = 80, /* LATF */
    /** @STABLE ICU 3.6 */
    LatinGaelic = 81, /* LATG */
    /** @STABLE ICU 3.6 */
    Lepcha = 82, /* LEPC */
    /** @STABLE ICU 3.6 */
    LinearA = 83, /* LINA */
    /** @STABLE ICU 4.6 */
    Mandaic = 84, /* MAND */
    /** @STABLE ICU 3.6 */
    Mandaean = Mandaic,
    /** @STABLE ICU 3.6 */
    MayanHieroglyphs = 85, /* MAYA */
    /** @STABLE ICU 4.6 */
    MeroiticHieroglyphs = 86, /* MERO */
    /** @STABLE ICU 3.6 */
    Meroitic = MeroiticHieroglyphs,
    /** @STABLE ICU 3.6 */
    Nko = 87, /* NKOO */
    /** @STABLE ICU 3.6 */
    Orkhon = 88, /* ORKH */
    /** @STABLE ICU 3.6 */
    OldPermic = 89, /* PERM */
    /** @STABLE ICU 3.6 */
    PhagsPa = 90, /* PHAG */
    /** @STABLE ICU 3.6 */
    Phoenician = 91, /* PHNX */
    /** @STABLE ICU 52 */
    Miao = 92, /* PLRD */
    /** @STABLE ICU 3.6 */
    PhoneticPollard = Miao,
    /** @STABLE ICU 3.6 */
    Rongorongo = 93, /* RORO */
    /** @STABLE ICU 3.6 */
    Sarati = 94, /* SARA */
    /** @STABLE ICU 3.6 */
    EstrangeloSyriac = 95, /* SYRE */
    /** @STABLE ICU 3.6 */
    WesternSyriac = 96, /* SYRJ */
    /** @STABLE ICU 3.6 */
    EasternSyriac = 97, /* SYRN */
    /** @STABLE ICU 3.6 */
    Tengwar = 98, /* TENG */
    /** @STABLE ICU 3.6 */
    Vai = 99, /* VAII */
    /** @STABLE ICU 3.6 */
    VisibleSpeech = 100, /* VISP */
    /** @STABLE ICU 3.6 */
    Cuneiform = 101, /* XSUX */
    /** @STABLE ICU 3.6 */
    UnwrittenLanguages = 102, /* ZXXX */
    /** @STABLE ICU 3.6 */
    Unknown = 103, /* ZZZZ */ /* unknown="CODE FOR UNCODED SCRIPT", FOR UNASSIGNED CODE POINTS */

    /** @STABLE ICU 3.8 */
    Carian = 104, /* CARI */
    /** @STABLE ICU 3.8 */
    Japanese = 105, /* JPAN */
    /** @STABLE ICU 3.8 */
    Lanna = 106, /* LANA */
    /** @STABLE ICU 3.8 */
    Lycian = 107, /* LYCI */
    /** @STABLE ICU 3.8 */
    Lydian = 108, /* LYDI */
    /** @STABLE ICU 3.8 */
    OlChiki = 109, /* OLCK */
    /** @STABLE ICU 3.8 */
    Rejang = 110, /* RJNG */
    /** @STABLE ICU 3.8 */
    Saurashtra = 111, /* SAUR */
    /** SUTTON SIGNWRITING @STABLE ICU 3.8 */
    SignWriting = 112, /* SGNW */
    /** @STABLE ICU 3.8 */
    Sundanese = 113, /* SUND */
    /** @STABLE ICU 3.8 */
    Moon = 114, /* MOON */
    /** @STABLE ICU 3.8 */
    MeiteiMayek = 115, /* MTEI */

    /** @STABLE ICU 4.0 */
    ImperialAramaic = 116, /* ARMI */
    /** @STABLE ICU 4.0 */
    Avestan = 117, /* AVST */
    /** @STABLE ICU 4.0 */
    Chakma = 118, /* CAKM */
    /** @STABLE ICU 4.0 */
    Korean = 119, /* KORE */
    /** @STABLE ICU 4.0 */
    Kaithi = 120, /* KTHI */
    /** @STABLE ICU 4.0 */
    Manichaean = 121, /* MANI */
    /** @STABLE ICU 4.0 */
    InscriptionalPahlavi = 122, /* PHLI */
    /** @STABLE ICU 4.0 */
    PsalterPahlavi = 123, /* PHLP */
    /** @STABLE ICU 4.0 */
    BookPahlavi = 124, /* PHLV */
    /** @STABLE ICU 4.0 */
    InscriptionalParthian = 125, /* PRTI */
    /** @STABLE ICU 4.0 */
    Samaritan = 126, /* SAMR */
    /** @STABLE ICU 4.0 */
    TaiViet = 127, /* TAVT */
    /** @STABLE ICU 4.0 */
    MathematicalNotation = 128, /* ZMTH */
    /** @STABLE ICU 4.0 */
    Symbols = 129, /* ZSYM */

    /** @STABLE ICU 4.4 */
    Bamum = 130, /* BAMU */
    /** @STABLE ICU 4.4 */
    Lisu = 131, /* LISU */
    /** @STABLE ICU 4.4 */
    NakhiGeba = 132, /* NKGB */
    /** @STABLE ICU 4.4 */
    OldSouthArabian = 133, /* SARB */

    /** @STABLE ICU 4.6 */
    BassaVah = 134, /* BASS */
    /** @STABLE ICU 54 */
    Duployan = 135, /* DUPL */
    /** @STABLE ICU 4.6 */
    Elbasan = 136, /* ELBA */
    /** @STABLE ICU 4.6 */
    Grantha = 137, /* GRAN */
    /** @STABLE ICU 4.6 */
    Kpelle = 138, /* KPEL */
    /** @STABLE ICU 4.6 */
    Loma = 139, /* LOMA */
    /** MENDE KIKAKUI @STABLE ICU 4.6 */
    Mende = 140, /* MEND */
    /** @STABLE ICU 4.6 */
    MeroiticCursive = 141, /* MERC */
    /** @STABLE ICU 4.6 */
    OldNorthArabian = 142, /* NARB */
    /** @STABLE ICU 4.6 */
    Nabataean = 143, /* NBAT */
    /** @STABLE ICU 4.6 */
    Palmyrene = 144, /* PALM */
    /** @STABLE ICU 54 */
    Khudawadi = 145, /* SIND */
    /** @STABLE ICU 4.6 */
    Sindhi = Khudawadi,
    /** @STABLE ICU 4.6 */
    WarangCiti = 146, /* WARA */

    /** @STABLE ICU 4.8 */
    Afaka = 147, /* AFAK */
    /** @STABLE ICU 4.8 */
    Jurchen = 148, /* JURC */
    /** @STABLE ICU 4.8 */
    Mro = 149, /* MROO */
    /** @STABLE ICU 4.8 */
    Nushu = 150, /* NSHU */
    /** @STABLE ICU 4.8 */
    Sharada = 151, /* SHRD */
    /** @STABLE ICU 4.8 */
    SoraSompeng = 152, /* SORA */
    /** @STABLE ICU 4.8 */
    Takri = 153, /* TAKR */
    /** @STABLE ICU 4.8 */
    Tangut = 154, /* TANG */
    /** @STABLE ICU 4.8 */
    Woleai = 155, /* WOLE */

    /** @STABLE ICU 49 */
    AnatolianHieroglyphs = 156, /* HLUW */
    /** @STABLE ICU 49 */
    Khojki = 157, /* KHOJ */
    /** @STABLE ICU 49 */
    Tirhuta = 158, /* TIRH */

    /** @STABLE ICU 52 */
    CaucasianAlbanian = 159, /* AGHB */
    /** @STABLE ICU 52 */
    Mahajani = 160, /* MAHJ */

    /** @STABLE ICU 54 */
    Ahom = 161, /* AHOM */
    /** @STABLE ICU 54 */
    Hatran = 162, /* HATR */
    /** @STABLE ICU 54 */
    Modi = 163, /* MODI */
    /** @STABLE ICU 54 */
    Multani = 164, /* MULT */
    /** @STABLE ICU 54 */
    PauCinHau = 165, /* PAUC */
    /** @STABLE ICU 54 */
    Siddham = 166, /* SIDD */

    /** @STABLE ICU 58 */
    Adlam = 167, /* ADLM */
    /** @STABLE ICU 58 */
    Bhaiksuki = 168, /* BHKS */
    /** @STABLE ICU 58 */
    Marchen = 169, /* MARC */
    /** @STABLE ICU 58 */
    Newa = 170, /* NEWA */
    /** @STABLE ICU 58 */
    Osage = 171, /* OSGE */

    /** @STABLE ICU 58 */
    HanWithBopomofo = 172, /* HANB */
    /** @STABLE ICU 58 */
    Jamo = 173, /* JAMO */
    /** @STABLE ICU 58 */
    SymbolsEmoji = 174, /* ZSYE */

    /** @STABLE ICU 60 */
    MasaramGondi = 175, /* GONM */
    /** @STABLE ICU 60 */
    Soyombo = 176, /* SOYO */
    /** @STABLE ICU 60 */
    ZanabazarSquare = 177, /* ZANB */

    /** @STABLE ICU 62 */
    Dogra = 178, /* DOGR */
    /** @STABLE ICU 62 */
    GunjalaGondi = 179, /* GONG */
    /** @STABLE ICU 62 */
    Makasar = 180, /* MAKA */
    /** @STABLE ICU 62 */
    Medefaidrin = 181, /* MEDF */
    /** @STABLE ICU 62 */
    HanifiRohingya = 182, /* ROHG */
    /** @STABLE ICU 62 */
    Sogdian = 183, /* SOGD */
    /** @STABLE ICU 62 */
    OldSogdian = 184, /* SOGO */

    /** @STABLE ICU 64 */
    Elymaic = 185, /* ELYM */
    /** @STABLE ICU 64 */
    NyiakengPuachueHmong = 186, /* HMNP */
    /** @STABLE ICU 64 */
    Nandinagari = 187, /* NAND */
    /** @STABLE ICU 64 */
    Wancho = 188, /* WCHO */

    /** @STABLE ICU 66 */
    Chorasmian = 189, /* CHRS */
    /** @STABLE ICU 66 */
    DivesAkuru = 190, /* DIAK */
    /** @STABLE ICU 66 */
    KhitanSmallScript = 191, /* KITS */
    /** @STABLE ICU 66 */
    Yezidi = 192, /* YEZI */

    /** @STABLE ICU 70 */
    CyproMinoan = 193, /* CPMN */
    /** @STABLE ICU 70 */
    OldUyghur = 194, /* OUGR */
    /** @STABLE ICU 70 */
    Tangsa = 195, /* TNSA */
    /** @STABLE ICU 70 */
    Toto = 196, /* TOTO */
    /** @STABLE ICU 70 */
    Vithkuqi = 197, /* VITH */

    /** @STABLE ICU 72 */
    Kawi = 198, /* KAWI */
    /** @STABLE ICU 72 */
    NagMundari = 199, /* NAGM */

    /** @STABLE ICU 75 */
    ArabicNastaliq = 200, /* ARAN */

    /** @STABLE ICU 76 */
    Garay = 201, /* GARA */
    /** @STABLE ICU 76 */
    GurungKhema = 202, /* GUKH */
    /** @STABLE ICU 76 */
    KiratRai = 203, /* KRAI */
    /** @STABLE ICU 76 */
    OlOnal = 204, /* ONAO */
    /** @STABLE ICU 76 */
    Sunuwar = 205, /* SUNU */
    /** @STABLE ICU 76 */
    Todhri = 206, /* TODR */
    /** @STABLE ICU 76 */
    TuluTigalari = 207, /* TUTG */

    /** @STABLE ICU 78 */
    BeriaErfe = 208, /* BERF */
    /** @STABLE ICU 78 */
    Sidetic = 209, /* SIDT */
    /** @STABLE ICU 78 */
    TaiYo = 210, /* TAYO */
    /** @STABLE ICU 78 */
    TolongSiki = 211, /* TOLS */
    /** @STABLE ICU 78 */
    TraditionalHanWithLatin = 212, /* HNTL */
}
