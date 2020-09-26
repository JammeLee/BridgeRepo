using System.Runtime.InteropServices;
using System.Text;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public struct TimeSpan : IComparable, IComparable<TimeSpan>, IEquatable<TimeSpan>
	{
		private struct StringParser
		{
			private enum ParseError
			{
				Format = 1,
				Overflow,
				OverflowHoursMinutesSeconds,
				ArgumentNull
			}

			private string str;

			private char ch;

			private int pos;

			private int len;

			private ParseError error;

			internal void NextChar()
			{
				if (pos < len)
				{
					pos++;
				}
				ch = ((pos < len) ? str[pos] : '\0');
			}

			internal char NextNonDigit()
			{
				for (int i = pos; i < len; i++)
				{
					char c = str[i];
					if (c < '0' || c > '9')
					{
						return c;
					}
				}
				return '\0';
			}

			internal long Parse(string s)
			{
				if (TryParse(s, out var value))
				{
					return value;
				}
				return error switch
				{
					ParseError.ArgumentNull => throw new ArgumentNullException("s"), 
					ParseError.Format => throw new FormatException(Environment.GetResourceString("Format_InvalidString")), 
					ParseError.Overflow => throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanTooLong")), 
					ParseError.OverflowHoursMinutesSeconds => throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanElementTooLarge")), 
					_ => 0L, 
				};
			}

			internal bool TryParse(string s, out long value)
			{
				value = 0L;
				if (s == null)
				{
					error = ParseError.ArgumentNull;
					return false;
				}
				str = s;
				len = s.Length;
				pos = -1;
				NextChar();
				SkipBlanks();
				bool flag = false;
				if (ch == '-')
				{
					flag = true;
					NextChar();
				}
				long time;
				if (NextNonDigit() == ':')
				{
					if (!ParseTime(out time))
					{
						return false;
					}
				}
				else
				{
					if (!ParseInt(10675199, out var i))
					{
						return false;
					}
					time = i * 864000000000L;
					if (ch == '.')
					{
						NextChar();
						if (!ParseTime(out var time2))
						{
							return false;
						}
						time += time2;
					}
				}
				if (flag)
				{
					time = -time;
					if (time > 0)
					{
						error = ParseError.Overflow;
						return false;
					}
				}
				else if (time < 0)
				{
					error = ParseError.Overflow;
					return false;
				}
				SkipBlanks();
				if (pos < len)
				{
					error = ParseError.Format;
					return false;
				}
				value = time;
				return true;
			}

			internal bool ParseInt(int max, out int i)
			{
				i = 0;
				int num = pos;
				while (ch >= '0' && ch <= '9')
				{
					if ((i & 0xF0000000u) != 0)
					{
						error = ParseError.Overflow;
						return false;
					}
					i = i * 10 + ch - 48;
					if (i < 0)
					{
						error = ParseError.Overflow;
						return false;
					}
					NextChar();
				}
				if (num == pos)
				{
					error = ParseError.Format;
					return false;
				}
				if (i > max)
				{
					error = ParseError.Overflow;
					return false;
				}
				return true;
			}

			internal bool ParseTime(out long time)
			{
				time = 0L;
				if (!ParseInt(23, out var i))
				{
					if (error == ParseError.Overflow)
					{
						error = ParseError.OverflowHoursMinutesSeconds;
					}
					return false;
				}
				time = i * 36000000000L;
				if (ch != ':')
				{
					error = ParseError.Format;
					return false;
				}
				NextChar();
				if (!ParseInt(59, out i))
				{
					if (error == ParseError.Overflow)
					{
						error = ParseError.OverflowHoursMinutesSeconds;
					}
					return false;
				}
				time += (long)i * 600000000L;
				if (ch == ':')
				{
					NextChar();
					if (ch != '.')
					{
						if (!ParseInt(59, out i))
						{
							if (error == ParseError.Overflow)
							{
								error = ParseError.OverflowHoursMinutesSeconds;
							}
							return false;
						}
						time += (long)i * 10000000L;
					}
					if (ch == '.')
					{
						NextChar();
						int num = 10000000;
						while (num > 1 && ch >= '0' && ch <= '9')
						{
							num /= 10;
							time += (ch - 48) * num;
							NextChar();
						}
					}
				}
				return true;
			}

			internal void SkipBlanks()
			{
				while (ch == ' ' || ch == '\t')
				{
					NextChar();
				}
			}
		}

		public const long TicksPerMillisecond = 10000L;

		private const double MillisecondsPerTick = 0.0001;

		public const long TicksPerSecond = 10000000L;

		private const double SecondsPerTick = 1E-07;

		public const long TicksPerMinute = 600000000L;

		private const double MinutesPerTick = 1.6666666666666667E-09;

		public const long TicksPerHour = 36000000000L;

		private const double HoursPerTick = 2.7777777777777777E-11;

		public const long TicksPerDay = 864000000000L;

		private const double DaysPerTick = 1.1574074074074074E-12;

		private const int MillisPerSecond = 1000;

		private const int MillisPerMinute = 60000;

		private const int MillisPerHour = 3600000;

		private const int MillisPerDay = 86400000;

		private const long MaxSeconds = 922337203685L;

		private const long MinSeconds = -922337203685L;

		private const long MaxMilliSeconds = 922337203685477L;

		private const long MinMilliSeconds = -922337203685477L;

		public static readonly TimeSpan Zero = new TimeSpan(0L);

		public static readonly TimeSpan MaxValue = new TimeSpan(long.MaxValue);

		public static readonly TimeSpan MinValue = new TimeSpan(long.MinValue);

		internal long _ticks;

		public long Ticks => _ticks;

		public int Days => (int)(_ticks / 864000000000L);

		public int Hours => (int)(_ticks / 36000000000L % 24);

		public int Milliseconds => (int)(_ticks / 10000 % 1000);

		public int Minutes => (int)(_ticks / 600000000 % 60);

		public int Seconds => (int)(_ticks / 10000000 % 60);

		public double TotalDays => (double)_ticks * 1.1574074074074074E-12;

		public double TotalHours => (double)_ticks * 2.7777777777777777E-11;

		public double TotalMilliseconds
		{
			get
			{
				double num = (double)_ticks * 0.0001;
				if (num > 922337203685477.0)
				{
					return 922337203685477.0;
				}
				if (num < -922337203685477.0)
				{
					return -922337203685477.0;
				}
				return num;
			}
		}

		public double TotalMinutes => (double)_ticks * 1.6666666666666667E-09;

		public double TotalSeconds => (double)_ticks * 1E-07;

		public TimeSpan(long ticks)
		{
			_ticks = ticks;
		}

		public TimeSpan(int hours, int minutes, int seconds)
		{
			_ticks = TimeToTicks(hours, minutes, seconds);
		}

		public TimeSpan(int days, int hours, int minutes, int seconds)
			: this(days, hours, minutes, seconds, 0)
		{
		}

		public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
		{
			long num = ((long)days * 3600L * 24 + (long)hours * 3600L + (long)minutes * 60L + seconds) * 1000 + milliseconds;
			if (num > 922337203685477L || num < -922337203685477L)
			{
				throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Overflow_TimeSpanTooLong"));
			}
			_ticks = num * 10000;
		}

		public TimeSpan Add(TimeSpan ts)
		{
			long num = _ticks + ts._ticks;
			if (_ticks >> 63 == ts._ticks >> 63 && _ticks >> 63 != num >> 63)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanTooLong"));
			}
			return new TimeSpan(num);
		}

		public static int Compare(TimeSpan t1, TimeSpan t2)
		{
			if (t1._ticks > t2._ticks)
			{
				return 1;
			}
			if (t1._ticks < t2._ticks)
			{
				return -1;
			}
			return 0;
		}

		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}
			if (!(value is TimeSpan))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeTimeSpan"));
			}
			long ticks = ((TimeSpan)value)._ticks;
			if (_ticks > ticks)
			{
				return 1;
			}
			if (_ticks < ticks)
			{
				return -1;
			}
			return 0;
		}

		public int CompareTo(TimeSpan value)
		{
			long ticks = value._ticks;
			if (_ticks > ticks)
			{
				return 1;
			}
			if (_ticks < ticks)
			{
				return -1;
			}
			return 0;
		}

		public static TimeSpan FromDays(double value)
		{
			return Interval(value, 86400000);
		}

		public TimeSpan Duration()
		{
			if (_ticks == MinValue._ticks)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Duration"));
			}
			return new TimeSpan((_ticks >= 0) ? _ticks : (-_ticks));
		}

		public override bool Equals(object value)
		{
			if (value is TimeSpan)
			{
				return _ticks == ((TimeSpan)value)._ticks;
			}
			return false;
		}

		public bool Equals(TimeSpan obj)
		{
			return _ticks == obj._ticks;
		}

		public static bool Equals(TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks == t2._ticks;
		}

		public override int GetHashCode()
		{
			return (int)_ticks ^ (int)(_ticks >> 32);
		}

		public static TimeSpan FromHours(double value)
		{
			return Interval(value, 3600000);
		}

		private static TimeSpan Interval(double value, int scale)
		{
			if (double.IsNaN(value))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_CannotBeNaN"));
			}
			double num = value * (double)scale;
			double num2 = num + ((value >= 0.0) ? 0.5 : (-0.5));
			if (num2 > 922337203685477.0 || num2 < -922337203685477.0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanTooLong"));
			}
			return new TimeSpan((long)num2 * 10000);
		}

		public static TimeSpan FromMilliseconds(double value)
		{
			return Interval(value, 1);
		}

		public static TimeSpan FromMinutes(double value)
		{
			return Interval(value, 60000);
		}

		public TimeSpan Negate()
		{
			if (_ticks == MinValue._ticks)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_NegateTwosCompNum"));
			}
			return new TimeSpan(-_ticks);
		}

		public static TimeSpan Parse(string s)
		{
			return new TimeSpan(default(StringParser).Parse(s));
		}

		public static bool TryParse(string s, out TimeSpan result)
		{
			if (default(StringParser).TryParse(s, out var value))
			{
				result = new TimeSpan(value);
				return true;
			}
			result = Zero;
			return false;
		}

		public static TimeSpan FromSeconds(double value)
		{
			return Interval(value, 1000);
		}

		public TimeSpan Subtract(TimeSpan ts)
		{
			long num = _ticks - ts._ticks;
			if (_ticks >> 63 != ts._ticks >> 63 && _ticks >> 63 != num >> 63)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanTooLong"));
			}
			return new TimeSpan(num);
		}

		public static TimeSpan FromTicks(long value)
		{
			return new TimeSpan(value);
		}

		internal static long TimeToTicks(int hour, int minute, int second)
		{
			long num = (long)hour * 3600L + (long)minute * 60L + second;
			if (num > 922337203685L || num < -922337203685L)
			{
				throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Overflow_TimeSpanTooLong"));
			}
			return num * 10000000;
		}

		private string IntToString(int n, int digits)
		{
			return ParseNumbers.IntToString(n, 10, digits, '0', 0);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = (int)(_ticks / 864000000000L);
			long num2 = _ticks % 864000000000L;
			if (_ticks < 0)
			{
				stringBuilder.Append("-");
				num = -num;
				num2 = -num2;
			}
			if (num != 0)
			{
				stringBuilder.Append(num);
				stringBuilder.Append(".");
			}
			stringBuilder.Append(IntToString((int)(num2 / 36000000000L % 24), 2));
			stringBuilder.Append(":");
			stringBuilder.Append(IntToString((int)(num2 / 600000000 % 60), 2));
			stringBuilder.Append(":");
			stringBuilder.Append(IntToString((int)(num2 / 10000000 % 60), 2));
			int num3 = (int)(num2 % 10000000);
			if (num3 != 0)
			{
				stringBuilder.Append(".");
				stringBuilder.Append(IntToString(num3, 7));
			}
			return stringBuilder.ToString();
		}

		public static TimeSpan operator -(TimeSpan t)
		{
			if (t._ticks == MinValue._ticks)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_NegateTwosCompNum"));
			}
			return new TimeSpan(-t._ticks);
		}

		public static TimeSpan operator -(TimeSpan t1, TimeSpan t2)
		{
			return t1.Subtract(t2);
		}

		public static TimeSpan operator +(TimeSpan t)
		{
			return t;
		}

		public static TimeSpan operator +(TimeSpan t1, TimeSpan t2)
		{
			return t1.Add(t2);
		}

		public static bool operator ==(TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks == t2._ticks;
		}

		public static bool operator !=(TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks != t2._ticks;
		}

		public static bool operator <(TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks < t2._ticks;
		}

		public static bool operator <=(TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks <= t2._ticks;
		}

		public static bool operator >(TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks > t2._ticks;
		}

		public static bool operator >=(TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks >= t2._ticks;
		}
	}
}
