namespace CSLib.Utility
{
	public class CUniqueIDU16 : CUniqueID<ushort>
	{
		protected override int _Compare(ushort id1, ushort id2)
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
					num = ((id1 >= id2) ? 1 : 0);
					break;
				case 2:
					return 0;
				case 1:
					if (id1 == id2)
					{
						num = 2;
						break;
					}
					return 1;
				case 0:
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
			m_startID = 1;
			m_allocID = m_startID;
			m_endID = ushort.MaxValue;
		}

		protected override ushort _AllocNextID()
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
							num = 0;
							continue;
						}
						goto case 2;
					case 0:
						m_allocID = m_startID;
						if (true)
						{
						}
						num = 2;
						continue;
					case 2:
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

		protected override bool _InvalidID(ushort id)
		{
			return id == m_endID;
		}
	}
}
