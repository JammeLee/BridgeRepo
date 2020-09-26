namespace CSLib.Utility
{
	public class CFlag64
	{
		private int ᜀ;

		private int ᜁ;

		public void Reset()
		{
			ᜀ = 0;
			ᜁ = 0;
		}

		public void AddFlag(int nFlag)
		{
			//Discarded unreachable code: IL_0027
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					num = ((nFlag < 32) ? 3 : 0);
					break;
				case 4:
					ᜁ |= 1 << nFlag;
					num = 2;
					break;
				case 2:
					return;
				case 0:
					if (nFlag < 64)
					{
						num = 4;
						break;
					}
					return;
				case 3:
					ᜀ |= 1 << nFlag;
					return;
				}
			}
		}

		public void DelFlag(int nFlag)
		{
			//Discarded unreachable code: IL_002c
			int num = 3;
			while (true)
			{
				switch (num)
				{
				case 0:
					ᜁ &= ~(1 << nFlag);
					num = 2;
					continue;
				case 2:
					return;
				case 4:
					if (nFlag < 64)
					{
						num = 0;
						continue;
					}
					return;
				case 1:
					ᜀ &= ~(1 << nFlag);
					return;
				}
				if (nFlag < 32)
				{
					if (true)
					{
					}
					num = 1;
				}
				else
				{
					num = 4;
				}
			}
		}

		public bool Containd(int nFlag)
		{
			//Discarded unreachable code: IL_0058
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((nFlag < 32) ? 1 : 2);
					break;
				case 2:
					if (nFlag < 64)
					{
						num = 0;
						break;
					}
					return false;
				case 0:
					if (true)
					{
					}
					return (ᜁ & (1 << nFlag)) != 0;
				case 1:
					return (ᜀ & (1 << nFlag)) != 0;
				}
			}
		}
	}
}
