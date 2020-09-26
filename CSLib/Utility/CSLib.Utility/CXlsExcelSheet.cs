using System.Data;

namespace CSLib.Utility
{
	public class CXlsExcelSheet : CTableSheet
	{
		private string ᜀ;

		private CTableRow ᜁ;

		public string Name
		{
			get
			{
				return ᜀ;
			}
			set
			{
				ᜀ = value;
			}
		}

		public CXlsExcelSheet(int columnMax)
		{
			int a_ = 4;
			base._002Ector(columnMax);
			ᜀ = CSimpleThreadPool.b("ጿ⩁⅃⍅㱇等", a_);
		}

		public bool ReadFromFile(DataTable dataTable)
		{
			//Discarded unreachable code: IL_008a
			switch (0)
			{
			}
			int num4 = default(int);
			DataRow dataRow = default(DataRow);
			while (true)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				while (true)
				{
					switch (num3)
					{
					case 9:
						return false;
					case 7:
						return false;
					case 3:
						ᜁ = new CXlsExcelRow(this, dataTable.Columns.Count);
						num4 = 0;
						num3 = 2;
						continue;
					case 2:
						if (1 == 0)
						{
						}
						goto case 10;
					case 4:
						if (ᜁ == null)
						{
							num3 = 3;
							continue;
						}
						goto case 12;
					case 5:
						if (((CXlsExcelRow)m_excelRows[num]).ReadFromFile(dataRow, dataTable.Columns.Count))
						{
							num++;
							num2++;
							num3 = 8;
						}
						else
						{
							num3 = 9;
						}
						continue;
					case 0:
					case 8:
						num3 = 6;
						continue;
					case 6:
						if (num2 < dataTable.Rows.Count)
						{
							dataRow = dataTable.Rows[num2];
							num3 = 4;
						}
						else
						{
							num3 = 1;
						}
						continue;
					case 10:
						num3 = 13;
						continue;
					case 13:
					{
						if (num4 >= dataTable.Columns.Count)
						{
							num3 = 12;
							continue;
						}
						string columnName = dataTable.Columns[num4].ColumnName.ToString();
						SetColumnName(num4, columnName);
						num4++;
						num3 = 10;
						continue;
					}
					case 12:
						num3 = 11;
						continue;
					case 11:
						num3 = ((!AppendRow(1)) ? 7 : 5);
						continue;
					case 1:
						return true;
					}
					break;
				}
			}
		}

		protected override CTableRow _CreateRow()
		{
			return new CXlsExcelRow(this, m_columnMax);
		}
	}
}
