using System.Diagnostics;
using CSLib.Utility;

internal abstract class ᜱ
{
	private ELogLevel[] ᜀ = new ELogLevel[65535];

	private bool[] m_ᜁ = new bool[65535];

	private ushort m_ᜂ = 1;

	private const int m_ᜃ = 1;

	private static ᜱ m_ᜄ;

	public ᜱ()
	{
		ᜁ(0, ELogLevel.TRACE);
		ᜂ(0);
	}

	public ELogLevel ᜃ(ushort A_0)
	{
		//Discarded unreachable code: IL_0006
		if (A_0 == 0)
		{
			if (true)
			{
			}
			return ELogLevel.ERROR;
		}
		return ᜀ[A_0 - 1];
	}

	public void ᜁ(ushort A_0, ELogLevel A_1)
	{
		//Discarded unreachable code: IL_0036
		int num = 3;
		int num2 = default(int);
		while (true)
		{
			switch (num)
			{
			default:
				if (A_0 == 0)
				{
					num = 5;
					break;
				}
				ᜀ[A_0 - 1] = A_1;
				return;
			case 5:
				if (true)
				{
				}
				num2 = 0;
				num = 4;
				break;
			case 0:
			case 4:
				num = 1;
				break;
			case 1:
				if (num2 >= ᜀ.Length)
				{
					num = 2;
					break;
				}
				ᜀ[num2] = A_1;
				num2++;
				num = 0;
				break;
			case 2:
				return;
			}
		}
	}

	public void ᜁ(ushort A_0, string A_1)
	{
		//Discarded unreachable code: IL_00e6
		int a_ = 15;
		while (true)
		{
			string[] array = new string[5]
			{
				CSimpleThreadPool.b("⽊⡌ⵎ\u2450㑒", a_),
				CSimpleThreadPool.b("㽊㽌\u2e4e㉐㙒", a_),
				CSimpleThreadPool.b("㱊ⱌ㵎㽐", a_),
				CSimpleThreadPool.b("≊⍌⥎㹐⅒", a_),
				CSimpleThreadPool.b("\u2e4a㽌㵎㹐⅒", a_)
			};
			string a = A_1.ToLower();
			int num = 0;
			int num2 = 1;
			while (true)
			{
				switch (num2)
				{
				case 2:
					ᜁ(A_0, (ELogLevel)num);
					return;
				case 0:
					if (!(a == array[num]))
					{
						num++;
						num2 = 4;
					}
					else
					{
						num2 = 2;
					}
					continue;
				case 1:
				case 4:
					num2 = 3;
					continue;
				case 3:
					if (true)
					{
					}
					num2 = ((num >= array.Length) ? 5 : 0);
					continue;
				case 5:
					return;
				}
				break;
			}
		}
	}

	public void ᜂ(ushort A_0)
	{
		//Discarded unreachable code: IL_0076
		int num = 3;
		int num2 = default(int);
		while (true)
		{
			switch (num)
			{
			default:
				if (A_0 == 0)
				{
					num = 0;
					break;
				}
				this.m_ᜁ[A_0 - 1] = true;
				return;
			case 4:
				num = 2;
				break;
			case 2:
				if (num2 >= this.m_ᜁ.Length)
				{
					num = 5;
					break;
				}
				this.m_ᜁ[num2] = true;
				num2++;
				num = 4;
				break;
			case 5:
				return;
			case 0:
				num2 = 0;
				num = 1;
				break;
			case 1:
				if (1 == 0)
				{
				}
				goto case 4;
			}
		}
	}

	public void ᜄ(ushort A_0)
	{
		//Discarded unreachable code: IL_006a
		int num = 4;
		int num2 = default(int);
		while (true)
		{
			switch (num)
			{
			default:
				if (A_0 == 0)
				{
					num = 1;
					break;
				}
				this.m_ᜁ[A_0 - 1] = false;
				return;
			case 2:
				num = 0;
				break;
			case 0:
				if (num2 >= this.m_ᜁ.Length)
				{
					num = 5;
					break;
				}
				this.m_ᜁ[num2] = false;
				num2++;
				num = 3;
				break;
			case 5:
				return;
			case 3:
				if (1 == 0)
				{
				}
				goto case 2;
			case 1:
				num2 = 0;
				num = 2;
				break;
			}
		}
	}

	public ushort ᜂ()
	{
		return this.m_ᜂ;
	}

	public void ᜅ(ushort A_0)
	{
		if (A_0 == 0)
		{
			this.m_ᜂ = 1;
		}
		else
		{
			this.m_ᜂ = A_0;
		}
	}

	public void ᜉ(string A_0, params object[] A_1)
	{
	}

	public void ᜈ(string A_0, params object[] A_1)
	{
		//Discarded unreachable code: IL_0023
		int num = 2;
		while (true)
		{
			switch (num)
			{
			default:
				if (true)
				{
				}
				num = ((ᜃ(this.m_ᜂ) <= ELogLevel.TRACE) ? 1 : 0);
				break;
			case 0:
				return;
			case 3:
				return;
			case 1:
			{
				if (!ᜁ(this.m_ᜂ))
				{
					num = 3;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				_171D(ELogLevel.TRACE, fileName, fileLineNumber, A_0, A_1);
				return;
			}
			}
		}
	}

	public void ᜇ(string A_0, params object[] A_1)
	{
		//Discarded unreachable code: IL_0023
		int num = 2;
		while (true)
		{
			switch (num)
			{
			default:
				if (true)
				{
				}
				num = ((ᜃ(this.m_ᜂ) > ELogLevel.WARN) ? 1 : 3);
				break;
			case 1:
				return;
			case 0:
				return;
			case 3:
			{
				if (!ᜁ(this.m_ᜂ))
				{
					num = 0;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				_171D(ELogLevel.WARN, fileName, fileLineNumber, A_0, A_1);
				return;
			}
			}
		}
	}

	public void ᜆ(string A_0, params object[] A_1)
	{
		//Discarded unreachable code: IL_003d
		int num = 3;
		while (true)
		{
			switch (num)
			{
			default:
				if (ᜃ(this.m_ᜂ) > ELogLevel.INFOR)
				{
					num = 1;
					break;
				}
				if (true)
				{
				}
				num = 2;
				break;
			case 1:
				return;
			case 0:
				return;
			case 2:
			{
				if (!ᜁ(this.m_ᜂ))
				{
					num = 0;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				_171D(ELogLevel.INFOR, fileName, fileLineNumber, A_0, A_1);
				return;
			}
			}
		}
	}

	public void ᜊ(string A_0, params object[] A_1)
	{
		//Discarded unreachable code: IL_0023
		int num = 2;
		while (true)
		{
			switch (num)
			{
			default:
				if (true)
				{
				}
				num = ((ᜃ(this.m_ᜂ) <= ELogLevel.ERROR) ? 3 : 0);
				break;
			case 0:
				return;
			case 1:
				return;
			case 3:
			{
				if (!ᜁ(this.m_ᜂ))
				{
					num = 1;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				_171D(ELogLevel.ERROR, fileName, fileLineNumber, A_0, A_1);
				return;
			}
			}
		}
	}

	public void ᜉ(ushort A_0, string A_1, params object[] A_2)
	{
	}

	public void ᜈ(ushort A_0, string A_1, params object[] A_2)
	{
		//Discarded unreachable code: IL_0051
		int num = 1;
		while (true)
		{
			switch (num)
			{
			default:
				num = ((ᜃ(A_0) > ELogLevel.TRACE) ? 2 : 0);
				break;
			case 2:
				return;
			case 0:
			{
				if (!ᜁ(A_0))
				{
					num = 3;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				_171D(ELogLevel.TRACE, fileName, fileLineNumber, A_1, A_2);
				return;
			}
			case 3:
				if (1 == 0)
				{
				}
				return;
			}
		}
	}

	public void ᜇ(ushort A_0, string A_1, params object[] A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		int num = 3;
		while (true)
		{
			switch (num)
			{
			default:
				num = ((ᜃ(A_0) > ELogLevel.WARN) ? 1 : 2);
				break;
			case 1:
				return;
			case 0:
				return;
			case 2:
			{
				if (!ᜁ(A_0))
				{
					num = 0;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				_171D(ELogLevel.WARN, fileName, fileLineNumber, A_1, A_2);
				return;
			}
			}
		}
	}

	public void ᜆ(ushort A_0, string A_1, params object[] A_2)
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
				num = ((ᜃ(A_0) > ELogLevel.INFOR) ? 1 : 2);
				break;
			case 1:
				return;
			case 0:
				return;
			case 2:
			{
				if (!ᜁ(A_0))
				{
					num = 0;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				_171D(ELogLevel.INFOR, fileName, fileLineNumber, A_1, A_2);
				return;
			}
			}
		}
	}

	public void ᜊ(ushort A_0, string A_1, params object[] A_2)
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
				num = ((ᜃ(A_0) <= ELogLevel.ERROR) ? 2 : 0);
				break;
			case 0:
				return;
			case 3:
				return;
			case 2:
			{
				if (!ᜁ(A_0))
				{
					num = 3;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				_171D(ELogLevel.ERROR, fileName, fileLineNumber, A_1, A_2);
				return;
			}
			}
		}
	}

	public static ᜱ ᜁ()
	{
		return ᜱ.m_ᜄ;
	}

	public static void ᜁ(ᜱ A_0)
	{
		ᜱ.m_ᜄ = A_0;
	}

	public static void ᜅ(string A_0, params object[] A_1)
	{
	}

	public static void ᜄ(string A_0, params object[] A_1)
	{
		//Discarded unreachable code: IL_003a
		int num = 0;
		while (true)
		{
			switch (num)
			{
			default:
				num = ((ᜱ.m_ᜄ == null) ? 1 : 2);
				break;
			case 1:
				if (1 == 0)
				{
				}
				return;
			case 3:
			{
				if (!ᜱ.m_ᜄ.ᜁ(ᜱ.m_ᜄ.m_ᜂ))
				{
					num = 5;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				ᜱ.m_ᜄ._171D(ELogLevel.TRACE, fileName, fileLineNumber, A_0, A_1);
				return;
			}
			case 5:
				return;
			case 4:
				return;
			case 2:
				num = ((ᜱ.m_ᜄ.ᜃ(ᜱ.m_ᜄ.m_ᜂ) > ELogLevel.TRACE) ? 4 : 3);
				break;
			}
		}
	}

	public static void ᜃ(string A_0, params object[] A_1)
	{
		//Discarded unreachable code: IL_0044
		int num = 0;
		while (true)
		{
			switch (num)
			{
			default:
				num = ((ᜱ.m_ᜄ != null) ? 3 : 2);
				break;
			case 2:
				return;
			case 1:
			{
				if (true)
				{
				}
				if (!ᜱ.m_ᜄ.ᜁ(ᜱ.m_ᜄ.m_ᜂ))
				{
					num = 4;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				ᜱ.m_ᜄ._171D(ELogLevel.WARN, fileName, fileLineNumber, A_0, A_1);
				return;
			}
			case 4:
				return;
			case 5:
				return;
			case 3:
				num = ((ᜱ.m_ᜄ.ᜃ(ᜱ.m_ᜄ.m_ᜂ) <= ELogLevel.WARN) ? 1 : 5);
				break;
			}
		}
	}

	public static void ᜂ(string A_0, params object[] A_1)
	{
		//Discarded unreachable code: IL_000b
		int num = 0;
		while (true)
		{
			switch (num)
			{
			case 0:
				if (1 == 0)
				{
				}
				goto default;
			default:
				num = ((ᜱ.m_ᜄ != null) ? 4 : 3);
				break;
			case 3:
				return;
			case 1:
			{
				if (!ᜱ.m_ᜄ.ᜁ(ᜱ.m_ᜄ.m_ᜂ))
				{
					num = 2;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				ᜱ.m_ᜄ._171D(ELogLevel.INFOR, fileName, fileLineNumber, A_0, A_1);
				return;
			}
			case 2:
				return;
			case 5:
				return;
			case 4:
				num = ((ᜱ.m_ᜄ.ᜃ(ᜱ.m_ᜄ.m_ᜂ) <= ELogLevel.INFOR) ? 1 : 5);
				break;
			}
		}
	}

	public static void ᜁ(string A_0, params object[] A_1)
	{
		//Discarded unreachable code: IL_006f
		int num = 3;
		while (true)
		{
			switch (num)
			{
			default:
				num = ((ᜱ.m_ᜄ != null) ? 2 : 0);
				break;
			case 0:
				return;
			case 1:
			{
				if (!ᜱ.m_ᜄ.ᜁ(ᜱ.m_ᜄ.m_ᜂ))
				{
					num = 4;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				ᜱ.m_ᜄ._171D(ELogLevel.ERROR, fileName, fileLineNumber, A_0, A_1);
				return;
			}
			case 4:
				return;
			case 5:
				return;
			case 2:
				if (true)
				{
				}
				num = ((ᜱ.m_ᜄ.ᜃ(ᜱ.m_ᜄ.m_ᜂ) <= ELogLevel.ERROR) ? 1 : 5);
				break;
			}
		}
	}

	public static void ᜅ(ushort A_0, string A_1, params object[] A_2)
	{
	}

	public static void ᜄ(ushort A_0, string A_1, params object[] A_2)
	{
		//Discarded unreachable code: IL_003a
		int num = 3;
		while (true)
		{
			switch (num)
			{
			default:
				num = ((ᜱ.m_ᜄ != null) ? 4 : 0);
				break;
			case 0:
				if (1 == 0)
				{
				}
				return;
			case 2:
			{
				if (!ᜱ.m_ᜄ.ᜁ(A_0))
				{
					num = 1;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				ᜱ.m_ᜄ._171D(ELogLevel.TRACE, fileName, fileLineNumber, A_1, A_2);
				return;
			}
			case 1:
				return;
			case 5:
				return;
			case 4:
				num = ((ᜱ.m_ᜄ.ᜃ(A_0) > ELogLevel.TRACE) ? 5 : 2);
				break;
			}
		}
	}

	public static void ᜃ(ushort A_0, string A_1, params object[] A_2)
	{
		//Discarded unreachable code: IL_007e
		int num = 4;
		while (true)
		{
			switch (num)
			{
			default:
				num = ((ᜱ.m_ᜄ != null) ? 3 : 0);
				break;
			case 0:
				return;
			case 2:
			{
				if (!ᜱ.m_ᜄ.ᜁ(A_0))
				{
					num = 5;
					break;
				}
				if (true)
				{
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				ᜱ.m_ᜄ._171D(ELogLevel.WARN, fileName, fileLineNumber, A_1, A_2);
				return;
			}
			case 5:
				return;
			case 1:
				return;
			case 3:
				num = ((ᜱ.m_ᜄ.ᜃ(A_0) > ELogLevel.WARN) ? 1 : 2);
				break;
			}
		}
	}

	public static void ᜂ(ushort A_0, string A_1, params object[] A_2)
	{
		//Discarded unreachable code: IL_000d
		int num = 4;
		while (true)
		{
			if (true)
			{
			}
			switch (num)
			{
			default:
				num = ((ᜱ.m_ᜄ != null) ? 3 : 2);
				break;
			case 2:
				return;
			case 0:
			{
				if (!ᜱ.m_ᜄ.ᜁ(A_0))
				{
					num = 1;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				ᜱ.m_ᜄ._171D(ELogLevel.INFOR, fileName, fileLineNumber, A_1, A_2);
				return;
			}
			case 1:
				return;
			case 5:
				return;
			case 3:
				num = ((ᜱ.m_ᜄ.ᜃ(A_0) > ELogLevel.INFOR) ? 5 : 0);
				break;
			}
		}
	}

	public static void ᜁ(ushort A_0, string A_1, params object[] A_2)
	{
		//Discarded unreachable code: IL_005c
		int num = 0;
		while (true)
		{
			switch (num)
			{
			default:
				num = ((ᜱ.m_ᜄ != null) ? 1 : 4);
				break;
			case 3:
			{
				if (!ᜱ.m_ᜄ.ᜁ(A_0))
				{
					num = 2;
					break;
				}
				StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
				string fileName = stackTrace.GetFrame(1).GetFileName();
				int fileLineNumber = stackTrace.GetFrame(1).GetFileLineNumber();
				ᜱ.m_ᜄ._171D(ELogLevel.ERROR, fileName, fileLineNumber, A_1, A_2);
				return;
			}
			case 2:
				return;
			case 5:
				return;
			case 4:
				if (1 == 0)
				{
				}
				return;
			case 1:
				num = ((ᜱ.m_ᜄ.ᜃ(A_0) > ELogLevel.ERROR) ? 5 : 3);
				break;
			}
		}
	}

	protected abstract void _171D(ELogLevel A_0, string A_1, int A_2, string A_3, params object[] A_4);

	private bool ᜁ(ushort A_0)
	{
		//Discarded unreachable code: IL_000a
		if (A_0 == 0)
		{
			return false;
		}
		if (true)
		{
		}
		return this.m_ᜁ[A_0 - 1];
	}
}
