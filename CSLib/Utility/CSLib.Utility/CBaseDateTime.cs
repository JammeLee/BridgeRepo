using System;
using System.Globalization;

namespace CSLib.Utility
{
	public class CBaseDateTime
	{
		public const string Format = "yyyy-MM-dd HH:mm:ss";

		public const string FormatDate = "yyyy-MM-dd";

		public static DateTime GetDateTime(string strDateTime, string strFormat = "yyyy-MM-dd HH:mm:ss")
		{
			return DateTime.ParseExact(strDateTime, strFormat, CultureInfo.InvariantCulture);
		}

		public static string GetString(DateTime dtDateTime)
		{
			int a_ = 15;
			return string.Format(CSimpleThreadPool.b("お経畎㕐\u2e52", a_), dtDateTime);
		}

		public static bool IsExpired(DateTime dtExpired)
		{
			if (DateTime.Now > dtExpired)
			{
				return true;
			}
			return false;
		}

		public static int CalLeftSecond(string strDateTime)
		{
			//Discarded unreachable code: IL_000c
			int a_ = 0;
			if (true)
			{
			}
			DateTime dateTime = GetDateTime(strDateTime, CSimpleThreadPool.b("䔻䜽㤿㭁楃\u0b45Շ杉⡋⩍灏ᩑ᱓汕㕗㝙晛ⵝ\u135f", a_));
			DateTime now = DateTime.Now;
			return (int)(dateTime - now).TotalSeconds;
		}

		public static bool GetCurWeek(out DateTime dtDateBegin, out DateTime dtDateEnd)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			DateTime today = DateTime.Today;
			dtDateBegin = today.AddDays(0 - today.DayOfWeek);
			dtDateEnd = today.AddDays((double)(6 - today.DayOfWeek + 1));
			return true;
		}

		public static bool GetPreWeek(out DateTime dtDateBegin, out DateTime dtDateEnd)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			DateTime today = DateTime.Today;
			dtDateBegin = today.AddDays(0 - today.DayOfWeek - 7);
			dtDateEnd = today.AddDays((double)(6 - today.DayOfWeek - 7 + 1));
			return true;
		}

		public static bool GetCurMonth(out DateTime dtDateBegin, out DateTime dtDateEnd)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			DateTime today = DateTime.Today;
			dtDateBegin = today.AddDays(-(today.Day - 1));
			dtDateEnd = dtDateBegin.AddMonths(1);
			return true;
		}

		public static bool GetPreMonth(out DateTime dtDateBegin, out DateTime dtDateEnd)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			DateTime today = DateTime.Today;
			dtDateBegin = today.AddDays(-(today.Day - 1));
			dtDateBegin = dtDateBegin.AddMonths(-1);
			dtDateEnd = dtDateBegin.AddMonths(1);
			return true;
		}

		public static bool GetCurYear(out DateTime dtDateBegin, out DateTime dtDateEnd)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			DateTime today = DateTime.Today;
			dtDateBegin = today.AddDays(-(today.Day - 1));
			dtDateBegin = dtDateBegin.AddMonths(-(dtDateBegin.Month - 1));
			dtDateEnd = dtDateBegin.AddMonths(12);
			return true;
		}

		public static bool GetPreYear(out DateTime dtDateBegin, out DateTime dtDateEnd)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			DateTime today = DateTime.Today;
			dtDateBegin = today.AddDays(-(today.Day - 1));
			dtDateBegin = dtDateBegin.AddMonths(-(dtDateBegin.Month - 1));
			dtDateBegin = dtDateBegin.AddYears(-1);
			dtDateEnd = dtDateBegin.AddMonths(12);
			return true;
		}
	}
}
