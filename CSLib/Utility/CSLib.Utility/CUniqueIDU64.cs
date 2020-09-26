namespace CSLib.Utility
{
	public class CUniqueIDU64 : CUniqueID<ulong>
	{
		protected override int _Compare(ulong id1, ulong id2)
		{
			//Discarded unreachable code: IL_0023
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					num = ((id1 >= id2) ? 2 : 3);
					break;
				case 1:
					return 0;
				case 2:
					if (id1 == id2)
					{
						num = 1;
						break;
					}
					return 1;
				case 3:
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
			m_startID = 1uL;
			m_allocID = m_startID;
			m_endID = ulong.MaxValue;
		}

		protected override ulong _AllocNextID()
		{
			//Discarded unreachable code: IL_0052
			while (true)
			{
				m_allocID++;
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (m_allocID >= m_endID)
						{
							num = 1;
							continue;
						}
						goto case 0;
					case 1:
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
			return m_endID - m_startID;
		}

		protected override bool _InvalidID(ulong id)
		{
			return id == m_endID;
		}
	}
}
