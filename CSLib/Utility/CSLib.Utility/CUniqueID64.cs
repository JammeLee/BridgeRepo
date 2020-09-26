namespace CSLib.Utility
{
	public class CUniqueID64 : CUniqueID<long>
	{
		protected override int _Compare(long id1, long id2)
		{
			//Discarded unreachable code: IL_0023
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					num = ((id1 >= id2) ? 1 : 2);
					break;
				case 0:
					return 0;
				case 1:
					if (id1 == id2)
					{
						num = 0;
						break;
					}
					return 1;
				case 2:
					return -1;
				}
			}
		}

		protected override void _Init()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			m_startID = 1L;
			m_allocID = m_startID;
			m_endID = long.MaxValue;
		}

		protected override long _AllocNextID()
		{
			//Discarded unreachable code: IL_0052
			while (true)
			{
				m_allocID++;
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 1:
						if (m_allocID >= m_endID)
						{
							num = 2;
							continue;
						}
						goto case 0;
					case 2:
						m_allocID = m_startID;
						if (true)
						{
						}
						num = 0;
						continue;
					case 0:
						return m_allocID;
					}
					break;
				}
			}
		}

		protected override ulong _AllocIDCount()
		{
			return (ulong)(m_endID - m_startID);
		}

		protected override bool _InvalidID(long id)
		{
			return id == m_endID;
		}
	}
}
