using System.Threading;

namespace System.Runtime.InteropServices
{
	public sealed class HandleCollector
	{
		private const int deltaPercent = 10;

		private string name;

		private int initialThreshold;

		private int maximumThreshold;

		private int threshold;

		private int handleCount;

		private int[] gc_counts = new int[3];

		private int gc_gen;

		public int Count => handleCount;

		public int InitialThreshold => initialThreshold;

		public int MaximumThreshold => maximumThreshold;

		public string Name => name;

		public HandleCollector(string name, int initialThreshold)
			: this(name, initialThreshold, int.MaxValue)
		{
		}

		public HandleCollector(string name, int initialThreshold, int maximumThreshold)
		{
			if (initialThreshold < 0)
			{
				throw new ArgumentOutOfRangeException("initialThreshold", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
			}
			if (maximumThreshold < 0)
			{
				throw new ArgumentOutOfRangeException("maximumThreshold", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
			}
			if (initialThreshold > maximumThreshold)
			{
				throw new ArgumentException(SR.GetString("Argument_InvalidThreshold"));
			}
			if (name != null)
			{
				this.name = name;
			}
			else
			{
				this.name = string.Empty;
			}
			this.initialThreshold = initialThreshold;
			this.maximumThreshold = maximumThreshold;
			threshold = initialThreshold;
			handleCount = 0;
		}

		public void Add()
		{
			int num = -1;
			Interlocked.Increment(ref handleCount);
			if (handleCount < 0)
			{
				throw new InvalidOperationException(SR.GetString("InvalidOperation_HCCountOverflow"));
			}
			if (handleCount > threshold)
			{
				lock (this)
				{
					threshold = handleCount + handleCount / 10;
					num = gc_gen;
					if (gc_gen < 2)
					{
						gc_gen++;
					}
				}
			}
			if (num >= 0 && (num == 0 || gc_counts[num] == GC.CollectionCount(num)))
			{
				GC.Collect(num);
				Thread.Sleep(10 * num);
			}
			for (int i = 1; i < 3; i++)
			{
				gc_counts[i] = GC.CollectionCount(i);
			}
		}

		public void Remove()
		{
			Interlocked.Decrement(ref handleCount);
			if (handleCount < 0)
			{
				throw new InvalidOperationException(SR.GetString("InvalidOperation_HCCountOverflow"));
			}
			int num = handleCount + handleCount / 10;
			if (num < threshold - threshold / 10)
			{
				lock (this)
				{
					if (num > initialThreshold)
					{
						threshold = num;
					}
					else
					{
						threshold = initialThreshold;
					}
					gc_gen = 0;
				}
			}
			for (int i = 1; i < 3; i++)
			{
				gc_counts[i] = GC.CollectionCount(i);
			}
		}
	}
}
