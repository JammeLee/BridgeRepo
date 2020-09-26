using System.Runtime.InteropServices;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public class JapaneseCalendar : Calendar
	{
		private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 99;

		internal static readonly DateTime calendarMinValue = new DateTime(1868, 9, 8);

		internal static EraInfo[] m_EraInfo = GregorianCalendarHelper.InitEraInfo(3);

		internal static Calendar m_defaultInstance;

		internal GregorianCalendarHelper helper;

		[ComVisible(false)]
		public override DateTime MinSupportedDateTime => calendarMinValue;

		[ComVisible(false)]
		public override DateTime MaxSupportedDateTime => DateTime.MaxValue;

		[ComVisible(false)]
		public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.SolarCalendar;

		internal override int ID => 3;

		public override int[] Eras => helper.Eras;

		public override int TwoDigitYearMax
		{
			get
			{
				if (twoDigitYearMax == -1)
				{
					twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 99);
				}
				return twoDigitYearMax;
			}
			set
			{
				VerifyWritable();
				if (value < 99 || value > helper.MaxYear)
				{
					throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 99, helper.MaxYear));
				}
				twoDigitYearMax = value;
			}
		}

		internal static Calendar GetDefaultInstance()
		{
			if (m_defaultInstance == null)
			{
				m_defaultInstance = new JapaneseCalendar();
			}
			return m_defaultInstance;
		}

		public JapaneseCalendar()
		{
			helper = new GregorianCalendarHelper(this, m_EraInfo);
		}

		public override DateTime AddMonths(DateTime time, int months)
		{
			return helper.AddMonths(time, months);
		}

		public override DateTime AddYears(DateTime time, int years)
		{
			return helper.AddYears(time, years);
		}

		public override int GetDaysInMonth(int year, int month, int era)
		{
			return helper.GetDaysInMonth(year, month, era);
		}

		public override int GetDaysInYear(int year, int era)
		{
			return helper.GetDaysInYear(year, era);
		}

		public override int GetDayOfMonth(DateTime time)
		{
			return helper.GetDayOfMonth(time);
		}

		public override DayOfWeek GetDayOfWeek(DateTime time)
		{
			return helper.GetDayOfWeek(time);
		}

		public override int GetDayOfYear(DateTime time)
		{
			return helper.GetDayOfYear(time);
		}

		public override int GetMonthsInYear(int year, int era)
		{
			return helper.GetMonthsInYear(year, era);
		}

		[ComVisible(false)]
		public override int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
		{
			return helper.GetWeekOfYear(time, rule, firstDayOfWeek);
		}

		public override int GetEra(DateTime time)
		{
			return helper.GetEra(time);
		}

		public override int GetMonth(DateTime time)
		{
			return helper.GetMonth(time);
		}

		public override int GetYear(DateTime time)
		{
			return helper.GetYear(time);
		}

		public override bool IsLeapDay(int year, int month, int day, int era)
		{
			return helper.IsLeapDay(year, month, day, era);
		}

		public override bool IsLeapYear(int year, int era)
		{
			return helper.IsLeapYear(year, era);
		}

		[ComVisible(false)]
		public override int GetLeapMonth(int year, int era)
		{
			return helper.GetLeapMonth(year, era);
		}

		public override bool IsLeapMonth(int year, int month, int era)
		{
			return helper.IsLeapMonth(year, month, era);
		}

		public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
		{
			return helper.ToDateTime(year, month, day, hour, minute, second, millisecond, era);
		}

		public override int ToFourDigitYear(int year)
		{
			if (year <= 0)
			{
				throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			if (year > helper.MaxYear)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, helper.MaxYear));
			}
			return year;
		}

		internal override bool IsValidYear(int year, int era)
		{
			return helper.IsValidYear(year, era);
		}
	}
}
