namespace System.Threading
{
	internal sealed class OverlappedDataCacheLine
	{
		internal const short CacheSize = 16;

		internal OverlappedData[] m_items;

		internal OverlappedDataCacheLine m_next;

		private bool m_removed;

		internal bool Removed
		{
			get
			{
				return m_removed;
			}
			set
			{
				m_removed = value;
			}
		}

		internal OverlappedDataCacheLine()
		{
			m_items = new OverlappedData[16];
			new object();
			for (short num = 0; num < 16; num = (short)(num + 1))
			{
				m_items[num] = new OverlappedData(this);
				m_items[num].m_slot = num;
			}
			new object();
		}

		~OverlappedDataCacheLine()
		{
			m_removed = true;
		}
	}
}
