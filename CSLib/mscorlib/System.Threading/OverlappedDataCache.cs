using System.Runtime.ConstrainedExecution;

namespace System.Threading
{
	internal sealed class OverlappedDataCache : CriticalFinalizerObject
	{
		private const float m_CleanupStep = 0.05f;

		private const float m_CleanupInitialThreadhold = 0.3f;

		private static OverlappedDataCacheLine m_overlappedDataCache;

		private static int m_overlappedDataCacheAccessed;

		private static int m_cleanupObjectCount;

		private static float m_CleanupThreshold;

		private static volatile OverlappedDataCacheLine s_firstFreeCacheLine;

		private int m_gen2GCCount;

		private bool m_ready;

		private static void GrowOverlappedDataCache()
		{
			OverlappedDataCacheLine value = new OverlappedDataCacheLine();
			if (m_overlappedDataCache == null && Interlocked.CompareExchange(ref m_overlappedDataCache, value, null) == null)
			{
				new OverlappedDataCache();
				return;
			}
			if (m_cleanupObjectCount == 0)
			{
				new OverlappedDataCache();
			}
			OverlappedDataCacheLine overlappedDataCacheLine;
			do
			{
				overlappedDataCacheLine = m_overlappedDataCache;
				while (overlappedDataCacheLine != null && overlappedDataCacheLine.m_next != null)
				{
					overlappedDataCacheLine = overlappedDataCacheLine.m_next;
				}
			}
			while (overlappedDataCacheLine != null && Interlocked.CompareExchange(ref overlappedDataCacheLine.m_next, value, null) != null);
		}

		internal static OverlappedData GetOverlappedData(Overlapped overlapped)
		{
			OverlappedData overlappedData = null;
			Interlocked.Exchange(ref m_overlappedDataCacheAccessed, 1);
			while (true)
			{
				OverlappedDataCacheLine overlappedDataCacheLine = s_firstFreeCacheLine;
				if (overlappedDataCacheLine == null)
				{
					overlappedDataCacheLine = m_overlappedDataCache;
				}
				while (overlappedDataCacheLine != null)
				{
					for (short num = 0; num < 16; num = (short)(num + 1))
					{
						if (overlappedDataCacheLine.m_items[num] != null)
						{
							overlappedData = Interlocked.Exchange(ref overlappedDataCacheLine.m_items[num], null);
							if (overlappedData != null)
							{
								s_firstFreeCacheLine = overlappedDataCacheLine;
								overlappedData.m_overlapped = overlapped;
								return overlappedData;
							}
						}
					}
					overlappedDataCacheLine = overlappedDataCacheLine.m_next;
				}
				GrowOverlappedDataCache();
			}
		}

		internal static void CacheOverlappedData(OverlappedData data)
		{
			data.ReInitialize();
			data.m_cacheLine.m_items[data.m_slot] = data;
			s_firstFreeCacheLine = null;
		}

		internal OverlappedDataCache()
		{
			if (m_cleanupObjectCount == 0)
			{
				m_CleanupThreshold = 0.3f;
				if (Interlocked.Exchange(ref m_cleanupObjectCount, 1) == 0)
				{
					m_ready = true;
				}
			}
		}

		~OverlappedDataCache()
		{
			if (!m_ready)
			{
				return;
			}
			if (m_overlappedDataCache == null)
			{
				Interlocked.Exchange(ref m_cleanupObjectCount, 0);
				return;
			}
			if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
			{
				GC.ReRegisterForFinalize(this);
			}
			int num = GC.CollectionCount(GC.MaxGeneration);
			if (num == m_gen2GCCount)
			{
				return;
			}
			m_gen2GCCount = num;
			OverlappedDataCacheLine overlappedDataCacheLine = null;
			OverlappedDataCacheLine overlappedDataCacheLine2 = m_overlappedDataCache;
			OverlappedDataCacheLine overlappedDataCacheLine3 = null;
			OverlappedDataCacheLine overlappedDataCacheLine4 = overlappedDataCacheLine;
			int num2 = 0;
			int num3 = 0;
			while (overlappedDataCacheLine2 != null)
			{
				num2++;
				bool flag = false;
				short num4 = 0;
				while (num4 < 16)
				{
					if (overlappedDataCacheLine2.m_items[num4] == null)
					{
						flag = true;
						num3++;
					}
					num4 = (short)(num4 + 1);
				}
				if (!flag)
				{
					overlappedDataCacheLine4 = overlappedDataCacheLine;
					overlappedDataCacheLine3 = overlappedDataCacheLine2;
				}
				overlappedDataCacheLine = overlappedDataCacheLine2;
				overlappedDataCacheLine2 = overlappedDataCacheLine2.m_next;
			}
			num2 *= 16;
			if (overlappedDataCacheLine3 != null && (float)num2 * m_CleanupThreshold > (float)num3)
			{
				if (overlappedDataCacheLine4 == null)
				{
					m_overlappedDataCache = overlappedDataCacheLine3.m_next;
				}
				else
				{
					overlappedDataCacheLine4.m_next = overlappedDataCacheLine3.m_next;
				}
				overlappedDataCacheLine3.Removed = true;
			}
			if (m_overlappedDataCacheAccessed != 0)
			{
				m_CleanupThreshold = 0.3f;
				Interlocked.Exchange(ref m_overlappedDataCacheAccessed, 0);
			}
			else
			{
				m_CleanupThreshold += 0.05f;
			}
		}
	}
}
