namespace System.Globalization
{
	[Serializable]
	public class UmAlQuraCalendar : Calendar
	{
		internal struct DateMapping
		{
			internal int HijriMonthsLengthFlags;

			internal DateTime GregorianDate;

			internal DateMapping(int MonthsLengthFlags, int GYear, int GMonth, int GDay)
			{
				HijriMonthsLengthFlags = MonthsLengthFlags;
				GregorianDate = new DateTime(GYear, GMonth, GDay);
			}
		}

		internal const int MinCalendarYear = 1318;

		internal const int MaxCalendarYear = 1450;

		public const int UmAlQuraEra = 1;

		internal const int DateCycle = 30;

		internal const int DatePartYear = 0;

		internal const int DatePartDayOfYear = 1;

		internal const int DatePartMonth = 2;

		internal const int DatePartDay = 3;

		private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 1451;

		private static readonly DateMapping[] HijriYearInfo = new DateMapping[134]
		{
			new DateMapping(746, 1900, 4, 30),
			new DateMapping(1769, 1901, 4, 19),
			new DateMapping(3794, 1902, 4, 9),
			new DateMapping(3748, 1903, 3, 30),
			new DateMapping(3402, 1904, 3, 18),
			new DateMapping(2710, 1905, 3, 7),
			new DateMapping(1334, 1906, 2, 24),
			new DateMapping(2741, 1907, 2, 13),
			new DateMapping(3498, 1908, 2, 3),
			new DateMapping(2980, 1909, 1, 23),
			new DateMapping(2889, 1910, 1, 12),
			new DateMapping(2707, 1911, 1, 1),
			new DateMapping(1323, 1911, 12, 21),
			new DateMapping(2647, 1912, 12, 9),
			new DateMapping(1206, 1913, 11, 29),
			new DateMapping(2741, 1914, 11, 18),
			new DateMapping(1450, 1915, 11, 8),
			new DateMapping(3413, 1916, 10, 27),
			new DateMapping(3370, 1917, 10, 17),
			new DateMapping(2646, 1918, 10, 6),
			new DateMapping(1198, 1919, 9, 25),
			new DateMapping(2397, 1920, 9, 13),
			new DateMapping(748, 1921, 9, 3),
			new DateMapping(1749, 1922, 8, 23),
			new DateMapping(1706, 1923, 8, 13),
			new DateMapping(1365, 1924, 8, 1),
			new DateMapping(1195, 1925, 7, 21),
			new DateMapping(2395, 1926, 7, 10),
			new DateMapping(698, 1927, 6, 30),
			new DateMapping(1397, 1928, 6, 18),
			new DateMapping(2994, 1929, 6, 8),
			new DateMapping(1892, 1930, 5, 29),
			new DateMapping(1865, 1931, 5, 18),
			new DateMapping(1621, 1932, 5, 6),
			new DateMapping(683, 1933, 4, 25),
			new DateMapping(1371, 1934, 4, 14),
			new DateMapping(2778, 1935, 4, 4),
			new DateMapping(1748, 1936, 3, 24),
			new DateMapping(3785, 1937, 3, 13),
			new DateMapping(3474, 1938, 3, 3),
			new DateMapping(3365, 1939, 2, 20),
			new DateMapping(2637, 1940, 2, 9),
			new DateMapping(685, 1941, 1, 28),
			new DateMapping(1389, 1942, 1, 17),
			new DateMapping(2922, 1943, 1, 7),
			new DateMapping(2898, 1943, 12, 28),
			new DateMapping(2725, 1944, 12, 16),
			new DateMapping(2635, 1945, 12, 5),
			new DateMapping(1175, 1946, 11, 24),
			new DateMapping(2359, 1947, 11, 13),
			new DateMapping(694, 1948, 11, 2),
			new DateMapping(1397, 1949, 10, 22),
			new DateMapping(3434, 1950, 10, 12),
			new DateMapping(3410, 1951, 10, 2),
			new DateMapping(2710, 1952, 9, 20),
			new DateMapping(2349, 1953, 9, 9),
			new DateMapping(605, 1954, 8, 29),
			new DateMapping(1245, 1955, 8, 18),
			new DateMapping(2778, 1956, 8, 7),
			new DateMapping(1492, 1957, 7, 28),
			new DateMapping(3497, 1958, 7, 17),
			new DateMapping(3410, 1959, 7, 7),
			new DateMapping(2730, 1960, 6, 25),
			new DateMapping(1238, 1961, 6, 14),
			new DateMapping(2486, 1962, 6, 3),
			new DateMapping(884, 1963, 5, 24),
			new DateMapping(1897, 1964, 5, 12),
			new DateMapping(1874, 1965, 5, 2),
			new DateMapping(1701, 1966, 4, 21),
			new DateMapping(1355, 1967, 4, 10),
			new DateMapping(2731, 1968, 3, 29),
			new DateMapping(1370, 1969, 3, 19),
			new DateMapping(2773, 1970, 3, 8),
			new DateMapping(3538, 1971, 2, 26),
			new DateMapping(3492, 1972, 2, 16),
			new DateMapping(3401, 1973, 2, 4),
			new DateMapping(2709, 1974, 1, 24),
			new DateMapping(1325, 1975, 1, 13),
			new DateMapping(2653, 1976, 1, 2),
			new DateMapping(1370, 1976, 12, 22),
			new DateMapping(2773, 1977, 12, 11),
			new DateMapping(1706, 1978, 12, 1),
			new DateMapping(1685, 1979, 11, 20),
			new DateMapping(1323, 1980, 11, 8),
			new DateMapping(2647, 1981, 10, 28),
			new DateMapping(1198, 1982, 10, 18),
			new DateMapping(2422, 1983, 10, 7),
			new DateMapping(1388, 1984, 9, 26),
			new DateMapping(2901, 1985, 9, 15),
			new DateMapping(2730, 1986, 9, 5),
			new DateMapping(2645, 1987, 8, 25),
			new DateMapping(1197, 1988, 8, 13),
			new DateMapping(2397, 1989, 8, 2),
			new DateMapping(730, 1990, 7, 23),
			new DateMapping(1497, 1991, 7, 12),
			new DateMapping(3506, 1992, 7, 1),
			new DateMapping(2980, 1993, 6, 21),
			new DateMapping(2890, 1994, 6, 10),
			new DateMapping(2645, 1995, 5, 30),
			new DateMapping(693, 1996, 5, 18),
			new DateMapping(1397, 1997, 5, 7),
			new DateMapping(2922, 1998, 4, 27),
			new DateMapping(3026, 1999, 4, 17),
			new DateMapping(3012, 2000, 4, 6),
			new DateMapping(2953, 2001, 3, 26),
			new DateMapping(2709, 2002, 3, 15),
			new DateMapping(1325, 2003, 3, 4),
			new DateMapping(1453, 2004, 2, 21),
			new DateMapping(2922, 2005, 2, 10),
			new DateMapping(1748, 2006, 1, 31),
			new DateMapping(3529, 2007, 1, 20),
			new DateMapping(3474, 2008, 1, 10),
			new DateMapping(2726, 2008, 12, 29),
			new DateMapping(2390, 2009, 12, 18),
			new DateMapping(686, 2010, 12, 7),
			new DateMapping(1389, 2011, 11, 26),
			new DateMapping(874, 2012, 11, 15),
			new DateMapping(2901, 2013, 11, 4),
			new DateMapping(2730, 2014, 10, 25),
			new DateMapping(2381, 2015, 10, 14),
			new DateMapping(1181, 2016, 10, 2),
			new DateMapping(2397, 2017, 9, 21),
			new DateMapping(698, 2018, 9, 11),
			new DateMapping(1461, 2019, 8, 31),
			new DateMapping(1450, 2020, 8, 20),
			new DateMapping(3413, 2021, 8, 9),
			new DateMapping(2714, 2022, 7, 30),
			new DateMapping(2350, 2023, 7, 19),
			new DateMapping(622, 2024, 7, 7),
			new DateMapping(1373, 2025, 6, 26),
			new DateMapping(2778, 2026, 6, 16),
			new DateMapping(1748, 2027, 6, 6),
			new DateMapping(1701, 2028, 5, 25),
			new DateMapping(0, 2029, 5, 14)
		};

		internal static short[] gmonth = new short[14]
		{
			31,
			31,
			28,
			31,
			30,
			31,
			30,
			31,
			31,
			30,
			31,
			30,
			31,
			31
		};

		internal static DateTime minDate = new DateTime(1900, 4, 30);

		internal static DateTime maxDate = new DateTime(new DateTime(2029, 5, 13, 23, 59, 59, 999).Ticks + 9999);

		public override DateTime MinSupportedDateTime => minDate;

		public override DateTime MaxSupportedDateTime => maxDate;

		public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.LunarCalendar;

		internal override int BaseCalendarID => 6;

		internal override int ID => 23;

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
					twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 1451);
				}
				return twoDigitYearMax;
			}
			set
			{
				VerifyWritable();
				if (value != 99 && (value < 1318 || value > 1450))
				{
					throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1318, 1450));
				}
				twoDigitYearMax = value;
			}
		}

		private void ConvertHijriToGregorian(int HijriYear, int HijriMonth, int HijriDay, ref int yg, ref int mg, ref int dg)
		{
			int num = HijriDay - 1;
			int num2 = HijriYear - 1318;
			DateTime gregorianDate = HijriYearInfo[num2].GregorianDate;
			int num3 = HijriYearInfo[num2].HijriMonthsLengthFlags;
			for (int i = 1; i < HijriMonth; i++)
			{
				num += 29 + (num3 & 1);
				num3 >>= 1;
			}
			gregorianDate = gregorianDate.AddDays(num);
			yg = gregorianDate.Year;
			mg = gregorianDate.Month;
			dg = gregorianDate.Day;
		}

		private long GetAbsoluteDateUmAlQura(int year, int month, int day)
		{
			int yg = 0;
			int mg = 0;
			int dg = 0;
			ConvertHijriToGregorian(year, month, day, ref yg, ref mg, ref dg);
			return GregorianCalendar.GetAbsoluteDate(yg, mg, dg);
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
			if (era != 0 && era != 1)
			{
				throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
			}
		}

		internal void CheckYearRange(int year, int era)
		{
			CheckEraRange(era);
			if (year < 1318 || year > 1450)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1318, 1450));
			}
		}

		internal void CheckYearMonthRange(int year, int month, int era)
		{
			CheckYearRange(year, era);
			if (month < 1 || month > 12)
			{
				throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
			}
		}

		private void ConvertGregorianToHijri(DateTime time, ref int HijriYear, ref int HijriMonth, ref int HijriDay)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = (int)((time.Ticks - minDate.Ticks) / 864000000000L) / 355;
			while (time.CompareTo(HijriYearInfo[++num4].GregorianDate) > 0)
			{
			}
			if (time.CompareTo(HijriYearInfo[num4].GregorianDate) != 0)
			{
				num4--;
			}
			TimeSpan timeSpan = time.Subtract(HijriYearInfo[num4].GregorianDate);
			num = num4 + 1318;
			num2 = 1;
			num3 = 1;
			double num5 = timeSpan.TotalDays;
			int num6 = HijriYearInfo[num4].HijriMonthsLengthFlags;
			int num7 = 29 + (num6 & 1);
			while (num5 >= (double)num7)
			{
				num5 -= (double)num7;
				num6 >>= 1;
				num7 = 29 + (num6 & 1);
				num2++;
			}
			num3 = (HijriDay = num3 + (int)num5);
			HijriMonth = num2;
			HijriYear = num;
		}

		internal virtual int GetDatePart(DateTime time, int part)
		{
			int HijriYear = 0;
			int HijriMonth = 0;
			int HijriDay = 0;
			long ticks = time.Ticks;
			CheckTicksRange(ticks);
			ConvertGregorianToHijri(time, ref HijriYear, ref HijriMonth, ref HijriDay);
			return part switch
			{
				0 => HijriYear, 
				2 => HijriMonth, 
				3 => HijriDay, 
				1 => (int)(GetAbsoluteDateUmAlQura(HijriYear, HijriMonth, HijriDay) - GetAbsoluteDateUmAlQura(HijriYear, 1, 1) + 1), 
				_ => throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DateTimeParsing")), 
			};
		}

		public override DateTime AddMonths(DateTime time, int months)
		{
			if (months < -120000 || months > 120000)
			{
				throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), -120000, 120000));
			}
			int datePart = GetDatePart(time, 0);
			int datePart2 = GetDatePart(time, 2);
			int num = GetDatePart(time, 3);
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
			if (num > 29)
			{
				int daysInMonth = GetDaysInMonth(datePart, datePart2);
				if (num > daysInMonth)
				{
					num = daysInMonth;
				}
			}
			CheckYearRange(datePart, 1);
			DateTime result = new DateTime(GetAbsoluteDateUmAlQura(datePart, datePart2, num) * 864000000000L + time.Ticks % 864000000000L);
			Calendar.CheckAddResult(result.Ticks, MinSupportedDateTime, MaxSupportedDateTime);
			return result;
		}

		public override DateTime AddYears(DateTime time, int years)
		{
			return AddMonths(time, years * 12);
		}

		public override int GetDayOfMonth(DateTime time)
		{
			return GetDatePart(time, 3);
		}

		public override DayOfWeek GetDayOfWeek(DateTime time)
		{
			return (DayOfWeek)((int)(time.Ticks / 864000000000L + 1) % 7);
		}

		public override int GetDayOfYear(DateTime time)
		{
			return GetDatePart(time, 1);
		}

		public override int GetDaysInMonth(int year, int month, int era)
		{
			CheckYearMonthRange(year, month, era);
			if ((HijriYearInfo[year - 1318].HijriMonthsLengthFlags & (1 << month - 1)) == 0)
			{
				return 29;
			}
			return 30;
		}

		internal int RealGetDaysInYear(int year)
		{
			int num = 0;
			int num2 = HijriYearInfo[year - 1318].HijriMonthsLengthFlags;
			for (int i = 1; i <= 12; i++)
			{
				num += 29 + (num2 & 1);
				num2 >>= 1;
			}
			return num;
		}

		public override int GetDaysInYear(int year, int era)
		{
			CheckYearRange(year, era);
			return RealGetDaysInYear(year);
		}

		public override int GetEra(DateTime time)
		{
			CheckTicksRange(time.Ticks);
			return 1;
		}

		public override int GetMonth(DateTime time)
		{
			return GetDatePart(time, 2);
		}

		public override int GetMonthsInYear(int year, int era)
		{
			CheckYearRange(year, era);
			return 12;
		}

		public override int GetYear(DateTime time)
		{
			return GetDatePart(time, 0);
		}

		public override bool IsLeapDay(int year, int month, int day, int era)
		{
			if (day >= 1 && day <= 29)
			{
				CheckYearMonthRange(year, month, era);
				return false;
			}
			int daysInMonth = GetDaysInMonth(year, month, era);
			if (day < 1 || day > daysInMonth)
			{
				throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), daysInMonth, month));
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
			if (RealGetDaysInYear(year) == 355)
			{
				return true;
			}
			return false;
		}

		public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
		{
			if (day >= 1 && day <= 29)
			{
				CheckYearMonthRange(year, month, era);
			}
			else
			{
				int daysInMonth = GetDaysInMonth(year, month, era);
				if (day < 1 || day > daysInMonth)
				{
					throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), daysInMonth, month));
				}
			}
			long absoluteDateUmAlQura = GetAbsoluteDateUmAlQura(year, month, day);
			if (absoluteDateUmAlQura >= 0)
			{
				return new DateTime(absoluteDateUmAlQura * 864000000000L + Calendar.TimeToTicks(hour, minute, second, millisecond));
			}
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
		}

		public override int ToFourDigitYear(int year)
		{
			if (year < 100)
			{
				return base.ToFourDigitYear(year);
			}
			if (year < 1318 || year > 1450)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1318, 1450));
			}
			return year;
		}
	}
}
