using System;
using System.Collections.Generic;

namespace CSLib.Utility
{
	[Serializable]
	public class CCollectionContainerSetType<KeyType>
	{
		private List<KeyType> m_ObjectList;

		public int Count => m_ObjectList.Count;

		public List<KeyType> Sets => m_ObjectList;

		public CCollectionContainerSetType()
		{
			m_ObjectList = new List<KeyType>();
		}

		public ERETURN_CODE Add(KeyType key)
		{
			//Discarded unreachable code: IL_0011
			if (!m_ObjectList.Contains(key))
			{
				if (true)
				{
				}
				m_ObjectList.Add(key);
				return ERETURN_CODE.ERETURN_SUCCESS;
			}
			return ERETURN_CODE.ERETURN_EXIST;
		}

		public bool Remove(KeyType key)
		{
			return m_ObjectList.Remove(key);
		}

		public bool Contains(KeyType key)
		{
			return m_ObjectList.Contains(key);
		}

		public virtual void Clear()
		{
			m_ObjectList.Clear();
		}

		public void Copy(ref CCollectionContainerSetType<KeyType> destObjectList)
		{
			//Discarded unreachable code: IL_006b
			using List<KeyType>.Enumerator enumerator = m_ObjectList.GetEnumerator();
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					num = 0;
					break;
				case 0:
					if (enumerator.MoveNext())
					{
						KeyType current = enumerator.Current;
						destObjectList.Add(current);
						num = 3;
					}
					else
					{
						num = 4;
					}
					break;
				case 4:
					if (true)
					{
					}
					num = 2;
					break;
				case 2:
					return;
				}
			}
		}
	}
}
