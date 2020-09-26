namespace CSLib.Framework
{
	public class CFSMState
	{
		private int ᜀ;

		private int ᜁ;

		private int[] ᜂ;

		private int[] ᜃ;

		private CFSMStateProcess ᜄ;

		public int StateID => ᜀ;

		public CFSMStateProcess StateProcess
		{
			get
			{
				return ᜄ;
			}
			set
			{
				ᜄ = value;
			}
		}

		public CFSMState(int stateID)
		{
			ᜀ = stateID;
			ᜁ = 1;
			ᜂ = new int[ᜁ];
			ᜃ = new int[ᜁ];
		}

		public void addTransition(int iInput, int iOutputID)
		{
			//Discarded unreachable code: IL_00b3
			switch (0)
			{
			}
			int[] array = default(int[]);
			int[] array2 = default(int[]);
			int num3 = default(int);
			while (true)
			{
				int num = 0;
				num = 0;
				int num2 = 2;
				while (true)
				{
					switch (num2)
					{
					case 15:
						array[num] = 0;
						array2[num] = 0;
						ᜂ = array;
						ᜃ = array2;
						num2 = 6;
						continue;
					case 7:
						if (1 == 0)
						{
						}
						goto case 10;
					case 14:
						num++;
						num2 = 0;
						continue;
					case 6:
						num2 = 3;
						continue;
					case 3:
						if (num < ᜁ)
						{
							num2 = 9;
							continue;
						}
						return;
					case 1:
						if (ᜃ[num] != 0)
						{
							num2 = 14;
							continue;
						}
						goto case 4;
					case 10:
						num2 = 13;
						continue;
					case 13:
						if (num3 < num)
						{
							array[num3] = ᜂ[num3];
							array2[num3] = ᜃ[num3];
							num3++;
							num2 = 7;
						}
						else
						{
							num2 = 15;
						}
						continue;
					case 9:
						ᜂ[num] = iInput;
						ᜃ[num] = iOutputID;
						num2 = 8;
						continue;
					case 8:
						return;
					case 4:
						num2 = 11;
						continue;
					case 11:
						if (num >= ᜁ)
						{
							num2 = 5;
							continue;
						}
						goto case 6;
					case 0:
					case 2:
						num2 = 12;
						continue;
					case 12:
						num2 = ((num < ᜁ) ? 1 : 4);
						continue;
					case 5:
						num = ᜁ;
						ᜁ++;
						array = new int[ᜁ];
						array2 = new int[ᜁ];
						num3 = 0;
						num2 = 10;
						continue;
					}
					break;
				}
			}
		}

		public void delTransition(int iOutputID)
		{
			//Discarded unreachable code: IL_0076
			while (true)
			{
				int num = 0;
				num = 0;
				int num2 = 6;
				while (true)
				{
					switch (num2)
					{
					case 9:
						if (ᜃ[num + 1] != 0)
						{
							num2 = 5;
							continue;
						}
						return;
					case 13:
						return;
					case 1:
						if (true)
						{
						}
						num++;
						num2 = 11;
						continue;
					case 5:
						ᜂ[num] = ᜂ[num + 1];
						ᜃ[num] = ᜃ[num + 1];
						num++;
						num2 = 10;
						continue;
					case 7:
						if (ᜃ[num] != iOutputID)
						{
							num2 = 1;
							continue;
						}
						goto case 4;
					case 2:
					case 10:
						num2 = 12;
						continue;
					case 12:
						num2 = ((num < ᜁ - 1) ? 9 : 0);
						continue;
					case 0:
						return;
					case 4:
						num2 = 8;
						continue;
					case 8:
						if (num < ᜁ)
						{
							ᜂ[num] = 0;
							ᜃ[num] = 0;
							num2 = 2;
						}
						else
						{
							num2 = 13;
						}
						continue;
					case 6:
					case 11:
						num2 = 3;
						continue;
					case 3:
						num2 = ((num >= ᜁ) ? 4 : 7);
						continue;
					}
					break;
				}
			}
		}

		public int getOutput(int iInput)
		{
			//Discarded unreachable code: IL_00a0
			while (true)
			{
				int result = ᜀ;
				int num = 0;
				int num2 = 3;
				while (true)
				{
					switch (num2)
					{
					case 7:
						result = ᜃ[num];
						num2 = 4;
						continue;
					case 0:
						if (ᜃ[num] != 0)
						{
							num2 = 8;
							continue;
						}
						goto case 4;
					case 1:
					case 3:
						num2 = 2;
						continue;
					case 2:
						num2 = ((num >= ᜁ) ? 5 : 0);
						continue;
					case 8:
						if (true)
						{
						}
						num2 = 6;
						continue;
					case 6:
						if (iInput != ᜂ[num])
						{
							num++;
							num2 = 1;
						}
						else
						{
							num2 = 7;
						}
						continue;
					case 4:
					case 5:
						return result;
					}
					break;
				}
			}
		}

		public void processState(float fDeltaTime)
		{
			if (ᜄ != null)
			{
				ᜄ.processState(fDeltaTime);
			}
		}

		public void onDestory()
		{
			if (ᜄ != null)
			{
				ᜄ.onDestory();
			}
		}
	}
}
