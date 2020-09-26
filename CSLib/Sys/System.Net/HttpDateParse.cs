using System.Globalization;

namespace System.Net
{
	internal static class HttpDateParse
	{
		private const int BASE_DEC = 10;

		private const int DATE_INDEX_DAY_OF_WEEK = 0;

		private const int DATE_1123_INDEX_DAY = 1;

		private const int DATE_1123_INDEX_MONTH = 2;

		private const int DATE_1123_INDEX_YEAR = 3;

		private const int DATE_1123_INDEX_HRS = 4;

		private const int DATE_1123_INDEX_MINS = 5;

		private const int DATE_1123_INDEX_SECS = 6;

		private const int DATE_ANSI_INDEX_MONTH = 1;

		private const int DATE_ANSI_INDEX_DAY = 2;

		private const int DATE_ANSI_INDEX_HRS = 3;

		private const int DATE_ANSI_INDEX_MINS = 4;

		private const int DATE_ANSI_INDEX_SECS = 5;

		private const int DATE_ANSI_INDEX_YEAR = 6;

		private const int DATE_INDEX_TZ = 7;

		private const int DATE_INDEX_LAST = 7;

		private const int MAX_FIELD_DATE_ENTRIES = 8;

		private const int DATE_TOKEN_JANUARY = 1;

		private const int DATE_TOKEN_FEBRUARY = 2;

		private const int DATE_TOKEN_MARCH = 3;

		private const int DATE_TOKEN_APRIL = 4;

		private const int DATE_TOKEN_MAY = 5;

		private const int DATE_TOKEN_JUNE = 6;

		private const int DATE_TOKEN_JULY = 7;

		private const int DATE_TOKEN_AUGUST = 8;

		private const int DATE_TOKEN_SEPTEMBER = 9;

		private const int DATE_TOKEN_OCTOBER = 10;

		private const int DATE_TOKEN_NOVEMBER = 11;

		private const int DATE_TOKEN_DECEMBER = 12;

		private const int DATE_TOKEN_LAST_MONTH = 13;

		private const int DATE_TOKEN_SUNDAY = 0;

		private const int DATE_TOKEN_MONDAY = 1;

		private const int DATE_TOKEN_TUESDAY = 2;

		private const int DATE_TOKEN_WEDNESDAY = 3;

		private const int DATE_TOKEN_THURSDAY = 4;

		private const int DATE_TOKEN_FRIDAY = 5;

		private const int DATE_TOKEN_SATURDAY = 6;

		private const int DATE_TOKEN_LAST_DAY = 7;

		private const int DATE_TOKEN_GMT = -1000;

		private const int DATE_TOKEN_LAST = -1000;

		private const int DATE_TOKEN_ERROR = -999;

		private static char MAKE_UPPER(char c)
		{
			return char.ToUpper(c, CultureInfo.InvariantCulture);
		}

		private static int MapDayMonthToDword(char[] lpszDay, int index)
		{
			switch (MAKE_UPPER(lpszDay[index]))
			{
			case 'A':
				return MAKE_UPPER(lpszDay[index + 1]) switch
				{
					'P' => 4, 
					'U' => 8, 
					_ => -999, 
				};
			case 'D':
				return 12;
			case 'F':
				return MAKE_UPPER(lpszDay[index + 1]) switch
				{
					'R' => 5, 
					'E' => 2, 
					_ => -999, 
				};
			case 'G':
				return -1000;
			case 'M':
				switch (MAKE_UPPER(lpszDay[index + 1]))
				{
				case 'O':
					return 1;
				case 'A':
					switch (MAKE_UPPER(lpszDay[index + 2]))
					{
					case 'R':
						return 3;
					case 'Y':
						return 5;
					}
					break;
				}
				return -999;
			case 'N':
				return 11;
			case 'J':
				switch (MAKE_UPPER(lpszDay[index + 1]))
				{
				case 'A':
					return 1;
				case 'U':
					switch (MAKE_UPPER(lpszDay[index + 2]))
					{
					case 'N':
						return 6;
					case 'L':
						return 7;
					}
					break;
				}
				return -999;
			case 'O':
				return 10;
			case 'S':
				return MAKE_UPPER(lpszDay[index + 1]) switch
				{
					'A' => 6, 
					'U' => 0, 
					'E' => 9, 
					_ => -999, 
				};
			case 'T':
				return MAKE_UPPER(lpszDay[index + 1]) switch
				{
					'U' => 2, 
					'H' => 4, 
					_ => -999, 
				};
			case 'U':
				return -1000;
			case 'W':
				return 3;
			default:
				return -999;
			}
		}

		public static bool ParseHttpDate(string DateString, out DateTime dtOut)
		{
			int num = 0;
			int num2 = 0;
			int num3 = -1;
			bool flag = false;
			int[] array = new int[8];
			bool result = true;
			char[] array2 = DateString.ToCharArray();
			dtOut = default(DateTime);
			while (true)
			{
				if (num < DateString.Length && num2 < 8)
				{
					if (array2[num] >= '0' && array2[num] <= '9')
					{
						array[num2] = 0;
						do
						{
							array[num2] *= 10;
							array[num2] += array2[num] - 48;
							num++;
						}
						while (num < DateString.Length && array2[num] >= '0' && array2[num] <= '9');
						num2++;
					}
					else if ((array2[num] >= 'A' && array2[num] <= 'Z') || (array2[num] >= 'a' && array2[num] <= 'z'))
					{
						array[num2] = MapDayMonthToDword(array2, num);
						num3 = num2;
						if (array[num2] == -999 && (!flag || num2 != 6))
						{
							result = false;
							break;
						}
						if (num2 == 1)
						{
							flag = true;
						}
						do
						{
							num++;
						}
						while (num < DateString.Length && ((array2[num] >= 'A' && array2[num] <= 'Z') || (array2[num] >= 'a' && array2[num] <= 'z')));
						num2++;
					}
					else
					{
						num++;
					}
					continue;
				}
				int millisecond = 0;
				int num4;
				int month;
				int num5;
				int num6;
				int num7;
				int num8;
				if (flag)
				{
					num4 = array[2];
					month = array[1];
					num5 = array[3];
					num6 = array[4];
					num7 = array[5];
					num8 = ((num3 == 6) ? array[7] : array[6]);
				}
				else
				{
					num4 = array[1];
					month = array[2];
					num8 = array[3];
					num5 = array[4];
					num6 = array[5];
					num7 = array[6];
				}
				if (num8 < 100)
				{
					num8 += ((num8 < 80) ? 2000 : 1900);
				}
				if (num2 < 4 || num4 > 31 || num5 > 23 || num6 > 59 || num7 > 59)
				{
					result = false;
					break;
				}
				dtOut = new DateTime(num8, month, num4, num5, num6, num7, millisecond);
				if (num3 == 6)
				{
					dtOut = dtOut.ToUniversalTime();
				}
				if (num2 > 7 && array[7] != -1000)
				{
					double value = array[7];
					dtOut.AddHours(value);
				}
				dtOut = dtOut.ToLocalTime();
				break;
			}
			return result;
		}

		public static bool ParseCookieDate(string dateString, out DateTime dtOut)
		{
			dtOut = DateTime.MinValue;
			char[] array = dateString.ToCharArray();
			if (array.Length < 18)
			{
				return false;
			}
			int num = 0;
			int num2 = 0;
			char c;
			if (!char.IsDigit(c = array[num++]))
			{
				return false;
			}
			num2 = c - 48;
			if (!char.IsDigit(c = array[num++]))
			{
				num--;
			}
			else
			{
				num2 = num2 * 10 + (c - 48);
			}
			if (num2 > 31)
			{
				return false;
			}
			num++;
			int num3 = MapDayMonthToDword(array, num);
			if (num3 == -999)
			{
				return false;
			}
			num += 4;
			int num4 = 0;
			int i;
			for (i = 0; i < 4; i++)
			{
				if (!char.IsDigit(c = array[i + num]))
				{
					if (i == 2)
					{
						break;
					}
					return false;
				}
				num4 = num4 * 10 + (c - 48);
			}
			if (i == 2)
			{
				num4 += ((num4 < 80) ? 2000 : 1900);
			}
			i += num;
			if (array[i++] != ' ')
			{
				return false;
			}
			int num5 = 0;
			if (!char.IsDigit(c = array[i++]))
			{
				return false;
			}
			num5 = c - 48;
			if (!char.IsDigit(c = array[i++]))
			{
				i--;
			}
			else
			{
				num5 = num5 * 10 + (c - 48);
			}
			if (num5 > 24 || array[i++] != ':')
			{
				return false;
			}
			int num6 = 0;
			if (!char.IsDigit(c = array[i++]))
			{
				return false;
			}
			num6 = c - 48;
			if (!char.IsDigit(c = array[i++]))
			{
				i--;
			}
			else
			{
				num6 = num6 * 10 + (c - 48);
			}
			if (num6 > 60 || array[i++] != ':')
			{
				return false;
			}
			if (array.Length - i < 5)
			{
				return false;
			}
			int num7 = 0;
			if (!char.IsDigit(c = array[i++]))
			{
				return false;
			}
			num7 = c - 48;
			if (!char.IsDigit(c = array[i++]))
			{
				i--;
			}
			else
			{
				num7 = num7 * 10 + (c - 48);
			}
			if (num7 > 60 || array[i++] != ' ')
			{
				return false;
			}
			if (array.Length - i < 3 || array[i++] != 'G' || array[i++] != 'M' || array[i++] != 'T')
			{
				return false;
			}
			dtOut = new DateTime(num4, num3, num2, num5, num6, num7, 0).ToLocalTime();
			return true;
		}
	}
}
