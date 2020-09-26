using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public class HijriCalendar : Calendar
	{
		internal const int DatePartYear = 0;

		internal const int DatePartDayOfYear = 1;

		internal const int DatePartMonth = 2;

		internal const int DatePartDay = 3;

		internal const int MinAdvancedHijri = -2;

		internal const int MaxAdvancedHijri = 2;

		internal const int MaxCalendarYear = 9666;

		internal const int MaxCalendarMonth = 4;

		internal const int MaxCalendarDay = 3;

		private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 1451;

		public static readonly int HijriEra = 1;

		internal static readonly int[] HijriMonthDays = new int[13]
		{
			0,
			30,
			59,
			89,
			118,
			148,
			177,
			207,
			236,
			266,
			295,
			325,
			355
		};

		private static string m_InternationalRegKey = "Control Panel\\International";

		private static string m_HijriAdvanceRegKeyEntry = "AddHijriDate";

		private int m_HijriAdvance = int.MinValue;

		internal static readonly DateTime calendarMinValue = new DateTime(622, 7, 18);

		internal static readonly DateTime calendarMaxValue = DateTime.MaxValue;

		[ComVisible(false)]
		public override DateTime MinSupportedDateTime => calendarMinValue;

		[ComVisible(false)]
		public override DateTime MaxSupportedDateTime => calendarMaxValue;

		[ComVisible(false)]
		public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.LunarCalendar;

		internal override int ID => 6;

		public int HijriAdjustment
		{
			get
			{
				if (m_HijriAdvance == int.MinValue)
				{
					m_HijriAdvance = GetAdvanceHijriDate();
				}
				return m_HijriAdvance;
			}
			set
			{
				VerifyWritable();
				if (value < -2 || value > 2)
				{
					throw new ArgumentOutOfRangeException("HijriAdjustment", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), -2, 2));
				}
				m_HijriAdvance = value;
			}
		}

		public override int[] Eras => new int[1]
		{
			HijriEra
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
				if (value < 99 || value > 9666)
				{
					throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 99, 9666));
				}
				twoDigitYearMax = value;
			}
		}

		private long GetAbsoluteDateHijri(int y, int m, int d)
		{
			return DaysUpToHijriYear(y) + HijriMonthDays[m - 1] + d - 1 - HijriAdjustment;
		}

		private long DaysUpToHijriYear(int HijriYear)
		{
			int num = (HijriYear - 1) / 30 * 30;
			int num2 = HijriYear - num - 1;
			long num3 = (long)num * 10631L / 30 + 227013;
			while (num2 > 0)
			{
				num3 += 354 + (IsLeapYear(num2, 0) ? 1 : 0);
				num2--;
			}
			return num3;
		}

		private int GetAdvanceHijriDate()
		{
			int result = 0;
			RegistryKey registryKey = null;
			try
			{
				registryKey = Registry.CurrentUser.InternalOpenSubKey(m_InternationalRegKey, writable: false);
			}
			catch (ObjectDisposedException)
			{
				return 0;
			}
			catch (ArgumentException)
			{
				return 0;
			}
			if (registryKey != null)
			{
				try
				{
					object obj = registryKey.InternalGetValue(m_HijriAdvanceRegKeyEntry, null, doNotExpand: false, checkSecurity: false);
					if (obj == null)
					{
						return 0;
					}
					string text = obj.ToString();
					if (string.Compare(text, 0, m_HijriAdvanceRegKeyEntry, 0, m_HijriAdvanceRegKeyEntry.Length, StringComparison.OrdinalIgnoreCase) == 0)
					{
						if (text.Length != m_HijriAdvanceRegKeyEntry.Length)
						{
							text = text.Substring(m_HijriAdvanceRegKeyEntry.Length);
							try
							{
								int num = int.Parse(text.ToString(), CultureInfo.InvariantCulture);
								if (num < -2)
								{
									return result;
								}
								if (num > 2)
								{
									return result;
								}
								result = num;
								return result;
							}
							catch (ArgumentException)
							{
								return result;
							}
							catch (FormatException)
							{
								return result;
							}
							catch (OverflowException)
							{
								return result;
							}
						}
						return -1;
					}
					return result;
				}
				finally
				{
					registryKey.Close();
				}
			}
			return result;
		}

		internal void CheckTicksRange(long ticks)
		{
			if (ticks < calendarMinValue.Ticks || ticks > calendarMaxValue.Ticks)
			{
				throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("ArgumentOutOfRange_CalendarRange"), calendarMinValue, calendarMaxValue));
			}
		}

		internal void CheckEraRange(int era)
		{
			if (era != 0 && era != HijriEra)
			{
				throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
			}
		}

		internal void CheckYearRange(int year, int era)
		{
			CheckEraRange(era);
			if (year < 1 || year > 9666)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9666));
			}
		}

		internal void CheckYearMonthRange(int year, int month, int era)
		{
			CheckYearRange(year, era);
			if (year == 9666 && month > 4)
			{
				throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 4));
			}
			if (month < 1 || month > 12)
			{
				throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
			}
		}

		internal virtual int GetDatePart(long ticks, int part)
		{
			CheckTicksRange(ticks);
			long num = ticks / 864000000000L + 1;
			num += HijriAdjustment;
			int num2 = (int)((num - 227013) * 30 / 10631) + 1;
			long num3 = DaysUpToHijriYear(num2);
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
			int i = 1;
			num -= num3;
			if (part == 1)
			{
				return (int)num;
			}
			for (; i <= 12 && num > HijriMonthDays[i - 1]; i++)
			{
			}
			i--;
			if (part == 2)
			{
				return i;
			}
			int result = (int)(num - HijriMonthDays[i - 1]);
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
			long ticks = GetAbsoluteDateHijri(datePart, datePart2, num) * 864000000000L + time.Ticks % 864000000000L;
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
			if (month == 12)
			{
				if (!IsLeapYear(year, 0))
				{
					return 29;
				}
				return 30;
			}
			if (month % 2 != 1)
			{
				return 29;
			}
			return 30;
		}

		public override int GetDaysInYear(int year, int era)
		{
			CheckYearRange(year, era);
			if (!IsLeapYear(year, 0))
			{
				return 354;
			}
			return 355;
		}

		public override int GetEra(DateTime time)
		{
			CheckTicksRange(time.Ticks);
			return HijriEra;
		}

		public override int GetMonth(DateTime time)
		{
			return GetDatePart(time.Ticks, 2);
		}

		public override int GetMonthsInYear(int year, int era)
		{
			CheckYearRange(year, era);
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

		[ComVisible(false)]
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
			return (year * 11 + 14) % 30 < 11;
		}

		public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
		{
			int daysInMonth = GetDaysInMonth(year, month, era);
			if (day < 1 || day > daysInMonth)
			{
				throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), daysInMonth, month));
			}
			long absoluteDateHijri = GetAbsoluteDateHijri(year, month, day);
			if (absoluteDateHijri >= 0)
			{
				return new DateTime(absoluteDateHijri * 864000000000L + Calendar.TimeToTicks(hour, minute, second, millisecond));
			}
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
		}

		public override int ToFourDigitYear(int year)
		{
			if (year < 100)
			{
				return base.ToFourDigitYear(year);
			}
			if (year > 9666)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9666));
			}
			return year;
		}
	}
}
