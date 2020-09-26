namespace CSLib.Utility
{
	public class CFlag32
	{
		private int ᜀ;

		public void Reset()
		{
			ᜀ = 0;
		}

		public void AddFlag(int nFlag)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ᜀ |= 1 << nFlag;
		}

		public void DelFlag(int nFlag)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ᜀ &= ~(1 << nFlag);
		}

		public bool Containd(int nFlag)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			return (ᜀ & (1 << nFlag)) != 0;
		}
	}
}
