using System.Data;
using CSLib.Utility;

internal class ᝁ
{
	private DataTable m_ᜀ;

	public ᝁ(DataTable A_0)
	{
		this.m_ᜀ = A_0;
	}

	private string ᜀ(int A_0, int A_1)
	{
		//Discarded unreachable code: IL_0033
		int num = 4;
		while (true)
		{
			switch (num)
			{
			default:
				if (this.m_ᜀ == null)
				{
					if (true)
					{
					}
					num = 3;
				}
				else
				{
					num = 1;
				}
				continue;
			case 0:
				num = 2;
				continue;
			case 2:
				if (A_1 >= this.m_ᜀ.Columns.Count)
				{
					num = 5;
					continue;
				}
				return this.m_ᜀ.Rows[A_0][A_1].ToString();
			case 1:
				if (A_1 != -1)
				{
					num = 0;
					continue;
				}
				break;
			case 3:
				return "";
			case 5:
				break;
			}
			break;
		}
		return "";
	}

	public bool ᜀ(int A_0, int A_1, ref string A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		return cTableCell.GetString(ref A_2);
	}

	public bool ᜀ(int A_0, int A_1, ref bool A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		return cTableCell.GetBoolean(ref A_2);
	}

	public bool ᜀ(int A_0, int A_1, ref double A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		return cTableCell.GetDouble(ref A_2);
	}

	public bool ᜀ(int A_0, int A_1, ref float A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		return cTableCell.GetFloat(ref A_2);
	}

	public bool ᜀ(int A_0, int A_1, ref sbyte A_2)
	{
		//Discarded unreachable code: IL_0022
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		int value = 0;
		if (!cTableCell.GetInt32(ref value))
		{
			if (true)
			{
			}
			return false;
		}
		A_2 = (sbyte)value;
		return true;
	}

	public bool ᜀ(int A_0, int A_1, ref byte A_2)
	{
		//Discarded unreachable code: IL_0022
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		uint value = 0u;
		if (!cTableCell.GetUInt32(ref value))
		{
			if (true)
			{
			}
			return false;
		}
		A_2 = (byte)value;
		return true;
	}

	public bool ᜀ(int A_0, int A_1, ref short A_2)
	{
		//Discarded unreachable code: IL_0024
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		int value = 0;
		if (!cTableCell.GetInt32(ref value))
		{
			if (true)
			{
			}
			return false;
		}
		A_2 = (short)value;
		return true;
	}

	public bool ᜀ(int A_0, int A_1, ref ushort A_2)
	{
		//Discarded unreachable code: IL_0022
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		uint value = 0u;
		if (!cTableCell.GetUInt32(ref value))
		{
			if (true)
			{
			}
			return false;
		}
		A_2 = (ushort)value;
		return true;
	}

	public bool ᜀ(int A_0, int A_1, ref int A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		return cTableCell.GetInt32(ref A_2);
	}

	public bool ᜀ(int A_0, int A_1, ref uint A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		return cTableCell.GetUInt32(ref A_2);
	}

	public bool ᜀ(int A_0, int A_1, ref long A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		return cTableCell.GetInt64(ref A_2);
	}

	public bool ᜀ(int A_0, int A_1, ref ulong A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CTableCell cTableCell = new CTableCell();
		cTableCell.SetValue(ᜀ(A_0, A_1));
		return cTableCell.GetUInt64(ref A_2);
	}

	private int ᜀ(string A_0)
	{
		//Discarded unreachable code: IL_0050
		int num = 8;
		int result = default(int);
		int num2 = default(int);
		while (true)
		{
			switch (num)
			{
			default:
				if (this.m_ᜀ == null)
				{
					num = 1;
					break;
				}
				result = -1;
				num2 = 0;
				num = 6;
				break;
			case 4:
				if (!(A_0 == this.m_ᜀ.Columns[num2].ColumnName.ToString()))
				{
					num2++;
					if (true)
					{
					}
					num = 7;
				}
				else
				{
					num = 3;
				}
				break;
			case 1:
				return -1;
			case 6:
			case 7:
				num = 5;
				break;
			case 5:
				num = ((num2 >= this.m_ᜀ.Columns.Count) ? 2 : 4);
				break;
			case 3:
				result = num2;
				num = 0;
				break;
			case 0:
			case 2:
				return result;
			}
		}
	}

	public bool ᜀ(int A_0, string A_1, ref string A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref bool A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref double A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref float A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref sbyte A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref byte A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref short A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref ushort A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref int A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref uint A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref long A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}

	public bool ᜀ(int A_0, string A_1, ref ulong A_2)
	{
		return ᜀ(A_0, ᜀ(A_1), ref A_2);
	}
}
