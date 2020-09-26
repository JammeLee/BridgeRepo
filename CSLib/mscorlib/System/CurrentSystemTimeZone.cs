using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
	[Serializable]
	internal class CurrentSystemTimeZone : TimeZone
	{
		private const long TicksPerMillisecond = 10000L;

		private const long TicksPerSecond = 10000000L;

		private const long TicksPerMinute = 600000000L;

		private Hashtable m_CachedDaylightChanges = new Hashtable();

		private long m_ticksOffset;

		private string m_standardName;

		private string m_daylightName;

		private static object s_InternalSyncObject;

		public override string StandardName
		{
			get
			{
				if (m_standardName == null)
				{
					m_standardName = nativeGetStandardName();
				}
				return m_standardName;
			}
		}

		public override string DaylightName
		{
			get
			{
				if (m_daylightName == null)
				{
					m_daylightName = nativeGetDaylightName();
					if (m_daylightName == null)
					{
						m_daylightName = StandardName;
					}
				}
				return m_daylightName;
			}
		}

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

		internal CurrentSystemTimeZone()
		{
			m_ticksOffset = (long)nativeGetTimeZoneMinuteOffset() * 600000000L;
			m_standardName = null;
			m_daylightName = null;
		}

		internal long GetUtcOffsetFromUniversalTime(DateTime time, ref bool isAmbiguousLocalDst)
		{
			TimeSpan t = new TimeSpan(m_ticksOffset);
			DaylightTime daylightChanges = GetDaylightChanges(time.Year);
			isAmbiguousLocalDst = false;
			if (daylightChanges == null || daylightChanges.Delta.Ticks == 0)
			{
				return t.Ticks;
			}
			DateTime dateTime = daylightChanges.Start - t;
			DateTime dateTime2 = daylightChanges.End - t - daylightChanges.Delta;
			DateTime t2;
			DateTime t3;
			if (daylightChanges.Delta.Ticks > 0)
			{
				t2 = dateTime2 - daylightChanges.Delta;
				t3 = dateTime2;
			}
			else
			{
				t2 = dateTime;
				t3 = dateTime - daylightChanges.Delta;
			}
			bool flag = false;
			if ((!(dateTime > dateTime2)) ? (time >= dateTime && time < dateTime2) : (time < dateTime2 || time >= dateTime))
			{
				t += daylightChanges.Delta;
				if (time >= t2 && time < t3)
				{
					isAmbiguousLocalDst = true;
				}
			}
			return t.Ticks;
		}

		public override DateTime ToLocalTime(DateTime time)
		{
			if (time.Kind == DateTimeKind.Local)
			{
				return time;
			}
			bool isAmbiguousLocalDst = false;
			long utcOffsetFromUniversalTime = GetUtcOffsetFromUniversalTime(time, ref isAmbiguousLocalDst);
			long num = time.Ticks + utcOffsetFromUniversalTime;
			if (num > 3155378975999999999L)
			{
				return new DateTime(3155378975999999999L, DateTimeKind.Local);
			}
			if (num < 0)
			{
				return new DateTime(0L, DateTimeKind.Local);
			}
			return new DateTime(num, DateTimeKind.Local, isAmbiguousLocalDst);
		}

		public override DaylightTime GetDaylightChanges(int year)
		{
			if (year < 1 || year > 9999)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 1, 9999));
			}
			object key = year;
			if (!m_CachedDaylightChanges.Contains(key))
			{
				lock (InternalSyncObject)
				{
					if (!m_CachedDaylightChanges.Contains(key))
					{
						short[] array = nativeGetDaylightChanges();
						if (array == null)
						{
							m_CachedDaylightChanges.Add(key, new DaylightTime(DateTime.MinValue, DateTime.MinValue, TimeSpan.Zero));
						}
						else
						{
							DateTime dayOfWeek = GetDayOfWeek(year, array[0] != 0, array[1], array[2], array[3], array[4], array[5], array[6], array[7]);
							DateTime dayOfWeek2 = GetDayOfWeek(year, array[8] != 0, array[9], array[10], array[11], array[12], array[13], array[14], array[15]);
							TimeSpan delta = new TimeSpan((long)array[16] * 600000000L);
							DaylightTime value = new DaylightTime(dayOfWeek, dayOfWeek2, delta);
							m_CachedDaylightChanges.Add(key, value);
						}
					}
				}
			}
			return (DaylightTime)m_CachedDaylightChanges[key];
		}

		public override TimeSpan GetUtcOffset(DateTime time)
		{
			if (time.Kind == DateTimeKind.Utc)
			{
				return TimeSpan.Zero;
			}
			return new TimeSpan(TimeZone.CalculateUtcOffset(time, GetDaylightChanges(time.Year)).Ticks + m_ticksOffset);
		}

		private static DateTime GetDayOfWeek(int year, bool fixedDate, int month, int targetDayOfWeek, int numberOfSunday, int hour, int minute, int second, int millisecond)
		{
			DateTime result;
			if (fixedDate)
			{
				int num = DateTime.DaysInMonth(year, month);
				result = new DateTime(year, month, (num < numberOfSunday) ? num : numberOfSunday, hour, minute, second, millisecond, DateTimeKind.Local);
			}
			else if (numberOfSunday <= 4)
			{
				result = new DateTime(year, month, 1, hour, minute, second, millisecond, DateTimeKind.Local);
				int dayOfWeek = (int)result.DayOfWeek;
				int num2 = targetDayOfWeek - dayOfWeek;
				if (num2 < 0)
				{
					num2 += 7;
				}
				num2 += 7 * (numberOfSunday - 1);
				if (num2 > 0)
				{
					result = result.AddDays(num2);
					return result;
				}
			}
			else
			{
				Calendar defaultInstance = GregorianCalendar.GetDefaultInstance();
				result = new DateTime(year, month, defaultInstance.GetDaysInMonth(year, month), hour, minute, second, millisecond, DateTimeKind.Local);
				int dayOfWeek2 = (int)result.DayOfWeek;
				int num3 = dayOfWeek2 - targetDayOfWeek;
				if (num3 < 0)
				{
					num3 += 7;
				}
				if (num3 > 0)
				{
					result = result.AddDays(-num3);
					return result;
				}
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int nativeGetTimeZoneMinuteOffset();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nativeGetDaylightName();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nativeGetStandardName();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern short[] nativeGetDaylightChanges();
	}
}
