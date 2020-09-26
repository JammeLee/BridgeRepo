using System.Runtime.InteropServices;

namespace System.Globalization
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct CultureTableData
	{
		internal const int sizeofDataFields = 304;

		internal const int LOCALE_IDIGITS = 17;

		internal const int LOCALE_INEGNUMBER = 4112;

		internal const int LOCALE_ICURRDIGITS = 25;

		internal const int LOCALE_ICURRENCY = 27;

		internal const int LOCALE_INEGCURR = 28;

		internal const int LOCALE_ILZERO = 18;

		internal const int LOCALE_IFIRSTDAYOFWEEK = 4108;

		internal const int LOCALE_IFIRSTWEEKOFYEAR = 4109;

		internal const int LOCALE_ICOUNTRY = 5;

		internal const int LOCALE_IMEASURE = 13;

		internal const int LOCALE_IDIGITSUBSTITUTION = 4116;

		internal const int LOCALE_SGROUPING = 16;

		internal const int LOCALE_SMONGROUPING = 24;

		internal const int LOCALE_SLIST = 12;

		internal const int LOCALE_SDECIMAL = 14;

		internal const int LOCALE_STHOUSAND = 15;

		internal const int LOCALE_SCURRENCY = 20;

		internal const int LOCALE_SMONDECIMALSEP = 22;

		internal const int LOCALE_SMONTHOUSANDSEP = 23;

		internal const int LOCALE_SPOSITIVESIGN = 80;

		internal const int LOCALE_SNEGATIVESIGN = 81;

		internal const int LOCALE_S1159 = 40;

		internal const int LOCALE_S2359 = 41;

		internal const int LOCALE_SNATIVEDIGITS = 19;

		internal const int LOCALE_STIMEFORMAT = 4099;

		internal const int LOCALE_SSHORTDATE = 31;

		internal const int LOCALE_SLONGDATE = 32;

		internal const int LOCALE_SYEARMONTH = 4102;

		internal uint sName;

		internal uint sUnused;

		internal ushort iLanguage;

		internal ushort iParent;

		internal ushort iDigits;

		internal ushort iNegativeNumber;

		internal ushort iCurrencyDigits;

		internal ushort iCurrency;

		internal ushort iNegativeCurrency;

		internal ushort iLeadingZeros;

		internal ushort iFlags;

		internal ushort iFirstDayOfWeek;

		internal ushort iFirstWeekOfYear;

		internal ushort iCountry;

		internal ushort iMeasure;

		internal ushort iDigitSubstitution;

		internal uint waGrouping;

		internal uint waMonetaryGrouping;

		internal uint sListSeparator;

		internal uint sDecimalSeparator;

		internal uint sThousandSeparator;

		internal uint sCurrency;

		internal uint sMonetaryDecimal;

		internal uint sMonetaryThousand;

		internal uint sPositiveSign;

		internal uint sNegativeSign;

		internal uint sAM1159;

		internal uint sPM2359;

		internal uint saNativeDigits;

		internal uint saTimeFormat;

		internal uint saShortDate;

		internal uint saLongDate;

		internal uint saYearMonth;

		internal uint saDuration;

		internal ushort iDefaultLanguage;

		internal ushort iDefaultAnsiCodePage;

		internal ushort iDefaultOemCodePage;

		internal ushort iDefaultMacCodePage;

		internal ushort iDefaultEbcdicCodePage;

		internal ushort iGeoId;

		internal ushort iPaperSize;

		internal ushort iIntlCurrencyDigits;

		internal uint waCalendars;

		internal uint sAbbrevLang;

		internal uint sISO639Language;

		internal uint sEnglishLanguage;

		internal uint sNativeLanguage;

		internal uint sEnglishCountry;

		internal uint sNativeCountry;

		internal uint sAbbrevCountry;

		internal uint sISO3166CountryName;

		internal uint sIntlMonetarySymbol;

		internal uint sEnglishCurrency;

		internal uint sNativeCurrency;

		internal uint waFontSignature;

		internal uint sISO639Language2;

		internal uint sISO3166CountryName2;

		internal uint sParent;

		internal uint saDayNames;

		internal uint saAbbrevDayNames;

		internal uint saMonthNames;

		internal uint saAbbrevMonthNames;

		internal uint saMonthGenitiveNames;

		internal uint saAbbrevMonthGenitiveNames;

		internal uint saNativeCalendarNames;

		internal uint saAltSortID;

		internal ushort iNegativePercent;

		internal ushort iPositivePercent;

		internal ushort iFormatFlags;

		internal ushort iLineOrientations;

		internal ushort iTextInfo;

		internal ushort iInputLanguageHandle;

		internal uint iCompareInfo;

		internal uint sEnglishDisplayName;

		internal uint sNativeDisplayName;

		internal uint sPercent;

		internal uint sNaN;

		internal uint sPositiveInfinity;

		internal uint sNegativeInfinity;

		internal uint sMonthDay;

		internal uint sAdEra;

		internal uint sAbbrevAdEra;

		internal uint sRegionName;

		internal uint sConsoleFallbackName;

		internal uint saShortTime;

		internal uint saSuperShortDayNames;

		internal uint saDateWords;

		internal uint sSpecificCulture;

		internal uint sKeyboardsToInstall;

		internal uint sScripts;
	}
}
