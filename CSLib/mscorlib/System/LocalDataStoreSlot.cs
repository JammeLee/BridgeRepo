using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
	[ComVisible(true)]
	public sealed class LocalDataStoreSlot
	{
		private static LdsSyncHelper m_helper = new LdsSyncHelper();

		private LocalDataStoreMgr m_mgr;

		private int m_slot;

		internal LocalDataStoreMgr Manager => m_mgr;

		internal int Slot => m_slot;

		internal LocalDataStoreSlot(LocalDataStoreMgr mgr, int slot)
		{
			m_mgr = mgr;
			m_slot = slot;
		}

		internal bool IsValid()
		{
			return m_helper.Get(ref m_slot) != -1;
		}

		~LocalDataStoreSlot()
		{
			int slot = m_slot;
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(m_mgr, ref tookLock);
				m_slot = -1;
				m_mgr.FreeDataSlot(slot);
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(m_mgr);
				}
			}
		}
	}
}
