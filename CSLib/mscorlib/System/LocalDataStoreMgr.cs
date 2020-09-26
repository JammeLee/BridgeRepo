using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
	internal class LocalDataStoreMgr
	{
		private const byte DataSlotOccupied = 1;

		private const int InitialSlotTableSize = 64;

		private const int SlotTableDoubleThreshold = 512;

		private const int LargeSlotTableSizeIncrease = 128;

		private byte[] m_SlotInfoTable = new byte[64];

		private int m_FirstAvailableSlot;

		private ArrayList m_ManagedLocalDataStores = new ArrayList();

		private Hashtable m_KeyToSlotMap = new Hashtable();

		public LocalDataStore CreateLocalDataStore()
		{
			LocalDataStore localDataStore = new LocalDataStore(this, m_SlotInfoTable.Length);
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				m_ManagedLocalDataStores.Add(localDataStore);
				return localDataStore;
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		public void DeleteLocalDataStore(LocalDataStore store)
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				m_ManagedLocalDataStores.Remove(store);
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		public LocalDataStoreSlot AllocateDataSlot()
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				int num = m_SlotInfoTable.Length;
				LocalDataStoreSlot result;
				if (m_FirstAvailableSlot < num)
				{
					result = new LocalDataStoreSlot(this, m_FirstAvailableSlot);
					m_SlotInfoTable[m_FirstAvailableSlot] = 1;
					int i;
					for (i = m_FirstAvailableSlot + 1; i < num && ((uint)m_SlotInfoTable[i] & (true ? 1u : 0u)) != 0; i++)
					{
					}
					m_FirstAvailableSlot = i;
					return result;
				}
				int num2 = ((num >= 512) ? (num + 128) : (num * 2));
				byte[] array = new byte[num2];
				Array.Copy(m_SlotInfoTable, array, num);
				m_SlotInfoTable = array;
				result = new LocalDataStoreSlot(this, num);
				m_SlotInfoTable[num] = 1;
				m_FirstAvailableSlot = num + 1;
				return result;
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		public LocalDataStoreSlot AllocateNamedDataSlot(string name)
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				LocalDataStoreSlot localDataStoreSlot = AllocateDataSlot();
				m_KeyToSlotMap.Add(name, localDataStoreSlot);
				return localDataStoreSlot;
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		public LocalDataStoreSlot GetNamedDataSlot(string name)
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				LocalDataStoreSlot localDataStoreSlot = (LocalDataStoreSlot)m_KeyToSlotMap[name];
				if (localDataStoreSlot == null)
				{
					return AllocateNamedDataSlot(name);
				}
				return localDataStoreSlot;
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		public void FreeNamedDataSlot(string name)
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				m_KeyToSlotMap.Remove(name);
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		internal void FreeDataSlot(int slot)
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				for (int i = 0; i < m_ManagedLocalDataStores.Count; i++)
				{
					((LocalDataStore)m_ManagedLocalDataStores[i]).SetDataInternal(slot, null, bAlloc: false);
				}
				m_SlotInfoTable[slot] = 0;
				if (slot < m_FirstAvailableSlot)
				{
					m_FirstAvailableSlot = slot;
				}
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		public void ValidateSlot(LocalDataStoreSlot slot)
		{
			if (slot == null || slot.Manager != this)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ALSInvalidSlot"));
			}
		}

		internal int GetSlotTableLength()
		{
			return m_SlotInfoTable.Length;
		}
	}
}
