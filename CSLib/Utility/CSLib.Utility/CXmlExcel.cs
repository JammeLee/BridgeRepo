using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CSLib.Utility
{
	public class CXmlExcel
	{
		public delegate void HandleFinishLoad(bool result, string error);

		private List<CXmlExcelSheet> m_ᜀ = new List<CXmlExcelSheet>();

		private Dictionary<string, int> ᜁ = new Dictionary<string, int>();

		public bool LoadFile(string fileName)
		{
			//Discarded unreachable code: IL_002c
			int a_ = 3;
			int num = 1;
			FileStream fileStream = default(FileStream);
			while (true)
			{
				switch (num)
				{
				case 0:
					CDebugOut.LogError(CSimpleThreadPool.b("社⡀⽂⁄絆楈睊", a_) + fileName + CSimpleThreadPool.b("ľ慀⽂⩄♆ⵈ歊⭌\u2e4e㡐㽒ご㍖硘", a_));
					return false;
				case 2:
				{
					if (fileStream == null)
					{
						num = 0;
						continue;
					}
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(fileStream);
					return ᜀ(xmlDocument);
				}
				case 3:
					CDebugOut.LogError(CSimpleThreadPool.b("社⡀⽂⁄絆楈睊", a_) + fileName + CSimpleThreadPool.b("ľ慀❂⩄≆㩈╊橌㭎煐㙒ⵔ㹖⩘⽚籜", a_));
					return false;
				}
				if (true)
				{
				}
				if (!File.Exists(fileName))
				{
					num = 3;
					continue;
				}
				fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				num = 2;
			}
		}

		public bool LoadString(string xmlString)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlString);
			return ᜀ(xmlDocument);
		}

		private bool ᜀ(XmlDocument A_0)
		{
			//Discarded unreachable code: IL_0137
			int a_ = 5;
			while (true)
			{
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(A_0.NameTable);
				xmlNamespaceManager.AddNamespace(CSimpleThreadPool.b("⹀", a_), CSimpleThreadPool.b("㑀ㅂ⭄絆㩈⡊╌⩎㱐㉒♔穖㑘㉚㹜ⵞ\u0e60\u1062\u0a64Ŧᵨ䙪\u0e6cnᱰ䥲\u1a74ᅶὸቺṼ\u1a7e뮀\uec82\ue384\ue186\ue088\ue88a\ue88c", a_));
				xmlNamespaceManager.AddNamespace(CSimpleThreadPool.b("㥀", a_), CSimpleThreadPool.b("㑀ㅂ⭄絆㩈⡊╌⩎㱐㉒♔穖㑘㉚㹜ⵞ\u0e60\u1062\u0a64Ŧᵨ䙪\u0e6cnᱰ䥲\u1a74ᅶὸቺṼ\u1a7e뮀\ue682ﶄ\ue486\uec88\ue78a", a_));
				xmlNamespaceManager.AddNamespace(CSimpleThreadPool.b("㉀あ", a_), CSimpleThreadPool.b("㑀ㅂ⭄絆㩈⡊╌⩎㱐㉒♔穖㑘㉚㹜ⵞ\u0e60\u1062\u0a64Ŧᵨ䙪\u0e6cnᱰ䥲\u1a74ᅶὸቺṼ\u1a7e뮀\uf082\uf584\uf586\uec88\uea8a\ue98cﲎ戀\uf692\uf094\ue396", a_));
				xmlNamespaceManager.AddNamespace(CSimpleThreadPool.b("⥀㝂⡄⭆", a_), CSimpleThreadPool.b("⥀㝂ㅄ㝆獈摊扌㡎♐\u2452答⁖橘畚㉜ⵞ٠䱢ㅤ㕦䙨㥪⡬Ɱ屰\u1b72Ŵ\u1a76ᕸ佺䵼", a_));
				xmlNamespaceManager.AddNamespace(CSimpleThreadPool.b("㑀牂", a_), CSimpleThreadPool.b("⥀㝂ㅄ㝆獈摊扌㡎♐\u2452答⁖橘畚㉜ⵞ٠䱢ㅤ㕦䙨㽪⡬Ɱ屰\u1b72Ŵ\u1a76ᕸ佺䵼", a_));
				XmlNodeList xmlNodeList = A_0.SelectSingleNode(CSimpleThreadPool.b("㉀あ罄၆♈㥊♌ⵎ㹐㱒㹔硖⩘⡚杜࡞\u0e60ᅢ\u0e64ᑦŨ\u0e6a\u086c\u1b6e幰rٴ䵶\u2d78\u1a7aὼ\u137e\ue480", a_), xmlNamespaceManager).SelectNodes(CSimpleThreadPool.b("㉀あ罄ᕆ♈㱊", a_), xmlNamespaceManager);
				string innerText = A_0.SelectSingleNode(CSimpleThreadPool.b("㉀あ罄၆♈㥊♌ⵎ㹐㱒㹔硖⩘⡚杜࡞\u0e60ᅢ\u0e64ᑦŨ\u0e6a\u086c\u1b6e", a_), xmlNamespaceManager).Attributes.GetNamedItem(CSimpleThreadPool.b("㉀あ罄\u0946⡈♊⡌", a_)).InnerText;
				if (true)
				{
				}
				int num = 3;
				while (true)
				{
					switch (num)
					{
					case 3:
						num = ((xmlNodeList.Count > 0) ? 1 : 2);
						continue;
					case 0:
						CDebugOut.LogError(CSimpleThreadPool.b("ᅀ≂㝄㑆ⱈ歊ࡌ㝎㉐㙒㥔罖⅘㙚ㅜ癞䅠╢\u0c64୦౨䭪⡬ᵮͰᱲݴ䵶奸", a_) + innerText.ToString());
						return false;
					case 1:
						if (!ᜀ(innerText, xmlNodeList[0].ChildNodes.Count).ReadFromFile(xmlNodeList))
						{
							num = 0;
							continue;
						}
						return true;
					case 2:
						CDebugOut.LogError(CSimpleThreadPool.b("᥀โ\u0944杆ᩈ㽊㽌♎㽐㑒畔Ṗ⩘筚ᡜ\u125eㅠ㝢㱤䙦", a_));
						return false;
					}
					break;
				}
			}
		}

		public CTableSheet GetTableSheet(string tableName)
		{
			//Discarded unreachable code: IL_0019
			int value = 0;
			if (!ᜁ.TryGetValue(tableName, out value))
			{
				return null;
			}
			if (true)
			{
			}
			return GetTableSheet(value);
		}

		public CTableSheet GetTableSheet(int tableNum)
		{
			//Discarded unreachable code: IL_0015
			if (tableNum >= this.m_ᜀ.Count)
			{
				return null;
			}
			if (true)
			{
			}
			return this.m_ᜀ[tableNum];
		}

		private CXmlExcelSheet ᜀ(string A_0, int A_1)
		{
			//Discarded unreachable code: IL_008d
			CXmlExcelSheet cXmlExcelSheet = default(CXmlExcelSheet);
			while (true)
			{
				int num = 0;
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					case 6:
						cXmlExcelSheet = new CXmlExcelSheet(A_1);
						num2 = 7;
						continue;
					case 7:
						if (cXmlExcelSheet == null)
						{
							num2 = 5;
							continue;
						}
						cXmlExcelSheet.Name = A_0;
						ᜁ.Add(A_0, this.m_ᜀ.Count);
						this.m_ᜀ.Add(cXmlExcelSheet);
						return cXmlExcelSheet;
					case 4:
						if (!(this.m_ᜀ[num].Name == A_0))
						{
							num++;
							num2 = 0;
						}
						else
						{
							num2 = 2;
						}
						continue;
					case 2:
						if (true)
						{
						}
						return null;
					case 5:
						return null;
					case 0:
					case 1:
						num2 = 3;
						continue;
					case 3:
						num2 = ((num >= this.m_ᜀ.Count) ? 6 : 4);
						continue;
					}
					break;
				}
			}
		}
	}
}
