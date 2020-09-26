using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Win32;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public class CultureInfo : ICloneable, IFormatProvider
	{
		internal const int zh_CHT_CultureID = 31748;

		internal const int zh_CHS_CultureID = 4;

		internal const int sr_CultureID = 31770;

		internal const int sr_SP_Latn_CultureID = 2074;

		internal const int LOCALE_INVARIANT = 127;

		private const int LOCALE_NEUTRAL = 0;

		internal const int LOCALE_USER_DEFAULT = 1024;

		internal const int LOCALE_SYSTEM_DEFAULT = 2048;

		internal const int LOCALE_CUSTOM_DEFAULT = 3072;

		internal const int LOCALE_CUSTOM_UNSPECIFIED = 4096;

		internal const int LOCALE_TRADITIONAL_SPANISH = 1034;

		internal const int LCID_INSTALLED = 1;

		internal const int LCID_SUPPORTED = 2;

		internal int cultureID;

		internal bool m_isReadOnly;

		internal CompareInfo compareInfo;

		internal TextInfo textInfo;

		internal NumberFormatInfo numInfo;

		internal DateTimeFormatInfo dateTimeInfo;

		internal Calendar calendar;

		[NonSerialized]
		internal CultureTableRecord m_cultureTableRecord;

		[NonSerialized]
		internal bool m_isInherited;

		[NonSerialized]
		private bool m_isSafeCrossDomain;

		[NonSerialized]
		private int m_createdDomainID;

		[NonSerialized]
		private CultureInfo m_consoleFallbackCulture;

		internal string m_name;

		[NonSerialized]
		private string m_nonSortName;

		[NonSerialized]
		private string m_sortName;

		private static CultureInfo m_userDefaultCulture;

		private static CultureInfo m_InvariantCultureInfo;

		private static CultureInfo m_userDefaultUICulture;

		private static CultureInfo m_InstalledUICultureInfo;

		private static Hashtable m_LcidCachedCultures;

		private static Hashtable m_NameCachedCultures;

		[NonSerialized]
		private CultureInfo m_parent;

		private int m_dataItem;

		private bool m_useUserOverride;

		internal bool IsSafeCrossDomain => m_isSafeCrossDomain;

		internal int CreatedDomainID => m_createdDomainID;

		public static CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;

		internal static CultureInfo UserDefaultCulture
		{
			get
			{
				CultureInfo cultureInfo = m_userDefaultCulture;
				if (cultureInfo == null)
				{
					m_userDefaultCulture = InvariantCulture;
					cultureInfo = (m_userDefaultCulture = InitUserDefaultCulture());
				}
				return cultureInfo;
			}
		}

		internal static CultureInfo UserDefaultUICulture
		{
			get
			{
				CultureInfo cultureInfo = m_userDefaultUICulture;
				if (cultureInfo == null)
				{
					m_userDefaultUICulture = InvariantCulture;
					cultureInfo = (m_userDefaultUICulture = InitUserDefaultUICulture());
				}
				return cultureInfo;
			}
		}

		public static CultureInfo CurrentUICulture => Thread.CurrentThread.CurrentUICulture;

		public unsafe static CultureInfo InstalledUICulture
		{
			get
			{
				CultureInfo cultureInfo = m_InstalledUICultureInfo;
				if (cultureInfo == null)
				{
					int preferLCID = default(int);
					string fallbackToString = nativeGetSystemDefaultUILanguage(&preferLCID);
					cultureInfo = GetCultureByLCIDOrName(preferLCID, fallbackToString);
					if (cultureInfo == null)
					{
						cultureInfo = new CultureInfo(127, useUserOverride: true);
					}
					cultureInfo.m_isReadOnly = true;
					m_InstalledUICultureInfo = cultureInfo;
				}
				return cultureInfo;
			}
		}

		public static CultureInfo InvariantCulture => m_InvariantCultureInfo;

		public virtual CultureInfo Parent
		{
			get
			{
				if (m_parent == null)
				{
					try
					{
						int iPARENT = m_cultureTableRecord.IPARENT;
						if (iPARENT == 127)
						{
							m_parent = InvariantCulture;
						}
						else if (CultureTableRecord.IsCustomCultureId(iPARENT) || CultureTable.IsOldNeutralChineseCulture(this))
						{
							m_parent = new CultureInfo(m_cultureTableRecord.SPARENT);
						}
						else
						{
							m_parent = new CultureInfo(iPARENT, m_cultureTableRecord.UseUserOverride);
						}
					}
					catch (ArgumentException)
					{
						m_parent = InvariantCulture;
					}
				}
				return m_parent;
			}
		}

		public virtual int LCID => cultureID;

		[ComVisible(false)]
		public virtual int KeyboardLayoutId => m_cultureTableRecord.IINPUTLANGUAGEHANDLE;

		public virtual string Name
		{
			get
			{
				if (m_nonSortName == null)
				{
					m_nonSortName = m_cultureTableRecord.CultureName;
				}
				return m_nonSortName;
			}
		}

		internal string SortName
		{
			get
			{
				if (m_sortName == null)
				{
					if (CultureTableRecord.IsCustomCultureId(cultureID))
					{
						CultureInfo cultureInfo = GetCultureInfo(CompareInfoId);
						if (CultureTableRecord.IsCustomCultureId(cultureInfo.cultureID))
						{
							m_sortName = m_cultureTableRecord.SNAME;
						}
						else
						{
							m_sortName = cultureInfo.SortName;
						}
					}
					else
					{
						m_sortName = m_name;
					}
				}
				return m_sortName;
			}
		}

		[ComVisible(false)]
		public string IetfLanguageTag
		{
			get
			{
				if (CultureTable.IsOldNeutralChineseCulture(this))
				{
					if (LCID == 31748)
					{
						return "zh-Hant";
					}
					if (LCID == 4)
					{
						return "zh-Hans";
					}
				}
				return Name;
			}
		}

		public virtual string DisplayName
		{
			get
			{
				if (m_cultureTableRecord.IsCustomCulture)
				{
					if (m_cultureTableRecord.IsReplacementCulture)
					{
						if (m_cultureTableRecord.IsSynthetic)
						{
							return m_cultureTableRecord.CultureNativeDisplayName;
						}
						return Environment.GetResourceString("Globalization.ci_" + m_name);
					}
					return m_cultureTableRecord.SNATIVEDISPLAYNAME;
				}
				if (m_cultureTableRecord.IsSynthetic)
				{
					return m_cultureTableRecord.CultureNativeDisplayName;
				}
				return Environment.GetResourceString("Globalization.ci_" + m_name);
			}
		}

		public virtual string NativeName => m_cultureTableRecord.SNATIVEDISPLAYNAME;

		public virtual string EnglishName => m_cultureTableRecord.SENGDISPLAYNAME;

		public virtual string TwoLetterISOLanguageName => m_cultureTableRecord.SISO639LANGNAME;

		public virtual string ThreeLetterISOLanguageName => m_cultureTableRecord.SISO639LANGNAME2;

		public virtual string ThreeLetterWindowsLanguageName => m_cultureTableRecord.SABBREVLANGNAME;

		public virtual CompareInfo CompareInfo
		{
			get
			{
				if (compareInfo == null)
				{
					int num = ((!IsNeutralCulture || CultureTableRecord.IsCustomCultureId(cultureID)) ? CompareInfoId : cultureID);
					if (Name == "zh-CHS" || Name == "zh-CHT")
					{
						num |= int.MinValue;
					}
					CompareInfo compareInfoWithPrefixedLcid = CompareInfo.GetCompareInfoWithPrefixedLcid(num, int.MinValue);
					compareInfoWithPrefixedLcid.SetName(SortName);
					compareInfo = compareInfoWithPrefixedLcid;
				}
				return compareInfo;
			}
		}

		internal int CompareInfoId
		{
			get
			{
				if (cultureID == 1034)
				{
					return 1034;
				}
				if (GetSortID(cultureID) != 0)
				{
					return cultureID;
				}
				return (int)m_cultureTableRecord.ICOMPAREINFO;
			}
		}

		public virtual TextInfo TextInfo
		{
			get
			{
				if (this.textInfo == null)
				{
					TextInfo textInfo = new TextInfo(m_cultureTableRecord);
					textInfo.SetReadOnlyState(m_isReadOnly);
					this.textInfo = textInfo;
				}
				return this.textInfo;
			}
		}

		public virtual bool IsNeutralCulture => m_cultureTableRecord.IsNeutralCulture;

		[ComVisible(false)]
		public CultureTypes CultureTypes
		{
			get
			{
				CultureTypes cultureTypes = (CultureTypes)0;
				cultureTypes = ((!m_cultureTableRecord.IsNeutralCulture) ? (cultureTypes | CultureTypes.SpecificCultures) : (cultureTypes | CultureTypes.NeutralCultures));
				if (m_cultureTableRecord.IsSynthetic)
				{
					cultureTypes |= CultureTypes.InstalledWin32Cultures | CultureTypes.WindowsOnlyCultures;
				}
				else
				{
					if (CultureTable.IsInstalledLCID(cultureID))
					{
						cultureTypes |= CultureTypes.InstalledWin32Cultures;
					}
					if (!m_cultureTableRecord.IsCustomCulture || m_cultureTableRecord.IsReplacementCulture)
					{
						cultureTypes |= CultureTypes.FrameworkCultures;
					}
				}
				if (m_cultureTableRecord.IsCustomCulture)
				{
					cultureTypes |= CultureTypes.UserCustomCulture;
					if (m_cultureTableRecord.IsReplacementCulture)
					{
						cultureTypes |= CultureTypes.ReplacementCultures;
					}
				}
				return cultureTypes;
			}
		}

		public virtual NumberFormatInfo NumberFormat
		{
			get
			{
				CheckNeutral(this);
				if (numInfo == null)
				{
					NumberFormatInfo numberFormatInfo = new NumberFormatInfo(m_cultureTableRecord);
					numberFormatInfo.isReadOnly = m_isReadOnly;
					numInfo = numberFormatInfo;
				}
				return numInfo;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Obj"));
				}
				numInfo = value;
			}
		}

		public virtual DateTimeFormatInfo DateTimeFormat
		{
			get
			{
				if (dateTimeInfo == null)
				{
					CheckNeutral(this);
					DateTimeFormatInfo dateTimeFormatInfo = new DateTimeFormatInfo(m_cultureTableRecord, GetLangID(cultureID), Calendar);
					dateTimeFormatInfo.m_isReadOnly = m_isReadOnly;
					Thread.MemoryBarrier();
					dateTimeInfo = dateTimeFormatInfo;
				}
				return dateTimeInfo;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Obj"));
				}
				dateTimeInfo = value;
			}
		}

		public virtual Calendar Calendar
		{
			get
			{
				if (calendar == null)
				{
					int iCALENDARTYPE = m_cultureTableRecord.ICALENDARTYPE;
					Calendar calendarInstance = GetCalendarInstance(iCALENDARTYPE);
					Thread.MemoryBarrier();
					calendarInstance.SetReadOnlyState(m_isReadOnly);
					calendar = calendarInstance;
				}
				return calendar;
			}
		}

		public virtual Calendar[] OptionalCalendars
		{
			get
			{
				int[] iOPTIONALCALENDARS = m_cultureTableRecord.IOPTIONALCALENDARS;
				Calendar[] array = new Calendar[iOPTIONALCALENDARS.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = GetCalendarInstance(iOPTIONALCALENDARS[i]);
				}
				return array;
			}
		}

		public bool UseUserOverride => m_cultureTableRecord.UseUserOverride;

		public bool IsReadOnly => m_isReadOnly;

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool IsValidLCID(int LCID, int flag);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool IsWin9xInstalledCulture(string cultureKey, int LCID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern string nativeGetUserDefaultLCID(int* LCID, int lcidType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern string nativeGetUserDefaultUILanguage(int* LCID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern string nativeGetSystemDefaultUILanguage(int* LCID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool nativeSetThreadLocale(int LCID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nativeGetLocaleInfo(int LCID, int field);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int nativeGetCurrentCalendar();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool nativeGetDTFIUserValues(int lcid, ref DTFIUserOverrideValues values);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool nativeGetNFIUserValues(int lcid, NumberFormatInfo nfi);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool nativeGetCultureData(int lcid, ref CultureData cultureData);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool nativeEnumSystemLocales(out int[] localesArray);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nativeGetCultureName(int lcid, bool useSNameLCType, bool getMonthName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nativeGetWindowsDirectory();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool nativeFileExists(string fileName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern int* nativeGetStaticInt32DataTable(int type, out int tableSize);

		internal unsafe static int GetNativeSortKey(int lcid, int flags, string source, int cchSrc, out byte[] sortKey)
		{
			sortKey = null;
			if (string.IsNullOrEmpty(source) || cchSrc == 0)
			{
				sortKey = new byte[0];
				source = "\0";
				cchSrc = 1;
			}
			int num;
			fixed (char* src = source)
			{
				num = Win32Native.LCMapStringW(lcid, flags | 0x400, src, cchSrc, null, 0);
				if (num == 0)
				{
					return -1;
				}
				if (sortKey == null)
				{
					sortKey = new byte[num];
					fixed (byte* target = sortKey)
					{
						num = Win32Native.LCMapStringW(lcid, flags | 0x400, src, cchSrc, (char*)target, num);
					}
				}
			}
			return num;
		}

		static CultureInfo()
		{
			if (m_InvariantCultureInfo == null)
			{
				m_InvariantCultureInfo = new CultureInfo(127, useUserOverride: false)
				{
					m_isReadOnly = true
				};
			}
			m_userDefaultCulture = (m_userDefaultUICulture = m_InvariantCultureInfo);
			m_userDefaultCulture = InitUserDefaultCulture();
			m_userDefaultUICulture = InitUserDefaultUICulture();
		}

		private unsafe static CultureInfo InitUserDefaultCulture()
		{
			int preferLCID = default(int);
			string fallbackToString = nativeGetUserDefaultLCID(&preferLCID, 1024);
			CultureInfo cultureByLCIDOrName = GetCultureByLCIDOrName(preferLCID, fallbackToString);
			if (cultureByLCIDOrName == null)
			{
				fallbackToString = nativeGetUserDefaultLCID(&preferLCID, 2048);
				cultureByLCIDOrName = GetCultureByLCIDOrName(preferLCID, fallbackToString);
				if (cultureByLCIDOrName == null)
				{
					return InvariantCulture;
				}
			}
			cultureByLCIDOrName.m_isReadOnly = true;
			return cultureByLCIDOrName;
		}

		private unsafe static CultureInfo InitUserDefaultUICulture()
		{
			int num = default(int);
			string text = nativeGetUserDefaultUILanguage(&num);
			if (num == UserDefaultCulture.LCID || text == UserDefaultCulture.Name)
			{
				return UserDefaultCulture;
			}
			CultureInfo cultureByLCIDOrName = GetCultureByLCIDOrName(num, text);
			if (cultureByLCIDOrName == null)
			{
				text = nativeGetSystemDefaultUILanguage(&num);
				cultureByLCIDOrName = GetCultureByLCIDOrName(num, text);
			}
			if (cultureByLCIDOrName == null)
			{
				return InvariantCulture;
			}
			cultureByLCIDOrName.m_isReadOnly = true;
			return cultureByLCIDOrName;
		}

		public CultureInfo(string name)
			: this(name, useUserOverride: true)
		{
		}

		public CultureInfo(string name, bool useUserOverride)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name", Environment.GetResourceString("ArgumentNull_String"));
			}
			m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(name, useUserOverride);
			cultureID = m_cultureTableRecord.ActualCultureID;
			m_name = m_cultureTableRecord.ActualName;
			m_isInherited = GetType() != typeof(CultureInfo);
		}

		public CultureInfo(int culture)
			: this(culture, useUserOverride: true)
		{
		}

		public CultureInfo(int culture, bool useUserOverride)
		{
			if (culture < 0)
			{
				throw new ArgumentOutOfRangeException("culture", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			switch (culture)
			{
			case 0:
			case 1024:
			case 2048:
			case 3072:
			case 4096:
				throw new ArgumentException(Environment.GetResourceString("Argument_CultureNotSupported", culture), "culture");
			}
			cultureID = culture;
			m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(cultureID, useUserOverride);
			m_name = m_cultureTableRecord.ActualName;
			m_isInherited = GetType() != typeof(CultureInfo);
		}

		internal static void CheckDomainSafetyObject(object obj, object container)
		{
			if (obj.GetType().Assembly != typeof(CultureInfo).Assembly)
			{
				throw new InvalidOperationException(string.Format(CurrentCulture, Environment.GetResourceString("InvalidOperation_SubclassedObject"), obj.GetType(), container.GetType()));
			}
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			if (m_name != null && cultureID != 1034)
			{
				m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(m_name, m_useUserOverride);
			}
			else
			{
				m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(cultureID, m_useUserOverride);
			}
			m_isInherited = GetType() != typeof(CultureInfo);
			if (m_name == null)
			{
				m_name = m_cultureTableRecord.ActualName;
			}
			if (GetType().Assembly == typeof(CultureInfo).Assembly)
			{
				if (textInfo != null)
				{
					CheckDomainSafetyObject(textInfo, this);
				}
				if (compareInfo != null)
				{
					CheckDomainSafetyObject(compareInfo, this);
				}
			}
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext ctx)
		{
			m_name = m_cultureTableRecord.CultureName;
			m_useUserOverride = m_cultureTableRecord.UseUserOverride;
			m_dataItem = m_cultureTableRecord.EverettDataItem();
		}

		internal void StartCrossDomainTracking()
		{
			if (m_createdDomainID == 0)
			{
				if (GetType() == typeof(CultureInfo))
				{
					m_isSafeCrossDomain = true;
				}
				Thread.MemoryBarrier();
				m_createdDomainID = Thread.GetDomainID();
			}
		}

		internal CultureInfo(string cultureName, string textAndCompareCultureName)
		{
			if (cultureName == null)
			{
				throw new ArgumentNullException("cultureName", Environment.GetResourceString("ArgumentNull_String"));
			}
			m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(cultureName, useUserOverride: false);
			cultureID = m_cultureTableRecord.ActualCultureID;
			m_name = m_cultureTableRecord.ActualName;
			CultureInfo cultureInfo = GetCultureInfo(textAndCompareCultureName);
			compareInfo = cultureInfo.CompareInfo;
			textInfo = cultureInfo.TextInfo;
		}

		private static CultureInfo GetCultureByLCIDOrName(int preferLCID, string fallbackToString)
		{
			CultureInfo cultureInfo = null;
			if (((uint)preferLCID & 0x3FFu) != 0)
			{
				try
				{
					cultureInfo = new CultureInfo(preferLCID);
				}
				catch (ArgumentException)
				{
				}
			}
			if (cultureInfo == null && fallbackToString != null && fallbackToString.Length > 0)
			{
				try
				{
					cultureInfo = new CultureInfo(fallbackToString);
					return cultureInfo;
				}
				catch (ArgumentException)
				{
					return cultureInfo;
				}
			}
			return cultureInfo;
		}

		public static CultureInfo CreateSpecificCulture(string name)
		{
			CultureInfo cultureInfo;
			try
			{
				cultureInfo = new CultureInfo(name);
			}
			catch (ArgumentException ex)
			{
				cultureInfo = null;
				for (int i = 0; i < name.Length; i++)
				{
					if ('-' == name[i])
					{
						try
						{
							cultureInfo = new CultureInfo(name.Substring(0, i));
						}
						catch (ArgumentException)
						{
							throw ex;
						}
						break;
					}
				}
				if (cultureInfo == null)
				{
					throw ex;
				}
			}
			if (!cultureInfo.IsNeutralCulture)
			{
				return cultureInfo;
			}
			int lCID = cultureInfo.LCID;
			if ((lCID & 0x3FF) == 4)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NoSpecificCulture"));
			}
			return new CultureInfo(cultureInfo.m_cultureTableRecord.SSPECIFICCULTURE);
		}

		internal static bool VerifyCultureName(CultureInfo culture, bool throwException)
		{
			if (!culture.m_isInherited)
			{
				return true;
			}
			string name = culture.Name;
			foreach (char c in name)
			{
				if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
				{
					if (throwException)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_InvalidResourceCultureName", name));
					}
					return false;
				}
			}
			return true;
		}

		internal static int GetSubLangID(int culture)
		{
			return (culture >> 10) & 0x3F;
		}

		internal static int GetLangID(int culture)
		{
			return culture & 0xFFFF;
		}

		internal static int GetSortID(int lcid)
		{
			return (lcid >> 16) & 0xF;
		}

		public static CultureInfo[] GetCultures(CultureTypes types)
		{
			return CultureTable.Default.GetCultures(types);
		}

		public override bool Equals(object value)
		{
			if (object.ReferenceEquals(this, value))
			{
				return true;
			}
			CultureInfo cultureInfo = value as CultureInfo;
			if (cultureInfo != null)
			{
				if (Name.Equals(cultureInfo.Name))
				{
					return CompareInfo.Equals(cultureInfo.CompareInfo);
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode() + CompareInfo.GetHashCode();
		}

		public override string ToString()
		{
			return m_name;
		}

		public virtual object GetFormat(Type formatType)
		{
			if (formatType == typeof(NumberFormatInfo))
			{
				return NumberFormat;
			}
			if (formatType == typeof(DateTimeFormatInfo))
			{
				return DateTimeFormat;
			}
			return null;
		}

		internal static void CheckNeutral(CultureInfo culture)
		{
			if (culture.IsNeutralCulture)
			{
				throw new NotSupportedException(Environment.GetResourceString("Argument_CultureInvalidFormat", culture.m_name));
			}
		}

		public void ClearCachedData()
		{
			m_userDefaultUICulture = null;
			m_userDefaultCulture = null;
			RegionInfo.m_currentRegionInfo = null;
			TimeZone.ResetTimeZone();
			m_LcidCachedCultures = null;
			m_NameCachedCultures = null;
			CultureTableRecord.ResetCustomCulturesCache();
			CompareInfo.ClearDefaultAssemblyCache();
		}

		internal static Calendar GetCalendarInstance(int calType)
		{
			if (calType == 1)
			{
				return new GregorianCalendar();
			}
			return GetCalendarInstanceRare(calType);
		}

		internal static Calendar GetCalendarInstanceRare(int calType)
		{
			switch (calType)
			{
			case 2:
			case 9:
			case 10:
			case 11:
			case 12:
				return new GregorianCalendar((GregorianCalendarTypes)calType);
			case 4:
				return new TaiwanCalendar();
			case 3:
				return new JapaneseCalendar();
			case 5:
				return new KoreanCalendar();
			case 6:
				return new HijriCalendar();
			case 7:
				return new ThaiBuddhistCalendar();
			case 8:
				return new HebrewCalendar();
			case 22:
				return new PersianCalendar();
			case 23:
				return new UmAlQuraCalendar();
			case 15:
				return new ChineseLunisolarCalendar();
			case 14:
				return new JapaneseLunisolarCalendar();
			case 20:
				return new KoreanLunisolarCalendar();
			case 21:
				return new TaiwanLunisolarCalendar();
			default:
				return new GregorianCalendar();
			}
		}

		[ComVisible(false)]
		public CultureInfo GetConsoleFallbackUICulture()
		{
			CultureInfo cultureInfo = m_consoleFallbackCulture;
			if (cultureInfo == null)
			{
				cultureInfo = GetCultureInfo(m_cultureTableRecord.SCONSOLEFALLBACKNAME);
				cultureInfo.m_isReadOnly = true;
				m_consoleFallbackCulture = cultureInfo;
			}
			return cultureInfo;
		}

		public virtual object Clone()
		{
			CultureInfo cultureInfo = (CultureInfo)MemberwiseClone();
			cultureInfo.m_isReadOnly = false;
			if (!cultureInfo.IsNeutralCulture)
			{
				if (!m_isInherited)
				{
					if (dateTimeInfo != null)
					{
						cultureInfo.dateTimeInfo = (DateTimeFormatInfo)dateTimeInfo.Clone();
					}
					if (numInfo != null)
					{
						cultureInfo.numInfo = (NumberFormatInfo)numInfo.Clone();
					}
				}
				else
				{
					cultureInfo.DateTimeFormat = (DateTimeFormatInfo)DateTimeFormat.Clone();
					cultureInfo.NumberFormat = (NumberFormatInfo)NumberFormat.Clone();
				}
			}
			if (textInfo != null)
			{
				cultureInfo.textInfo = (TextInfo)textInfo.Clone();
			}
			if (calendar != null)
			{
				cultureInfo.calendar = (Calendar)calendar.Clone();
			}
			return cultureInfo;
		}

		public static CultureInfo ReadOnly(CultureInfo ci)
		{
			if (ci == null)
			{
				throw new ArgumentNullException("ci");
			}
			if (ci.IsReadOnly)
			{
				return ci;
			}
			CultureInfo cultureInfo = (CultureInfo)ci.MemberwiseClone();
			if (!ci.IsNeutralCulture)
			{
				if (!ci.m_isInherited)
				{
					if (ci.dateTimeInfo != null)
					{
						cultureInfo.dateTimeInfo = DateTimeFormatInfo.ReadOnly(ci.dateTimeInfo);
					}
					if (ci.numInfo != null)
					{
						cultureInfo.numInfo = NumberFormatInfo.ReadOnly(ci.numInfo);
					}
				}
				else
				{
					cultureInfo.DateTimeFormat = DateTimeFormatInfo.ReadOnly(ci.DateTimeFormat);
					cultureInfo.NumberFormat = NumberFormatInfo.ReadOnly(ci.NumberFormat);
				}
			}
			if (ci.textInfo != null)
			{
				cultureInfo.textInfo = TextInfo.ReadOnly(ci.textInfo);
			}
			if (ci.calendar != null)
			{
				cultureInfo.calendar = Calendar.ReadOnly(ci.calendar);
			}
			cultureInfo.m_isReadOnly = true;
			return cultureInfo;
		}

		private void VerifyWritable()
		{
			if (m_isReadOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
			}
		}

		internal static CultureInfo GetCultureInfoHelper(int lcid, string name, string altName)
		{
			Hashtable hashtable = m_NameCachedCultures;
			if (name != null)
			{
				name = CultureTableRecord.AnsiToLower(name);
			}
			if (altName != null)
			{
				altName = CultureTableRecord.AnsiToLower(altName);
			}
			CultureInfo cultureInfo;
			if (hashtable == null)
			{
				hashtable = Hashtable.Synchronized(new Hashtable());
			}
			else
			{
				switch (lcid)
				{
				case -1:
					cultureInfo = (CultureInfo)hashtable[name + '\ufffd' + altName];
					if (cultureInfo != null)
					{
						return cultureInfo;
					}
					break;
				case 0:
					cultureInfo = (CultureInfo)hashtable[name];
					if (cultureInfo != null)
					{
						return cultureInfo;
					}
					break;
				}
			}
			Hashtable hashtable2 = m_LcidCachedCultures;
			if (hashtable2 == null)
			{
				hashtable2 = Hashtable.Synchronized(new Hashtable());
			}
			else if (lcid > 0)
			{
				cultureInfo = (CultureInfo)hashtable2[lcid];
				if (cultureInfo != null)
				{
					return cultureInfo;
				}
			}
			try
			{
				switch (lcid)
				{
				case -1:
					cultureInfo = new CultureInfo(name, altName);
					break;
				case 0:
					cultureInfo = new CultureInfo(name, useUserOverride: false);
					break;
				default:
					if (m_userDefaultCulture != null && m_userDefaultCulture.LCID == lcid)
					{
						cultureInfo = (CultureInfo)m_userDefaultCulture.Clone();
						cultureInfo.m_cultureTableRecord = cultureInfo.m_cultureTableRecord.CloneWithUserOverride(userOverride: false);
					}
					else
					{
						cultureInfo = new CultureInfo(lcid, useUserOverride: false);
					}
					break;
				}
			}
			catch (ArgumentException)
			{
				return null;
			}
			cultureInfo.m_isReadOnly = true;
			if (lcid == -1)
			{
				hashtable[name + '\ufffd' + altName] = cultureInfo;
				cultureInfo.TextInfo.SetReadOnlyState(readOnly: true);
			}
			else
			{
				if (!CultureTable.IsNewNeutralChineseCulture(cultureInfo))
				{
					hashtable2[cultureInfo.LCID] = cultureInfo;
				}
				string key = CultureTableRecord.AnsiToLower(cultureInfo.m_name);
				hashtable[key] = cultureInfo;
			}
			if (-1 != lcid)
			{
				m_LcidCachedCultures = hashtable2;
			}
			m_NameCachedCultures = hashtable;
			return cultureInfo;
		}

		public static CultureInfo GetCultureInfo(int culture)
		{
			if (culture <= 0)
			{
				throw new ArgumentOutOfRangeException("culture", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			CultureInfo cultureInfoHelper = GetCultureInfoHelper(culture, null, null);
			if (cultureInfoHelper == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_CultureNotSupported", culture), "culture");
			}
			return cultureInfoHelper;
		}

		public static CultureInfo GetCultureInfo(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			CultureInfo cultureInfoHelper = GetCultureInfoHelper(0, name, null);
			if (cultureInfoHelper == null)
			{
				throw new ArgumentException(string.Format(CurrentCulture, Environment.GetResourceString("Argument_InvalidCultureName"), name), "name");
			}
			return cultureInfoHelper;
		}

		public static CultureInfo GetCultureInfo(string name, string altName)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (altName == null)
			{
				throw new ArgumentNullException("altName");
			}
			CultureInfo cultureInfoHelper = GetCultureInfoHelper(-1, name, altName);
			if (cultureInfoHelper == null)
			{
				throw new ArgumentException(string.Format(CurrentCulture, Environment.GetResourceString("Argument_OneOfCulturesNotSupported"), name, altName), "name");
			}
			return cultureInfoHelper;
		}

		public static CultureInfo GetCultureInfoByIetfLanguageTag(string name)
		{
			if ("zh-CHT".Equals(name, StringComparison.OrdinalIgnoreCase) || "zh-CHS".Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException(string.Format(CurrentCulture, Environment.GetResourceString("Argument_CultureIetfNotSupported"), name), "name");
			}
			CultureInfo cultureInfo = GetCultureInfo(name);
			if (GetSortID(cultureInfo.cultureID) != 0 || cultureInfo.cultureID == 1034)
			{
				throw new ArgumentException(string.Format(CurrentCulture, Environment.GetResourceString("Argument_CultureIetfNotSupported"), name), "name");
			}
			return cultureInfo;
		}
	}
}
