using System.Collections;

namespace CSLib.Utility
{
	public class CCsvExcelSheet : CTableSheet
	{
		private string ᜀ;

		private CCsvExcelRow ᜁ;

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

		public CCsvExcelSheet(int columnMax)
		{
			int a_ = 18;
			base._002Ector(columnMax);
			ᜀ = CSimpleThreadPool.b("ᵍ㡏㝑ㅓ≕楗", a_);
		}

		public bool ReadFromFile(CCsvReader csvReader)
		{
			//Discarded unreachable code: IL_00cd
			switch (0)
			{
			}
			ArrayList colAL = default(ArrayList);
			int num4 = default(int);
			while (true)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 15;
				while (true)
				{
					switch (num3)
					{
					case 17:
						if (!((CCsvExcelRow)m_excelRows[num]).ReadFromFile(colAL))
						{
							num3 = 13;
							continue;
						}
						num++;
						num3 = 4;
						continue;
					case 16:
						num3 = ((!AppendRow(1)) ? 12 : 17);
						continue;
					case 6:
						if (true)
						{
						}
						return false;
					case 3:
						ᜁ = (CCsvExcelRow)_CreateRow();
						num3 = 2;
						continue;
					case 2:
						if (!ᜁ.ReadFromFile(colAL))
						{
							num3 = 6;
							continue;
						}
						num4 = 0;
						num3 = 7;
						continue;
					case 13:
						return false;
					case 5:
					case 15:
						num3 = 8;
						continue;
					case 8:
						if (num2 >= csvReader.RowCount)
						{
							num3 = 0;
							continue;
						}
						colAL = csvReader[num2 + 1];
						num3 = 11;
						continue;
					case 14:
						num3 = 10;
						continue;
					case 4:
					case 10:
						num2++;
						num3 = 5;
						continue;
					case 12:
						return false;
					case 11:
						num3 = ((ᜁ == null) ? 3 : 16);
						continue;
					case 1:
					case 7:
						num3 = 9;
						continue;
					case 9:
						if (num4 < ᜁ.CountCell())
						{
							SetColumnName(num4, ᜁ.GetCell(num4));
							num4++;
							num3 = 1;
						}
						else
						{
							num3 = 14;
						}
						continue;
					case 0:
						return true;
					}
					break;
				}
			}
		}

		protected override CTableRow _CreateRow()
		{
			return new CCsvExcelRow(this, m_columnMax);
		}
	}
}
