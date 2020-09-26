using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public abstract class TimeZone
	{
		private static TimeZone currentTimeZone;

		private static object s_InternalSyncObject;

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

		public static TimeZone CurrentTimeZone
		{
			get
			{
				TimeZone timeZone = currentTimeZone;
				if (timeZone == null)
				{
					lock (InternalSyncObject)
					{
						if (currentTimeZone == null)
						{
							currentTimeZone = new CurrentSystemTimeZone();
						}
						return currentTimeZone;
					}
				}
				return timeZone;
			}
		}

		public abstract string StandardName
		{
			get;
		}

		public abstract string DaylightName
		{
			get;
		}

		internal static void ResetTimeZone()
		{
			if (currentTimeZone != null)
			{
				lock (InternalSyncObject)
				{
					currentTimeZone = null;
				}
			}
		}

		public abstract TimeSpan GetUtcOffset(DateTime time);

		public virtual DateTime ToUniversalTime(DateTime time)
		{
			if (time.Kind == DateTimeKind.Utc)
			{
				return time;
			}
			long num = time.Ticks - GetUtcOffset(time).Ticks;
			if (num > 3155378975999999999L)
			{
				return new DateTime(3155378975999999999L, DateTimeKind.Utc);
			}
			if (num < 0)
			{
				return new DateTime(0L, DateTimeKind.Utc);
			}
			return new DateTime(num, DateTimeKind.Utc);
		}

		public virtual DateTime ToLocalTime(DateTime time)
		{
			if (time.Kind == DateTimeKind.Local)
			{
				return time;
			}
			bool isAmbiguousLocalDst = false;
			long utcOffsetFromUniversalTime = ((CurrentSystemTimeZone)CurrentTimeZone).GetUtcOffsetFromUniversalTime(time, ref isAmbiguousLocalDst);
			return new DateTime(time.Ticks + utcOffsetFromUniversalTime, DateTimeKind.Local, isAmbiguousLocalDst);
		}

		public abstract DaylightTime GetDaylightChanges(int year);

		public virtual bool IsDaylightSavingTime(DateTime time)
		{
			return IsDaylightSavingTime(time, GetDaylightChanges(time.Year));
		}

		public static bool IsDaylightSavingTime(DateTime time, DaylightTime daylightTimes)
		{
			return CalculateUtcOffset(time, daylightTimes) != TimeSpan.Zero;
		}

		internal static TimeSpan CalculateUtcOffset(DateTime time, DaylightTime daylightTimes)
		{
			if (daylightTimes == null)
			{
				return TimeSpan.Zero;
			}
			DateTimeKind kind = time.Kind;
			if (kind == DateTimeKind.Utc)
			{
				return TimeSpan.Zero;
			}
			DateTime dateTime = daylightTimes.Start + daylightTimes.Delta;
			DateTime end = daylightTimes.End;
			DateTime t;
			DateTime t2;
			if (daylightTimes.Delta.Ticks > 0)
			{
				t = end - daylightTimes.Delta;
				t2 = end;
			}
			else
			{
				t = dateTime;
				t2 = dateTime - daylightTimes.Delta;
			}
			bool flag = false;
			if (dateTime > end)
			{
				if (time >= dateTime || time < end)
				{
					flag = true;
				}
			}
			else if (time >= dateTime && time < end)
			{
				flag = true;
			}
			if (flag && time >= t && time < t2)
			{
				flag = time.IsAmbiguousDaylightSavingTime();
			}
			if (flag)
			{
				return daylightTimes.Delta;
			}
			return TimeSpan.Zero;
		}
	}
}
