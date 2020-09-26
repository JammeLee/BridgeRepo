using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public sealed class DateTimeFormatInfo : ICloneable, IFormatProvider
	{
		internal const string rfc1123Pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";

		internal const string sortableDateTimePattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

		internal const string universalSortableDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";

		private const int DEFAULT_ALL_DATETIMES_SIZE = 132;

		internal const DateTimeStyles InvalidDateTimeStyles = ~(DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal | DateTimeStyles.AssumeUniversal | DateTimeStyles.RoundtripKind);

		private const int TOKEN_HASH_SIZE = 199;

		private const int SECOND_PRIME = 197;

		private const string dateSeparatorOrTimeZoneOffset = "-";

		private const string invariantDateSeparator = "/";

		private const string invariantTimeSeparator = ":";

		internal const string CJKYearSuff = "年";

		internal const string CJKMonthSuff = "月";

		internal const string CJKDaySuff = "日";

		internal const string KoreanYearSuff = "년";

		internal const string KoreanMonthSuff = "월";

		internal const string KoreanDaySuff = "일";

		internal const string KoreanHourSuff = "시";

		internal const string KoreanMinuteSuff = "분";

		internal const string KoreanSecondSuff = "초";

		internal const string CJKHourSuff = "時";

		internal const string ChineseHourSuff = "时";

		internal const string CJKMinuteSuff = "分";

		internal const string CJKSecondSuff = "秒";

		internal const string JapaneseEraStart = "元";

		internal const string LocalTimeMark = "T";

		internal const string KoreanLangName = "ko";

		internal const string JapaneseLangName = "ja";

		internal const string EnglishLangName = "en";

		internal const int CAL_SCALNAME = 2;

		private static DateTimeFormatInfo invariantInfo;

		[NonSerialized]
		internal CultureTableRecord m_cultureTableRecord;

		[OptionalField(VersionAdded = 2)]
		internal string m_name;

		[NonSerialized]
		internal string m_langName;

		[NonSerialized]
		internal CompareInfo m_compareInfo;

		internal bool m_isDefaultCalendar;

		internal bool bUseCalendarInfo;

		internal string amDesignator;

		internal string pmDesignator;

		internal string dateSeparator;

		internal string longTimePattern;

		internal string shortTimePattern;

		internal string generalShortTimePattern;

		internal string generalLongTimePattern;

		internal string timeSeparator;

		internal string monthDayPattern;

		[OptionalField(VersionAdded = 3)]
		internal string dateTimeOffsetPattern;

		internal string[] allShortTimePatterns;

		internal string[] allLongTimePatterns;

		internal Calendar calendar;

		internal int firstDayOfWeek = -1;

		internal int calendarWeekRule = -1;

		internal string fullDateTimePattern;

		internal string longDatePattern;

		internal string shortDatePattern;

		internal string yearMonthPattern;

		internal string[] abbreviatedDayNames;

		[OptionalField(VersionAdded = 2)]
		internal string[] m_superShortDayNames;

		internal string[] dayNames;

		internal string[] abbreviatedMonthNames;

		internal string[] monthNames;

		[OptionalField(VersionAdded = 2)]
		internal string[] genitiveMonthNames;

		[OptionalField(VersionAdded = 2)]
		internal string[] m_genitiveAbbreviatedMonthNames;

		[OptionalField(VersionAdded = 2)]
		internal string[] leapYearMonthNames;

		[NonSerialized]
		internal string[] allYearMonthPatterns;

		internal string[] allShortDatePatterns;

		internal string[] allLongDatePatterns;

		internal string[] m_eraNames;

		internal string[] m_abbrevEraNames;

		internal string[] m_abbrevEnglishEraNames;

		internal string[] m_dateWords;

		internal int[] optionalCalendars;

		internal bool m_isReadOnly;

		[OptionalField(VersionAdded = 2)]
		internal DateTimeFormatFlags formatFlags = DateTimeFormatFlags.NotInitialized;

		private static Hashtable m_calendarNativeNames;

		private static object s_InternalSyncObject;

		private int CultureID;

		private bool m_useUserOverride;

		private int nDataItem;

		private static char[] MonthSpaces = new char[2]
		{
			' ',
			'\u00a0'
		};

		[NonSerialized]
		private TokenHashValue[] m_dtfiTokenHash;

		[NonSerialized]
		private bool m_scanDateWords;

		private static DateTimeFormatInfo m_jajpDTFI = null;

		private static DateTimeFormatInfo m_zhtwDTFI = null;

		internal int CultureId => m_cultureTableRecord.CultureID;

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

		internal string CultureName
		{
			get
			{
				if (m_name == null)
				{
					m_name = m_cultureTableRecord.SNAME;
				}
				return m_name;
			}
		}

		internal string LanguageName
		{
			get
			{
				if (m_langName == null)
				{
					m_langName = m_cultureTableRecord.SISO639LANGNAME;
				}
				return m_langName;
			}
		}

		public static DateTimeFormatInfo InvariantInfo
		{
			get
			{
				if (invariantInfo == null)
				{
					DateTimeFormatInfo dateTimeFormatInfo = new DateTimeFormatInfo();
					dateTimeFormatInfo.Calendar.SetReadOnlyState(readOnly: true);
					dateTimeFormatInfo.m_isReadOnly = true;
					invariantInfo = dateTimeFormatInfo;
				}
				return invariantInfo;
			}
		}

		public static DateTimeFormatInfo CurrentInfo
		{
			get
			{
				CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
				if (!currentCulture.m_isInherited)
				{
					DateTimeFormatInfo dateTimeInfo = currentCulture.dateTimeInfo;
					if (dateTimeInfo != null)
					{
						return dateTimeInfo;
					}
				}
				return (DateTimeFormatInfo)currentCulture.GetFormat(typeof(DateTimeFormatInfo));
			}
		}

		public string AMDesignator
		{
			get
			{
				return amDesignator;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				ClearTokenHashTable(scanDateWords: true);
				amDesignator = value;
			}
		}

		public Calendar Calendar
		{
			get
			{
				return calendar;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Obj"));
				}
				if (value == calendar)
				{
					return;
				}
				CultureInfo.CheckDomainSafetyObject(value, this);
				for (int i = 0; i < OptionalCalendars.Length; i++)
				{
					if (OptionalCalendars[i] != value.ID)
					{
						continue;
					}
					ClearTokenHashTable(scanDateWords: false);
					m_isDefaultCalendar = value.ID == 1;
					if (calendar != null)
					{
						m_eraNames = null;
						m_abbrevEraNames = null;
						m_abbrevEnglishEraNames = null;
						shortDatePattern = null;
						yearMonthPattern = null;
						monthDayPattern = null;
						longDatePattern = null;
						dayNames = null;
						abbreviatedDayNames = null;
						m_superShortDayNames = null;
						monthNames = null;
						abbreviatedMonthNames = null;
						genitiveMonthNames = null;
						m_genitiveAbbreviatedMonthNames = null;
						leapYearMonthNames = null;
						formatFlags = DateTimeFormatFlags.NotInitialized;
						fullDateTimePattern = null;
						generalShortTimePattern = null;
						generalLongTimePattern = null;
						allShortDatePatterns = null;
						allLongDatePatterns = null;
						allYearMonthPatterns = null;
						dateTimeOffsetPattern = null;
					}
					calendar = value;
					if (m_cultureTableRecord.UseCurrentCalendar(value.ID))
					{
						DTFIUserOverrideValues values = default(DTFIUserOverrideValues);
						m_cultureTableRecord.GetDTFIOverrideValues(ref values);
						if (m_cultureTableRecord.SLONGDATE != values.longDatePattern || m_cultureTableRecord.SSHORTDATE != values.shortDatePattern || m_cultureTableRecord.STIMEFORMAT != values.longTimePattern || m_cultureTableRecord.SYEARMONTH != values.yearMonthPattern)
						{
							m_scanDateWords = true;
						}
						amDesignator = values.amDesignator;
						pmDesignator = values.pmDesignator;
						longTimePattern = values.longTimePattern;
						firstDayOfWeek = values.firstDayOfWeek;
						calendarWeekRule = values.calendarWeekRule;
						shortDatePattern = values.shortDatePattern;
						longDatePattern = values.longDatePattern;
						yearMonthPattern = values.yearMonthPattern;
						if (yearMonthPattern == null || yearMonthPattern.Length == 0)
						{
							yearMonthPattern = GetYearMonthPattern(value.ID);
						}
					}
					else
					{
						InitializeOverridableProperties();
					}
					return;
				}
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Argument_InvalidCalendar"));
			}
		}

		internal int[] OptionalCalendars
		{
			get
			{
				if (optionalCalendars == null)
				{
					optionalCalendars = m_cultureTableRecord.IOPTIONALCALENDARS;
				}
				return optionalCalendars;
			}
		}

		internal string[] EraNames
		{
			get
			{
				if (m_eraNames == null)
				{
					if (Calendar.ID == 1)
					{
						m_eraNames = new string[1]
						{
							m_cultureTableRecord.SADERA
						};
					}
					else if (Calendar.ID != 4)
					{
						m_eraNames = CalendarTable.Default.SERANAMES(Calendar.ID);
					}
					else
					{
						m_eraNames = new string[1]
						{
							CalendarTable.nativeGetEraName(1028, Calendar.ID)
						};
					}
				}
				return m_eraNames;
			}
		}

		internal string[] AbbreviatedEraNames
		{
			get
			{
				if (m_abbrevEraNames == null)
				{
					if (Calendar.ID == 4)
					{
						string eraName = GetEraName(1);
						if (eraName.Length > 0)
						{
							if (eraName.Length == 4)
							{
								m_abbrevEraNames = new string[1]
								{
									eraName.Substring(2, 2)
								};
							}
							else
							{
								m_abbrevEraNames = new string[1]
								{
									eraName
								};
							}
						}
						else
						{
							m_abbrevEraNames = new string[0];
						}
					}
					else if (Calendar.ID == 1)
					{
						m_abbrevEraNames = new string[1]
						{
							m_cultureTableRecord.SABBREVADERA
						};
					}
					else
					{
						m_abbrevEraNames = CalendarTable.Default.SABBREVERANAMES(Calendar.ID);
					}
				}
				return m_abbrevEraNames;
			}
		}

		internal string[] AbbreviatedEnglishEraNames
		{
			get
			{
				if (m_abbrevEnglishEraNames == null)
				{
					m_abbrevEnglishEraNames = CalendarTable.Default.SABBREVENGERANAMES(Calendar.ID);
				}
				return m_abbrevEnglishEraNames;
			}
		}

		public string DateSeparator
		{
			get
			{
				if (dateSeparator == null)
				{
					if (Calendar.ID == 3 && !GregorianCalendarHelper.EnforceLegacyJapaneseDateParsing)
					{
						dateSeparator = "/";
					}
					else
					{
						dateSeparator = m_cultureTableRecord.SDATE;
					}
				}
				return dateSeparator;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				ClearTokenHashTable(scanDateWords: true);
				dateSeparator = value;
			}
		}

		public DayOfWeek FirstDayOfWeek
		{
			get
			{
				return (DayOfWeek)firstDayOfWeek;
			}
			set
			{
				VerifyWritable();
				if (value >= DayOfWeek.Sunday && value <= DayOfWeek.Saturday)
				{
					firstDayOfWeek = (int)value;
					return;
				}
				throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), DayOfWeek.Sunday, DayOfWeek.Saturday));
			}
		}

		public CalendarWeekRule CalendarWeekRule
		{
			get
			{
				return (CalendarWeekRule)calendarWeekRule;
			}
			set
			{
				VerifyWritable();
				if (value >= CalendarWeekRule.FirstDay && value <= CalendarWeekRule.FirstFourDayWeek)
				{
					calendarWeekRule = (int)value;
					return;
				}
				throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), CalendarWeekRule.FirstDay, CalendarWeekRule.FirstFourDayWeek));
			}
		}

		public string FullDateTimePattern
		{
			get
			{
				if (fullDateTimePattern == null)
				{
					fullDateTimePattern = LongDatePattern + " " + LongTimePattern;
				}
				return fullDateTimePattern;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				fullDateTimePattern = value;
			}
		}

		public string LongDatePattern
		{
			get
			{
				return longDatePattern;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				ClearTokenHashTable(scanDateWords: true);
				SetDefaultPatternAsFirstItem(allLongDatePatterns, value);
				longDatePattern = value;
				fullDateTimePattern = null;
			}
		}

		public string LongTimePattern
		{
			get
			{
				return longTimePattern;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				longTimePattern = value;
				fullDateTimePattern = null;
				generalLongTimePattern = null;
				dateTimeOffsetPattern = null;
			}
		}

		public string MonthDayPattern
		{
			get
			{
				if (monthDayPattern == null)
				{
					string text;
					if (m_isDefaultCalendar)
					{
						text = m_cultureTableRecord.SMONTHDAY;
					}
					else
					{
						text = CalendarTable.Default.SMONTHDAY(Calendar.ID);
						if (text.Length == 0)
						{
							text = m_cultureTableRecord.SMONTHDAY;
						}
					}
					monthDayPattern = text;
				}
				return monthDayPattern;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				monthDayPattern = value;
			}
		}

		public string PMDesignator
		{
			get
			{
				return pmDesignator;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				ClearTokenHashTable(scanDateWords: true);
				pmDesignator = value;
			}
		}

		public string RFC1123Pattern => "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";

		public string ShortDatePattern
		{
			get
			{
				return shortDatePattern;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				SetDefaultPatternAsFirstItem(allShortDatePatterns, value);
				shortDatePattern = value;
				generalLongTimePattern = null;
				generalShortTimePattern = null;
				dateTimeOffsetPattern = null;
			}
		}

		public string ShortTimePattern
		{
			get
			{
				if (shortTimePattern == null)
				{
					shortTimePattern = m_cultureTableRecord.SSHORTTIME;
				}
				return shortTimePattern;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				shortTimePattern = value;
				generalShortTimePattern = null;
			}
		}

		public string SortableDateTimePattern => "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

		internal string GeneralShortTimePattern
		{
			get
			{
				if (generalShortTimePattern == null)
				{
					generalShortTimePattern = ShortDatePattern + " " + ShortTimePattern;
				}
				return generalShortTimePattern;
			}
		}

		internal string GeneralLongTimePattern
		{
			get
			{
				if (generalLongTimePattern == null)
				{
					generalLongTimePattern = ShortDatePattern + " " + LongTimePattern;
				}
				return generalLongTimePattern;
			}
		}

		internal string DateTimeOffsetPattern
		{
			get
			{
				if (dateTimeOffsetPattern == null)
				{
					dateTimeOffsetPattern = ShortDatePattern + " " + LongTimePattern + " zzz";
				}
				return dateTimeOffsetPattern;
			}
		}

		public string TimeSeparator
		{
			get
			{
				if (timeSeparator == null)
				{
					timeSeparator = m_cultureTableRecord.STIME;
				}
				return timeSeparator;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				ClearTokenHashTable(scanDateWords: true);
				timeSeparator = value;
			}
		}

		public string UniversalSortableDateTimePattern => "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";

		public string YearMonthPattern
		{
			get
			{
				return yearMonthPattern;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				yearMonthPattern = value;
				SetDefaultPatternAsFirstItem(allYearMonthPatterns, yearMonthPattern);
			}
		}

		public string[] AbbreviatedDayNames
		{
			get
			{
				return (string[])GetAbbreviatedDayOfWeekNames().Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
				}
				if (value.Length != 7)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidArrayLength"), 7), "value");
				}
				CheckNullValue(value, value.Length);
				ClearTokenHashTable(scanDateWords: true);
				abbreviatedDayNames = value;
			}
		}

		[ComVisible(false)]
		public string[] ShortestDayNames
		{
			get
			{
				return (string[])internalGetSuperShortDayNames().Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
				}
				if (value.Length != 7)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidArrayLength"), 7), "value");
				}
				CheckNullValue(value, value.Length);
				m_superShortDayNames = value;
			}
		}

		public string[] DayNames
		{
			get
			{
				return (string[])GetDayOfWeekNames().Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
				}
				if (value.Length != 7)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidArrayLength"), 7), "value");
				}
				CheckNullValue(value, value.Length);
				ClearTokenHashTable(scanDateWords: true);
				dayNames = value;
			}
		}

		public string[] AbbreviatedMonthNames
		{
			get
			{
				return (string[])GetAbbreviatedMonthNames().Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
				}
				if (value.Length != 13)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidArrayLength"), 13), "value");
				}
				CheckNullValue(value, value.Length - 1);
				ClearTokenHashTable(scanDateWords: true);
				abbreviatedMonthNames = value;
			}
		}

		public string[] MonthNames
		{
			get
			{
				return (string[])GetMonthNames().Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
				}
				if (value.Length != 13)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidArrayLength"), 13), "value");
				}
				CheckNullValue(value, value.Length - 1);
				monthNames = value;
				ClearTokenHashTable(scanDateWords: true);
			}
		}

		internal bool HasSpacesInMonthNames => (FormatFlags & DateTimeFormatFlags.UseSpacesInMonthNames) != 0;

		internal bool HasSpacesInDayNames => (FormatFlags & DateTimeFormatFlags.UseSpacesInDayNames) != 0;

		internal string[] ClonedAllYearMonthPatterns
		{
			get
			{
				if (allYearMonthPatterns == null)
				{
					string[] array = null;
					if (!m_isDefaultCalendar)
					{
						array = CalendarTable.Default.SYEARMONTH(Calendar.ID);
						if (array == null)
						{
							array = m_cultureTableRecord.SYEARMONTHS;
						}
					}
					else
					{
						array = m_cultureTableRecord.SYEARMONTHS;
					}
					Thread.MemoryBarrier();
					SetDefaultPatternAsFirstItem(array, YearMonthPattern);
					allYearMonthPatterns = array;
				}
				if (allYearMonthPatterns[0].Equals(YearMonthPattern))
				{
					return (string[])allYearMonthPatterns.Clone();
				}
				return AddDefaultFormat(allYearMonthPatterns, YearMonthPattern);
			}
		}

		internal string[] ClonedAllShortDatePatterns
		{
			get
			{
				if (allShortDatePatterns == null)
				{
					string[] array = null;
					if (!m_isDefaultCalendar)
					{
						array = CalendarTable.Default.SSHORTDATE(Calendar.ID);
						if (array == null)
						{
							array = m_cultureTableRecord.SSHORTDATES;
						}
					}
					else
					{
						array = m_cultureTableRecord.SSHORTDATES;
					}
					Thread.MemoryBarrier();
					SetDefaultPatternAsFirstItem(array, ShortDatePattern);
					allShortDatePatterns = array;
				}
				if (allShortDatePatterns[0].Equals(ShortDatePattern))
				{
					return (string[])allShortDatePatterns.Clone();
				}
				return AddDefaultFormat(allShortDatePatterns, ShortDatePattern);
			}
		}

		internal string[] ClonedAllLongDatePatterns
		{
			get
			{
				if (allLongDatePatterns == null)
				{
					string[] array = null;
					if (!m_isDefaultCalendar)
					{
						array = CalendarTable.Default.SLONGDATE(Calendar.ID);
						if (array == null)
						{
							array = m_cultureTableRecord.SLONGDATES;
						}
					}
					else
					{
						array = m_cultureTableRecord.SLONGDATES;
					}
					Thread.MemoryBarrier();
					SetDefaultPatternAsFirstItem(array, LongDatePattern);
					allLongDatePatterns = array;
				}
				if (allLongDatePatterns[0].Equals(LongDatePattern))
				{
					return (string[])allLongDatePatterns.Clone();
				}
				return AddDefaultFormat(allLongDatePatterns, LongDatePattern);
			}
		}

		internal string[] ClonedAllShortTimePatterns
		{
			get
			{
				if (allShortTimePatterns == null)
				{
					allShortTimePatterns = m_cultureTableRecord.SSHORTTIMES;
					SetDefaultPatternAsFirstItem(allShortTimePatterns, ShortTimePattern);
				}
				if (allShortTimePatterns[0].Equals(ShortTimePattern))
				{
					return (string[])allShortTimePatterns.Clone();
				}
				return AddDefaultFormat(allShortTimePatterns, ShortTimePattern);
			}
		}

		internal string[] ClonedAllLongTimePatterns
		{
			get
			{
				if (allLongTimePatterns == null)
				{
					allLongTimePatterns = m_cultureTableRecord.STIMEFORMATS;
					SetDefaultPatternAsFirstItem(allLongTimePatterns, LongTimePattern);
				}
				if (allLongTimePatterns[0].Equals(LongTimePattern))
				{
					return (string[])allLongTimePatterns.Clone();
				}
				return AddDefaultFormat(allLongTimePatterns, LongTimePattern);
			}
		}

		internal string[] DateWords
		{
			get
			{
				if (m_dateWords == null)
				{
					m_dateWords = m_cultureTableRecord.SDATEWORDS;
				}
				return m_dateWords;
			}
		}

		public bool IsReadOnly => m_isReadOnly;

		[ComVisible(false)]
		public string NativeCalendarName
		{
			get
			{
				if (Calendar.ID == 4)
				{
					string text = GetCalendarInfo(1028, 4, 2);
					if (text == null)
					{
						text = CalendarTable.nativeGetEraName(1028, 4);
						if (text == null)
						{
							text = string.Empty;
						}
					}
					return text;
				}
				string[] sNATIVECALNAMES = m_cultureTableRecord.SNATIVECALNAMES;
				int num = calendar.ID - 1;
				if (num < sNATIVECALNAMES.Length)
				{
					if (sNATIVECALNAMES[num].Length <= 0)
					{
						return GetCalendarNativeNameFallback(calendar.ID);
					}
					if (sNATIVECALNAMES[num][0] != '\ufeff')
					{
						return sNATIVECALNAMES[num];
					}
				}
				return string.Empty;
			}
		}

		[ComVisible(false)]
		public string[] AbbreviatedMonthGenitiveNames
		{
			get
			{
				return (string[])internalGetGenitiveMonthNames(abbreviated: true).Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
				}
				if (value.Length != 13)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidArrayLength"), 13), "value");
				}
				CheckNullValue(value, value.Length - 1);
				ClearTokenHashTable(scanDateWords: true);
				m_genitiveAbbreviatedMonthNames = value;
			}
		}

		[ComVisible(false)]
		public string[] MonthGenitiveNames
		{
			get
			{
				return (string[])internalGetGenitiveMonthNames(abbreviated: false).Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
				}
				if (value.Length != 13)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidArrayLength"), 13), "value");
				}
				CheckNullValue(value, value.Length - 1);
				genitiveMonthNames = value;
				ClearTokenHashTable(scanDateWords: true);
			}
		}

		internal CompareInfo CompareInfo
		{
			get
			{
				if (m_compareInfo == null)
				{
					if (CultureTableRecord.IsCustomCultureId(CultureId))
					{
						m_compareInfo = CompareInfo.GetCompareInfo((int)m_cultureTableRecord.ICOMPAREINFO);
					}
					else
					{
						m_compareInfo = CompareInfo.GetCompareInfo(CultureId);
					}
				}
				return m_compareInfo;
			}
		}

		internal DateTimeFormatFlags FormatFlags
		{
			get
			{
				if (formatFlags == DateTimeFormatFlags.NotInitialized)
				{
					if (m_scanDateWords || m_cultureTableRecord.IsSynthetic)
					{
						formatFlags = DateTimeFormatFlags.None;
						formatFlags |= (DateTimeFormatFlags)DateTimeFormatInfoScanner.GetFormatFlagGenitiveMonth(MonthNames, internalGetGenitiveMonthNames(abbreviated: false), AbbreviatedMonthNames, internalGetGenitiveMonthNames(abbreviated: true));
						formatFlags |= (DateTimeFormatFlags)DateTimeFormatInfoScanner.GetFormatFlagUseSpaceInMonthNames(MonthNames, internalGetGenitiveMonthNames(abbreviated: false), AbbreviatedMonthNames, internalGetGenitiveMonthNames(abbreviated: true));
						formatFlags |= (DateTimeFormatFlags)DateTimeFormatInfoScanner.GetFormatFlagUseSpaceInDayNames(DayNames, AbbreviatedDayNames);
						formatFlags |= (DateTimeFormatFlags)DateTimeFormatInfoScanner.GetFormatFlagUseHebrewCalendar(Calendar.ID);
					}
					else if (m_isDefaultCalendar)
					{
						formatFlags = m_cultureTableRecord.IFORMATFLAGS;
					}
					else
					{
						formatFlags = (DateTimeFormatFlags)CalendarTable.Default.IFORMATFLAGS(Calendar.ID);
					}
				}
				return formatFlags;
			}
		}

		internal bool HasForceTwoDigitYears
		{
			get
			{
				switch (calendar.ID)
				{
				case 3:
				case 4:
					return true;
				default:
					return false;
				}
			}
		}

		internal bool HasYearMonthAdjustment => (FormatFlags & DateTimeFormatFlags.UseHebrewRule) != 0;

		private string[] GetAbbreviatedDayOfWeekNames()
		{
			if (abbreviatedDayNames == null && abbreviatedDayNames == null)
			{
				string[] array = null;
				if (!m_isDefaultCalendar)
				{
					array = CalendarTable.Default.SABBREVDAYNAMES(Calendar.ID);
				}
				if (array == null || array.Length == 0 || array[0].Length == 0)
				{
					array = m_cultureTableRecord.SABBREVDAYNAMES;
				}
				Thread.MemoryBarrier();
				abbreviatedDayNames = array;
			}
			return abbreviatedDayNames;
		}

		private string[] internalGetSuperShortDayNames()
		{
			if (m_superShortDayNames == null && m_superShortDayNames == null)
			{
				string[] array = null;
				if (!m_isDefaultCalendar)
				{
					array = CalendarTable.Default.SSUPERSHORTDAYNAMES(Calendar.ID);
				}
				if (array == null || array.Length == 0 || array[0].Length == 0)
				{
					array = m_cultureTableRecord.SSUPERSHORTDAYNAMES;
				}
				Thread.MemoryBarrier();
				m_superShortDayNames = array;
			}
			return m_superShortDayNames;
		}

		private string[] GetDayOfWeekNames()
		{
			if (dayNames == null && dayNames == null)
			{
				string[] array = null;
				if (!m_isDefaultCalendar)
				{
					array = CalendarTable.Default.SDAYNAMES(Calendar.ID);
				}
				if (array == null || array.Length == 0 || array[0].Length == 0)
				{
					array = m_cultureTableRecord.SDAYNAMES;
				}
				Thread.MemoryBarrier();
				dayNames = array;
			}
			return dayNames;
		}

		private string[] GetAbbreviatedMonthNames()
		{
			if (abbreviatedMonthNames == null && abbreviatedMonthNames == null)
			{
				string[] array = null;
				if (!m_isDefaultCalendar)
				{
					array = CalendarTable.Default.SABBREVMONTHNAMES(Calendar.ID);
				}
				if (array == null || array.Length == 0 || array[0].Length == 0)
				{
					array = m_cultureTableRecord.SABBREVMONTHNAMES;
				}
				Thread.MemoryBarrier();
				abbreviatedMonthNames = array;
			}
			return abbreviatedMonthNames;
		}

		private string[] GetMonthNames()
		{
			if (monthNames == null)
			{
				string[] array = null;
				if (!m_isDefaultCalendar)
				{
					array = CalendarTable.Default.SMONTHNAMES(Calendar.ID);
				}
				if (array == null || array.Length == 0 || array[0].Length == 0)
				{
					array = m_cultureTableRecord.SMONTHNAMES;
				}
				Thread.MemoryBarrier();
				monthNames = array;
			}
			return monthNames;
		}

		public DateTimeFormatInfo()
		{
			m_cultureTableRecord = CultureInfo.InvariantCulture.m_cultureTableRecord;
			m_isDefaultCalendar = true;
			calendar = GregorianCalendar.GetDefaultInstance();
			InitializeOverridableProperties();
		}

		internal DateTimeFormatInfo(CultureTableRecord cultureTable, int cultureID, Calendar cal)
		{
			m_cultureTableRecord = cultureTable;
			Calendar = cal;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			if (CultureTableRecord.IsCustomCultureId(CultureID))
			{
				m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(m_name, m_useUserOverride);
			}
			else
			{
				m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(CultureID, m_useUserOverride);
			}
			if (calendar == null)
			{
				calendar = (Calendar)GregorianCalendar.GetDefaultInstance().Clone();
				calendar.SetReadOnlyState(m_isReadOnly);
			}
			else
			{
				CultureInfo.CheckDomainSafetyObject(calendar, this);
			}
			InitializeOverridableProperties();
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext ctx)
		{
			CultureID = m_cultureTableRecord.CultureID;
			m_useUserOverride = m_cultureTableRecord.UseUserOverride;
			nDataItem = m_cultureTableRecord.EverettDataItem();
			if (CultureTableRecord.IsCustomCultureId(CultureID))
			{
				m_name = CultureName;
			}
		}

		public static DateTimeFormatInfo GetInstance(IFormatProvider provider)
		{
			CultureInfo cultureInfo = provider as CultureInfo;
			DateTimeFormatInfo dateTimeInfo;
			if (cultureInfo != null && !cultureInfo.m_isInherited)
			{
				dateTimeInfo = cultureInfo.dateTimeInfo;
				if (dateTimeInfo != null)
				{
					return dateTimeInfo;
				}
				return cultureInfo.DateTimeFormat;
			}
			dateTimeInfo = provider as DateTimeFormatInfo;
			if (dateTimeInfo != null)
			{
				return dateTimeInfo;
			}
			if (provider != null)
			{
				dateTimeInfo = provider.GetFormat(typeof(DateTimeFormatInfo)) as DateTimeFormatInfo;
				if (dateTimeInfo != null)
				{
					return dateTimeInfo;
				}
			}
			return CurrentInfo;
		}

		public object GetFormat(Type formatType)
		{
			if (formatType != typeof(DateTimeFormatInfo))
			{
				return null;
			}
			return this;
		}

		public object Clone()
		{
			DateTimeFormatInfo dateTimeFormatInfo = (DateTimeFormatInfo)MemberwiseClone();
			dateTimeFormatInfo.calendar = (Calendar)Calendar.Clone();
			dateTimeFormatInfo.m_isReadOnly = false;
			return dateTimeFormatInfo;
		}

		private void InitializeOverridableProperties()
		{
			if (amDesignator == null)
			{
				amDesignator = m_cultureTableRecord.S1159;
			}
			if (pmDesignator == null)
			{
				pmDesignator = m_cultureTableRecord.S2359;
			}
			if (longTimePattern == null)
			{
				longTimePattern = m_cultureTableRecord.STIMEFORMAT;
			}
			if (firstDayOfWeek == -1)
			{
				firstDayOfWeek = m_cultureTableRecord.IFIRSTDAYOFWEEK;
			}
			if (calendarWeekRule == -1)
			{
				calendarWeekRule = m_cultureTableRecord.IFIRSTWEEKOFYEAR;
			}
			if (yearMonthPattern == null)
			{
				yearMonthPattern = GetYearMonthPattern(calendar.ID);
			}
			if (shortDatePattern == null)
			{
				shortDatePattern = GetShortDatePattern(calendar.ID);
			}
			if (longDatePattern == null)
			{
				longDatePattern = GetLongDatePattern(calendar.ID);
			}
		}

		public int GetEra(string eraName)
		{
			if (eraName == null)
			{
				throw new ArgumentNullException("eraName", Environment.GetResourceString("ArgumentNull_String"));
			}
			for (int i = 0; i < EraNames.Length; i++)
			{
				if (m_eraNames[i].Length > 0 && string.Compare(eraName, m_eraNames[i], ignoreCase: true, CultureInfo.CurrentCulture) == 0)
				{
					return i + 1;
				}
			}
			for (int j = 0; j < AbbreviatedEraNames.Length; j++)
			{
				if (string.Compare(eraName, m_abbrevEraNames[j], ignoreCase: true, CultureInfo.CurrentCulture) == 0)
				{
					return j + 1;
				}
			}
			for (int k = 0; k < AbbreviatedEnglishEraNames.Length; k++)
			{
				if (string.Compare(eraName, m_abbrevEnglishEraNames[k], ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return k + 1;
				}
			}
			return -1;
		}

		public string GetEraName(int era)
		{
			if (era == 0)
			{
				era = Calendar.CurrentEraValue;
			}
			if (--era < EraNames.Length && era >= 0)
			{
				return m_eraNames[era];
			}
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
		}

		public string GetAbbreviatedEraName(int era)
		{
			if (AbbreviatedEraNames.Length == 0)
			{
				return GetEraName(era);
			}
			if (era == 0)
			{
				era = Calendar.CurrentEraValue;
			}
			if (--era < m_abbrevEraNames.Length && era >= 0)
			{
				return m_abbrevEraNames[era];
			}
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
		}

		private string GetLongDatePattern(int calID)
		{
			string empty = string.Empty;
			if (!m_isDefaultCalendar)
			{
				return CalendarTable.Default.SLONGDATE(calID)[0];
			}
			return m_cultureTableRecord.SLONGDATE;
		}

		internal string GetShortDatePattern(int calID)
		{
			string empty = string.Empty;
			if (!m_isDefaultCalendar)
			{
				return CalendarTable.Default.SSHORTDATE(calID)[0];
			}
			return m_cultureTableRecord.SSHORTDATE;
		}

		private string GetYearMonthPattern(int calID)
		{
			string text = null;
			if (!m_isDefaultCalendar)
			{
				return CalendarTable.Default.SYEARMONTH(calID)[0];
			}
			return m_cultureTableRecord.SYEARMONTHS[0];
		}

		private void CheckNullValue(string[] values, int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (values[i] == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_ArrayValue"));
				}
			}
		}

		internal string internalGetMonthName(int month, MonthNameStyles style, bool abbreviated)
		{
			string[] array = null;
			array = style switch
			{
				MonthNameStyles.Genitive => internalGetGenitiveMonthNames(abbreviated), 
				MonthNameStyles.LeapYear => internalGetLeapYearMonthNames(), 
				_ => abbreviated ? GetAbbreviatedMonthNames() : GetMonthNames(), 
			};
			if (month < 1 || month > array.Length)
			{
				throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, array.Length));
			}
			return array[month - 1];
		}

		private string[] internalGetGenitiveMonthNames(bool abbreviated)
		{
			if (abbreviated)
			{
				if (m_genitiveAbbreviatedMonthNames == null)
				{
					if (m_isDefaultCalendar)
					{
						string[] sABBREVMONTHGENITIVENAMES = m_cultureTableRecord.SABBREVMONTHGENITIVENAMES;
						if (sABBREVMONTHGENITIVENAMES.Length > 0)
						{
							m_genitiveAbbreviatedMonthNames = sABBREVMONTHGENITIVENAMES;
						}
						else
						{
							m_genitiveAbbreviatedMonthNames = GetAbbreviatedMonthNames();
						}
					}
					else
					{
						m_genitiveAbbreviatedMonthNames = GetAbbreviatedMonthNames();
					}
				}
				return m_genitiveAbbreviatedMonthNames;
			}
			if (genitiveMonthNames == null)
			{
				if (m_isDefaultCalendar)
				{
					string[] sMONTHGENITIVENAMES = m_cultureTableRecord.SMONTHGENITIVENAMES;
					if (sMONTHGENITIVENAMES.Length > 0)
					{
						genitiveMonthNames = sMONTHGENITIVENAMES;
					}
					else
					{
						genitiveMonthNames = GetMonthNames();
					}
				}
				else
				{
					genitiveMonthNames = GetMonthNames();
				}
			}
			return genitiveMonthNames;
		}

		internal string[] internalGetLeapYearMonthNames()
		{
			if (leapYearMonthNames == null)
			{
				if (m_isDefaultCalendar)
				{
					leapYearMonthNames = GetMonthNames();
				}
				else
				{
					string[] array = CalendarTable.Default.SLEAPYEARMONTHNAMES(Calendar.ID);
					if (array.Length > 0)
					{
						leapYearMonthNames = array;
					}
					else
					{
						leapYearMonthNames = GetMonthNames();
					}
				}
			}
			return leapYearMonthNames;
		}

		public string GetAbbreviatedDayName(DayOfWeek dayofweek)
		{
			if (dayofweek < DayOfWeek.Sunday || dayofweek > DayOfWeek.Saturday)
			{
				throw new ArgumentOutOfRangeException("dayofweek", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), DayOfWeek.Sunday, DayOfWeek.Saturday));
			}
			return GetAbbreviatedDayOfWeekNames()[(int)dayofweek];
		}

		[ComVisible(false)]
		public string GetShortestDayName(DayOfWeek dayOfWeek)
		{
			if (dayOfWeek < DayOfWeek.Sunday || dayOfWeek > DayOfWeek.Saturday)
			{
				throw new ArgumentOutOfRangeException("dayOfWeek", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), DayOfWeek.Sunday, DayOfWeek.Saturday));
			}
			return internalGetSuperShortDayNames()[(int)dayOfWeek];
		}

		internal string[] GetCombinedPatterns(string[] patterns1, string[] patterns2, string connectString)
		{
			string[] array = new string[patterns1.Length * patterns2.Length];
			for (int i = 0; i < patterns1.Length; i++)
			{
				for (int j = 0; j < patterns2.Length; j++)
				{
					array[i * patterns2.Length + j] = patterns1[i] + connectString + patterns2[j];
				}
			}
			return array;
		}

		public string[] GetAllDateTimePatterns()
		{
			ArrayList arrayList = new ArrayList(132);
			for (int i = 0; i < DateTimeFormat.allStandardFormats.Length; i++)
			{
				string[] allDateTimePatterns = GetAllDateTimePatterns(DateTimeFormat.allStandardFormats[i]);
				for (int j = 0; j < allDateTimePatterns.Length; j++)
				{
					arrayList.Add(allDateTimePatterns[j]);
				}
			}
			string[] array = new string[arrayList.Count];
			arrayList.CopyTo(0, array, 0, arrayList.Count);
			return array;
		}

		public string[] GetAllDateTimePatterns(char format)
		{
			string[] array = null;
			switch (format)
			{
			case 'd':
				return ClonedAllShortDatePatterns;
			case 'D':
				return ClonedAllLongDatePatterns;
			case 'f':
				return GetCombinedPatterns(ClonedAllLongDatePatterns, ClonedAllShortTimePatterns, " ");
			case 'F':
				return GetCombinedPatterns(ClonedAllLongDatePatterns, ClonedAllLongTimePatterns, " ");
			case 'g':
				return GetCombinedPatterns(ClonedAllShortDatePatterns, ClonedAllShortTimePatterns, " ");
			case 'G':
				return GetCombinedPatterns(ClonedAllShortDatePatterns, ClonedAllLongTimePatterns, " ");
			case 'M':
			case 'm':
				return new string[1]
				{
					MonthDayPattern
				};
			case 'O':
			case 'o':
				return new string[1]
				{
					"yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK"
				};
			case 'R':
			case 'r':
				return new string[1]
				{
					"ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"
				};
			case 's':
				return new string[1]
				{
					"yyyy'-'MM'-'dd'T'HH':'mm':'ss"
				};
			case 't':
				return ClonedAllShortTimePatterns;
			case 'T':
				return ClonedAllLongTimePatterns;
			case 'u':
				return new string[1]
				{
					UniversalSortableDateTimePattern
				};
			case 'U':
				return GetCombinedPatterns(ClonedAllLongDatePatterns, ClonedAllLongTimePatterns, " ");
			case 'Y':
			case 'y':
				return ClonedAllYearMonthPatterns;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_BadFormatSpecifier"), "format");
			}
		}

		public string GetDayName(DayOfWeek dayofweek)
		{
			if (dayofweek < DayOfWeek.Sunday || dayofweek > DayOfWeek.Saturday)
			{
				throw new ArgumentOutOfRangeException("dayofweek", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), DayOfWeek.Sunday, DayOfWeek.Saturday));
			}
			return GetDayOfWeekNames()[(int)dayofweek];
		}

		public string GetAbbreviatedMonthName(int month)
		{
			if (month < 1 || month > 13)
			{
				throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 13));
			}
			return GetAbbreviatedMonthNames()[month - 1];
		}

		public string GetMonthName(int month)
		{
			if (month < 1 || month > 13)
			{
				throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 13));
			}
			return GetMonthNames()[month - 1];
		}

		internal void SetDefaultPatternAsFirstItem(string[] patterns, string defaultPattern)
		{
			if (patterns == null)
			{
				return;
			}
			for (int i = 0; i < patterns.Length; i++)
			{
				if (!patterns[i].Equals(defaultPattern))
				{
					continue;
				}
				if (i != 0)
				{
					string text = patterns[i];
					for (int num = i; num > 0; num--)
					{
						patterns[num] = patterns[num - 1];
					}
					patterns[0] = text;
				}
				break;
			}
		}

		internal string[] AddDefaultFormat(string[] datePatterns, string defaultDateFormat)
		{
			string[] array = new string[datePatterns.Length + 1];
			array[0] = defaultDateFormat;
			Array.Copy(datePatterns, 0, array, 1, datePatterns.Length);
			m_scanDateWords = true;
			return array;
		}

		public static DateTimeFormatInfo ReadOnly(DateTimeFormatInfo dtfi)
		{
			if (dtfi == null)
			{
				throw new ArgumentNullException("dtfi", Environment.GetResourceString("ArgumentNull_Obj"));
			}
			if (dtfi.IsReadOnly)
			{
				return dtfi;
			}
			DateTimeFormatInfo dateTimeFormatInfo = (DateTimeFormatInfo)dtfi.MemberwiseClone();
			dateTimeFormatInfo.Calendar = Calendar.ReadOnly(dateTimeFormatInfo.Calendar);
			dateTimeFormatInfo.m_isReadOnly = true;
			return dateTimeFormatInfo;
		}

		private static int CalendarIdToCultureId(int calendarId)
		{
			switch (calendarId)
			{
			case 2:
				return 1065;
			case 3:
				return 1041;
			case 4:
				return 1028;
			case 5:
				return 1042;
			case 6:
			case 10:
			case 23:
				return 1025;
			case 7:
				return 1054;
			case 8:
				return 1037;
			case 9:
				return 5121;
			case 11:
			case 12:
				return 2049;
			default:
				return 0;
			}
		}

		private string GetCalendarNativeNameFallback(int calendarId)
		{
			if (m_calendarNativeNames == null)
			{
				lock (InternalSyncObject)
				{
					if (m_calendarNativeNames == null)
					{
						m_calendarNativeNames = new Hashtable();
					}
				}
			}
			string text = (string)m_calendarNativeNames[calendarId];
			if (text != null)
			{
				return text;
			}
			string text2 = string.Empty;
			int num = CalendarIdToCultureId(calendarId);
			if (num != 0)
			{
				string[] sNATIVECALNAMES = new CultureTableRecord(num, useUserOverride: false).SNATIVECALNAMES;
				int num2 = calendar.ID - 1;
				if (num2 < sNATIVECALNAMES.Length && sNATIVECALNAMES[num2].Length > 0 && sNATIVECALNAMES[num2][0] != '\ufeff')
				{
					text2 = sNATIVECALNAMES[num2];
				}
			}
			lock (InternalSyncObject)
			{
				if (m_calendarNativeNames[calendarId] == null)
				{
					m_calendarNativeNames[calendarId] = text2;
					return text2;
				}
				return text2;
			}
		}

		[ComVisible(false)]
		public void SetAllDateTimePatterns(string[] patterns, char format)
		{
			VerifyWritable();
			if (patterns == null)
			{
				throw new ArgumentNullException("patterns", Environment.GetResourceString("ArgumentNull_Array"));
			}
			if (patterns.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayZeroError"), "patterns");
			}
			for (int i = 0; i < patterns.Length; i++)
			{
				if (patterns[i] == null)
				{
					throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_ArrayValue"));
				}
			}
			switch (format)
			{
			case 'd':
				ShortDatePattern = patterns[0];
				allShortDatePatterns = patterns;
				break;
			case 'D':
				LongDatePattern = patterns[0];
				allLongDatePatterns = patterns;
				break;
			case 't':
				ShortTimePattern = patterns[0];
				allShortTimePatterns = patterns;
				break;
			case 'T':
				LongTimePattern = patterns[0];
				allLongTimePatterns = patterns;
				break;
			case 'Y':
			case 'y':
				yearMonthPattern = patterns[0];
				allYearMonthPatterns = patterns;
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_BadFormatSpecifier"), "format");
			}
		}

		private void VerifyWritable()
		{
			if (m_isReadOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
			}
		}

		internal static void ValidateStyles(DateTimeStyles style, string parameterName)
		{
			if (((uint)style & 0xFFFFFF00u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDateTimeStyles"), parameterName);
			}
			if ((style & DateTimeStyles.AssumeLocal) != 0 && (style & DateTimeStyles.AssumeUniversal) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ConflictingDateTimeStyles"), parameterName);
			}
			if ((style & DateTimeStyles.RoundtripKind) != 0 && (style & (DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal | DateTimeStyles.AssumeUniversal)) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ConflictingDateTimeRoundtripStyles"), parameterName);
			}
		}

		internal bool YearMonthAdjustment(ref int year, ref int month, bool parsedMonthName)
		{
			if ((FormatFlags & DateTimeFormatFlags.UseHebrewRule) != 0)
			{
				if (year < 1000)
				{
					year += 5000;
				}
				if (year < Calendar.GetYear(Calendar.MinSupportedDateTime) || year > Calendar.GetYear(Calendar.MaxSupportedDateTime))
				{
					return false;
				}
				if (parsedMonthName && !Calendar.IsLeapYear(year))
				{
					if (month >= 8)
					{
						month--;
					}
					else if (month == 7)
					{
						return false;
					}
				}
			}
			return true;
		}

		internal static DateTimeFormatInfo GetJapaneseCalendarDTFI()
		{
			DateTimeFormatInfo dateTimeFormatInfo = m_jajpDTFI;
			if (dateTimeFormatInfo == null)
			{
				dateTimeFormatInfo = new CultureInfo("ja-JP", useUserOverride: false).DateTimeFormat;
				dateTimeFormatInfo.Calendar = JapaneseCalendar.GetDefaultInstance();
				m_jajpDTFI = dateTimeFormatInfo;
			}
			return dateTimeFormatInfo;
		}

		internal static DateTimeFormatInfo GetTaiwanCalendarDTFI()
		{
			DateTimeFormatInfo dateTimeFormatInfo = m_zhtwDTFI;
			if (dateTimeFormatInfo == null)
			{
				dateTimeFormatInfo = new CultureInfo("zh-TW", useUserOverride: false).DateTimeFormat;
				dateTimeFormatInfo.Calendar = TaiwanCalendar.GetDefaultInstance();
				m_zhtwDTFI = dateTimeFormatInfo;
			}
			return dateTimeFormatInfo;
		}

		private void ClearTokenHashTable(bool scanDateWords)
		{
			m_dtfiTokenHash = null;
			m_dateWords = null;
			m_scanDateWords = scanDateWords;
			formatFlags = DateTimeFormatFlags.NotInitialized;
		}

		internal TokenHashValue[] CreateTokenHashTable()
		{
			TokenHashValue[] array = m_dtfiTokenHash;
			if (array == null)
			{
				array = new TokenHashValue[199];
				InsertHash(array, ",", TokenType.IgnorableSymbol, 0);
				InsertHash(array, ".", TokenType.IgnorableSymbol, 0);
				InsertHash(array, TimeSeparator, TokenType.SEP_Time, 0);
				InsertHash(array, AMDesignator, (TokenType)1027, 0);
				InsertHash(array, PMDesignator, (TokenType)1284, 1);
				if (CultureName.Equals("sq-AL"))
				{
					InsertHash(array, "." + AMDesignator, (TokenType)1027, 0);
					InsertHash(array, "." + PMDesignator, (TokenType)1284, 1);
				}
				InsertHash(array, "年", TokenType.SEP_YearSuff, 0);
				InsertHash(array, "년", TokenType.SEP_YearSuff, 0);
				InsertHash(array, "月", TokenType.SEP_MonthSuff, 0);
				InsertHash(array, "월", TokenType.SEP_MonthSuff, 0);
				InsertHash(array, "日", TokenType.SEP_DaySuff, 0);
				InsertHash(array, "일", TokenType.SEP_DaySuff, 0);
				InsertHash(array, "時", TokenType.SEP_HourSuff, 0);
				InsertHash(array, "时", TokenType.SEP_HourSuff, 0);
				InsertHash(array, "分", TokenType.SEP_MinuteSuff, 0);
				InsertHash(array, "秒", TokenType.SEP_SecondSuff, 0);
				if (!GregorianCalendarHelper.EnforceLegacyJapaneseDateParsing && Calendar.ID == 3)
				{
					InsertHash(array, "元", TokenType.YearNumberToken, 1);
					InsertHash(array, "(", TokenType.IgnorableSymbol, 0);
					InsertHash(array, ")", TokenType.IgnorableSymbol, 0);
				}
				if (LanguageName.Equals("ko"))
				{
					InsertHash(array, "시", TokenType.SEP_HourSuff, 0);
					InsertHash(array, "분", TokenType.SEP_MinuteSuff, 0);
					InsertHash(array, "초", TokenType.SEP_SecondSuff, 0);
				}
				if (CultureName.Equals("ky-KG"))
				{
					InsertHash(array, "-", TokenType.IgnorableSymbol, 0);
				}
				else
				{
					InsertHash(array, "-", TokenType.SEP_DateOrOffset, 0);
				}
				string[] array2 = null;
				DateTimeFormatInfoScanner dateTimeFormatInfoScanner = null;
				if (!m_scanDateWords)
				{
					array2 = ClonedAllLongDatePatterns;
				}
				if (m_scanDateWords || m_cultureTableRecord.IsSynthetic)
				{
					dateTimeFormatInfoScanner = new DateTimeFormatInfoScanner();
					array2 = (m_dateWords = dateTimeFormatInfoScanner.GetDateWordsOfDTFI(this));
					_ = FormatFlags;
					m_scanDateWords = false;
				}
				else
				{
					array2 = DateWords;
				}
				bool flag = false;
				string text = null;
				if (array2 != null)
				{
					for (int i = 0; i < array2.Length; i++)
					{
						switch (array2[i][0])
						{
						case '\ue000':
							text = array2[i].Substring(1);
							AddMonthNames(array, text);
							break;
						case '\ue001':
						{
							string text2 = array2[i].Substring(1);
							InsertHash(array, text2, TokenType.IgnorableSymbol, 0);
							if (DateSeparator.Trim(null).Equals(text2))
							{
								flag = true;
							}
							break;
						}
						default:
							InsertHash(array, array2[i], TokenType.DateWordToken, 0);
							if (CultureName.Equals("eu-ES"))
							{
								InsertHash(array, "." + array2[i], TokenType.DateWordToken, 0);
							}
							break;
						}
					}
				}
				if (!flag)
				{
					InsertHash(array, DateSeparator, TokenType.SEP_Date, 0);
				}
				AddMonthNames(array, null);
				for (int j = 1; j <= 13; j++)
				{
					InsertHash(array, GetAbbreviatedMonthName(j), TokenType.MonthToken, j);
				}
				if (CultureName.Equals("gl-ES"))
				{
					for (int k = 1; k <= 13; k++)
					{
						string monthName = GetMonthName(k);
						if (monthName.Length > 0)
						{
							InsertHash(array, monthName + "de", TokenType.MonthToken, k);
						}
					}
				}
				if ((FormatFlags & DateTimeFormatFlags.UseGenitiveMonth) != 0)
				{
					for (int l = 1; l <= 13; l++)
					{
						string str = internalGetMonthName(l, MonthNameStyles.Genitive, abbreviated: false);
						InsertHash(array, str, TokenType.MonthToken, l);
					}
				}
				if ((FormatFlags & DateTimeFormatFlags.UseLeapYearMonth) != 0)
				{
					for (int m = 1; m <= 13; m++)
					{
						string str2 = internalGetMonthName(m, MonthNameStyles.LeapYear, abbreviated: false);
						InsertHash(array, str2, TokenType.MonthToken, m);
					}
				}
				for (int n = 0; n < 7; n++)
				{
					string dayName = GetDayName((DayOfWeek)n);
					InsertHash(array, dayName, TokenType.DayOfWeekToken, n);
					dayName = GetAbbreviatedDayName((DayOfWeek)n);
					InsertHash(array, dayName, TokenType.DayOfWeekToken, n);
				}
				int[] eras = calendar.Eras;
				for (int num = 1; num <= eras.Length; num++)
				{
					InsertHash(array, GetEraName(num), TokenType.EraToken, num);
					InsertHash(array, GetAbbreviatedEraName(num), TokenType.EraToken, num);
				}
				if (LanguageName.Equals("ja"))
				{
					for (int num2 = 0; num2 < 7; num2++)
					{
						string str3 = "(" + GetAbbreviatedDayName((DayOfWeek)num2) + ")";
						InsertHash(array, str3, TokenType.DayOfWeekToken, num2);
					}
					if (Calendar.GetType() != typeof(JapaneseCalendar))
					{
						DateTimeFormatInfo japaneseCalendarDTFI = GetJapaneseCalendarDTFI();
						for (int num3 = 1; num3 <= japaneseCalendarDTFI.Calendar.Eras.Length; num3++)
						{
							InsertHash(array, japaneseCalendarDTFI.GetEraName(num3), TokenType.JapaneseEraToken, num3);
							InsertHash(array, japaneseCalendarDTFI.GetAbbreviatedEraName(num3), TokenType.JapaneseEraToken, num3);
							InsertHash(array, japaneseCalendarDTFI.AbbreviatedEnglishEraNames[num3 - 1], TokenType.JapaneseEraToken, num3);
						}
					}
				}
				else if (CultureName.Equals("zh-TW"))
				{
					DateTimeFormatInfo taiwanCalendarDTFI = GetTaiwanCalendarDTFI();
					for (int num4 = 1; num4 <= taiwanCalendarDTFI.Calendar.Eras.Length; num4++)
					{
						if (taiwanCalendarDTFI.GetEraName(num4).Length > 0)
						{
							InsertHash(array, taiwanCalendarDTFI.GetEraName(num4), TokenType.TEraToken, num4);
						}
					}
				}
				InsertHash(array, InvariantInfo.AMDesignator, (TokenType)1027, 0);
				InsertHash(array, InvariantInfo.PMDesignator, (TokenType)1284, 1);
				for (int num5 = 1; num5 <= 12; num5++)
				{
					string monthName2 = InvariantInfo.GetMonthName(num5);
					InsertHash(array, monthName2, TokenType.MonthToken, num5);
					monthName2 = InvariantInfo.GetAbbreviatedMonthName(num5);
					InsertHash(array, monthName2, TokenType.MonthToken, num5);
				}
				for (int num6 = 0; num6 < 7; num6++)
				{
					string dayName2 = InvariantInfo.GetDayName((DayOfWeek)num6);
					InsertHash(array, dayName2, TokenType.DayOfWeekToken, num6);
					dayName2 = InvariantInfo.GetAbbreviatedDayName((DayOfWeek)num6);
					InsertHash(array, dayName2, TokenType.DayOfWeekToken, num6);
				}
				for (int num7 = 0; num7 < AbbreviatedEnglishEraNames.Length; num7++)
				{
					InsertHash(array, AbbreviatedEnglishEraNames[num7], TokenType.EraToken, num7 + 1);
				}
				InsertHash(array, "T", TokenType.SEP_LocalTimeMark, 0);
				InsertHash(array, "GMT", TokenType.TimeZoneToken, 0);
				InsertHash(array, "Z", TokenType.TimeZoneToken, 0);
				InsertHash(array, "/", TokenType.SEP_Date, 0);
				InsertHash(array, ":", TokenType.SEP_Time, 0);
				m_dtfiTokenHash = array;
			}
			return array;
		}

		private void AddMonthNames(TokenHashValue[] temp, string monthPostfix)
		{
			for (int i = 1; i <= 13; i++)
			{
				string monthName = GetMonthName(i);
				if (monthName.Length > 0)
				{
					if (monthPostfix != null)
					{
						InsertHash(temp, monthName + monthPostfix, TokenType.MonthToken, i);
					}
					else
					{
						InsertHash(temp, monthName, TokenType.MonthToken, i);
					}
				}
				monthName = GetAbbreviatedMonthName(i);
				InsertHash(temp, monthName, TokenType.MonthToken, i);
			}
		}

		private static bool TryParseHebrewNumber(ref __DTString str, out bool badFormat, out int number)
		{
			number = -1;
			badFormat = false;
			int index = str.Index;
			if (!HebrewNumber.IsDigit(str.Value[index]))
			{
				return false;
			}
			HebrewNumberParsingContext context = new HebrewNumberParsingContext(0);
			while (true)
			{
				HebrewNumberParsingState hebrewNumberParsingState = HebrewNumber.ParseByChar(str.Value[index++], ref context);
				switch (hebrewNumberParsingState)
				{
				case HebrewNumberParsingState.InvalidHebrewNumber:
				case HebrewNumberParsingState.NotHebrewDigit:
					return false;
				}
				if (index >= str.Value.Length || hebrewNumberParsingState == HebrewNumberParsingState.FoundEndOfHebrewNumber)
				{
					if (hebrewNumberParsingState != HebrewNumberParsingState.FoundEndOfHebrewNumber)
					{
						return false;
					}
					str.Advance(index - str.Index);
					number = context.result;
					return true;
				}
			}
		}

		private static bool IsHebrewChar(char ch)
		{
			if (ch >= '\u0590')
			{
				return ch <= '\u05ff';
			}
			return false;
		}

		private bool IsAllowedJapaneseTokenFollowedByNonSpaceLetter(string tokenString, char nextCh)
		{
			if (!GregorianCalendarHelper.EnforceLegacyJapaneseDateParsing && Calendar.ID == 3 && (nextCh == "元"[0] || (tokenString == "元" && nextCh == "年"[0])))
			{
				return true;
			}
			return false;
		}

		internal bool Tokenize(TokenType TokenMask, out TokenType tokenType, out int tokenValue, ref __DTString str)
		{
			tokenType = TokenType.UnknownToken;
			tokenValue = 0;
			char c = str.m_current;
			bool flag = char.IsLetter(c);
			if (flag)
			{
				c = char.ToLower(c, CultureInfo.CurrentCulture);
				if (IsHebrewChar(c) && TokenMask == TokenType.RegularTokenMask && TryParseHebrewNumber(ref str, out var badFormat, out tokenValue))
				{
					if (badFormat)
					{
						tokenType = TokenType.UnknownToken;
						return false;
					}
					tokenType = TokenType.HebrewNumber;
					return true;
				}
			}
			int num = (int)c % 199;
			int num2 = 1 + (int)c % 197;
			int num3 = str.len - str.Index;
			int num4 = 0;
			TokenHashValue[] array = m_dtfiTokenHash;
			if (array == null)
			{
				array = CreateTokenHashTable();
			}
			do
			{
				TokenHashValue tokenHashValue = array[num];
				if (tokenHashValue == null)
				{
					break;
				}
				if ((tokenHashValue.tokenType & TokenMask) > (TokenType)0 && tokenHashValue.tokenString.Length <= num3)
				{
					if (string.Compare(str.Value, str.Index, tokenHashValue.tokenString, 0, tokenHashValue.tokenString.Length, ignoreCase: true, CultureInfo.CurrentCulture) == 0)
					{
						int index;
						if (flag && (index = str.Index + tokenHashValue.tokenString.Length) < str.len)
						{
							char c2 = str.Value[index];
							if (char.IsLetter(c2) && !IsAllowedJapaneseTokenFollowedByNonSpaceLetter(tokenHashValue.tokenString, c2))
							{
								return false;
							}
						}
						tokenType = tokenHashValue.tokenType & TokenMask;
						tokenValue = tokenHashValue.tokenValue;
						str.Advance(tokenHashValue.tokenString.Length);
						return true;
					}
					if (tokenHashValue.tokenType == TokenType.MonthToken && HasSpacesInMonthNames)
					{
						int matchLength = 0;
						if (str.MatchSpecifiedWords(tokenHashValue.tokenString, checkWordBoundary: true, ref matchLength))
						{
							tokenType = tokenHashValue.tokenType & TokenMask;
							tokenValue = tokenHashValue.tokenValue;
							str.Advance(matchLength);
							return true;
						}
					}
					else if (tokenHashValue.tokenType == TokenType.DayOfWeekToken && HasSpacesInDayNames)
					{
						int matchLength2 = 0;
						if (str.MatchSpecifiedWords(tokenHashValue.tokenString, checkWordBoundary: true, ref matchLength2))
						{
							tokenType = tokenHashValue.tokenType & TokenMask;
							tokenValue = tokenHashValue.tokenValue;
							str.Advance(matchLength2);
							return true;
						}
					}
				}
				num4++;
				num += num2;
				if (num >= 199)
				{
					num -= 199;
				}
			}
			while (num4 < 199);
			return false;
		}

		private void InsertAtCurrentHashNode(TokenHashValue[] hashTable, string str, char ch, TokenType tokenType, int tokenValue, int pos, int hashcode, int hashProbe)
		{
			TokenHashValue tokenHashValue = hashTable[hashcode];
			hashTable[hashcode] = new TokenHashValue(str, tokenType, tokenValue);
			while (++pos < 199)
			{
				hashcode += hashProbe;
				if (hashcode >= 199)
				{
					hashcode -= 199;
				}
				TokenHashValue tokenHashValue2 = hashTable[hashcode];
				if (tokenHashValue2 == null || char.ToLower(tokenHashValue2.tokenString[0], CultureInfo.CurrentCulture) == ch)
				{
					hashTable[hashcode] = tokenHashValue;
					if (tokenHashValue2 == null)
					{
						break;
					}
					tokenHashValue = tokenHashValue2;
				}
			}
		}

		private void InsertHash(TokenHashValue[] hashTable, string str, TokenType tokenType, int tokenValue)
		{
			if (str == null || str.Length == 0)
			{
				return;
			}
			int num = 0;
			if (char.IsWhiteSpace(str[0]) || char.IsWhiteSpace(str[str.Length - 1]))
			{
				str = str.Trim(null);
				if (str.Length == 0)
				{
					return;
				}
			}
			char c = char.ToLower(str[0], CultureInfo.CurrentCulture);
			int num2 = (int)c % 199;
			int num3 = 1 + (int)c % 197;
			do
			{
				TokenHashValue tokenHashValue = hashTable[num2];
				if (tokenHashValue == null)
				{
					hashTable[num2] = new TokenHashValue(str, tokenType, tokenValue);
					break;
				}
				if (str.Length >= tokenHashValue.tokenString.Length && string.Compare(str, 0, tokenHashValue.tokenString, 0, tokenHashValue.tokenString.Length, ignoreCase: true, CultureInfo.CurrentCulture) == 0)
				{
					if (str.Length > tokenHashValue.tokenString.Length)
					{
						InsertAtCurrentHashNode(hashTable, str, c, tokenType, tokenValue, num, num2, num3);
						break;
					}
					if ((tokenType & TokenType.SeparatorTokenMask) != (tokenHashValue.tokenType & TokenType.SeparatorTokenMask))
					{
						tokenHashValue.tokenType |= tokenType;
						if (tokenValue != 0)
						{
							tokenHashValue.tokenValue = tokenValue;
						}
					}
				}
				num++;
				num2 += num3;
				if (num2 >= 199)
				{
					num2 -= 199;
				}
			}
			while (num < 199);
		}

		internal static string GetCalendarInfo(int culture, int calendar, int calType)
		{
			int calendarInfo = Win32Native.GetCalendarInfo(culture, calendar, calType, null, 0, IntPtr.Zero);
			if (calendarInfo > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(calendarInfo);
				calendarInfo = Win32Native.GetCalendarInfo(culture, calendar, calType, stringBuilder, calendarInfo, IntPtr.Zero);
				if (calendarInfo > 0)
				{
					return stringBuilder.ToString(0, calendarInfo - 1);
				}
			}
			return null;
		}
	}
}
