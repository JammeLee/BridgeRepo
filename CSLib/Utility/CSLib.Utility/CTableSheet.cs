using System;
using System.Collections.Generic;

namespace CSLib.Utility
{
	public class CTableSheet
	{
		protected int m_columnMax;

		protected List<CTableRow> m_excelRows = new List<CTableRow>();

		protected Dictionary<string, int> m_excelColumnNameHashMap = new Dictionary<string, int>();

		protected string[] m_excelColumnNameList;

		public CTableSheet()
		{
		}

		public CTableSheet(int columnMax)
		{
			m_columnMax = columnMax;
			m_excelColumnNameList = new string[columnMax];
		}

		public CTableRow GetRow(int rowNum)
		{
			//Discarded unreachable code: IL_0011
			if (rowNum >= m_excelRows.Count)
			{
				if (true)
				{
				}
				return null;
			}
			return m_excelRows[rowNum];
		}

		public virtual CTableRow GetRowById(uint id)
		{
			throw new NotImplementedException();
		}

		public virtual bool GetRowById(uint id, out CTableRow data)
		{
			throw new NotImplementedException();
		}

		public int CountRows()
		{
			return m_excelRows.Count;
		}

		public int CountColumns()
		{
			return m_columnMax;
		}

		public void SetColumnName(int columnNum, string columnName)
		{
			//Discarded unreachable code: IL_000b
			if (m_excelColumnNameList == null)
			{
				if (1 == 0)
				{
				}
			}
			else
			{
				m_excelColumnNameList[columnNum] = columnName;
				m_excelColumnNameHashMap.Add(columnName, columnNum);
			}
		}

		public int GetColumnNum(string name)
		{
			//Discarded unreachable code: IL_0019
			int value = -1;
			if (!m_excelColumnNameHashMap.TryGetValue(name, out value))
			{
				return -1;
			}
			if (true)
			{
			}
			return value;
		}

		public string GetColumnName(int columnNum)
		{
			if (m_excelColumnNameList == null)
			{
				return "";
			}
			return m_excelColumnNameList[columnNum];
		}

		public bool AppendRow(int num)
		{
			//Discarded unreachable code: IL_0054
			while (true)
			{
				int num2 = 0;
				int num3 = 1;
				while (true)
				{
					switch (num3)
					{
					case 0:
					case 1:
						num3 = 2;
						continue;
					case 2:
					{
						if (num2 >= num)
						{
							num3 = 3;
							continue;
						}
						CTableRow item = _CreateRow();
						m_excelRows.Add(item);
						num2++;
						if (true)
						{
						}
						num3 = 0;
						continue;
					}
					case 3:
						return true;
					}
					break;
				}
			}
		}

		public bool RemoveRow(int whereRowNum, int num)
		{
			//Discarded unreachable code: IL_005d
			while (true)
			{
				int count = m_excelRows.Count;
				int num2 = 3;
				while (true)
				{
					switch (num2)
					{
					case 3:
						if (whereRowNum >= 0)
						{
							num2 = 4;
							continue;
						}
						goto IL_0042;
					case 0:
						num2 = 5;
						continue;
					case 5:
						if (whereRowNum + num > count)
						{
							num2 = 1;
							continue;
						}
						m_excelRows.RemoveRange(whereRowNum, num);
						return true;
					case 1:
						if (1 == 0)
						{
						}
						goto IL_0042;
					case 4:
						num2 = 2;
						continue;
					case 2:
						{
							if (whereRowNum < count)
							{
								num2 = 0;
								continue;
							}
							goto IL_0042;
						}
						IL_0042:
						return false;
					}
					break;
				}
			}
		}

		public bool InsertRow(int whereRowNum, int num)
		{
			//Discarded unreachable code: IL_005f
			switch (0)
			{
			}
			List<CTableRow> list = default(List<CTableRow>);
			int num3 = default(int);
			while (true)
			{
				int count = m_excelRows.Count;
				int num2 = 2;
				while (true)
				{
					switch (num2)
					{
					case 2:
						if (whereRowNum >= 0)
						{
							num2 = 7;
							continue;
						}
						goto case 1;
					case 7:
						if (true)
						{
						}
						num2 = 3;
						continue;
					case 3:
						if (whereRowNum < count)
						{
							list = new List<CTableRow>();
							num3 = 0;
							num2 = 4;
						}
						else
						{
							num2 = 1;
						}
						continue;
					case 4:
					case 6:
						num2 = 0;
						continue;
					case 0:
						if (num3 < num)
						{
							CTableRow item = _CreateRow();
							list.Add(item);
							num3++;
							num2 = 6;
						}
						else
						{
							num2 = 5;
						}
						continue;
					case 1:
						return false;
					case 5:
						m_excelRows.InsertRange(whereRowNum, list);
						return true;
					}
					break;
				}
			}
		}

		public void Clear()
		{
			m_excelRows.Clear();
		}

		private CTableCell ᜀ(int A_0, int A_1)
		{
			//Discarded unreachable code: IL_0023
			int num = 3;
			CTableRow cTableRow = default(CTableRow);
			while (true)
			{
				switch (num)
				{
				case 2:
					return null;
				case 0:
					if (cTableRow == null)
					{
						num = 2;
						continue;
					}
					return cTableRow.GetColumn(A_1);
				case 1:
					return null;
				}
				if (true)
				{
				}
				if (A_0 >= m_excelRows.Count)
				{
					num = 1;
					continue;
				}
				cTableRow = m_excelRows[A_0];
				num = 0;
			}
		}

		public bool GetColumn(int rowNum, int columnNum, ref string value)
		{
			//Discarded unreachable code: IL_0011
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref bool value)
		{
			//Discarded unreachable code: IL_000f
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref double value)
		{
			//Discarded unreachable code: IL_0013
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				return false;
			}
			if (true)
			{
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref float value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref sbyte value)
		{
			//Discarded unreachable code: IL_000f
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref byte value)
		{
			//Discarded unreachable code: IL_000f
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref short value)
		{
			//Discarded unreachable code: IL_000f
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref ushort value)
		{
			//Discarded unreachable code: IL_000f
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref int value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref uint value)
		{
			//Discarded unreachable code: IL_0013
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				return false;
			}
			if (true)
			{
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref long value)
		{
			//Discarded unreachable code: IL_000f
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, int columnNum, ref ulong value)
		{
			//Discarded unreachable code: IL_0013
			CTableCell cTableCell = ᜀ(rowNum, columnNum);
			if (cTableCell == null)
			{
				return false;
			}
			if (true)
			{
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		private CTableCell ᜀ(int A_0, string A_1)
		{
			//Discarded unreachable code: IL_000d
			int num = 3;
			CTableRow cTableRow = default(CTableRow);
			while (true)
			{
				if (true)
				{
				}
				switch (num)
				{
				case 1:
					return null;
				case 2:
					if (cTableRow == null)
					{
						num = 1;
						continue;
					}
					return cTableRow.GetColumn(A_1);
				case 0:
					return null;
				}
				if (A_0 >= m_excelRows.Count)
				{
					num = 0;
					continue;
				}
				cTableRow = m_excelRows[A_0];
				num = 2;
			}
		}

		public bool GetColumn(int rowNum, string columnName, ref string value)
		{
			//Discarded unreachable code: IL_0011
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref bool value)
		{
			//Discarded unreachable code: IL_0013
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				return false;
			}
			if (true)
			{
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref double value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref float value)
		{
			//Discarded unreachable code: IL_0013
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				return false;
			}
			if (true)
			{
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref sbyte value)
		{
			//Discarded unreachable code: IL_0013
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				return false;
			}
			if (true)
			{
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref byte value)
		{
			//Discarded unreachable code: IL_0013
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				return false;
			}
			if (true)
			{
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref short value)
		{
			//Discarded unreachable code: IL_000f
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref ushort value)
		{
			//Discarded unreachable code: IL_0011
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref int value)
		{
			//Discarded unreachable code: IL_0013
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				return false;
			}
			if (true)
			{
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref uint value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref long value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		public bool GetColumn(int rowNum, string columnName, ref ulong value)
		{
			//Discarded unreachable code: IL_0011
			CTableCell cTableCell = ᜀ(rowNum, columnName);
			if (cTableCell == null)
			{
				if (true)
				{
				}
				return false;
			}
			cTableCell.GetValue(ref value);
			return true;
		}

		protected virtual CTableRow _CreateRow()
		{
			return new CTableRow(this, m_columnMax);
		}
	}
}
