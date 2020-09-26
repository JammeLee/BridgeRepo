using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace System.Globalization
{
	internal class CultureTableRecord
	{
		private struct CompositeCultureData
		{
			internal string sname;

			internal string englishDisplayName;

			internal string sNativeDisplayName;

			internal string waCalendars;

			internal string consoleFallbackName;

			internal string parentName;

			internal int parentLcid;
		}

		internal class AdjustedSyntheticCultureName
		{
			internal int lcid;

			internal string isoLanguage;

			internal string isoCountry;

			internal string sName;

			internal AdjustedSyntheticCultureName(int lcid, string isoLanguage, string isoCountry, string sName)
			{
				this.lcid = lcid;
				this.isoLanguage = isoLanguage;
				this.isoCountry = isoCountry;
				this.sName = sName;
			}
		}

		internal const int SPANISH_TRADITIONAL_SORT = 1034;

		private const int SPANISH_INTERNATIONAL_SORT = 3082;

		private const int MAXSIZE_LANGUAGE = 8;

		private const int MAXSIZE_REGION = 8;

		private const int MAXSIZE_SUFFIX = 64;

		private const int MAXSIZE_FULLTAGNAME = 84;

		private const int LOCALE_SLANGUAGE = 2;

		private const int LOCALE_SCOUNTRY = 6;

		private const int LOCALE_SNATIVELANGNAME = 4;

		private const int LOCALE_SNATIVECTRYNAME = 8;

		private const int LOCALE_ICALENDARTYPE = 4105;

		private const int INT32TABLE_EVERETT_REGION_DATA_ITEM_MAPPINGS = 0;

		private const int INT32TABLE_EVERETT_CULTURE_DATA_ITEM_MAPPINGS = 1;

		private const int INT32TABLE_EVERETT_DATA_ITEM_TO_LCID_MAPPINGS = 2;

		private const int INT32TABLE_EVERETT_REGION_DATA_ITEM_TO_LCID_MAPPINGS = 3;

		private static object s_InternalSyncObject;

		private static Hashtable CultureTableRecordCache;

		private static Hashtable CultureTableRecordRegionCache;

		private static Hashtable SyntheticDataCache;

		internal static Hashtable SyntheticLcidToNameCache;

		internal static Hashtable SyntheticNameToLcidCache;

		private CultureTable m_CultureTable;

		private unsafe CultureTableData* m_pData;

		private unsafe ushort* m_pPool;

		private bool m_bUseUserOverride;

		private int m_CultureID;

		private string m_CultureName;

		private int m_ActualCultureID;

		private string m_ActualName;

		private bool m_synthetic;

		private AgileSafeNativeMemoryHandle nativeMemoryHandle;

		private string m_windowsPath;

		private static AdjustedSyntheticCultureName[] s_adjustedSyntheticNames = null;

		private unsafe static int* m_EverettRegionDataItemMappings = null;

		private static int m_EverettRegionDataItemMappingsSize = 0;

		private unsafe static int* m_EverettCultureDataItemMappings = null;

		private static int m_EverettCultureDataItemMappingsSize = 0;

		private unsafe static int* m_EverettDataItemToLCIDMappings = null;

		private static int m_EverettDataItemToLCIDMappingsSize = 0;

		private unsafe static int* m_EverettRegionInfoDataItemToLCIDMappings = null;

		private static int m_EverettRegionInfoDataItemToLCIDMappingsSize = 0;

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		private static AdjustedSyntheticCultureName[] AdjustedSyntheticNames
		{
			get
			{
				if (s_adjustedSyntheticNames == null)
				{
					s_adjustedSyntheticNames = new AdjustedSyntheticCultureName[10]
					{
						new AdjustedSyntheticCultureName(5146, "bs", "BA", "bs-Latn-BA"),
						new AdjustedSyntheticCultureName(9275, "smn", "FI", "smn-FI"),
						new AdjustedSyntheticCultureName(4155, "smj", "NO", "smj-NO"),
						new AdjustedSyntheticCultureName(5179, "smj", "SE", "smj-SE"),
						new AdjustedSyntheticCultureName(8251, "sms", "FI", "sms-FI"),
						new AdjustedSyntheticCultureName(6203, "sma", "NO", "sma-NO"),
						new AdjustedSyntheticCultureName(7227, "sma", "SE", "sma-SE"),
						new AdjustedSyntheticCultureName(1131, "quz", "BO", "quz-BO"),
						new AdjustedSyntheticCultureName(2155, "quz", "EC", "quz-EC"),
						new AdjustedSyntheticCultureName(3179, "quz", "PE", "quz-PE")
					};
				}
				return s_adjustedSyntheticNames;
			}
		}

		private string WindowsPath
		{
			get
			{
				if (m_windowsPath == null)
				{
					m_windowsPath = CultureInfo.nativeGetWindowsDirectory();
				}
				return m_windowsPath;
			}
		}

		internal bool IsSynthetic => m_synthetic;

		internal bool IsCustomCulture => !m_CultureTable.fromAssembly;

		internal bool IsReplacementCulture
		{
			get
			{
				if (IsCustomCulture)
				{
					return !IsCustomCultureId(m_CultureID);
				}
				return false;
			}
		}

		internal int CultureID => m_CultureID;

		internal string CultureName
		{
			get
			{
				return m_CultureName;
			}
			set
			{
				m_CultureName = value;
			}
		}

		internal bool UseUserOverride => m_bUseUserOverride;

		internal unsafe bool UseGetLocaleInfo
		{
			get
			{
				if (!m_bUseUserOverride)
				{
					return false;
				}
				int num = default(int);
				CultureInfo.nativeGetUserDefaultLCID(&num, 1024);
				if (ActualCultureID == 4096 && num == 3072)
				{
					if (SNAME.Equals(CultureInfo.nativeGetCultureName(num, useSNameLCType: true, getMonthName: false)))
					{
						return true;
					}
					return false;
				}
				return ActualCultureID == num;
			}
		}

		internal unsafe string CultureNativeDisplayName
		{
			get
			{
				int culture = default(int);
				CultureInfo.nativeGetUserDefaultUILanguage(&culture);
				if (CultureInfo.GetLangID(culture) == CultureInfo.GetLangID(CultureInfo.CurrentUICulture.LCID))
				{
					string text = CultureInfo.nativeGetLocaleInfo(m_ActualCultureID, 2);
					if (text != null)
					{
						if (text[text.Length - 1] == '\0')
						{
							return text.Substring(0, text.Length - 1);
						}
						return text;
					}
				}
				return SNATIVEDISPLAYNAME;
			}
		}

		internal unsafe string RegionNativeDisplayName
		{
			get
			{
				int culture = default(int);
				CultureInfo.nativeGetUserDefaultUILanguage(&culture);
				if (CultureInfo.GetLangID(culture) == CultureInfo.GetLangID(CultureInfo.CurrentUICulture.LCID))
				{
					string text = CultureInfo.nativeGetLocaleInfo(m_ActualCultureID, 6);
					if (text != null)
					{
						if (text[text.Length - 1] == '\0')
						{
							return text.Substring(0, text.Length - 1);
						}
						return text;
					}
				}
				return SNATIVECOUNTRY;
			}
		}

		private int InteropLCID
		{
			get
			{
				if (ActualCultureID != 4096)
				{
					return ActualCultureID;
				}
				return 3072;
			}
		}

		internal int ActualCultureID
		{
			get
			{
				if (m_ActualCultureID == 0)
				{
					m_ActualCultureID = ILANGUAGE;
				}
				return m_ActualCultureID;
			}
		}

		internal string ActualName
		{
			get
			{
				if (m_ActualName == null)
				{
					m_ActualName = SNAME;
				}
				return m_ActualName;
			}
		}

		internal bool IsNeutralCulture => (IFLAGS & 1) == 0;

		internal unsafe ushort IDIGITS => m_pData->iDigits;

		internal unsafe ushort INEGNUMBER => m_pData->iNegativeNumber;

		internal unsafe ushort ICURRDIGITS => m_pData->iCurrencyDigits;

		internal unsafe ushort ICURRENCY => m_pData->iCurrency;

		internal unsafe ushort INEGCURR => m_pData->iNegativeCurrency;

		internal ushort ICALENDARTYPE
		{
			get
			{
				if (m_bUseUserOverride)
				{
					string text = CultureInfo.nativeGetLocaleInfo(ActualCultureID, 4105);
					if (text != null && text.Length > 0 && short.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && IsOptionalCalendar(result))
					{
						return (ushort)result;
					}
				}
				return (ushort)IOPTIONALCALENDARS[0];
			}
		}

		internal unsafe ushort IFIRSTWEEKOFYEAR => GetOverrideUSHORT(m_pData->iFirstWeekOfYear, 4109);

		internal unsafe ushort IMEASURE => GetOverrideUSHORT(m_pData->iMeasure, 13);

		internal unsafe ushort IDIGITSUBSTITUTION => GetOverrideUSHORT(m_pData->iDigitSubstitution, 4116);

		internal unsafe int[] SGROUPING => GetOverrideGrouping(m_pData->waGrouping, 16);

		internal unsafe int[] SMONGROUPING => GetOverrideGrouping(m_pData->waMonetaryGrouping, 24);

		internal unsafe string SLIST => GetOverrideString(m_pData->sListSeparator, 12);

		internal unsafe string SDECIMAL => GetString(m_pData->sDecimalSeparator);

		internal unsafe string STHOUSAND => GetString(m_pData->sThousandSeparator);

		internal unsafe string SCURRENCY => GetString(m_pData->sCurrency);

		internal unsafe string SMONDECIMALSEP => GetString(m_pData->sMonetaryDecimal);

		internal unsafe string SMONTHOUSANDSEP => GetString(m_pData->sMonetaryThousand);

		internal unsafe string SNEGATIVESIGN => GetString(m_pData->sNegativeSign);

		internal unsafe string S1159 => GetString(m_pData->sAM1159);

		internal unsafe string S2359 => GetString(m_pData->sPM2359);

		internal unsafe string STIMEFORMAT => ReescapeWin32String(GetStringArrayDefault(m_pData->saTimeFormat));

		internal unsafe string SSHORTTIME => ReescapeWin32String(GetStringArrayDefault(m_pData->saShortTime));

		internal unsafe string SSHORTDATE => ReescapeWin32String(GetStringArrayDefault(m_pData->saShortDate));

		internal unsafe string SLONGDATE => ReescapeWin32String(GetStringArrayDefault(m_pData->saLongDate));

		internal unsafe string SYEARMONTH => ReescapeWin32String(GetStringArrayDefault(m_pData->saYearMonth));

		internal unsafe string SMONTHDAY => ReescapeWin32String(GetString(m_pData->sMonthDay));

		internal unsafe string[] STIMEFORMATS => ReescapeWin32Strings(GetStringArray(m_pData->saTimeFormat));

		internal unsafe string[] SSHORTTIMES => ReescapeWin32Strings(GetStringArray(m_pData->saShortTime));

		internal unsafe string[] SSHORTDATES => ReescapeWin32Strings(GetStringArray(m_pData->saShortDate));

		internal unsafe string[] SLONGDATES => ReescapeWin32Strings(GetStringArray(m_pData->saLongDate));

		internal unsafe string[] SYEARMONTHS => ReescapeWin32Strings(GetStringArray(m_pData->saYearMonth));

		internal unsafe string[] SNATIVEDIGITS
		{
			get
			{
				string text;
				if (m_bUseUserOverride && CultureID != 3072 && (text = CultureInfo.nativeGetLocaleInfo(ActualCultureID, 19)) != null && text.Length == 10)
				{
					string[] array = new string[10];
					for (int i = 0; i < text.Length; i++)
					{
						array[i] = text[i].ToString(CultureInfo.InvariantCulture);
					}
					return array;
				}
				return GetStringArray(m_pData->saNativeDigits);
			}
		}

		internal unsafe ushort ILANGUAGE => m_pData->iLanguage;

		internal unsafe ushort IDEFAULTANSICODEPAGE => m_pData->iDefaultAnsiCodePage;

		internal unsafe ushort IDEFAULTOEMCODEPAGE => m_pData->iDefaultOemCodePage;

		internal unsafe ushort IDEFAULTMACCODEPAGE => m_pData->iDefaultMacCodePage;

		internal unsafe ushort IDEFAULTEBCDICCODEPAGE => m_pData->iDefaultEbcdicCodePage;

		internal unsafe ushort IGEOID => m_pData->iGeoId;

		internal unsafe ushort INEGATIVEPERCENT => m_pData->iNegativePercent;

		internal unsafe ushort IPOSITIVEPERCENT => m_pData->iPositivePercent;

		internal unsafe ushort IPARENT => m_pData->iParent;

		internal unsafe ushort ILINEORIENTATIONS => m_pData->iLineOrientations;

		internal unsafe uint ICOMPAREINFO => m_pData->iCompareInfo;

		internal unsafe uint IFLAGS => m_pData->iFlags;

		internal unsafe int[] IOPTIONALCALENDARS => GetWordArray(m_pData->waCalendars);

		internal unsafe string SNAME => GetString(m_pData->sName);

		internal unsafe string SABBREVLANGNAME => GetString(m_pData->sAbbrevLang);

		internal unsafe string SISO639LANGNAME => GetString(m_pData->sISO639Language);

		internal unsafe string SENGCOUNTRY => GetString(m_pData->sEnglishCountry);

		internal unsafe string SNATIVECOUNTRY => GetString(m_pData->sNativeCountry);

		internal unsafe string SABBREVCTRYNAME => GetString(m_pData->sAbbrevCountry);

		internal unsafe string SISO3166CTRYNAME => GetString(m_pData->sISO3166CountryName);

		internal unsafe string SINTLSYMBOL => GetString(m_pData->sIntlMonetarySymbol);

		internal unsafe string SENGLISHCURRENCY => GetString(m_pData->sEnglishCurrency);

		internal unsafe string SNATIVECURRENCY => GetString(m_pData->sNativeCurrency);

		internal unsafe string SENGDISPLAYNAME => GetString(m_pData->sEnglishDisplayName);

		internal unsafe string SISO639LANGNAME2 => GetString(m_pData->sISO639Language2);

		internal unsafe string SNATIVEDISPLAYNAME
		{
			get
			{
				if (CultureInfo.GetLangID(ActualCultureID) == 1028 && CultureInfo.GetLangID(CultureInfo.InstalledUICulture.LCID) == 1028 && !IsCustomCulture)
				{
					return CultureInfo.nativeGetLocaleInfo(1028, 4) + " (" + CultureInfo.nativeGetLocaleInfo(1028, 8) + ")";
				}
				return GetString(m_pData->sNativeDisplayName);
			}
		}

		internal unsafe string SPERCENT => GetString(m_pData->sPercent);

		internal unsafe string SNAN => GetString(m_pData->sNaN);

		internal unsafe string SPOSINFINITY => GetString(m_pData->sPositiveInfinity);

		internal unsafe string SNEGINFINITY => GetString(m_pData->sNegativeInfinity);

		internal unsafe string SADERA => GetString(m_pData->sAdEra);

		internal unsafe string SABBREVADERA => GetString(m_pData->sAbbrevAdEra);

		internal unsafe string SISO3166CTRYNAME2 => GetString(m_pData->sISO3166CountryName2);

		internal unsafe string SREGIONNAME => GetString(m_pData->sRegionName);

		internal unsafe string SPARENT => GetString(m_pData->sParent);

		internal unsafe string SCONSOLEFALLBACKNAME => GetString(m_pData->sConsoleFallbackName);

		internal unsafe string SSPECIFICCULTURE => GetString(m_pData->sSpecificCulture);

		internal unsafe string[] SDAYNAMES => GetStringArray(m_pData->saDayNames);

		internal unsafe string[] SABBREVDAYNAMES => GetStringArray(m_pData->saAbbrevDayNames);

		internal unsafe string[] SSUPERSHORTDAYNAMES => GetStringArray(m_pData->saSuperShortDayNames);

		internal unsafe string[] SMONTHNAMES => GetStringArray(m_pData->saMonthNames);

		internal unsafe string[] SABBREVMONTHNAMES => GetStringArray(m_pData->saAbbrevMonthNames);

		internal unsafe string[] SMONTHGENITIVENAMES => GetStringArray(m_pData->saMonthGenitiveNames);

		internal unsafe string[] SABBREVMONTHGENITIVENAMES => GetStringArray(m_pData->saAbbrevMonthGenitiveNames);

		internal unsafe string[] SNATIVECALNAMES => GetStringArray(m_pData->saNativeCalendarNames);

		internal unsafe string[] SDATEWORDS => GetStringArray(m_pData->saDateWords);

		internal unsafe string[] SALTSORTID => GetStringArray(m_pData->saAltSortID);

		internal unsafe DateTimeFormatFlags IFORMATFLAGS => (DateTimeFormatFlags)m_pData->iFormatFlags;

		internal unsafe string SPOSITIVESIGN
		{
			get
			{
				string text = GetString(m_pData->sPositiveSign);
				if (text == null || text.Length == 0)
				{
					text = "+";
				}
				return text;
			}
		}

		internal unsafe ushort IFIRSTDAYOFWEEK => m_pData->iFirstDayOfWeek;

		internal unsafe ushort IINPUTLANGUAGEHANDLE => m_pData->iInputLanguageHandle;

		internal unsafe ushort ITEXTINFO
		{
			get
			{
				ushort num = m_pData->iTextInfo;
				if (CultureID == 1034)
				{
					num = 1034;
				}
				if (num == 3072 || num == 0)
				{
					num = 127;
				}
				return num;
			}
		}

		internal unsafe string STIME
		{
			get
			{
				string overrideStringArrayDefault = GetOverrideStringArrayDefault(m_pData->saTimeFormat, 4099);
				return GetTimeSeparator(overrideStringArrayDefault);
			}
		}

		internal unsafe string SDATE
		{
			get
			{
				string overrideStringArrayDefault = GetOverrideStringArrayDefault(m_pData->saShortDate, 31);
				return GetDateSeparator(overrideStringArrayDefault);
			}
		}

		private CultureTable GetCustomCultureTable(string name)
		{
			CultureTable cultureTable = null;
			string customCultureFile = GetCustomCultureFile(name);
			if (customCultureFile == null)
			{
				return null;
			}
			try
			{
				cultureTable = new CultureTable(customCultureFile, fromAssembly: false);
				if (!cultureTable.IsValid)
				{
					int culture;
					string actualName;
					int dataItemFromCultureName = CultureTable.Default.GetDataItemFromCultureName(name, out culture, out actualName);
					if (dataItemFromCultureName < 0)
					{
						InitSyntheticMapping();
						if (SyntheticNameToLcidCache[name] == null)
						{
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_CorruptedCustomCultureFile"), name));
						}
					}
					return null;
				}
				return cultureTable;
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		internal CultureTable TryCreateReplacementCulture(string replacementCultureName, out int dataItem)
		{
			string name = ValidateCulturePieceToLower(replacementCultureName, "cultureName", 84);
			CultureTable customCultureTable = GetCustomCultureTable(name);
			if (customCultureTable == null)
			{
				dataItem = -1;
				return null;
			}
			dataItem = customCultureTable.GetDataItemFromCultureName(name, out var _, out var _);
			if (dataItem < 0)
			{
				return null;
			}
			return customCultureTable;
		}

		internal static void InitSyntheticMapping()
		{
			if (SyntheticLcidToNameCache == null || SyntheticNameToLcidCache == null)
			{
				CacheSyntheticNameLcidMapping();
			}
		}

		internal static CultureTableRecord GetCultureTableRecord(string name, bool useUserOverride)
		{
			if (CultureTableRecordCache == null)
			{
				if (name.Length == 0)
				{
					return new CultureTableRecord(name, useUserOverride);
				}
				lock (InternalSyncObject)
				{
					if (CultureTableRecordCache == null)
					{
						CultureTableRecordCache = new Hashtable();
					}
				}
			}
			name = ValidateCulturePieceToLower(name, "name", 84);
			CultureTableRecord[] array = (CultureTableRecord[])CultureTableRecordCache[name];
			if (array != null)
			{
				int num = ((!useUserOverride) ? 1 : 0);
				if (array[num] == null)
				{
					int num2 = ((num == 0) ? 1 : 0);
					array[num] = array[num2].CloneWithUserOverride(useUserOverride);
				}
				return array[num];
			}
			CultureTableRecord cultureTableRecord = new CultureTableRecord(name, useUserOverride);
			lock (InternalSyncObject)
			{
				if (CultureTableRecordCache[name] == null)
				{
					array = new CultureTableRecord[2];
					array[(!useUserOverride) ? 1 : 0] = cultureTableRecord;
					CultureTableRecordCache[name] = array;
					return cultureTableRecord;
				}
				return cultureTableRecord;
			}
		}

		internal static CultureTableRecord GetCultureTableRecord(int cultureId, bool useUserOverride)
		{
			if (cultureId == 127)
			{
				return GetCultureTableRecord("", useUserOverride: false);
			}
			string actualName = null;
			if (CultureTable.Default.GetDataItemFromCultureID(cultureId, out actualName) < 0 && CultureInfo.IsValidLCID(cultureId, 1))
			{
				InitSyntheticMapping();
				actualName = (string)SyntheticLcidToNameCache[cultureId];
			}
			if (actualName != null && actualName.Length > 0)
			{
				return GetCultureTableRecord(actualName, useUserOverride);
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_CultureNotSupported"), cultureId), "culture");
		}

		internal static CultureTableRecord GetCultureTableRecordForRegion(string regionName, bool useUserOverride)
		{
			if (CultureTableRecordRegionCache == null)
			{
				lock (InternalSyncObject)
				{
					if (CultureTableRecordRegionCache == null)
					{
						CultureTableRecordRegionCache = new Hashtable();
					}
				}
			}
			regionName = ValidateCulturePieceToLower(regionName, "regionName", 84);
			CultureTableRecord[] array = (CultureTableRecord[])CultureTableRecordRegionCache[regionName];
			if (array != null)
			{
				int num = ((!useUserOverride) ? 1 : 0);
				if (array[num] == null)
				{
					array[num] = array[(num == 0) ? 1 : 0].CloneWithUserOverride(useUserOverride);
				}
				return array[num];
			}
			int dataItemFromRegionName = CultureTable.Default.GetDataItemFromRegionName(regionName);
			CultureTableRecord cultureTableRecord = null;
			if (dataItemFromRegionName > 0)
			{
				cultureTableRecord = new CultureTableRecord(regionName, dataItemFromRegionName, useUserOverride);
			}
			else
			{
				try
				{
					cultureTableRecord = GetCultureTableRecord(regionName, useUserOverride);
				}
				catch (ArgumentException)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidRegionName"), regionName), "name");
				}
			}
			lock (InternalSyncObject)
			{
				if (CultureTableRecordRegionCache[regionName] == null)
				{
					array = new CultureTableRecord[2];
					array[(!useUserOverride) ? 1 : 0] = cultureTableRecord.CloneWithUserOverride(useUserOverride);
					CultureTableRecordRegionCache[regionName] = array;
					return cultureTableRecord;
				}
				return cultureTableRecord;
			}
		}

		internal unsafe CultureTableRecord(int cultureId, bool useUserOverride)
		{
			m_bUseUserOverride = useUserOverride;
			int dataItemFromCultureID = CultureTable.Default.GetDataItemFromCultureID(cultureId, out m_ActualName);
			if (dataItemFromCultureID < 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_CultureNotSupported"), cultureId), "culture");
			}
			m_ActualCultureID = cultureId;
			m_CultureTable = CultureTable.Default;
			m_pData = (CultureTableData*)(m_CultureTable.m_pItemData + m_CultureTable.m_itemSize * dataItemFromCultureID);
			m_pPool = m_CultureTable.m_pDataPool;
			m_CultureName = SNAME;
			m_CultureID = ((cultureId == 1034) ? cultureId : ILANGUAGE);
		}

		private unsafe CultureTableRecord(string cultureName, bool useUserOverride)
		{
			int num = 0;
			if (cultureName.Length == 0)
			{
				useUserOverride = false;
				num = 127;
			}
			m_bUseUserOverride = useUserOverride;
			int num2 = -1;
			if (cultureName.Length > 0)
			{
				string text = cultureName;
				int culture;
				string actualName;
				int dataItemFromCultureName = CultureTable.Default.GetDataItemFromCultureName(text, out culture, out actualName);
				if (dataItemFromCultureName >= 0 && (CultureInfo.GetSortID(culture) > 0 || culture == 1034))
				{
					int cultureID = ((culture != 1034) ? CultureInfo.GetLangID(culture) : 3082);
					if (CultureTable.Default.GetDataItemFromCultureID(cultureID, out var actualName2) >= 0)
					{
						text = ValidateCulturePieceToLower(actualName2, "cultureName", 84);
					}
				}
				if (!Environment.GetCompatibilityFlag(CompatibilityFlag.DisableReplacementCustomCulture) || IsCustomCultureId(culture))
				{
					m_CultureTable = GetCustomCultureTable(text);
				}
				if (m_CultureTable != null)
				{
					num2 = m_CultureTable.GetDataItemFromCultureName(text, out m_ActualCultureID, out m_ActualName);
					if (dataItemFromCultureName >= 0)
					{
						m_ActualCultureID = culture;
						m_ActualName = actualName;
					}
				}
				if (num2 < 0 && dataItemFromCultureName >= 0)
				{
					m_CultureTable = CultureTable.Default;
					m_ActualCultureID = culture;
					m_ActualName = actualName;
					num2 = dataItemFromCultureName;
				}
				if (num2 < 0)
				{
					InitSyntheticMapping();
					if (SyntheticNameToLcidCache[text] != null)
					{
						num = (int)SyntheticNameToLcidCache[text];
					}
				}
			}
			if (num2 < 0 && num > 0)
			{
				if (num == 127)
				{
					num2 = CultureTable.Default.GetDataItemFromCultureID(num, out m_ActualName);
					if (num2 > 0)
					{
						m_ActualCultureID = num;
						m_CultureTable = CultureTable.Default;
					}
				}
				else
				{
					CultureTable cultureTable = null;
					string actualName3 = null;
					if (CultureInfo.GetSortID(num) > 0)
					{
						num2 = CultureTable.Default.GetDataItemFromCultureID(CultureInfo.GetLangID(num), out actualName3);
					}
					if (num2 < 0)
					{
						actualName3 = (string)SyntheticLcidToNameCache[CultureInfo.GetLangID(num)];
					}
					string text2 = (string)SyntheticLcidToNameCache[num];
					int dataItem = -1;
					if (text2 != null && actualName3 != null && !Environment.GetCompatibilityFlag(CompatibilityFlag.DisableReplacementCustomCulture))
					{
						cultureTable = TryCreateReplacementCulture(actualName3, out dataItem);
					}
					if (cultureTable == null)
					{
						if (num2 > 0)
						{
							m_CultureTable = CultureTable.Default;
							m_ActualCultureID = num;
							m_synthetic = true;
							m_ActualName = CultureInfo.nativeGetCultureName(num, useSNameLCType: true, getMonthName: false);
						}
						else if (GetSyntheticCulture(num))
						{
							return;
						}
					}
					else
					{
						m_CultureTable = cultureTable;
						num2 = dataItem;
						m_ActualName = CultureInfo.nativeGetCultureName(num, useSNameLCType: true, getMonthName: false);
						m_ActualCultureID = num;
					}
				}
			}
			if (num2 >= 0)
			{
				m_pData = (CultureTableData*)(m_CultureTable.m_pItemData + m_CultureTable.m_itemSize * num2);
				m_pPool = m_CultureTable.m_pDataPool;
				m_CultureName = SNAME;
				m_CultureID = ((m_ActualCultureID == 1034) ? m_ActualCultureID : ILANGUAGE);
				CheckCustomSynthetic();
				return;
			}
			if (cultureName != null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidCultureName"), cultureName), "name");
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_CultureNotSupported"), num), "culture");
		}

		private unsafe CultureTableRecord(string regionName, int dataItem, bool useUserOverride)
		{
			m_bUseUserOverride = useUserOverride;
			m_CultureName = regionName;
			m_CultureTable = CultureTable.Default;
			m_pData = (CultureTableData*)(m_CultureTable.m_pItemData + m_CultureTable.m_itemSize * dataItem);
			m_pPool = m_CultureTable.m_pDataPool;
			m_CultureID = ILANGUAGE;
		}

		private void CheckCustomSynthetic()
		{
			if (!IsCustomCulture)
			{
				return;
			}
			InitSyntheticMapping();
			if (IsCustomCultureId(m_CultureID))
			{
				string key = ValidateCulturePieceToLower(m_CultureName, "CultureName", 84);
				if (SyntheticNameToLcidCache[key] != null)
				{
					m_synthetic = true;
					m_ActualCultureID = (m_CultureID = (int)SyntheticNameToLcidCache[key]);
				}
			}
			else if (SyntheticLcidToNameCache[m_CultureID] != null)
			{
				m_synthetic = true;
				m_ActualCultureID = m_CultureID;
			}
			else if (m_CultureID != m_ActualCultureID && SyntheticLcidToNameCache[m_ActualCultureID] != null)
			{
				m_synthetic = true;
			}
		}

		internal static void ResetCustomCulturesCache()
		{
			CultureTableRecordCache = null;
			CultureTableRecordRegionCache = null;
		}

		private static bool GetScriptTag(int lcid, out string script)
		{
			script = null;
			string text = CultureInfo.nativeGetCultureName(lcid, useSNameLCType: false, getMonthName: true);
			if (text == null)
			{
				return false;
			}
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] >= 'ᐁ' && text[i] <= 'ᙶ')
				{
					script = "cans";
					return true;
				}
				if (text[i] >= 'ሀ' && text[i] <= '፼')
				{
					script = "ethi";
					return true;
				}
				if (text[i] >= '᠀' && text[i] <= '᠙')
				{
					script = "mong";
					return true;
				}
				if (text[i] >= 'ꀀ' && text[i] <= '꓆')
				{
					script = "yiii";
					return true;
				}
				if (text[i] >= 'Ꭰ' && text[i] <= 'Ᏼ')
				{
					script = "cher";
					return true;
				}
				if (text[i] >= 'ក' && text[i] <= '៹')
				{
					script = "khmr";
					return true;
				}
			}
			byte[] sortKey;
			int nativeSortKey = CultureInfo.GetNativeSortKey(lcid, 0, text, text.Length, out sortKey);
			if (nativeSortKey == 0)
			{
				return false;
			}
			for (int j = 0; j < nativeSortKey && sortKey[j] != 1; j += 2)
			{
				switch (sortKey[j])
				{
				case 14:
					script = "latn";
					return true;
				case 15:
					script = "grek";
					return true;
				case 16:
					script = "cyrl";
					return true;
				case 17:
					script = "armn";
					return true;
				case 18:
					script = "hebr";
					return true;
				case 19:
					script = "arab";
					return true;
				case 20:
					script = "deva";
					return true;
				case 21:
					script = "beng";
					return true;
				case 22:
					script = "guru";
					return true;
				case 23:
					script = "gujr";
					return true;
				case 24:
					script = "orya";
					return true;
				case 25:
					script = "taml";
					return true;
				case 26:
					script = "telu";
					return true;
				case 27:
					script = "knda";
					return true;
				case 28:
					script = "mlym";
					return true;
				case 29:
					script = "sinh";
					return true;
				case 30:
					script = "thai";
					return true;
				case 31:
					script = "laoo";
					return true;
				case 32:
					script = "tibt";
					return true;
				case 33:
					script = "geor";
					return true;
				case 34:
					script = "kana";
					return true;
				case 35:
					script = "bopo";
					return true;
				case 36:
					script = "hang";
					return true;
				case 128:
					script = "hani";
					return true;
				}
			}
			return false;
		}

		private static bool IsBuiltInCulture(int lcid)
		{
			return CultureTable.Default.IsExistingCulture(lcid);
		}

		internal static string Concatenate(StringBuilder helper, params string[] stringsToConcat)
		{
			if (helper.Length > 0)
			{
				helper.Remove(0, helper.Length);
			}
			for (int i = 0; i < stringsToConcat.Length; i++)
			{
				helper.Append(stringsToConcat[i]);
			}
			return helper.ToString();
		}

		internal static bool GetCultureNamesUsingSNameLCType(int[] lcidArray, Hashtable lcidToName, Hashtable nameToLcid)
		{
			string text = CultureInfo.nativeGetCultureName(lcidArray[0], useSNameLCType: true, getMonthName: false);
			if (text == null)
			{
				return false;
			}
			if (!IsBuiltInCulture(lcidArray[0]) && !IsCustomCultureId(lcidArray[0]))
			{
				text = ValidateCulturePieceToLower(text, "cultureName", text.Length);
				nameToLcid[text] = lcidArray[0];
				lcidToName[lcidArray[0]] = text;
			}
			for (int i = 1; i < lcidArray.Length; i++)
			{
				if (!IsBuiltInCulture(lcidArray[i]) || IsCustomCultureId(lcidArray[0]))
				{
					text = CultureInfo.nativeGetCultureName(lcidArray[i], useSNameLCType: true, getMonthName: false);
					if (text != null)
					{
						text = ValidateCulturePieceToLower(text, "cultureName", text.Length);
						nameToLcid[text] = lcidArray[i];
						lcidToName[lcidArray[i]] = text;
					}
				}
			}
			return true;
		}

		internal static void CacheSyntheticNameLcidMapping()
		{
			Hashtable hashtable = new Hashtable();
			Hashtable hashtable2 = new Hashtable();
			int[] localesArray = null;
			bool flag = false;
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(typeof(CultureTableRecord), ref tookLock);
				flag = CultureInfo.nativeEnumSystemLocales(out localesArray);
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(typeof(CultureTableRecord));
				}
			}
			if (flag && !GetCultureNamesUsingSNameLCType(localesArray, hashtable, hashtable2))
			{
				Hashtable namesHashtable = GetNamesHashtable();
				StringBuilder helper = new StringBuilder();
				foreach (int num in localesArray)
				{
					if (IsBuiltInCulture(num) || IsCustomCultureId(num))
					{
						continue;
					}
					GetAdjustedNames(num, out var adjustedNames);
					string text = ((adjustedNames == null) ? CultureInfo.nativeGetCultureName(num, useSNameLCType: false, getMonthName: false) : adjustedNames.sName);
					if (text == null)
					{
						continue;
					}
					text = ValidateCulturePieceToLower(text, "cultureName", text.Length);
					string script;
					if (namesHashtable[text] != null)
					{
						if (GetScriptTag(num, out script))
						{
							script = Concatenate(helper, text, "-", script);
							script = GetQualifiedName(script);
							hashtable2[script] = num;
							hashtable[num] = script;
						}
						continue;
					}
					if (hashtable2[text] == null)
					{
						hashtable2[text] = num;
						hashtable[num] = text;
						continue;
					}
					int num2 = (int)hashtable2[text];
					hashtable2.Remove(text);
					hashtable.Remove(num2);
					namesHashtable[text] = "";
					if (GetScriptTag(num2, out script))
					{
						script = Concatenate(helper, text, "-", script);
						script = GetQualifiedName(script);
						hashtable2[script] = num2;
						hashtable[num2] = script;
					}
					if (GetScriptTag(num, out script))
					{
						script = Concatenate(helper, text, "-", script);
						script = GetQualifiedName(script);
						hashtable2[script] = num;
						hashtable[num] = script;
					}
				}
			}
			lock (InternalSyncObject)
			{
				SyntheticLcidToNameCache = hashtable;
				SyntheticNameToLcidCache = hashtable2;
			}
		}

		private static void AdjustSyntheticCalendars(ref CultureData data, ref CompositeCultureData compositeData)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			ushort num2 = data.waCalendars[0];
			stringBuilder.Append((char)num2);
			for (int i = 1; i < data.waCalendars.Length; i++)
			{
				stringBuilder.Append((char)data.waCalendars[i]);
				if (data.waCalendars[i] == (ushort)data.iDefaultCalender)
				{
					num = i;
				}
				if (data.waCalendars[i] > num2)
				{
					num2 = data.waCalendars[i];
				}
			}
			if (num2 > 1)
			{
				string[] array = new string[num2];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = string.Empty;
				}
				for (int k = 0; k < data.waCalendars.Length; k++)
				{
					array[data.waCalendars[k] - 1] = data.saNativeCalendarNames[k];
				}
				data.saNativeCalendarNames = array;
			}
			if (num > 0)
			{
				char value = stringBuilder[num];
				stringBuilder[num] = stringBuilder[0];
				stringBuilder[0] = value;
			}
			compositeData.waCalendars = stringBuilder.ToString();
		}

		private unsafe bool GetSyntheticCulture(int cultureID)
		{
			if (SyntheticLcidToNameCache == null || SyntheticNameToLcidCache == null)
			{
				CacheSyntheticNameLcidMapping();
			}
			if (SyntheticLcidToNameCache[cultureID] == null)
			{
				return false;
			}
			if (SyntheticDataCache == null)
			{
				SyntheticDataCache = new Hashtable();
			}
			else
			{
				nativeMemoryHandle = (AgileSafeNativeMemoryHandle)SyntheticDataCache[cultureID];
			}
			if (nativeMemoryHandle != null)
			{
				m_pData = (CultureTableData*)(void*)nativeMemoryHandle.DangerousGetHandle();
				m_pPool = (ushort*)(m_pData + 1);
				m_CultureTable = CultureTable.Default;
				m_CultureName = SNAME;
				m_CultureID = cultureID;
				m_synthetic = true;
				m_ActualCultureID = cultureID;
				m_ActualName = m_CultureName;
				return true;
			}
			CultureData cultureData = default(CultureData);
			bool flag = false;
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(typeof(CultureTableRecord), ref tookLock);
				flag = CultureInfo.nativeGetCultureData(cultureID, ref cultureData);
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(typeof(CultureTableRecord));
				}
			}
			if (!flag)
			{
				return false;
			}
			CompositeCultureData compositeData = default(CompositeCultureData);
			int cultureDataSize = GetCultureDataSize(cultureID, ref cultureData, ref compositeData);
			IntPtr intPtr = IntPtr.Zero;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					intPtr = Marshal.AllocHGlobal(cultureDataSize);
					if (intPtr != IntPtr.Zero)
					{
						nativeMemoryHandle = new AgileSafeNativeMemoryHandle(intPtr, ownsHandle: true);
					}
				}
			}
			finally
			{
				if (nativeMemoryHandle == null && intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
					intPtr = IntPtr.Zero;
				}
			}
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException(Environment.GetResourceString("OutOfMemory_MemFailPoint"));
			}
			m_pData = (CultureTableData*)(void*)nativeMemoryHandle.DangerousGetHandle();
			m_pPool = (ushort*)(m_pData + 1);
			FillCultureDataMemory(cultureID, ref cultureData, ref compositeData);
			m_CultureTable = CultureTable.Default;
			m_CultureName = SNAME;
			m_CultureID = cultureID;
			m_synthetic = true;
			m_ActualCultureID = cultureID;
			m_ActualName = m_CultureName;
			lock (SyntheticDataCache)
			{
				if (SyntheticDataCache[cultureID] == null)
				{
					SyntheticDataCache[cultureID] = nativeMemoryHandle;
				}
			}
			return true;
		}

		internal static Hashtable GetNamesHashtable()
		{
			Hashtable hashtable = new Hashtable();
			hashtable["bs-ba"] = "";
			hashtable["tg-tj"] = "";
			hashtable["mn-cn"] = "";
			hashtable["iu-ca"] = "";
			return hashtable;
		}

		internal static void GetAdjustedNames(int lcid, out AdjustedSyntheticCultureName adjustedNames)
		{
			for (int i = 0; i < AdjustedSyntheticNames.Length; i++)
			{
				if (AdjustedSyntheticNames[i].lcid == lcid)
				{
					adjustedNames = AdjustedSyntheticNames[i];
					return;
				}
			}
			adjustedNames = null;
		}

		private unsafe uint FillCultureDataMemory(int cultureID, ref CultureData data, ref CompositeCultureData compositeData)
		{
			uint num = 0u;
			Hashtable hashtable = new Hashtable(30);
			m_pPool[num] = 0;
			num++;
			SetPoolString("", hashtable, ref num);
			hashtable[""] = 0u;
			m_pData->iLanguage = (ushort)cultureID;
			m_pData->sName = (ushort)SetPoolString(compositeData.sname, hashtable, ref num);
			m_pData->iDigits = (ushort)data.iDigits;
			m_pData->iNegativeNumber = (ushort)data.iNegativeNumber;
			m_pData->iCurrencyDigits = (ushort)data.iCurrencyDigits;
			m_pData->iCurrency = (ushort)data.iCurrency;
			m_pData->iNegativeCurrency = (ushort)data.iNegativeCurrency;
			m_pData->iLeadingZeros = (ushort)data.iLeadingZeros;
			m_pData->iFlags = 1;
			m_pData->iFirstDayOfWeek = ConvertFirstDayOfWeekMonToSun(data.iFirstDayOfWeek);
			m_pData->iFirstWeekOfYear = (ushort)data.iFirstWeekOfYear;
			m_pData->iCountry = (ushort)data.iCountry;
			m_pData->iMeasure = (ushort)data.iMeasure;
			m_pData->iDigitSubstitution = (ushort)data.iDigitSubstitution;
			m_pData->waGrouping = (ushort)SetPoolString(data.waGrouping, hashtable, ref num);
			m_pData->waMonetaryGrouping = (ushort)SetPoolString(data.waMonetaryGrouping, hashtable, ref num);
			m_pData->sListSeparator = (ushort)SetPoolString(data.sListSeparator, hashtable, ref num);
			m_pData->sDecimalSeparator = (ushort)SetPoolString(data.sDecimalSeparator, hashtable, ref num);
			m_pData->sThousandSeparator = (ushort)SetPoolString(data.sThousandSeparator, hashtable, ref num);
			m_pData->sCurrency = (ushort)SetPoolString(data.sCurrency, hashtable, ref num);
			m_pData->sMonetaryDecimal = (ushort)SetPoolString(data.sMonetaryDecimal, hashtable, ref num);
			m_pData->sMonetaryThousand = (ushort)SetPoolString(data.sMonetaryThousand, hashtable, ref num);
			m_pData->sPositiveSign = (ushort)SetPoolString(data.sPositiveSign, hashtable, ref num);
			m_pData->sNegativeSign = (ushort)SetPoolString(data.sNegativeSign, hashtable, ref num);
			m_pData->sAM1159 = (ushort)SetPoolString(data.sAM1159, hashtable, ref num);
			m_pData->sPM2359 = (ushort)SetPoolString(data.sPM2359, hashtable, ref num);
			m_pData->saNativeDigits = (ushort)SetPoolStringArrayFromSingleString(data.saNativeDigits, hashtable, ref num);
			m_pData->saTimeFormat = (ushort)SetPoolStringArray(hashtable, ref num, data.saTimeFormat);
			m_pData->saShortDate = (ushort)SetPoolStringArray(hashtable, ref num, data.saShortDate);
			m_pData->saLongDate = (ushort)SetPoolStringArray(hashtable, ref num, data.saLongDate);
			m_pData->saYearMonth = (ushort)SetPoolStringArray(hashtable, ref num, data.saYearMonth);
			m_pData->saDuration = (ushort)SetPoolStringArray(hashtable, ref num, "");
			m_pData->iDefaultLanguage = m_pData->iLanguage;
			m_pData->iDefaultAnsiCodePage = (ushort)data.iDefaultAnsiCodePage;
			m_pData->iDefaultOemCodePage = (ushort)data.iDefaultOemCodePage;
			m_pData->iDefaultMacCodePage = (ushort)data.iDefaultMacCodePage;
			m_pData->iDefaultEbcdicCodePage = (ushort)data.iDefaultEbcdicCodePage;
			m_pData->iGeoId = (ushort)data.iGeoId;
			m_pData->iPaperSize = (ushort)data.iPaperSize;
			m_pData->iIntlCurrencyDigits = (ushort)data.iIntlCurrencyDigits;
			m_pData->iParent = (ushort)compositeData.parentLcid;
			m_pData->waCalendars = (ushort)SetPoolString(compositeData.waCalendars, hashtable, ref num);
			m_pData->sAbbrevLang = (ushort)SetPoolString(data.sAbbrevLang, hashtable, ref num);
			m_pData->sISO639Language = (ushort)SetPoolString(data.sIso639Language, hashtable, ref num);
			m_pData->sEnglishLanguage = (ushort)SetPoolString(data.sEnglishLanguage, hashtable, ref num);
			m_pData->sNativeLanguage = (ushort)SetPoolString(data.sNativeLanguage, hashtable, ref num);
			m_pData->sEnglishCountry = (ushort)SetPoolString(data.sEnglishCountry, hashtable, ref num);
			m_pData->sNativeCountry = (ushort)SetPoolString(data.sNativeCountry, hashtable, ref num);
			m_pData->sAbbrevCountry = (ushort)SetPoolString(data.sAbbrevCountry, hashtable, ref num);
			m_pData->sISO3166CountryName = (ushort)SetPoolString(data.sIso3166CountryName, hashtable, ref num);
			m_pData->sIntlMonetarySymbol = (ushort)SetPoolString(data.sIntlMonetarySymbol, hashtable, ref num);
			m_pData->sEnglishCurrency = (ushort)SetPoolString(data.sEnglishCurrency, hashtable, ref num);
			m_pData->sNativeCurrency = (ushort)SetPoolString(data.sNativeCurrency, hashtable, ref num);
			m_pData->waFontSignature = (ushort)SetPoolString(data.waFontSignature, hashtable, ref num);
			m_pData->sISO639Language2 = (ushort)SetPoolString(data.sISO639Language2, hashtable, ref num);
			m_pData->sISO3166CountryName2 = (ushort)SetPoolString(data.sISO3166CountryName2, hashtable, ref num);
			m_pData->sParent = (ushort)SetPoolString(compositeData.parentName, hashtable, ref num);
			m_pData->saDayNames = (ushort)SetPoolStringArray(hashtable, ref num, data.saDayNames);
			m_pData->saAbbrevDayNames = (ushort)SetPoolStringArray(hashtable, ref num, data.saAbbrevDayNames);
			m_pData->saMonthNames = (ushort)SetPoolStringArray(hashtable, ref num, data.saMonthNames);
			m_pData->saAbbrevMonthNames = (ushort)SetPoolStringArray(hashtable, ref num, data.saAbbrevMonthNames);
			m_pData->saMonthGenitiveNames = (ushort)SetPoolStringArray(hashtable, ref num, data.saGenitiveMonthNames);
			m_pData->saAbbrevMonthGenitiveNames = (ushort)SetPoolStringArray(hashtable, ref num, data.saAbbrevGenitiveMonthNames);
			m_pData->saNativeCalendarNames = (ushort)SetPoolStringArray(hashtable, ref num, data.saNativeCalendarNames);
			m_pData->saAltSortID = (ushort)SetPoolStringArray(hashtable, ref num, "");
			m_pData->iNegativePercent = (ushort)CultureInfo.InvariantCulture.NumberFormat.PercentNegativePattern;
			m_pData->iPositivePercent = (ushort)CultureInfo.InvariantCulture.NumberFormat.PercentPositivePattern;
			m_pData->iFormatFlags = 0;
			m_pData->iLineOrientations = 0;
			m_pData->iTextInfo = m_pData->iLanguage;
			m_pData->iInputLanguageHandle = m_pData->iLanguage;
			m_pData->iCompareInfo = m_pData->iLanguage;
			m_pData->sEnglishDisplayName = (ushort)SetPoolString(compositeData.englishDisplayName, hashtable, ref num);
			m_pData->sNativeDisplayName = (ushort)SetPoolString(compositeData.sNativeDisplayName, hashtable, ref num);
			m_pData->sPercent = (ushort)SetPoolString(CultureInfo.InvariantCulture.NumberFormat.PercentSymbol, hashtable, ref num);
			m_pData->sNaN = (ushort)SetPoolString(data.sNaN, hashtable, ref num);
			m_pData->sPositiveInfinity = (ushort)SetPoolString(data.sPositiveInfinity, hashtable, ref num);
			m_pData->sNegativeInfinity = (ushort)SetPoolString(data.sNegativeInfinity, hashtable, ref num);
			m_pData->sMonthDay = (ushort)SetPoolString(CultureInfo.InvariantCulture.DateTimeFormat.MonthDayPattern, hashtable, ref num);
			m_pData->sAdEra = (ushort)SetPoolString(CultureInfo.InvariantCulture.DateTimeFormat.GetEraName(0), hashtable, ref num);
			m_pData->sAbbrevAdEra = (ushort)SetPoolString(CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedEraName(0), hashtable, ref num);
			m_pData->sRegionName = m_pData->sISO3166CountryName;
			m_pData->sConsoleFallbackName = (ushort)SetPoolString(compositeData.consoleFallbackName, hashtable, ref num);
			m_pData->saShortTime = m_pData->saTimeFormat;
			m_pData->saSuperShortDayNames = (ushort)SetPoolStringArray(hashtable, ref num, data.saSuperShortDayNames);
			m_pData->saDateWords = m_pData->saDuration;
			m_pData->sSpecificCulture = m_pData->sName;
			m_pData->sScripts = 0u;
			return 2 * num;
		}

		private unsafe uint SetPoolString(string s, Hashtable offsetTable, ref uint currentOffset)
		{
			uint result = currentOffset;
			if (offsetTable[s] == null)
			{
				offsetTable[s] = currentOffset;
				m_pPool[currentOffset] = (ushort)s.Length;
				currentOffset++;
				for (int i = 0; i < s.Length; i++)
				{
					m_pPool[currentOffset] = s[i];
					currentOffset++;
				}
				if ((currentOffset & 1) == 0)
				{
					m_pPool[currentOffset] = 0;
					currentOffset++;
				}
				return result;
			}
			return (uint)offsetTable[s];
		}

		private unsafe uint SetPoolStringArray(Hashtable offsetTable, ref uint currentOffset, params string[] array)
		{
			uint[] array2 = new uint[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = SetPoolString(array[i], offsetTable, ref currentOffset);
			}
			uint result = currentOffset;
			m_pPool[currentOffset] = (ushort)array2.Length;
			currentOffset++;
			uint* ptr = (uint*)(m_pPool + currentOffset);
			for (int j = 0; j < array2.Length; j++)
			{
				ptr[j] = array2[j];
				currentOffset += 2u;
			}
			if ((currentOffset & 1) == 0)
			{
				m_pPool[currentOffset] = 0;
				currentOffset++;
			}
			return result;
		}

		private uint SetPoolStringArrayFromSingleString(string s, Hashtable offsetTable, ref uint currentOffset)
		{
			string[] array = new string[s.Length];
			for (int i = 0; i < s.Length; i++)
			{
				array[i] = s.Substring(i, 1);
			}
			return SetPoolStringArray(offsetTable, ref currentOffset, array);
		}

		private bool NameHasScriptTag(string tempName)
		{
			int num = 0;
			for (int i = 0; i < tempName.Length; i++)
			{
				if (num >= 2)
				{
					break;
				}
				if (tempName[i] == '-')
				{
					num++;
				}
			}
			return num > 1;
		}

		private static string GetCasedName(string name)
		{
			StringBuilder stringBuilder = new StringBuilder(name.Length);
			int i;
			for (i = 0; i < name.Length && name[i] != '-'; i++)
			{
				stringBuilder.Append(name[i]);
			}
			stringBuilder.Append("-");
			i++;
			char value = char.ToUpper(name[i], CultureInfo.InvariantCulture);
			stringBuilder.Append(value);
			for (i++; i < name.Length && name[i] != '-'; i++)
			{
				stringBuilder.Append(name[i]);
			}
			stringBuilder.Append("-");
			for (i++; i < name.Length; i++)
			{
				value = char.ToUpper(name[i], CultureInfo.InvariantCulture);
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}

		private static string GetQualifiedName(string name)
		{
			StringBuilder stringBuilder = new StringBuilder(name.Length);
			int i;
			for (i = 0; i < name.Length && name[i] != '-'; i++)
			{
				stringBuilder.Append(name[i]);
			}
			stringBuilder.Append("--");
			i++;
			int num = i;
			for (; i < name.Length && name[i] != '-'; i++)
			{
				stringBuilder.Append(name[i]);
			}
			for (i++; i < name.Length; i++)
			{
				stringBuilder.Insert(num, name[i]);
				num++;
			}
			return stringBuilder.ToString();
		}

		private static void GetSyntheticParentData(ref CultureData data, ref CompositeCultureData compositeData)
		{
			compositeData.parentLcid = CultureInfo.InvariantCulture.LCID;
			compositeData.parentName = CultureInfo.InvariantCulture.Name;
			if (data.sParentName != null)
			{
				string text = ValidateCulturePieceToLower(data.sParentName, "ParentName", 84);
				int culture;
				string actualName;
				int dataItemFromCultureName = CultureTable.Default.GetDataItemFromCultureName(text, out culture, out actualName);
				if (dataItemFromCultureName >= 0)
				{
					compositeData.parentLcid = culture;
					compositeData.parentName = actualName;
				}
				else if (SyntheticNameToLcidCache[text] != null)
				{
					compositeData.parentLcid = (int)SyntheticNameToLcidCache[text];
					compositeData.parentName = data.sParentName;
				}
			}
		}

		private static void GetSyntheticConsoleFallback(ref CultureData data, ref CompositeCultureData compositeData)
		{
			compositeData.consoleFallbackName = CultureInfo.InvariantCulture.GetConsoleFallbackUICulture().Name;
			if (data.sConsoleFallbackName != null)
			{
				string text = ValidateCulturePieceToLower(data.sConsoleFallbackName, "ConsoleFallbackName", 84);
				int culture;
				string actualName;
				int dataItemFromCultureName = CultureTable.Default.GetDataItemFromCultureName(text, out culture, out actualName);
				if (dataItemFromCultureName >= 0)
				{
					compositeData.consoleFallbackName = actualName;
				}
				else if (SyntheticNameToLcidCache[text] != null)
				{
					compositeData.consoleFallbackName = data.sConsoleFallbackName;
				}
			}
		}

		private unsafe int GetCultureDataSize(int cultureID, ref CultureData data, ref CompositeCultureData compositeData)
		{
			int num = sizeof(CultureTableData);
			Hashtable offsetTable = new Hashtable(30);
			num += 2;
			num += GetPoolStringSize("", offsetTable);
			compositeData.sname = CultureInfo.nativeGetCultureName(cultureID, useSNameLCType: true, getMonthName: false);
			if (compositeData.sname == null)
			{
				GetAdjustedNames(cultureID, out var adjustedNames);
				if (adjustedNames != null)
				{
					data.sIso639Language = adjustedNames.isoLanguage;
					data.sIso3166CountryName = adjustedNames.isoCountry;
					compositeData.sname = adjustedNames.sName;
				}
				else
				{
					string text = (string)SyntheticLcidToNameCache[cultureID];
					if (NameHasScriptTag(text))
					{
						compositeData.sname = GetCasedName(text);
					}
					else
					{
						compositeData.sname = data.sIso639Language + "-" + data.sIso3166CountryName;
					}
				}
			}
			compositeData.englishDisplayName = data.sEnglishLanguage + " (" + data.sEnglishCountry + ")";
			compositeData.sNativeDisplayName = data.sNativeLanguage + " (" + data.sNativeCountry + ")";
			AdjustSyntheticCalendars(ref data, ref compositeData);
			num += GetPoolStringSize(compositeData.sname, offsetTable);
			num += GetPoolStringSize(compositeData.englishDisplayName, offsetTable);
			num += GetPoolStringSize(compositeData.sNativeDisplayName, offsetTable);
			num += GetPoolStringSize(compositeData.waCalendars, offsetTable);
			GetSyntheticParentData(ref data, ref compositeData);
			num += GetPoolStringSize(compositeData.parentName, offsetTable);
			num += GetPoolStringSize(data.sIso639Language, offsetTable);
			num += GetPoolStringSize(data.sListSeparator, offsetTable);
			num += GetPoolStringSize(data.sDecimalSeparator, offsetTable);
			num += GetPoolStringSize(data.sThousandSeparator, offsetTable);
			num += GetPoolStringSize(data.sCurrency, offsetTable);
			num += GetPoolStringSize(data.sMonetaryDecimal, offsetTable);
			num += GetPoolStringSize(data.sMonetaryThousand, offsetTable);
			num += GetPoolStringSize(data.sPositiveSign, offsetTable);
			num += GetPoolStringSize(data.sNegativeSign, offsetTable);
			num += GetPoolStringSize(data.sAM1159, offsetTable);
			num += GetPoolStringSize(data.sPM2359, offsetTable);
			num += GetPoolStringSize(data.sAbbrevLang, offsetTable);
			num += GetPoolStringSize(data.sEnglishLanguage, offsetTable);
			num += GetPoolStringSize(data.sNativeLanguage, offsetTable);
			num += GetPoolStringSize(data.sEnglishCountry, offsetTable);
			num += GetPoolStringSize(data.sNativeCountry, offsetTable);
			num += GetPoolStringSize(data.sAbbrevCountry, offsetTable);
			num += GetPoolStringSize(data.sIso3166CountryName, offsetTable);
			num += GetPoolStringSize(data.sIntlMonetarySymbol, offsetTable);
			num += GetPoolStringSize(data.sEnglishCurrency, offsetTable);
			num += GetPoolStringSize(data.sNativeCurrency, offsetTable);
			num += GetPoolStringSize(CultureInfo.InvariantCulture.NumberFormat.PercentSymbol, offsetTable);
			if (data.sNaN == null)
			{
				data.sNaN = CultureInfo.InvariantCulture.NumberFormat.NaNSymbol;
			}
			num += GetPoolStringSize(data.sNaN, offsetTable);
			if (data.sPositiveInfinity == null)
			{
				data.sPositiveInfinity = CultureInfo.InvariantCulture.NumberFormat.PositiveInfinitySymbol;
			}
			num += GetPoolStringSize(data.sPositiveInfinity, offsetTable);
			if (data.sNegativeInfinity == null)
			{
				data.sNegativeInfinity = CultureInfo.InvariantCulture.NumberFormat.NegativeInfinitySymbol;
			}
			num += GetPoolStringSize(data.sNegativeInfinity, offsetTable);
			num += GetPoolStringSize(CultureInfo.InvariantCulture.DateTimeFormat.MonthDayPattern, offsetTable);
			num += GetPoolStringSize(CultureInfo.InvariantCulture.DateTimeFormat.GetEraName(0), offsetTable);
			num += GetPoolStringSize(CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedEraName(0), offsetTable);
			GetSyntheticConsoleFallback(ref data, ref compositeData);
			num += GetPoolStringSize(compositeData.consoleFallbackName, offsetTable);
			num += GetPoolStringArraySize(offsetTable, data.saMonthNames);
			num += GetPoolStringArraySize(offsetTable, data.saDayNames);
			num += GetPoolStringArraySize(offsetTable, data.saAbbrevDayNames);
			num += GetPoolStringArraySize(offsetTable, data.saAbbrevMonthNames);
			data.saGenitiveMonthNames[12] = data.saMonthNames[12];
			num += GetPoolStringArraySize(offsetTable, data.saGenitiveMonthNames);
			data.saAbbrevGenitiveMonthNames[12] = data.saAbbrevMonthNames[12];
			num += GetPoolStringArraySize(offsetTable, data.saAbbrevGenitiveMonthNames);
			num += GetPoolStringArraySize(offsetTable, data.saNativeCalendarNames);
			num += GetPoolStringArraySize(offsetTable, data.saTimeFormat);
			num += GetPoolStringArraySize(offsetTable, data.saShortDate);
			num += GetPoolStringArraySize(offsetTable, data.saLongDate);
			num += GetPoolStringArraySize(offsetTable, data.saYearMonth);
			num += GetPoolStringArraySize(offsetTable, "");
			num += GetPoolStringArraySize(offsetTable, "");
			data.waGrouping = GroupSizesConstruction(data.waGrouping);
			num += GetPoolStringSize(data.waGrouping, offsetTable);
			data.waMonetaryGrouping = GroupSizesConstruction(data.waMonetaryGrouping);
			num += GetPoolStringSize(data.waMonetaryGrouping, offsetTable);
			num += GetPoolStringArraySize(data.saNativeDigits, offsetTable);
			num += GetPoolStringSize(data.waFontSignature, offsetTable);
			if (data.sISO3166CountryName2 == null)
			{
				data.sISO3166CountryName2 = data.sIso3166CountryName;
			}
			num += GetPoolStringSize(data.sISO3166CountryName2, offsetTable);
			if (data.sISO639Language2 == null)
			{
				data.sISO639Language2 = data.sIso639Language;
			}
			num += GetPoolStringSize(data.sISO639Language2, offsetTable);
			if (data.saSuperShortDayNames == null)
			{
				data.saSuperShortDayNames = data.saAbbrevDayNames;
			}
			return num + GetPoolStringArraySize(offsetTable, data.saSuperShortDayNames);
		}

		private int GetPoolStringSize(string s, Hashtable offsetTable)
		{
			int result = 0;
			if (offsetTable[s] == null)
			{
				offsetTable[s] = "";
				result = 2 * (s.Length + 1 + (1 - (s.Length & 1)));
			}
			return result;
		}

		private int GetPoolStringArraySize(string s, Hashtable offsetTable)
		{
			string[] array = new string[s.Length];
			for (int i = 0; i < s.Length; i++)
			{
				array[i] = s.Substring(i, 1);
			}
			return GetPoolStringArraySize(offsetTable, array);
		}

		private int GetPoolStringArraySize(Hashtable offsetTable, params string[] array)
		{
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				num += GetPoolStringSize(array[i], offsetTable);
			}
			return num + 2 * (array.Length * 2 + 1 + 1);
		}

		private string GroupSizesConstruction(string rawGroupSize)
		{
			int num = rawGroupSize.Length;
			if (rawGroupSize[num - 1] == '0')
			{
				num--;
			}
			int num2 = 0;
			StringBuilder stringBuilder = new StringBuilder();
			while (num2 < num)
			{
				stringBuilder.Append((char)(rawGroupSize[num2] - 48));
				num2++;
				if (num2 < num)
				{
					num2++;
				}
			}
			if (num == rawGroupSize.Length)
			{
				stringBuilder.Append('\0');
			}
			return stringBuilder.ToString();
		}

		private string GetCustomCultureFile(string name)
		{
			StringBuilder stringBuilder = new StringBuilder(WindowsPath);
			stringBuilder.Append("\\Globalization\\");
			stringBuilder.Append(name);
			stringBuilder.Append(".nlp");
			string text = stringBuilder.ToString();
			if (CultureInfo.nativeFileExists(text))
			{
				return text;
			}
			return null;
		}

		private static string ValidateCulturePieceToLower(string testString, string paramName, int maxLength)
		{
			if (testString.Length > maxLength)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_NameTooLong"), testString, maxLength), paramName);
			}
			StringBuilder stringBuilder = new StringBuilder(testString.Length);
			foreach (char c in testString)
			{
				if (c <= 'Z' && c >= 'A')
				{
					stringBuilder.Append((char)(c - 65 + 97));
					continue;
				}
				if ((c <= 'z' && c >= 'a') || (c <= '9' && c >= '0') || c == '_' || c == '-')
				{
					stringBuilder.Append(c);
					continue;
				}
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_NameContainsInvalidCharacters"), testString), paramName);
			}
			return stringBuilder.ToString();
		}

		internal static string AnsiToLower(string testString)
		{
			StringBuilder stringBuilder = new StringBuilder(testString.Length);
			foreach (char c in testString)
			{
				stringBuilder.Append((c <= 'Z' && c >= 'A') ? ((char)(c - 65 + 97)) : c);
			}
			return stringBuilder.ToString();
		}

		internal bool UseCurrentCalendar(int calID)
		{
			if (UseGetLocaleInfo)
			{
				return CultureInfo.nativeGetCurrentCalendar() == calID;
			}
			return false;
		}

		internal bool IsValidSortID(int sortID)
		{
			if (sortID == 0 || (SALTSORTID != null && SALTSORTID.Length >= sortID && SALTSORTID[sortID - 1].Length != 0))
			{
				return true;
			}
			return false;
		}

		internal CultureTableRecord CloneWithUserOverride(bool userOverride)
		{
			if (m_bUseUserOverride == userOverride)
			{
				return this;
			}
			CultureTableRecord cultureTableRecord = (CultureTableRecord)MemberwiseClone();
			cultureTableRecord.m_bUseUserOverride = userOverride;
			return cultureTableRecord;
		}

		public unsafe override bool Equals(object value)
		{
			CultureTableRecord cultureTableRecord = value as CultureTableRecord;
			if (cultureTableRecord != null)
			{
				if (m_pData == cultureTableRecord.m_pData && m_bUseUserOverride == cultureTableRecord.m_bUseUserOverride && m_CultureID == cultureTableRecord.m_CultureID && CultureInfo.InvariantCulture.CompareInfo.Compare(m_CultureName, cultureTableRecord.m_CultureName, CompareOptions.IgnoreCase) == 0)
				{
					return m_CultureTable.Equals(cultureTableRecord.m_CultureTable);
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (!IsCustomCultureId(m_CultureID))
			{
				return m_CultureID;
			}
			return m_CultureName.GetHashCode();
		}

		private unsafe string GetString(uint iOffset)
		{
			char* ptr = (char*)(m_pPool + iOffset);
			if (ptr[1] == '\0')
			{
				return string.Empty;
			}
			return new string(ptr + 1, 0, *ptr);
		}

		private string GetOverrideString(uint iOffset, int iWindowsFlag)
		{
			if (UseGetLocaleInfo)
			{
				string text = CultureInfo.nativeGetLocaleInfo(InteropLCID, iWindowsFlag);
				if (text != null && text.Length > 0)
				{
					return text;
				}
			}
			return GetString(iOffset);
		}

		private unsafe string[] GetStringArray(uint iOffset)
		{
			if (iOffset == 0)
			{
				return new string[0];
			}
			ushort* ptr = m_pPool + iOffset;
			int num = *ptr;
			string[] array = new string[num];
			uint* ptr2 = (uint*)(ptr + 1);
			for (int i = 0; i < num; i++)
			{
				array[i] = GetString(ptr2[i]);
			}
			return array;
		}

		private unsafe string GetStringArrayDefault(uint iOffset)
		{
			if (iOffset == 0)
			{
				return string.Empty;
			}
			ushort* ptr = m_pPool + iOffset;
			uint* ptr2 = (uint*)(ptr + 1);
			return GetString(*ptr2);
		}

		private string GetOverrideStringArrayDefault(uint iOffset, int iWindowsFlag)
		{
			if (UseGetLocaleInfo)
			{
				string text = CultureInfo.nativeGetLocaleInfo(InteropLCID, iWindowsFlag);
				if (text != null && text.Length > 0)
				{
					return text;
				}
			}
			return GetStringArrayDefault(iOffset);
		}

		private ushort GetOverrideUSHORT(ushort iData, int iWindowsFlag)
		{
			if (UseGetLocaleInfo)
			{
				string text = CultureInfo.nativeGetLocaleInfo(InteropLCID, iWindowsFlag);
				if (text != null && text.Length > 0 && short.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var result))
				{
					return (ushort)result;
				}
			}
			return iData;
		}

		private unsafe int[] GetWordArray(uint iData)
		{
			if (iData == 0)
			{
				return new int[0];
			}
			ushort* ptr = m_pPool + iData;
			int num = *ptr;
			int[] array = new int[num];
			ptr++;
			for (int i = 0; i < num; i++)
			{
				array[i] = ptr[i];
			}
			return array;
		}

		private int[] GetOverrideGrouping(uint iData, int iWindowsFlag)
		{
			if (UseGetLocaleInfo)
			{
				string text = CultureInfo.nativeGetLocaleInfo(InteropLCID, iWindowsFlag);
				if (text != null && text.Length > 0)
				{
					int[] array = ConvertWin32GroupString(text);
					if (array != null)
					{
						return array;
					}
				}
			}
			return GetWordArray(iData);
		}

		private bool IsOptionalCalendar(int calendarId)
		{
			for (int i = 0; i < IOPTIONALCALENDARS.Length; i++)
			{
				if (IOPTIONALCALENDARS[i] == calendarId)
				{
					return true;
				}
			}
			return false;
		}

		internal static bool IsCustomCultureId(int cultureId)
		{
			if (cultureId == 3072 || cultureId == 4096)
			{
				return true;
			}
			return false;
		}

		private ushort ConvertFirstDayOfWeekMonToSun(int iTemp)
		{
			switch (iTemp)
			{
			default:
				iTemp = 1;
				break;
			case 6:
				iTemp = 0;
				break;
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
				iTemp++;
				break;
			}
			return (ushort)iTemp;
		}

		private static int[] ConvertWin32GroupString(string win32Str)
		{
			if (win32Str == null || win32Str.Length == 0 || win32Str[0] == '0')
			{
				return new int[1]
				{
					3
				};
			}
			int[] array;
			if (win32Str[win32Str.Length - 1] == '0')
			{
				array = new int[win32Str.Length / 2];
			}
			else
			{
				array = new int[win32Str.Length / 2 + 2];
				array[array.Length - 1] = 0;
			}
			int num = 0;
			int num2 = 0;
			while (num < win32Str.Length && num2 < array.Length)
			{
				if (win32Str[num] < '1' || win32Str[num] > '9')
				{
					return new int[1]
					{
						3
					};
				}
				array[num2] = win32Str[num] - 48;
				num += 2;
				num2++;
			}
			return array;
		}

		private static string UnescapeWin32String(string str, int start, int end)
		{
			StringBuilder stringBuilder = null;
			bool flag = false;
			for (int i = start; i < str.Length && i <= end; i++)
			{
				if (str[i] == '\'')
				{
					if (flag)
					{
						if (i + 1 < str.Length && str[i + 1] == '\'')
						{
							stringBuilder.Append('\'');
							i++;
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = true;
						if (stringBuilder == null)
						{
							stringBuilder = new StringBuilder(str, start, i - start, str.Length);
						}
					}
				}
				else
				{
					stringBuilder?.Append(str[i]);
				}
			}
			if (stringBuilder == null)
			{
				return str.Substring(start, end - start + 1);
			}
			return stringBuilder.ToString();
		}

		private static string ReescapeWin32String(string str)
		{
			if (str == null)
			{
				return null;
			}
			StringBuilder stringBuilder = null;
			bool flag = false;
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '\'')
				{
					if (flag)
					{
						if (i + 1 < str.Length && str[i + 1] == '\'')
						{
							if (stringBuilder == null)
							{
								stringBuilder = new StringBuilder(str, 0, i, str.Length * 2);
							}
							stringBuilder.Append("\\'");
							i++;
							continue;
						}
						flag = false;
					}
					else
					{
						flag = true;
					}
				}
				else if (str[i] == '\\')
				{
					if (stringBuilder == null)
					{
						stringBuilder = new StringBuilder(str, 0, i, str.Length * 2);
					}
					stringBuilder.Append("\\\\");
					continue;
				}
				stringBuilder?.Append(str[i]);
			}
			if (stringBuilder == null)
			{
				return str;
			}
			return stringBuilder.ToString();
		}

		private static string[] ReescapeWin32Strings(string[] array)
		{
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = ReescapeWin32String(array[i]);
				}
			}
			return array;
		}

		private static string GetTimeSeparator(string format)
		{
			string result = string.Empty;
			int num = 0;
			int num2 = -1;
			for (num = 0; num < format.Length; num++)
			{
				if (format[num] == 'H' || format[num] == 'h' || format[num] == 'm' || format[num] == 's')
				{
					char c = format[num];
					for (num++; num < format.Length && format[num] == c; num++)
					{
					}
					if (num < format.Length)
					{
						num2 = num;
					}
					break;
				}
				if (format[num] == '\'')
				{
					for (num++; num < format.Length && format[num] != '\''; num++)
					{
					}
				}
			}
			if (num2 != -1)
			{
				for (num = num2; num < format.Length; num++)
				{
					if (format[num] == 'H' || format[num] == 'h' || format[num] == 'm' || format[num] == 's')
					{
						result = UnescapeWin32String(format, num2, num - 1);
						break;
					}
					if (format[num] == '\'')
					{
						for (num++; num < format.Length && format[num] != '\''; num++)
						{
						}
					}
				}
			}
			return result;
		}

		private static string GetDateSeparator(string format)
		{
			string result = string.Empty;
			int num = 0;
			int num2 = -1;
			for (num = 0; num < format.Length; num++)
			{
				if (format[num] == 'd' || format[num] == 'y' || format[num] == 'M')
				{
					char c = format[num];
					for (num++; num < format.Length && format[num] == c; num++)
					{
					}
					if (num < format.Length)
					{
						num2 = num;
					}
					break;
				}
				if (format[num] == '\'')
				{
					for (num++; num < format.Length && format[num] != '\''; num++)
					{
					}
				}
			}
			if (num2 != -1)
			{
				for (num = num2; num < format.Length; num++)
				{
					if (format[num] == 'y' || format[num] == 'M' || format[num] == 'd')
					{
						result = UnescapeWin32String(format, num2, num - 1);
						break;
					}
					if (format[num] == '\'')
					{
						for (num++; num < format.Length && format[num] != '\''; num++)
						{
						}
					}
				}
			}
			return result;
		}

		internal void GetDTFIOverrideValues(ref DTFIUserOverrideValues values)
		{
			bool flag = false;
			if (UseGetLocaleInfo)
			{
				flag = CultureInfo.nativeGetDTFIUserValues(InteropLCID, ref values);
			}
			if (flag)
			{
				values.firstDayOfWeek = ConvertFirstDayOfWeekMonToSun(values.firstDayOfWeek);
				values.shortDatePattern = ReescapeWin32String(values.shortDatePattern);
				values.longDatePattern = ReescapeWin32String(values.longDatePattern);
				values.longTimePattern = ReescapeWin32String(values.longTimePattern);
				values.yearMonthPattern = ReescapeWin32String(values.yearMonthPattern);
			}
			else
			{
				values.firstDayOfWeek = IFIRSTDAYOFWEEK;
				values.calendarWeekRule = IFIRSTWEEKOFYEAR;
				values.shortDatePattern = SSHORTDATE;
				values.longDatePattern = SLONGDATE;
				values.yearMonthPattern = SYEARMONTH;
				values.amDesignator = S1159;
				values.pmDesignator = S2359;
				values.longTimePattern = STIMEFORMAT;
			}
		}

		internal void GetNFIOverrideValues(NumberFormatInfo nfi)
		{
			bool flag = false;
			if (UseGetLocaleInfo)
			{
				flag = CultureInfo.nativeGetNFIUserValues(InteropLCID, nfi);
			}
			if (!flag)
			{
				nfi.numberDecimalDigits = IDIGITS;
				nfi.numberNegativePattern = INEGNUMBER;
				nfi.currencyDecimalDigits = ICURRDIGITS;
				nfi.currencyPositivePattern = ICURRENCY;
				nfi.currencyNegativePattern = INEGCURR;
				nfi.negativeSign = SNEGATIVESIGN;
				nfi.numberDecimalSeparator = SDECIMAL;
				nfi.numberGroupSeparator = STHOUSAND;
				nfi.positiveSign = SPOSITIVESIGN;
				nfi.currencyDecimalSeparator = SMONDECIMALSEP;
				nfi.currencySymbol = SCURRENCY;
				nfi.currencyGroupSeparator = SMONTHOUSANDSEP;
				nfi.nativeDigits = SNATIVEDIGITS;
				nfi.digitSubstitution = IDIGITSUBSTITUTION;
			}
			else if (-1 == nfi.digitSubstitution)
			{
				nfi.digitSubstitution = IDIGITSUBSTITUTION;
			}
			nfi.numberGroupSizes = SGROUPING;
			nfi.currencyGroupSizes = SMONGROUPING;
			nfi.percentDecimalDigits = nfi.numberDecimalDigits;
			nfi.percentDecimalSeparator = nfi.numberDecimalSeparator;
			nfi.percentGroupSizes = nfi.numberGroupSizes;
			nfi.percentGroupSeparator = nfi.numberGroupSeparator;
			nfi.percentNegativePattern = INEGATIVEPERCENT;
			nfi.percentPositivePattern = IPOSITIVEPERCENT;
			nfi.percentSymbol = SPERCENT;
			if (nfi.positiveSign == null || nfi.positiveSign.Length == 0)
			{
				nfi.positiveSign = "+";
			}
			if (nfi.currencyDecimalSeparator.Length == 0)
			{
				nfi.currencyDecimalSeparator = SMONDECIMALSEP;
			}
		}

		internal unsafe int EverettDataItem()
		{
			if (IsCustomCulture)
			{
				return 0;
			}
			InitEverettCultureDataItemMapping();
			int num = 0;
			int num2 = m_EverettCultureDataItemMappingsSize / 2 - 1;
			while (num <= num2)
			{
				int num3 = (num + num2) / 2;
				int num4 = m_CultureID - m_EverettCultureDataItemMappings[num3 * 2];
				if (num4 == 0)
				{
					return m_EverettCultureDataItemMappings[num3 * 2 + 1];
				}
				if (num4 < 0)
				{
					num2 = num3 - 1;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return 0;
		}

		internal unsafe int EverettRegionDataItem()
		{
			if (IsCustomCulture)
			{
				return 0;
			}
			InitEverettRegionDataItemMapping();
			int num = 0;
			int num2 = m_EverettRegionDataItemMappingsSize / 2 - 1;
			while (num <= num2)
			{
				int num3 = (num + num2) / 2;
				int num4 = m_CultureID - m_EverettRegionDataItemMappings[num3 * 2];
				if (num4 == 0)
				{
					return m_EverettRegionDataItemMappings[num3 * 2 + 1];
				}
				if (num4 < 0)
				{
					num2 = num3 - 1;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return 0;
		}

		internal unsafe static int IdFromEverettDataItem(int iDataItem)
		{
			InitEverettDataItemToLCIDMappings();
			if (iDataItem < 0 || iDataItem >= m_EverettDataItemToLCIDMappingsSize)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFieldState"));
			}
			return m_EverettDataItemToLCIDMappings[iDataItem];
		}

		internal unsafe static int IdFromEverettRegionInfoDataItem(int iDataItem)
		{
			InitEverettRegionDataItemToLCIDMappings();
			if (iDataItem < 0 || iDataItem >= m_EverettRegionInfoDataItemToLCIDMappingsSize)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFieldState"));
			}
			return m_EverettRegionInfoDataItemToLCIDMappings[iDataItem];
		}

		private unsafe static void InitEverettRegionDataItemMapping()
		{
			if (m_EverettRegionDataItemMappings == null)
			{
				int* ptr = (m_EverettRegionDataItemMappings = CultureInfo.nativeGetStaticInt32DataTable(0, out m_EverettRegionDataItemMappingsSize));
			}
		}

		private unsafe static void InitEverettCultureDataItemMapping()
		{
			if (m_EverettCultureDataItemMappings == null)
			{
				int* ptr = (m_EverettCultureDataItemMappings = CultureInfo.nativeGetStaticInt32DataTable(1, out m_EverettCultureDataItemMappingsSize));
			}
		}

		private unsafe static void InitEverettDataItemToLCIDMappings()
		{
			if (m_EverettDataItemToLCIDMappings == null)
			{
				int* ptr = (m_EverettDataItemToLCIDMappings = CultureInfo.nativeGetStaticInt32DataTable(2, out m_EverettDataItemToLCIDMappingsSize));
			}
		}

		private unsafe static void InitEverettRegionDataItemToLCIDMappings()
		{
			if (m_EverettRegionInfoDataItemToLCIDMappings == null)
			{
				int* ptr = (m_EverettRegionInfoDataItemToLCIDMappings = CultureInfo.nativeGetStaticInt32DataTable(3, out m_EverettRegionInfoDataItemToLCIDMappingsSize));
			}
		}
	}
}
