using System;
using System.Xml;

namespace CSLib.Utility
{
	public class CXmlExcelRow : CTableRow
	{
		public CXmlExcelRow(CTableSheet table, int cellMax)
			: base(table, cellMax)
		{
		}

		public bool ReadFromFile(XmlNodeList xmlCellList)
		{
			//Discarded unreachable code: IL_00c5
			int a_ = 13;
			switch (0)
			{
			}
			CTableCell cTableCell = default(CTableCell);
			int num4 = default(int);
			XmlNode namedItem = default(XmlNode);
			XmlNode xmlNode = default(XmlNode);
			while (true)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 13;
				while (true)
				{
					switch (num3)
					{
					case 7:
						cTableCell = new CTableCell();
						m_cells[num] = cTableCell;
						num3 = 4;
						continue;
					case 12:
						CDebugOut.LogError(CSimpleThreadPool.b("\u0a48⑊⅌㩎㱐㵒᱔㥖㵘㹚╜捞", a_) + num + CSimpleThreadPool.b("睈歊\u244c㱎煐㽒㑔╖㹘㹚⽜罞ᕠ\u0b62Ѥ०䥨Ὢլ੮兰\u1072\u1074᭶ᕸ\u087a嵼ᅾ\uf480\uee82\ue784\ue286ﮈꪊ게", a_));
						return false;
					case 6:
						num4 = Convert.ToInt32(namedItem.InnerText);
						if (true)
						{
						}
						num3 = 9;
						continue;
					case 9:
						if (num4 > 0)
						{
							num3 = 10;
							continue;
						}
						goto case 11;
					case 4:
						cTableCell.SetValue(xmlNode.InnerText);
						num++;
						num2++;
						num3 = 2;
						continue;
					case 0:
						if (namedItem != null)
						{
							num3 = 6;
							continue;
						}
						goto case 11;
					case 3:
						if (cTableCell == null)
						{
							num3 = 7;
							continue;
						}
						goto case 4;
					case 2:
					case 13:
						num3 = 5;
						continue;
					case 5:
						if (num2 < xmlCellList.Count)
						{
							xmlNode = xmlCellList[num2];
							namedItem = xmlNode.Attributes.GetNamedItem(CSimpleThreadPool.b("㩈㡊睌\u064e㽐㝒ご⽖", a_));
							num3 = 0;
						}
						else
						{
							num3 = 1;
						}
						continue;
					case 11:
						num3 = 8;
						continue;
					case 8:
						if (num < m_cells.Length)
						{
							cTableCell = m_cells[num];
							num3 = 3;
						}
						else
						{
							num3 = 12;
						}
						continue;
					case 10:
						num = num4 - 1;
						num3 = 11;
						continue;
					case 1:
						return true;
					}
					break;
				}
			}
		}
	}
}
