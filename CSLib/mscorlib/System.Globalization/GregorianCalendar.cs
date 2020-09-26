using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public class GregorianCalendar : Calendar
	{
		public const int ADEra = 1;

		internal const int DatePartYear = 0;

		internal const int DatePartDayOfYear = 1;

		internal const int DatePartMonth = 2;

		internal const int DatePartDay = 3;

		internal const int MaxYear = 9999;

		private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 2029;

		internal GregorianCalendarTypes m_type;

		internal static readonly int[] DaysToMonth365 = new int[13]
		{
			0,
			31,
			59,
			90,
			120,
			151,
			181,
			212,
			243,
			273,
			304,
			334,
			365
		};

		internal static readonly int[] DaysToMonth366 = new int[13]
		{
			0,
			31,
			60,
			91,
			121,
			152,
			182,
			213,
			244,
			274,
			305,
			335,
			366
		};

		internal static Calendar m_defaultInstance = null;

		[ComVisible(false)]
		public override DateTime MinSupportedDateTime => DateTime.MinValue;

		[ComVisible(false)]
		public override DateTime MaxSupportedDateTime => DateTime.MaxValue;

		[ComVisible(false)]
		public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.SolarCalendar;

		public virtual GregorianCalendarTypes CalendarType
		{
			get
			{
				return m_type;
			}
			set
			{
				VerifyWritable();
				switch (value)
				{
				case GregorianCalendarTypes.Localized:
				case GregorianCalendarTypes.USEnglish:
				case GregorianCalendarTypes.MiddleEastFrench:
				case GregorianCalendarTypes.Arabic:
				case GregorianCalendarTypes.TransliteratedEnglish:
				case GregorianCalendarTypes.TransliteratedFrench:
					m_type = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("m_type", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
				}
			}
		}

		internal override int ID => (int)m_type;

		public override int[] Eras => new int[1]
		{
			1
		};

		public override int TwoDigitYearMax
		{
			get
			{
				if (twoDigitYearMax == -1)
				{
					twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 2029);
				}
				return twoDigitYearMax;
			}
			set
			{
				VerifyWritable();
				if (value < 99 || value > 9999)
				{
					throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 99, 9999));
				}
				twoDigitYearMax = value;
			}
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			if (m_type < GregorianCalendarTypes.Localized || m_type > GregorianCalendarTypes.TransliteratedFrench)
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_MemberOutOfRange"), "type", "GregorianCalendar"));
			}
		}

		internal static Calendar GetDefaultInstance()
		{
			if (m_defaultInstance == null)
			{
				m_defaultInstance = new GregorianCalendar();
			}
			return m_defaultInstance;
		}

		public GregorianCalendar()
			: this(GregorianCalendarTypes.Localized)
		{
		}

		public GregorianCalendar(GregorianCalendarTypes type)
		{
			if (type < GregorianCalendarTypes.Localized || type > GregorianCalendarTypes.TransliteratedFrench)
			{
				throw new ArgumentOutOfRangeException("type", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), GregorianCalendarTypes.Localized, GregorianCalendarTypes.TransliteratedFrench));
			}
			m_type = type;
		}

		internal virtual int GetDatePart(long ticks, int part)
		{
			int num = (int)(ticks / 864000000000L);
			int num2 = num / 146097;
			num -= num2 * 146097;
			int num3 = num / 36524;
			if (num3 == 4)
			{
				num3 = 3;
			}
			num -= num3 * 36524;
			int num4 = num / 1461;
			num -= num4 * 1461;
			int num5 = num / 365;
			if (num5 == 4)
			{
				num5 = 3;
			}
			if (part == 0)
			{
				return num2 * 400 + num3 * 100 + num4 * 4 + num5 + 1;
			}
			num -= num5 * 365;
			if (part == 1)
			{
				return num + 1;
			}
			int[] array = ((num5 == 3 && (num4 != 24 || num3 == 3)) ? DaysToMonth366 : DaysToMonth365);
			int i;
			for (i = num >> 6; num >= array[i]; i++)
			{
			}
			if (part == 2)
			{
				return i;
			}
			return num - array[i - 1] + 1;
		}

		internal static long GetAbsoluteDate(int year, int month, int day)
		{
			if (year >= 1 && year <= 9999 && month >= 1 && month <= 12)
			{
				int[] array = ((year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) ? DaysToMonth366 : DaysToMonth365);
				if (day >= 1 && day <= array[month] - array[month - 1])
				{
					int num = year - 1;
					int num2 = num * 365 + num / 4 - num / 100 + num / 400 + array[month - 1] + day - 1;
					return num2;
				}
			}
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
		}

		internal virtual long DateToTicks(int year, int month, int day)
		{
			return GetAbsoluteDate(year, month, day) * 864000000000L;
		}

		public override DateTime AddMonths(DateTime time, int months)
		{
			if (months < -120000 || months > 120000)
			{
				throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), -120000, 120000));
			}
			int datePart = GetDatePart(time.Ticks, 0);
			int datePart2 = GetDatePart(time.Ticks, 2);
			int num = GetDatePart(time.Ticks, 3);
			int num2 = datePart2 - 1 + months;
			if (num2 >= 0)
			{
				datePart2 = num2 % 12 + 1;
				datePart += num2 / 12;
			}
			else
			{
				datePart2 = 12 + (num2 + 1) % 12;
				datePart += (num2 - 11) / 12;
			}
			int[] array = ((datePart % 4 == 0 && (datePart % 100 != 0 || datePart % 400 == 0)) ? DaysToMonth366 : DaysToMonth365);
			int num3 = array[datePart2] - array[datePart2 - 1];
			if (num > num3)
			{
				num = num3;
			}
			long ticks = DateToTicks(datePart, datePart2, num) + time.Ticks % 864000000000L;
			Calendar.CheckAddResult(ticks, MinSupportedDateTime, MaxSupportedDateTime);
			return new DateTime(ticks);
		}

		public override DateTime AddYears(DateTime time, int years)
		{
			return AddMonths(time, years * 12);
		}

		public override int GetDayOfMonth(DateTime time)
		{
			return GetDatePart(time.Ticks, 3);
		}

		public override DayOfWeek GetDayOfWeek(DateTime time)
		{
			return (DayOfWeek)((int)(time.Ticks / 864000000000L + 1) % 7);
		}

		[ComVisible(false)]
		public override int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
		{
			if (firstDayOfWeek < DayOfWeek.Sunday || firstDayOfWeek > DayOfWeek.Saturday)
			{
				throw new ArgumentOutOfRangeException("firstDayOfWeek", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), DayOfWeek.Sunday, DayOfWeek.Saturday));
			}
			return rule switch
			{
				CalendarWeekRule.FirstDay => GetFirstDayWeekOfYear(time, (int)firstDayOfWeek), 
				CalendarWeekRule.FirstFullWeek => InternalGetWeekOfYearFullDays(this, time, (int)firstDayOfWeek, 7, 365), 
				CalendarWeekRule.FirstFourDayWeek => InternalGetWeekOfYearFullDays(this, time, (int)firstDayOfWeek, 4, 365), 
				_ => throw new ArgumentOutOfRangeException("rule", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), CalendarWeekRule.FirstDay, CalendarWeekRule.FirstFourDayWeek)), 
			};
		}

		internal static int InternalGetWeekOfYearFullDays(Calendar cal, DateTime time, int firstDayOfWeek, int fullDays, int daysOfMinYearMinusOne)
		{
			int num = cal.GetDayOfYear(time) - 1;
			int num2 = (int)(cal.GetDayOfWeek(time) - num % 7);
			int num3 = (firstDayOfWeek - num2 + 14) % 7;
			if (num3 != 0 && num3 >= fullDays)
			{
				num3 -= 7;
			}
			int num4 = num - num3;
			if (num4 >= 0)
			{
				return num4 / 7 + 1;
			}
			int year = cal.GetYear(time);
			num = ((year > cal.GetYear(cal.MinSupportedDateTime)) ? cal.GetDaysInYear(year - 1) : daysOfMinYearMinusOne);
			num2 -= num % 7;
			num3 = (firstDayOfWeek - num2 + 14) % 7;
			if (num3 != 0 && num3 >= fullDays)
			{
				num3 -= 7;
			}
			num4 = num - num3;
			return num4 / 7 + 1;
		}

		public override int GetDayOfYear(DateTime time)
		{
			return GetDatePart(time.Ticks, 1);
		}

		public override int GetDaysInMonth(int year, int month, int era)
		{
			if (era == 0 || era == 1)
			{
				if (year < 1 || year > 9999)
				{
					throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
				}
				if (month < 1 || month > 12)
				{
					throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
				}
				int[] array = ((year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) ? DaysToMonth366 : DaysToMonth365);
				return array[month] - array[month - 1];
			}
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
		}

		public override int GetDaysInYear(int year, int era)
		{
			if (era == 0 || era == 1)
			{
				if (year >= 1 && year <= 9999)
				{
					if (year % 4 != 0 || (year % 100 == 0 && year % 400 != 0))
					{
						return 365;
					}
					return 366;
				}
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
			}
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
		}

		public override int GetEra(DateTime time)
		{
			return 1;
		}

		public override int GetMonth(DateTime time)
		{
			return GetDatePart(time.Ticks, 2);
		}

		public override int GetMonthsInYear(int year, int era)
		{
			if (era == 0 || era == 1)
			{
				if (year >= 1 && year <= 9999)
				{
					return 12;
				}
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
			}
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
		}

		public override int GetYear(DateTime time)
		{
			return GetDatePart(time.Ticks, 0);
		}

		public override bool IsLeapDay(int year, int month, int day, int era)
		{
			if (era != 0 && era != 1)
			{
				throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
			}
			if (year < 1 || year > 9999)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
			}
			if (month < 1 || month > 12)
			{
				throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 12));
			}
			if (day < 1 || day > GetDaysInMonth(year, month))
			{
				throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, GetDaysInMonth(year, month)));
			}
			if (!IsLeapYear(year))
			{
				return false;
			}
			if (month == 2 && day == 29)
			{
				return true;
			}
			return false;
		}

		[ComVisible(false)]
		public override int GetLeapMonth(int year, int era)
		{
			if (era != 0 && era != 1)
			{
				throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
			}
			if (year < 1 || year > 9999)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
			}
			return 0;
		}

		public override bool IsLeapMonth(int year, int month, int era)
		{
			if (era != 0 && era != 1)
			{
				throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
			}
			if (year < 1 || year > 9999)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
			}
			if (month < 1 || month > 12)
			{
				throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 12));
			}
			return false;
		}

		public override bool IsLeapYear(int year, int era)
		{
			if (era == 0 || era == 1)
			{
				if (year >= 1 && year <= 9999)
				{
					if (year % 4 == 0)
					{
						if (year % 100 == 0)
						{
							return year % 400 == 0;
						}
						return true;
					}
					return false;
				}
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
			}
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
		}

		public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
		{
			if (era == 0 || era == 1)
			{
				return new DateTime(year, month, day, hour, minute, second, millisecond);
			}
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
		}

		internal override bool TryToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era, out DateTime result)
		{
			if (era == 0 || era == 1)
			{
				return DateTime.TryCreate(year, month, day, hour, minute, second, millisecond, out result);
			}
			result = DateTime.MinValue;
			return false;
		}

		public override int ToFourDigitYear(int year)
		{
			if (year > 9999)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
			}
			return base.ToFourDigitYear(year);
		}
	}
}
