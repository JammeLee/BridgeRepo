using System.Data;

namespace CSLib.Utility
{
	public class CXlsExcelRow : CTableRow
	{
		public CXlsExcelRow(CTableSheet table, int cellMax)
			: base(table, cellMax)
		{
		}

		public bool ReadFromFile(DataRow dataRow, int colCount)
		{
			//Discarded unreachable code: IL_0035
			CTableCell cTableCell = default(CTableCell);
			while (true)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 6;
				while (true)
				{
					switch (num3)
					{
					case 1:
						if (true)
						{
						}
						cTableCell.SetValue(dataRow[num].ToString());
						num++;
						num2++;
						num3 = 5;
						continue;
					case 0:
						cTableCell = new CTableCell();
						m_cells[num] = cTableCell;
						num3 = 1;
						continue;
					case 3:
						if (cTableCell == null)
						{
							num3 = 0;
							continue;
						}
						goto case 1;
					case 5:
					case 6:
						num3 = 4;
						continue;
					case 4:
						if (num2 < colCount)
						{
							cTableCell = m_cells[num];
							num3 = 3;
						}
						else
						{
							num3 = 2;
						}
						continue;
					case 2:
						return true;
					}
					break;
				}
			}
		}
	}
}
