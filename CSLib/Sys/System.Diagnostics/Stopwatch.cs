using Microsoft.Win32;

namespace System.Diagnostics
{
	public class Stopwatch
	{
		private const long TicksPerMillisecond = 10000L;

		private const long TicksPerSecond = 10000000L;

		private long elapsed;

		private long startTimeStamp;

		private bool isRunning;

		public static readonly long Frequency;

		public static readonly bool IsHighResolution;

		private static readonly double tickFrequency;

		public bool IsRunning => isRunning;

		public TimeSpan Elapsed => new TimeSpan(GetElapsedDateTimeTicks());

		public long ElapsedMilliseconds => GetElapsedDateTimeTicks() / 10000;

		public long ElapsedTicks => GetRawElapsedTicks();

		static Stopwatch()
		{
			if (!Microsoft.Win32.SafeNativeMethods.QueryPerformanceFrequency(out Frequency))
			{
				IsHighResolution = false;
				Frequency = 10000000L;
				tickFrequency = 1.0;
			}
			else
			{
				IsHighResolution = true;
				tickFrequency = 10000000.0;
				tickFrequency /= Frequency;
			}
		}

		public Stopwatch()
		{
			Reset();
		}

		public void Start()
		{
			if (!isRunning)
			{
				startTimeStamp = GetTimestamp();
				isRunning = true;
			}
		}

		public static Stopwatch StartNew()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			return stopwatch;
		}

		public void Stop()
		{
			if (isRunning)
			{
				long timestamp = GetTimestamp();
				long num = timestamp - startTimeStamp;
				elapsed += num;
				isRunning = false;
			}
		}

		public void Reset()
		{
			elapsed = 0L;
			isRunning = false;
			startTimeStamp = 0L;
		}

		public static long GetTimestamp()
		{
			if (IsHighResolution)
			{
				long value = 0L;
				Microsoft.Win32.SafeNativeMethods.QueryPerformanceCounter(out value);
				return value;
			}
			return DateTime.UtcNow.Ticks;
		}

		private long GetRawElapsedTicks()
		{
			long num = elapsed;
			if (isRunning)
			{
				long timestamp = GetTimestamp();
				long num2 = timestamp - startTimeStamp;
				num += num2;
			}
			return num;
		}

		private long GetElapsedDateTimeTicks()
		{
			long rawElapsedTicks = GetRawElapsedTicks();
			if (IsHighResolution)
			{
				double num = rawElapsedTicks;
				num *= tickFrequency;
				return (long)num;
			}
			return rawElapsedTicks;
		}
	}
}
