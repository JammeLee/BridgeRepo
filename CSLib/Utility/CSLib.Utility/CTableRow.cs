namespace CSLib.Utility
{
	public class CTableRow
	{
		protected CTableCell[] m_cells;

		private CTableSheet m_ᜀ;

		public CTableRow()
		{
		}

		public CTableRow(CTableSheet table, int cellMax)
		{
			this.m_ᜀ = table;
			m_cells = new CTableCell[cellMax];
			for (int i = 0; i < m_cells.Length; i++)
			{
				m_cells[i] = ᜀ();
			}
		}

		public CTableCell GetColumn(int columnNum)
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
					num = ((m_cells != null) ? 2 : 3);
					break;
				case 0:
					return null;
				case 2:
					if (columnNum >= m_cells.Length)
					{
						num = 0;
						break;
					}
					return m_cells[columnNum];
				case 3:
					return null;
				}
			}
		}

		public CTableCell GetColumn(string columnName)
		{
			//Discarded unreachable code: IL_003b
			int num = 5;
			int columnNum = default(int);
			while (true)
			{
				switch (num)
				{
				default:
					if (this.m_ᜀ != null)
					{
						num = 3;
						break;
					}
					goto case 4;
				case 3:
					if (true)
					{
					}
					num = 2;
					break;
				case 1:
					if (columnNum < 0)
					{
						num = 0;
						break;
					}
					return m_cells[columnNum];
				case 4:
					return null;
				case 2:
					if (m_cells != null)
					{
						columnNum = this.m_ᜀ.GetColumnNum(columnName);
						num = 1;
					}
					else
					{
						num = 4;
					}
					break;
				case 0:
					return null;
				}
			}
		}

		public int CountCell()
		{
			if (m_cells == null)
			{
				return 0;
			}
			return m_cells.Length;
		}

		private void ᜁ()
		{
			//Discarded unreachable code: IL_0069
			int num = 4;
			int num2 = default(int);
			while (true)
			{
				switch (num)
				{
				default:
					if (m_cells == null)
					{
						num = 0;
						break;
					}
					num2 = 0;
					num = 5;
					break;
				case 0:
					return;
				case 2:
				case 5:
					num = 3;
					break;
				case 3:
					if (true)
					{
					}
					if (num2 < m_cells.Length)
					{
						m_cells[num2] = null;
						num2++;
						num = 2;
					}
					else
					{
						num = 1;
					}
					break;
				case 1:
					return;
				}
			}
		}

		private CTableCell ᜀ()
		{
			return new CTableCell();
		}
	}
}
