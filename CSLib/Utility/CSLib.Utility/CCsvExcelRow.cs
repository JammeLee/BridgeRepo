using System.Collections;

namespace CSLib.Utility
{
	public class CCsvExcelRow : CTableRow
	{
		public CCsvExcelRow(CTableSheet table, int cellMax)
			: base(table, cellMax)
		{
		}

		public bool ReadFromFile(ArrayList colAL)
		{
			//Discarded unreachable code: IL_0035
			CTableCell cTableCell = default(CTableCell);
			while (true)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 2;
				while (true)
				{
					switch (num3)
					{
					case 4:
						if (true)
						{
						}
						cTableCell.SetValue(colAL[num2].ToString());
						num++;
						num2++;
						num3 = 5;
						continue;
					case 3:
						cTableCell = new CTableCell();
						m_cells[num] = cTableCell;
						num3 = 4;
						continue;
					case 1:
						if (cTableCell == null)
						{
							num3 = 3;
							continue;
						}
						goto case 4;
					case 2:
					case 5:
						num3 = 6;
						continue;
					case 6:
						if (num2 < colAL.Count)
						{
							cTableCell = m_cells[num];
							num3 = 1;
						}
						else
						{
							num3 = 0;
						}
						continue;
					case 0:
						return true;
					}
					break;
				}
			}
		}

		public string GetCell(int cellNum)
		{
			//Discarded unreachable code: IL_002f
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (cellNum >= 0)
					{
						if (true)
						{
						}
						num = 5;
						break;
					}
					goto case 3;
				case 1:
					if (m_cells[cellNum] == null)
					{
						num = 0;
						break;
					}
					return m_cells[cellNum].GetString();
				case 3:
					return string.Empty;
				case 5:
					num = 4;
					break;
				case 4:
					num = ((cellNum <= m_cells.Length) ? 1 : 3);
					break;
				case 0:
					return string.Empty;
				}
			}
		}
	}
}
