namespace CSLib.Utility
{
	public class CUniqueIDU8 : CUniqueID<byte>
	{
		protected override int _Compare(byte id1, byte id2)
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
					num = ((id1 < id2) ? 2 : 0);
					break;
				case 3:
					return 0;
				case 0:
					if (id1 == id2)
					{
						num = 3;
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
			m_startID = 1;
			m_allocID = m_startID;
			m_endID = byte.MaxValue;
		}

		protected override byte _AllocNextID()
		{
			//Discarded unreachable code: IL_0052
			while (true)
			{
				m_allocID++;
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (m_allocID >= m_endID)
						{
							num = 1;
							continue;
						}
						goto case 2;
					case 1:
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

		protected override bool _InvalidID(byte id)
		{
			return id == m_endID;
		}
	}
}
