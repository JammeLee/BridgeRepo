using System;
using System.Collections;
using System.Threading;

namespace CSLib.Utility
{
	public class CUniqueID<UniqueIDType>
	{
		public delegate bool DTraversalAllObject(UniqueIDType id, object data);

		protected UniqueIDType m_startID;

		protected UniqueIDType m_allocID;

		protected UniqueIDType m_endID;

		protected Hashtable m_hashTable = new Hashtable();

		public UniqueIDType InvalidID => m_endID;

		public CUniqueID()
		{
			_Init();
		}

		public void Init(UniqueIDType startID, UniqueIDType endID)
		{
			//Discarded unreachable code: IL_000e
			if (_Compare(startID, endID) >= 0)
			{
				if (1 == 0)
				{
				}
			}
			else
			{
				m_startID = startID;
				m_allocID = startID;
				m_endID = endID;
			}
		}

		public bool IsValidID(UniqueIDType id)
		{
			//Discarded unreachable code: IL_0023
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (_Compare(m_startID, id) <= 0)
					{
						num = 0;
						continue;
					}
					break;
				case 1:
					return true;
				case 0:
					num = 3;
					continue;
				case 3:
					if (_Compare(id, m_endID) < 0)
					{
						num = 1;
						continue;
					}
					break;
				}
				break;
			}
			return false;
		}

		public object GetObjectByID(UniqueIDType id)
		{
			//Discarded unreachable code: IL_002d
			object result;
			lock (m_hashTable)
			{
				result = m_hashTable[id];
			}
			if (true)
			{
			}
			return result;
		}

		public void Traversal(DTraversalAllObject traversalFun)
		{
			//Discarded unreachable code: IL_00ea
			IDictionaryEnumerator enumerator = m_hashTable.GetEnumerator();
			try
			{
				int num = 0;
				DictionaryEntry dictionaryEntry = default(DictionaryEntry);
				while (true)
				{
					switch (num)
					{
					case 4:
						if (!traversalFun((UniqueIDType)dictionaryEntry.Key, dictionaryEntry.Value))
						{
							num = 2;
							continue;
						}
						goto default;
					default:
						num = 5;
						continue;
					case 5:
						if (enumerator.MoveNext())
						{
							dictionaryEntry = (DictionaryEntry)enumerator.Current;
							num = 4;
						}
						else
						{
							num = 3;
						}
						continue;
					case 2:
						num = 1;
						continue;
					case 1:
						break;
					case 3:
						num = 6;
						continue;
					case 6:
						break;
					}
					break;
				}
			}
			finally
			{
				while (true)
				{
					IDisposable disposable = enumerator as IDisposable;
					int num = 1;
					while (true)
					{
						switch (num)
						{
						case 1:
							if (disposable != null)
							{
								num = 2;
								continue;
							}
							goto end_IL_00a6;
						case 2:
							disposable.Dispose();
							num = 0;
							continue;
						case 0:
							goto end_IL_00a6;
						}
						break;
					}
				}
				end_IL_00a6:;
			}
			if (1 == 0)
			{
			}
		}

		public UniqueIDType AllocID()
		{
			//Discarded unreachable code: IL_0145
			switch (0)
			{
			default:
			{
				ulong num = _AllocIDCount();
				Hashtable hashTable = m_hashTable;
				Monitor.Enter(hashTable);
				try
				{
					UniqueIDType val = default(UniqueIDType);
					UniqueIDType result = default(UniqueIDType);
					while (true)
					{
						ulong num2 = 0uL;
						int num3 = 7;
						while (true)
						{
							switch (num3)
							{
							case 5:
								num3 = (_InvalidID(val) ? 4 : 0);
								continue;
							case 10:
								m_hashTable[val] = val;
								result = val;
								num3 = 3;
								continue;
							case 1:
							case 7:
								num3 = 9;
								continue;
							case 9:
								if (num2 < num)
								{
									val = _AllocNextID();
									num3 = 5;
								}
								else
								{
									num3 = 2;
								}
								continue;
							case 0:
								if (m_hashTable[val] != null)
								{
									num2++;
									num3 = 1;
								}
								else
								{
									num3 = 10;
								}
								continue;
							case 4:
								result = InvalidID;
								num3 = 8;
								continue;
							case 2:
								num3 = 6;
								continue;
							case 6:
								goto end_IL_002b;
							case 3:
								return result;
							case 8:
								return result;
							}
							break;
						}
					}
					end_IL_002b:;
				}
				finally
				{
					if (true)
					{
					}
					Monitor.Exit(hashTable);
				}
				return InvalidID;
			}
			}
		}

		public UniqueIDType AllocID(object tmpData)
		{
			//Discarded unreachable code: IL_0011
			switch (0)
			{
			default:
			{
				if (true)
				{
				}
				ulong num = _AllocIDCount();
				lock (m_hashTable)
				{
					UniqueIDType val = default(UniqueIDType);
					UniqueIDType result = default(UniqueIDType);
					while (true)
					{
						ulong num2 = 0uL;
						int num3 = 9;
						while (true)
						{
							switch (num3)
							{
							case 2:
								num3 = ((!_InvalidID(val)) ? 1 : 7);
								continue;
							case 4:
								m_hashTable[val] = tmpData;
								result = val;
								num3 = 8;
								continue;
							case 6:
							case 9:
								num3 = 10;
								continue;
							case 10:
								if (num2 < num)
								{
									val = _AllocNextID();
									num3 = 2;
								}
								else
								{
									num3 = 5;
								}
								continue;
							case 1:
								if (m_hashTable[val] != null)
								{
									num2++;
									num3 = 6;
								}
								else
								{
									num3 = 4;
								}
								continue;
							case 7:
								result = InvalidID;
								num3 = 0;
								continue;
							case 5:
								num3 = 3;
								continue;
							case 3:
								goto end_IL_0033;
							case 0:
								return result;
							case 8:
								return result;
							}
							break;
						}
					}
					end_IL_0033:;
				}
				return InvalidID;
			}
			}
		}

		public void RecycleID(UniqueIDType id)
		{
			//Discarded unreachable code: IL_006a
			Hashtable hashTable = m_hashTable;
			Monitor.Enter(hashTable);
			try
			{
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 3:
						num = 0;
						continue;
					case 0:
						return;
					case 2:
						return;
					}
					if (!IsValidID(id))
					{
						num = 3;
						continue;
					}
					m_hashTable.Remove(id);
					num = 2;
				}
			}
			finally
			{
				if (true)
				{
				}
				Monitor.Exit(hashTable);
			}
		}

		protected virtual int _Compare(UniqueIDType id1, UniqueIDType id2)
		{
			int a_ = 10;
			throw new Exception(CSimpleThreadPool.b("\ue729䄠퐒ﰸ湍ཏᅑ㭓㭕⡗㭙\u2e5b㭝䁟꼅㠬", a_));
		}

		protected virtual void _Init()
		{
			int a_ = 3;
			throw new Exception(CSimpleThreadPool.b("鹒䠧\udd19\uf537杆ᙈɊ⍌♎═獒頰ଙ", a_));
		}

		protected virtual UniqueIDType _AllocNextID()
		{
			int a_ = 10;
			throw new Exception(CSimpleThreadPool.b("\ue729䄠퐒ﰸ湍ཏፑ㡓㩕㝗㥙ቛ㭝ᡟᙡⵣ≥䡧\ua70d〤", a_));
		}

		protected virtual ulong _AllocIDCount()
		{
			int a_ = 0;
			throw new Exception(CSimpleThreadPool.b("鵗㝚\ude64\uf232摃᥅\u0947♉⁋⅍㍏᭑ၓᕕ㝗⽙㉛⩝䁟꼅㠬", a_));
		}

		protected virtual bool _InvalidID(UniqueIDType id)
		{
			int a_ = 2;
			throw new Exception(CSimpleThreadPool.b("齑䥘\udc1a\uf430晅ᝇ\u0b49⁋≍㽏ㅑᵓቕ᭗㕙⥛そᑟ䉡꤇㨪", a_));
		}
	}
}
