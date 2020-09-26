using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[StructLayout(LayoutKind.Auto)]
	public struct DateTimeOffset : IComparable, IFormattable, ISerializable, IDeserializationCallback, IComparable<DateTimeOffset>, IEquatable<DateTimeOffset>
	{
		internal const long MaxOffset = 504000000000L;

		internal const long MinOffset = -504000000000L;

		public static readonly DateTimeOffset MinValue = new DateTimeOffset(0L, TimeSpan.Zero);

		public static readonly DateTimeOffset MaxValue = new DateTimeOffset(3155378975999999999L, TimeSpan.Zero);

		private DateTime m_dateTime;

		private short m_offsetMinutes;

		public static DateTimeOffset Now => new DateTimeOffset(DateTime.Now);

		public static DateTimeOffset UtcNow => new DateTimeOffset(DateTime.UtcNow);

		public DateTime DateTime => ClockDateTime;

		public DateTime UtcDateTime => DateTime.SpecifyKind(m_dateTime, DateTimeKind.Utc);

		public DateTime LocalDateTime => UtcDateTime.ToLocalTime();

		private DateTime ClockDateTime => new DateTime((m_dateTime + Offset).Ticks, DateTimeKind.Unspecified);

		public DateTime Date => ClockDateTime.Date;

		public int Day => ClockDateTime.Day;

		public DayOfWeek DayOfWeek => ClockDateTime.DayOfWeek;

		public int DayOfYear => ClockDateTime.DayOfYear;

		public int Hour => ClockDateTime.Hour;

		public int Millisecond => ClockDateTime.Millisecond;

		public int Minute => ClockDateTime.Minute;

		public int Month => ClockDateTime.Month;

		public TimeSpan Offset => new TimeSpan(0, m_offsetMinutes, 0);

		public int Second => ClockDateTime.Second;

		public long Ticks => ClockDateTime.Ticks;

		public long UtcTicks => UtcDateTime.Ticks;

		public TimeSpan TimeOfDay => ClockDateTime.TimeOfDay;

		public int Year => ClockDateTime.Year;

		public DateTimeOffset(long ticks, TimeSpan offset)
		{
			m_offsetMinutes = ValidateOffset(offset);
			DateTime dateTime = new DateTime(ticks);
			m_dateTime = ValidateDate(dateTime, offset);
		}

		public DateTimeOffset(DateTime dateTime)
		{
			TimeSpan offset = ((dateTime.Kind != DateTimeKind.Utc) ? TimeZone.CurrentTimeZone.GetUtcOffset(dateTime) : new TimeSpan(0L));
			m_offsetMinutes = ValidateOffset(offset);
			m_dateTime = ValidateDate(dateTime, offset);
		}

		public DateTimeOffset(DateTime dateTime, TimeSpan offset)
		{
			if (dateTime.Kind == DateTimeKind.Local)
			{
				if (offset != TimeZone.CurrentTimeZone.GetUtcOffset(dateTime))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_OffsetLocalMismatch"), "offset");
				}
			}
			else if (dateTime.Kind == DateTimeKind.Utc && offset != TimeSpan.Zero)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_OffsetUtcMismatch"), "offset");
			}
			m_offsetMinutes = ValidateOffset(offset);
			m_dateTime = ValidateDate(dateTime, offset);
		}

		public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, TimeSpan offset)
		{
			m_offsetMinutes = ValidateOffset(offset);
			int num = second;
			if (second == 60 && DateTime.s_isLeapSecondsSupportedSystem)
			{
				second = 59;
			}
			m_dateTime = ValidateDate(new DateTime(year, month, day, hour, minute, second), offset);
			if (num == 60 && !DateTime.IsValidTimeWithLeapSeconds(m_dateTime.Year, m_dateTime.Month, m_dateTime.Day, m_dateTime.Hour, m_dateTime.Minute, 60, DateTimeKind.Utc))
			{
				throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadHourMinuteSecond"));
			}
		}

		public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, TimeSpan offset)
		{
			m_offsetMinutes = ValidateOffset(offset);
			int num = second;
			if (second == 60 && DateTime.s_isLeapSecondsSupportedSystem)
			{
				second = 59;
			}
			m_dateTime = ValidateDate(new DateTime(year, month, day, hour, minute, second, millisecond), offset);
			if (num == 60 && !DateTime.IsValidTimeWithLeapSeconds(m_dateTime.Year, m_dateTime.Month, m_dateTime.Day, m_dateTime.Hour, m_dateTime.Minute, 60, DateTimeKind.Utc))
			{
				throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadHourMinuteSecond"));
			}
		}

		public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, TimeSpan offset)
		{
			m_offsetMinutes = ValidateOffset(offset);
			int num = second;
			if (second == 60 && DateTime.s_isLeapSecondsSupportedSystem)
			{
				second = 59;
			}
			m_dateTime = ValidateDate(new DateTime(year, month, day, hour, minute, second, millisecond, calendar), offset);
			if (num == 60 && !DateTime.IsValidTimeWithLeapSeconds(m_dateTime.Year, m_dateTime.Month, m_dateTime.Day, m_dateTime.Hour, m_dateTime.Minute, 60, DateTimeKind.Utc))
			{
				throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadHourMinuteSecond"));
			}
		}

		public DateTimeOffset ToOffset(TimeSpan offset)
		{
			return new DateTimeOffset((m_dateTime + offset).Ticks, offset);
		}

		public DateTimeOffset Add(TimeSpan timeSpan)
		{
			return new DateTimeOffset(ClockDateTime.Add(timeSpan), Offset);
		}

		public DateTimeOffset AddDays(double days)
		{
			return new DateTimeOffset(ClockDateTime.AddDays(days), Offset);
		}

		public DateTimeOffset AddHours(double hours)
		{
			return new DateTimeOffset(ClockDateTime.AddHours(hours), Offset);
		}

		public DateTimeOffset AddMilliseconds(double milliseconds)
		{
			return new DateTimeOffset(ClockDateTime.AddMilliseconds(milliseconds), Offset);
		}

		public DateTimeOffset AddMinutes(double minutes)
		{
			return new DateTimeOffset(ClockDateTime.AddMinutes(minutes), Offset);
		}

		public DateTimeOffset AddMonths(int months)
		{
			return new DateTimeOffset(ClockDateTime.AddMonths(months), Offset);
		}

		public DateTimeOffset AddSeconds(double seconds)
		{
			return new DateTimeOffset(ClockDateTime.AddSeconds(seconds), Offset);
		}

		public DateTimeOffset AddTicks(long ticks)
		{
			return new DateTimeOffset(ClockDateTime.AddTicks(ticks), Offset);
		}

		public DateTimeOffset AddYears(int years)
		{
			return new DateTimeOffset(ClockDateTime.AddYears(years), Offset);
		}

		public static int Compare(DateTimeOffset first, DateTimeOffset second)
		{
			return DateTime.Compare(first.UtcDateTime, second.UtcDateTime);
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (!(obj is DateTimeOffset))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDateTimeOffset"));
			}
			DateTime utcDateTime = ((DateTimeOffset)obj).UtcDateTime;
			DateTime utcDateTime2 = UtcDateTime;
			if (utcDateTime2 > utcDateTime)
			{
				return 1;
			}
			if (utcDateTime2 < utcDateTime)
			{
				return -1;
			}
			return 0;
		}

		public int CompareTo(DateTimeOffset other)
		{
			DateTime utcDateTime = other.UtcDateTime;
			DateTime utcDateTime2 = UtcDateTime;
			if (utcDateTime2 > utcDateTime)
			{
				return 1;
			}
			if (utcDateTime2 < utcDateTime)
			{
				return -1;
			}
			return 0;
		}

		public override bool Equals(object obj)
		{
			if (obj is DateTimeOffset)
			{
				return UtcDateTime.Equals(((DateTimeOffset)obj).UtcDateTime);
			}
			return false;
		}

		public bool Equals(DateTimeOffset other)
		{
			return UtcDateTime.Equals(other.UtcDateTime);
		}

		public bool EqualsExact(DateTimeOffset other)
		{
			if (ClockDateTime == other.ClockDateTime && Offset == other.Offset)
			{
				return ClockDateTime.Kind == other.ClockDateTime.Kind;
			}
			return false;
		}

		public static bool Equals(DateTimeOffset first, DateTimeOffset second)
		{
			return DateTime.Equals(first.UtcDateTime, second.UtcDateTime);
		}

		public static DateTimeOffset FromFileTime(long fileTime)
		{
			return new DateTimeOffset(DateTime.FromFileTime(fileTime));
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
			try
			{
				m_offsetMinutes = ValidateOffset(Offset);
				m_dateTime = ValidateDate(ClockDateTime, Offset);
			}
			catch (ArgumentException innerException)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), innerException);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("DateTime", m_dateTime);
			info.AddValue("OffsetMinutes", m_offsetMinutes);
		}

		private DateTimeOffset(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			m_dateTime = (DateTime)info.GetValue("DateTime", typeof(DateTime));
			m_offsetMinutes = (short)info.GetValue("OffsetMinutes", typeof(short));
		}

		public override int GetHashCode()
		{
			return UtcDateTime.GetHashCode();
		}

		public static DateTimeOffset Parse(string input)
		{
			TimeSpan offset;
			return new DateTimeOffset(DateTimeParse.Parse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out offset).Ticks, offset);
		}

		public static DateTimeOffset Parse(string input, IFormatProvider formatProvider)
		{
			return Parse(input, formatProvider, DateTimeStyles.None);
		}

		public static DateTimeOffset Parse(string input, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			styles = ValidateStyles(styles, "styles");
			TimeSpan offset;
			return new DateTimeOffset(DateTimeParse.Parse(input, DateTimeFormatInfo.GetInstance(formatProvider), styles, out offset).Ticks, offset);
		}

		public static DateTimeOffset ParseExact(string input, string format, IFormatProvider formatProvider)
		{
			return ParseExact(input, format, formatProvider, DateTimeStyles.None);
		}

		public static DateTimeOffset ParseExact(string input, string format, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			styles = ValidateStyles(styles, "styles");
			TimeSpan offset;
			return new DateTimeOffset(DateTimeParse.ParseExact(input, format, DateTimeFormatInfo.GetInstance(formatProvider), styles, out offset).Ticks, offset);
		}

		public static DateTimeOffset ParseExact(string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			styles = ValidateStyles(styles, "styles");
			TimeSpan offset;
			return new DateTimeOffset(DateTimeParse.ParseExactMultiple(input, formats, DateTimeFormatInfo.GetInstance(formatProvider), styles, out offset).Ticks, offset);
		}

		public TimeSpan Subtract(DateTimeOffset value)
		{
			return UtcDateTime.Subtract(value.UtcDateTime);
		}

		public DateTimeOffset Subtract(TimeSpan value)
		{
			return new DateTimeOffset(ClockDateTime.Subtract(value), Offset);
		}

		public long ToFileTime()
		{
			return UtcDateTime.ToFileTime();
		}

		public DateTimeOffset ToLocalTime()
		{
			return new DateTimeOffset(UtcDateTime.ToLocalTime());
		}

		public override string ToString()
		{
			return DateTimeFormat.Format(ClockDateTime, null, DateTimeFormatInfo.CurrentInfo, Offset);
		}

		public string ToString(string format)
		{
			return DateTimeFormat.Format(ClockDateTime, format, DateTimeFormatInfo.CurrentInfo, Offset);
		}

		public string ToString(IFormatProvider formatProvider)
		{
			return DateTimeFormat.Format(ClockDateTime, null, DateTimeFormatInfo.GetInstance(formatProvider), Offset);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return DateTimeFormat.Format(ClockDateTime, format, DateTimeFormatInfo.GetInstance(formatProvider), Offset);
		}

		public DateTimeOffset ToUniversalTime()
		{
			return new DateTimeOffset(UtcDateTime);
		}

		public static bool TryParse(string input, out DateTimeOffset result)
		{
			DateTime result2;
			TimeSpan offset;
			bool result3 = DateTimeParse.TryParse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out result2, out offset);
			result = new DateTimeOffset(result2.Ticks, offset);
			return result3;
		}

		public static bool TryParse(string input, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
		{
			styles = ValidateStyles(styles, "styles");
			DateTime result2;
			TimeSpan offset;
			bool result3 = DateTimeParse.TryParse(input, DateTimeFormatInfo.GetInstance(formatProvider), styles, out result2, out offset);
			result = new DateTimeOffset(result2.Ticks, offset);
			return result3;
		}

		public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
		{
			styles = ValidateStyles(styles, "styles");
			DateTime result2;
			TimeSpan offset;
			bool result3 = DateTimeParse.TryParseExact(input, format, DateTimeFormatInfo.GetInstance(formatProvider), styles, out result2, out offset);
			result = new DateTimeOffset(result2.Ticks, offset);
			return result3;
		}

		public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
		{
			styles = ValidateStyles(styles, "styles");
			DateTime result2;
			TimeSpan offset;
			bool result3 = DateTimeParse.TryParseExactMultiple(input, formats, DateTimeFormatInfo.GetInstance(formatProvider), styles, out result2, out offset);
			result = new DateTimeOffset(result2.Ticks, offset);
			return result3;
		}

		private static short ValidateOffset(TimeSpan offset)
		{
			long ticks = offset.Ticks;
			if (ticks % 600000000 != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_OffsetPrecision"), "offset");
			}
			if (ticks < -504000000000L || ticks > 504000000000L)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Argument_OffsetOutOfRange"));
			}
			return (short)(offset.Ticks / 600000000);
		}

		private static DateTime ValidateDate(DateTime dateTime, TimeSpan offset)
		{
			long num = dateTime.Ticks - offset.Ticks;
			if (num < 0 || num > 3155378975999999999L)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Argument_UTCOutOfRange"));
			}
			return new DateTime(num, DateTimeKind.Unspecified);
		}

		private static DateTimeStyles ValidateStyles(DateTimeStyles style, string parameterName)
		{
			if (((uint)style & 0xFFFFFF00u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDateTimeStyles"), parameterName);
			}
			if ((style & DateTimeStyles.AssumeLocal) != 0 && (style & DateTimeStyles.AssumeUniversal) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ConflictingDateTimeStyles"), parameterName);
			}
			if ((style & DateTimeStyles.NoCurrentDateDefault) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeOffsetInvalidDateTimeStyles"), parameterName);
			}
			style &= ~DateTimeStyles.RoundtripKind;
			style &= ~DateTimeStyles.AssumeLocal;
			return style;
		}

		public static implicit operator DateTimeOffset(DateTime dateTime)
		{
			return new DateTimeOffset(dateTime);
		}

		public static DateTimeOffset operator +(DateTimeOffset dateTimeTz, TimeSpan timeSpan)
		{
			return new DateTimeOffset(dateTimeTz.ClockDateTime + timeSpan, dateTimeTz.Offset);
		}

		public static DateTimeOffset operator -(DateTimeOffset dateTimeTz, TimeSpan timeSpan)
		{
			return new DateTimeOffset(dateTimeTz.ClockDateTime - timeSpan, dateTimeTz.Offset);
		}

		public static TimeSpan operator -(DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime - right.UtcDateTime;
		}

		public static bool operator ==(DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime == right.UtcDateTime;
		}

		public static bool operator !=(DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime != right.UtcDateTime;
		}

		public static bool operator <(DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime < right.UtcDateTime;
		}

		public static bool operator <=(DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime <= right.UtcDateTime;
		}

		public static bool operator >(DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime > right.UtcDateTime;
		}

		public static bool operator >=(DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime >= right.UtcDateTime;
		}
	}
}
