namespace CSLib.Utility
{
	public class CArray1<OBJECT>
	{
		public delegate void CbTraversalHandler(int index, OBJECT obj);

		private OBJECT[] ᜀ;

		public OBJECT[] Objects => ᜀ;

		public CArray1()
		{
		}

		public CArray1(int num)
		{
			SetLength(num);
		}

		public void SetLength(int length)
		{
			//Discarded unreachable code: IL_0045
			int num = 1;
			int num2 = default(int);
			OBJECT[] array = default(OBJECT[]);
			while (true)
			{
				switch (num)
				{
				default:
					num = ((ᜀ == null) ? 3 : 0);
					break;
				case 5:
					return;
				case 3:
					ᜀ = new OBJECT[length];
					return;
				case 4:
				case 6:
					num = 7;
					break;
				case 7:
					if (num2 < ᜀ.Length)
					{
						if (true)
						{
						}
						ᜀ[num2] = array[num2];
						num2++;
						num = 4;
					}
					else
					{
						num = 2;
					}
					break;
				case 2:
					return;
				case 0:
					if (ᜀ.Length >= length)
					{
						num = 5;
						break;
					}
					array = ᜀ;
					ᜀ = new OBJECT[length];
					num2 = 0;
					num = 6;
					break;
				}
			}
		}

		public bool AddObject(int index, OBJECT obj)
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
					num = ((ᜀ != null) ? 1 : 3);
					break;
				case 2:
					return false;
				case 1:
					if (ᜀ.Length <= index)
					{
						num = 2;
						break;
					}
					ᜀ[index] = obj;
					return true;
				case 3:
					return false;
				}
			}
		}

		public bool DelObject(int index)
		{
			//Discarded unreachable code: IL_0052
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((ᜀ != null) ? 1 : 3);
					break;
				case 1:
					if (ᜀ.Length <= index)
					{
						num = 0;
						break;
					}
					ᜀ[index] = default(OBJECT);
					return true;
				case 0:
					if (true)
					{
					}
					return false;
				case 3:
					return false;
				}
			}
		}

		public OBJECT GelObject(int index)
		{
			//Discarded unreachable code: IL_005a
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((ᜀ == null) ? 3 : 0);
					break;
				case 0:
					if (ᜀ.Length <= index)
					{
						num = 2;
						break;
					}
					return ᜀ[index];
				case 2:
					if (true)
					{
					}
					return default(OBJECT);
				case 3:
					return default(OBJECT);
				}
			}
		}

		public void Traversal(CbTraversalHandler cbHandler)
		{
			//Discarded unreachable code: IL_0085
			int num = 6;
			int num2 = default(int);
			while (true)
			{
				switch (num)
				{
				case 7:
					return;
				case 1:
					return;
				case 2:
				case 3:
					num = 0;
					continue;
				case 0:
					if (num2 < ᜀ.Length)
					{
						cbHandler(num2, ᜀ[num2]);
						num2++;
						num = 3;
					}
					else
					{
						num = 5;
					}
					continue;
				case 5:
					return;
				case 4:
					if (cbHandler == null)
					{
						num = 1;
						continue;
					}
					num2 = 0;
					num = 2;
					continue;
				}
				if (ᜀ == null)
				{
					num = 7;
					continue;
				}
				if (true)
				{
				}
				num = 4;
			}
		}
	}
}
