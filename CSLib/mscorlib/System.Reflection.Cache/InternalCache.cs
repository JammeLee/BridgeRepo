using System.Diagnostics;

namespace System.Reflection.Cache
{
	[Serializable]
	internal class InternalCache
	{
		private const int MinCacheSize = 2;

		private InternalCacheItem[] m_cache;

		private int m_numItems;

		internal object this[CacheObjType cacheType]
		{
			get
			{
				InternalCacheItem[] cache = m_cache;
				int numItems = m_numItems;
				int num = FindObjectPosition(cache, numItems, cacheType, findEmpty: false);
				if (num >= 0)
				{
					_ = BCLDebug.m_loggingNotEnabled;
					return cache[num].Value;
				}
				_ = BCLDebug.m_loggingNotEnabled;
				return null;
			}
			set
			{
				_ = BCLDebug.m_loggingNotEnabled;
				lock (this)
				{
					int num = FindObjectPosition(m_cache, m_numItems, cacheType, findEmpty: true);
					if (num > 0)
					{
						m_cache[num].Value = value;
						m_cache[num].Key = cacheType;
						if (num == m_numItems)
						{
							m_numItems++;
						}
						return;
					}
					if (m_cache == null)
					{
						_ = BCLDebug.m_loggingNotEnabled;
						m_cache = new InternalCacheItem[2];
						m_cache[0].Value = value;
						m_cache[0].Key = cacheType;
						m_numItems = 1;
						return;
					}
					_ = BCLDebug.m_loggingNotEnabled;
					InternalCacheItem[] array = new InternalCacheItem[m_numItems * 2];
					for (int i = 0; i < m_numItems; i++)
					{
						ref InternalCacheItem reference = ref array[i];
						reference = m_cache[i];
					}
					array[m_numItems].Value = value;
					array[m_numItems].Key = cacheType;
					m_cache = array;
					m_numItems++;
				}
			}
		}

		internal InternalCache(string cacheName)
		{
		}

		private int FindObjectPosition(InternalCacheItem[] cache, int itemCount, CacheObjType cacheType, bool findEmpty)
		{
			if (cache == null)
			{
				return -1;
			}
			if (itemCount > cache.Length)
			{
				itemCount = cache.Length;
			}
			for (int i = 0; i < itemCount; i++)
			{
				if (cacheType == cache[i].Key)
				{
					return i;
				}
			}
			if (findEmpty && itemCount < cache.Length - 1)
			{
				return itemCount + 1;
			}
			return -1;
		}

		[Conditional("_LOGGING")]
		private void LogAction(CacheAction action, CacheObjType cacheType)
		{
		}

		[Conditional("_LOGGING")]
		private void LogAction(CacheAction action, CacheObjType cacheType, object obj)
		{
		}
	}
}
