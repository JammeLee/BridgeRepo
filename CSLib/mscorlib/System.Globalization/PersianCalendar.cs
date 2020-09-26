namespace System.Globalization
{
	[Serializable]
	public class PersianCalendar : Calendar
	{
		internal const int DateCycle = 33;

		internal const int DatePartYear = 0;

		internal const int DatePartDayOfYear = 1;

		internal const int DatePartMonth = 2;

		internal const int DatePartDay = 3;

		internal const int LeapYearsPerCycle = 8;

		internal const long GregorianOffset = 226894L;

		internal const long DaysPerCycle = 12053L;

		internal const int MaxCalendarYear = 9378;

		internal const int MaxCalendarMonth = 10;

		internal const int MaxCalendarDay = 10;

		private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 1410;

		public static readonly int PersianEra = 1;

		internal static int[] DaysToMonth = new int[12]
		{
			0,
			31,
			62,
			93,
			124,
			155,
			186,
			216,
			246,
			276,
			306,
			336
		};

		internal static int[] LeapYears33 = new int[33]
		{
			0,
			1,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			1,
			0,
			0
		};

		internal static DateTime minDate = new DateTime(622, 3, 21);

		internal static DateTime maxDate = DateTime.MaxValue;

		public override DateTime MinSupportedDateTime => minDate;

		public override DateTime MaxSupportedDateTime => maxDate;

		public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.SolarCalendar;

		internal override int BaseCalendarID => 1;

		internal override int ID => 22;

		public override int[] Eras => new int[1]
		{
			PersianEra
		};

		public override int TwoDigitYearMax
		{
			get
			{
				if (twoDigitYearMax == -1)
				{
					twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 1410);
				}
				return twoDigitYearMax;
			}
			set
			{
				VerifyWritable();
				if (value < 99 || value > 9378)
				{
					throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 99, 9378));
				}
				twoDigitYearMax = value;
			}
		}

		private long GetAbsoluteDatePersian(int year, int month, int day)
		{
			if (year >= 1 && year <= 9378 && month >= 1 && month <= 12)
			{
				return DaysUpToPersianYear(year) + DaysToMonth[month - 1] + day - 1;
			}
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
		}

		private long DaysUpToPersianYear(int PersianYear)
		{
			int num = (PersianYear - 1) / 33;
			int num2 = (PersianYear - 1) % 33;
			long num3 = (long)num * 12053L + 226894;
			while (num2 > 0)
			{
				num3 += 365;
				if (IsLeapYear(num2, 0))
				{
					num3++;
				}
				num2--;
			}
			return num3;
		}

		internal void CheckTicksRange(long ticks)
		{
			if (ticks < minDate.Ticks || ticks > maxDate.Ticks)
			{
				throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("ArgumentOutOfRange_CalendarRange"), minDate, maxDate));
			}
		}

		internal void CheckEraRange(int era)
		{
			if (era != 0 && era != PersianEra)
			{
				throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
			}
		}

		internal void CheckYearRange(int year, int era)
		{
			CheckEraRange(era);
			if (year < 1 || year > 9378)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9378));
			}
		}

		internal void CheckYearMonthRange(int year, int month, int era)
		{
			CheckYearRange(year, era);
			if (year == 9378 && month > 10)
			{
				throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 10));
			}
			if (month < 1 || month > 12)
			{
				throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
			}
		}

		internal int GetDatePart(long ticks, int part)
		{
			CheckTicksRange(ticks);
			long num = ticks / 864000000000L + 1;
			int num2 = (int)((num - 226894) * 33 / 12053) + 1;
			long num3 = DaysUpToPersianYear(num2);
			long num4 = GetDaysInYear(num2, 0);
			if (num < num3)
			{
				num3 -= num4;
				num2--;
			}
			else if (num == num3)
			{
				num2--;
				num3 -= GetDaysInYear(num2, 0);
			}
			else if (num > num3 + num4)
			{
				num3 += num4;
				num2++;
			}
			if (part == 0)
			{
				return num2;
			}
			num -= num3;
			if (part == 1)
			{
				return (int)num;
			}
			int i;
			for (i = 0; i < 12 && num > DaysToMonth[i]; i++)
			{
			}
			if (part == 2)
			{
				return i;
			}
			int result = (int)(num - DaysToMonth[i - 1]);
			if (part == 3)
			{
				return result;
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DateTimeParsing"));
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
			int daysInMonth = GetDaysInMonth(datePart, datePart2);
			if (num > daysInMonth)
			{
				num = daysInMonth;
			}
			long ticks = GetAbsoluteDatePersian(datePart, datePart2, num) * 864000000000L + time.Ticks % 864000000000L;
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

		public override int GetDayOfYear(DateTime time)
		{
			return GetDatePart(time.Ticks, 1);
		}

		public override int GetDaysInMonth(int year, int month, int era)
		{
			CheckYearMonthRange(year, month, era);
			if (month == 10 && year == 9378)
			{
				return 10;
			}
			if (month == 12)
			{
				if (!IsLeapYear(year, 0))
				{
					return 29;
				}
				return 30;
			}
			if (month <= 6)
			{
				return 31;
			}
			return 30;
		}

		public override int GetDaysInYear(int year, int era)
		{
			CheckYearRange(year, era);
			if (year == 9378)
			{
				return DaysToMonth[9] + 10;
			}
			if (!IsLeapYear(year, 0))
			{
				return 365;
			}
			return 366;
		}

		public override int GetEra(DateTime time)
		{
			CheckTicksRange(time.Ticks);
			return PersianEra;
		}

		public override int GetMonth(DateTime time)
		{
			return GetDatePart(time.Ticks, 2);
		}

		public override int GetMonthsInYear(int year, int era)
		{
			CheckYearRange(year, era);
			if (year == 9378)
			{
				return 10;
			}
			return 12;
		}

		public override int GetYear(DateTime time)
		{
			return GetDatePart(time.Ticks, 0);
		}

		public override bool IsLeapDay(int year, int month, int day, int era)
		{
			int daysInMonth = GetDaysInMonth(year, month, era);
			if (day < 1 || day > daysInMonth)
			{
				throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), daysInMonth, month));
			}
			if (IsLeapYear(year, era) && month == 12)
			{
				return day == 30;
			}
			return false;
		}

		public override int GetLeapMonth(int year, int era)
		{
			CheckYearRange(year, era);
			return 0;
		}

		public override bool IsLeapMonth(int year, int month, int era)
		{
			CheckYearMonthRange(year, month, era);
			return false;
		}

		public override bool IsLeapYear(int year, int era)
		{
			CheckYearRange(year, era);
			return LeapYears33[year % 33] == 1;
		}

		public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
		{
			int daysInMonth = GetDaysInMonth(year, month, era);
			if (day < 1 || day > daysInMonth)
			{
				throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), daysInMonth, month));
			}
			long absoluteDatePersian = GetAbsoluteDatePersian(year, month, day);
			if (absoluteDatePersian >= 0)
			{
				return new DateTime(absoluteDatePersian * 864000000000L + Calendar.TimeToTicks(hour, minute, second, millisecond));
			}
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
		}

		public override int ToFourDigitYear(int year)
		{
			if (year < 100)
			{
				return base.ToFourDigitYear(year);
			}
			if (year > 9378)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9378));
			}
			return year;
		}
	}
}
