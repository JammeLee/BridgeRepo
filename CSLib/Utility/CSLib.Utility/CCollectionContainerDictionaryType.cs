using System;
using System.Collections.Generic;

namespace CSLib.Utility
{
	[Serializable]
	public class CCollectionContainerDictionaryType<KeyType, ValueType>
	{
		private Dictionary<KeyType, ValueType> m_ObjectList;

		public int Count => m_ObjectList.Count;

		public Dictionary<KeyType, ValueType>.KeyCollection Keys => m_ObjectList.Keys;

		public Dictionary<KeyType, ValueType>.ValueCollection Values => m_ObjectList.Values;

		public CCollectionContainerDictionaryType()
		{
			m_ObjectList = new Dictionary<KeyType, ValueType>();
		}

		public ERETURN_CODE Add(KeyType key, ValueType value)
		{
			//Discarded unreachable code: IL_0011
			if (!m_ObjectList.ContainsKey(key))
			{
				if (true)
				{
				}
				m_ObjectList[key] = value;
				return ERETURN_CODE.ERETURN_SUCCESS;
			}
			return ERETURN_CODE.ERETURN_EXIST;
		}

		public ERETURN_CODE Set(KeyType key, ValueType value)
		{
			m_ObjectList[key] = value;
			return ERETURN_CODE.ERETURN_SUCCESS;
		}

		public bool Remove(KeyType key)
		{
			return m_ObjectList.Remove(key);
		}

		public bool ContainsKey(KeyType key)
		{
			return m_ObjectList.ContainsKey(key);
		}

		public ValueType Get(KeyType key)
		{
			//Discarded unreachable code: IL_0011
			if (m_ObjectList.ContainsKey(key))
			{
				if (true)
				{
				}
				return m_ObjectList[key];
			}
			return default(ValueType);
		}

		public virtual void Clear()
		{
			m_ObjectList.Clear();
		}

		public void Copy(ref CCollectionContainerDictionaryType<KeyType, ValueType> destObjectList)
		{
			//Discarded unreachable code: IL_0096
			using (Dictionary<KeyType, ValueType>.KeyCollection.Enumerator enumerator = m_ObjectList.Keys.GetEnumerator())
			{
				int num = 2;
				while (true)
				{
					switch (num)
					{
					default:
						num = 1;
						continue;
					case 1:
						if (enumerator.MoveNext())
						{
							KeyType current = enumerator.Current;
							destObjectList.Set(current, m_ObjectList[current]);
							num = 0;
						}
						else
						{
							num = 4;
						}
						continue;
					case 4:
						num = 3;
						continue;
					case 3:
						break;
					}
					break;
				}
			}
			if (1 == 0)
			{
			}
		}

		public void GetKeysCopy(ref KeyType[] array)
		{
			//Discarded unreachable code: IL_004c
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					if (array == null)
					{
						num = 0;
						continue;
					}
					break;
				case 0:
					array = new KeyType[m_ObjectList.Keys.Count];
					num = 2;
					continue;
				case 2:
					if (1 == 0)
					{
					}
					break;
				}
				break;
			}
			m_ObjectList.Keys.CopyTo(array, 0);
		}
	}
}
