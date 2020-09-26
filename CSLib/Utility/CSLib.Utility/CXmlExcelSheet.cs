using System.Xml;

namespace CSLib.Utility
{
	public class CXmlExcelSheet : CTableSheet
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

		public CXmlExcelSheet(int columnMax)
		{
			int a_ = 9;
			base._002Ector(columnMax);
			ᜀ = CSimpleThreadPool.b("ᙄ⽆ⱈ\u2e4a㥌繎", a_);
		}

		public bool ReadFromFile(XmlNodeList xmlRowList)
		{
			//Discarded unreachable code: IL_0099
			switch (0)
			{
			}
			int num4 = default(int);
			XmlNodeList childNodes = default(XmlNodeList);
			while (true)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 14;
				while (true)
				{
					switch (num3)
					{
					case 5:
						return false;
					case 7:
						return false;
					case 9:
					case 13:
						num2++;
						if (true)
						{
						}
						num3 = 11;
						continue;
					case 8:
						ᜁ = new CXmlExcelRow(this, xmlRowList.Count);
						num4 = 0;
						num3 = 0;
						continue;
					case 10:
						if (!((CXmlExcelRow)m_excelRows[num]).ReadFromFile(childNodes))
						{
							num3 = 5;
							continue;
						}
						num++;
						num3 = 9;
						continue;
					case 11:
					case 14:
						num3 = 12;
						continue;
					case 12:
						if (num2 >= xmlRowList.Count)
						{
							num3 = 2;
							continue;
						}
						childNodes = xmlRowList[num2].ChildNodes;
						num3 = 15;
						continue;
					case 4:
						num3 = 13;
						continue;
					case 15:
						num3 = ((ᜁ != null) ? 1 : 8);
						continue;
					case 1:
						num3 = ((!AppendRow(1)) ? 7 : 10);
						continue;
					case 0:
					case 6:
						num3 = 3;
						continue;
					case 3:
						if (num4 < childNodes.Count)
						{
							SetColumnName(num4, childNodes[num4].InnerText);
							num4++;
							num3 = 6;
						}
						else
						{
							num3 = 4;
						}
						continue;
					case 2:
						return true;
					}
					break;
				}
			}
		}

		protected override CTableRow _CreateRow()
		{
			return new CXmlExcelRow(this, m_columnMax);
		}
	}
}
