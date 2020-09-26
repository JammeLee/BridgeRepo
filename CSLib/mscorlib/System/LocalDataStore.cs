using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
	internal class LocalDataStore
	{
		private object[] m_DataTable;

		private LocalDataStoreMgr m_Manager;

		private int DONT_USE_InternalStore;

		public LocalDataStore(LocalDataStoreMgr mgr, int InitialCapacity)
		{
			if (mgr == null)
			{
				throw new ArgumentNullException("mgr");
			}
			m_Manager = mgr;
			m_DataTable = new object[InitialCapacity];
		}

		public object GetData(LocalDataStoreSlot slot)
		{
			object result = null;
			m_Manager.ValidateSlot(slot);
			int slot2 = slot.Slot;
			if (slot2 >= 0)
			{
				if (slot2 >= m_DataTable.Length)
				{
					return null;
				}
				result = m_DataTable[slot2];
			}
			if (!slot.IsValid())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SlotHasBeenFreed"));
			}
			return result;
		}

		public void SetData(LocalDataStoreSlot slot, object data)
		{
			m_Manager.ValidateSlot(slot);
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(m_Manager, ref tookLock);
				if (!slot.IsValid())
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SlotHasBeenFreed"));
				}
				SetDataInternal(slot.Slot, data, bAlloc: true);
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(m_Manager);
				}
			}
		}

		internal void SetDataInternal(int slot, object data, bool bAlloc)
		{
			if (slot >= m_DataTable.Length)
			{
				if (!bAlloc)
				{
					return;
				}
				SetCapacity(m_Manager.GetSlotTableLength());
			}
			m_DataTable[slot] = data;
		}

		private void SetCapacity(int capacity)
		{
			if (capacity < m_DataTable.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ALSInvalidCapacity"));
			}
			object[] array = new object[capacity];
			Array.Copy(m_DataTable, array, m_DataTable.Length);
			m_DataTable = array;
		}
	}
}
