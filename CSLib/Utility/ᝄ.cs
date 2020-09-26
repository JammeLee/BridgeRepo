using System;
using System.Runtime.InteropServices;
using System.Threading;
using CSLib.Utility;

internal class ᝄ
{
	private struct ᜀ
	{
		public IntPtr ᜀ;

		public uint ᜁ;

		public IntPtr ᜂ;

		public IntPtr ᜃ;

		public uint ᜄ;

		public IntPtr ᜅ;

		public IntPtr ᜆ;
	}

	private int m_ᜀ;

	private Thread ᜁ;

	private DReceiveCallback m_ᜂ;

	public ᝄ()
	{
		ᜁ = new Thread(ᜂ);
		this.m_ᜂ = ᜂ;
	}

	public bool ᜅ()
	{
		return ᜁ.IsAlive;
	}

	public DReceiveCallback ᜆ()
	{
		return this.m_ᜂ;
	}

	public void ᜂ(DReceiveCallback A_0)
	{
		this.m_ᜂ = A_0;
	}

	public bool ᜄ()
	{
		//Discarded unreachable code: IL_0038
		int num = 0;
		while (true)
		{
			switch (num)
			{
			case 2:
				return false;
			case 1:
			case 4:
				num = 3;
				continue;
			case 3:
				if (this.m_ᜀ == 0)
				{
					Thread.Sleep(10);
					num = 1;
				}
				else
				{
					num = 5;
				}
				continue;
			case 5:
				return true;
			}
			if (ᜁ.IsAlive)
			{
				if (true)
				{
				}
				num = 2;
			}
			else
			{
				ᜁ.Start();
				num = 4;
			}
		}
	}

	public void ᜇ()
	{
		if (ᜁ.IsAlive)
		{
			ᜁ.Abort();
		}
	}

	public void ᜈ()
	{
		if (ᜁ.IsAlive)
		{
			ᜁ.Join();
		}
	}

	public bool ᜃ(uint A_0, IntPtr A_1, IntPtr A_2)
	{
		//Discarded unreachable code: IL_000b
		if (this.m_ᜀ == 0)
		{
			if (true)
			{
			}
			return false;
		}
		return ᜂ(this.m_ᜀ, A_0, A_1, A_2);
	}

	public static int ᜃ()
	{
		PeekMessage(out var _, 0u, 0u, 0u, 0u);
		return GetCurrentThreadId();
	}

	public static bool ᜂ(out uint A_0, out IntPtr A_1, out IntPtr A_2)
	{
		//Discarded unreachable code: IL_0099
		ᜀ A_3 = default(ᜀ);
		while (true)
		{
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 2:
					break;
				default:
					num = ((GetMessage(out A_3, 0u, 0u, 0u) > 0) ? 5 : 3);
					continue;
				case 4:
					num = 6;
					continue;
				case 6:
					if (A_3.ᜁ <= 32767)
					{
						num = 1;
						continue;
					}
					goto IL_00be;
				case 5:
					if (A_3.ᜁ >= 1024)
					{
						num = 4;
						continue;
					}
					goto IL_00be;
				case 3:
					A_0 = 0u;
					A_1 = new IntPtr(0);
					A_2 = new IntPtr(0);
					return false;
				case 1:
					{
						if (true)
						{
						}
						A_0 = A_3.ᜁ - 1024;
						A_1 = A_3.ᜂ;
						A_2 = A_3.ᜃ;
						return true;
					}
					IL_00be:
					TranslateMessage(ref A_3);
					DispatchMessage(ref A_3);
					num = 2;
					continue;
				}
				break;
			}
		}
	}

	public static bool ᜂ(int A_0, uint A_1, IntPtr A_2, IntPtr A_3)
	{
		//Discarded unreachable code: IL_0057
		while (true)
		{
			uint num = A_1 + 1024;
			int num2 = 1;
			while (true)
			{
				switch (num2)
				{
				case 1:
					if (num >= 1024)
					{
						num2 = 0;
						continue;
					}
					goto IL_0066;
				case 3:
					return PostThreadMessage(A_0, num, A_2, A_3);
				case 0:
					num2 = 2;
					continue;
				case 2:
					{
						if (num <= 32767)
						{
							if (true)
							{
							}
							num2 = 3;
							continue;
						}
						goto IL_0066;
					}
					IL_0066:
					return false;
				}
				break;
			}
		}
	}

	protected virtual bool ᜂ(uint A_0, IntPtr A_1, IntPtr A_2)
	{
		return false;
	}

	private void ᜂ()
	{
		//Discarded unreachable code: IL_0053
		uint A_ = default(uint);
		IntPtr A_2 = default(IntPtr);
		IntPtr A_3 = default(IntPtr);
		while (true)
		{
			this.m_ᜀ = ᜃ();
			int num = 4;
			while (true)
			{
				switch (num)
				{
				case 1:
				case 4:
					num = 2;
					continue;
				case 2:
					if (!ᜂ(out A_, out A_2, out A_3))
					{
						num = 3;
						continue;
					}
					if (true)
					{
					}
					num = 0;
					continue;
				case 3:
					return;
				case 0:
					if (this.m_ᜂ(A_, A_2, A_3))
					{
						num = 1;
						continue;
					}
					return;
				}
				break;
			}
		}
	}

	[DllImport("Kernel32", CharSet = CharSet.Ansi)]
	private static extern int GetCurrentThreadId();

	[DllImport("user32", CharSet = CharSet.Ansi)]
	private static extern bool PeekMessage(out ᜀ A_0, uint A_1, uint A_2, uint A_3, uint A_4);

	[DllImport("user32", CharSet = CharSet.Ansi)]
	private static extern int GetMessage(out ᜀ A_0, uint A_1, uint A_2, uint A_3);

	[DllImport("user32", CharSet = CharSet.Ansi)]
	private static extern bool PostThreadMessage(int A_0, uint A_1, IntPtr A_2, IntPtr A_3);

	[DllImport("user32", CharSet = CharSet.Ansi)]
	private static extern bool TranslateMessage(ref ᜀ A_0);

	[DllImport("user32", CharSet = CharSet.Ansi)]
	private static extern bool DispatchMessage(ref ᜀ A_0);
}
