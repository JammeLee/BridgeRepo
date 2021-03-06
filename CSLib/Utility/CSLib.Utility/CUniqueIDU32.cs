namespace CSLib.Utility
{
	public class CUniqueIDU32 : CUniqueID<uint>
	{
		protected override int _Compare(uint id1, uint id2)
		{
			//Discarded unreachable code: IL_0023
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					num = ((id1 >= id2) ? 2 : 0);
					break;
				case 3:
					return 0;
				case 2:
					if (id1 == id2)
					{
						num = 3;
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
			m_startID = 1u;
			m_allocID = m_startID;
			m_endID = uint.MaxValue;
		}

		protected override uint _AllocNextID()
		{
			//Discarded unreachable code: IL_0051
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
			return m_endID - m_startID;
		}

		protected override bool _InvalidID(uint id)
		{
			return id == m_endID;
		}
	}
}
