using System;
using System.IO;

namespace CSLib.Utility
{
	public class CExcelTableReader
	{
		public delegate void HandleFinishLoad(bool result, string error);

		public enum EExcelExt
		{
			EE_NOT_SUPPORT,
			EE_XML,
			EE_CSV
		}

		private CXmlExcel m_ᜀ;

		private CCsvExcel ᜁ;

		private EExcelExt ᜂ;

		public EExcelExt ExcelExt => ᜂ;

		public bool LoadFile(string fileName)
		{
			//Discarded unreachable code: IL_0079
			int a_ = 2;
			int num = 6;
			bool flag = default(bool);
			EExcelExt eExcelExt = default(EExcelExt);
			while (true)
			{
				switch (num)
				{
				default:
					if (!File.Exists(fileName))
					{
						num = 5;
						continue;
					}
					ᜂ = ᜀ(fileName);
					flag = false;
					eExcelExt = ᜂ;
					num = 11;
					continue;
				case 4:
					this.m_ᜀ = null;
					num = 8;
					continue;
				case 2:
					if (!flag)
					{
						num = 4;
						continue;
					}
					break;
				case 11:
					if (eExcelExt == EExcelExt.EE_XML)
					{
						if (true)
						{
						}
						this.m_ᜀ = new CXmlExcel();
						flag = this.m_ᜀ.LoadFile(fileName);
						num = 2;
					}
					else
					{
						num = 1;
					}
					continue;
				case 9:
					ᜁ = null;
					num = 10;
					continue;
				case 5:
					CDebugOut.LogError(CSimpleThreadPool.b("砽⤿⹁⅃籅桇癉", a_) + fileName + CSimpleThreadPool.b("=怿♁⭃⍅㭇⑉歋㩍灏㝑ⱓ㽕⭗\u2e59絛", a_));
					return false;
				case 12:
					if (!flag)
					{
						num = 9;
						continue;
					}
					break;
				case 7:
					num = 3;
					continue;
				case 1:
					num = 0;
					continue;
				case 0:
					if (eExcelExt == EExcelExt.EE_CSV)
					{
						ᜁ = new CCsvExcel();
						flag = ᜁ.LoadFile(fileName);
						num = 12;
					}
					else
					{
						num = 7;
					}
					continue;
				case 3:
				case 8:
				case 10:
					break;
				}
				break;
			}
			return flag;
		}

		public bool LoadString(string fileString, EExcelExt ext)
		{
			//Discarded unreachable code: IL_0075
			while (true)
			{
				ᜂ = ext;
				bool flag = false;
				EExcelExt eExcelExt = ᜂ;
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (eExcelExt != EExcelExt.EE_XML)
						{
							num = 2;
							continue;
						}
						this.m_ᜀ = new CXmlExcel();
						flag = this.m_ᜀ.LoadString(fileString);
						num = 10;
						continue;
					case 1:
						if (!flag)
						{
							num = 4;
							continue;
						}
						goto case 5;
					case 10:
						if (!flag)
						{
							num = 8;
							continue;
						}
						goto case 5;
					case 2:
						num = 9;
						continue;
					case 9:
						if (eExcelExt == EExcelExt.EE_CSV)
						{
							ᜁ = new CCsvExcel();
							flag = ᜁ.LoadString(fileString);
							if (true)
							{
							}
							num = 1;
						}
						else
						{
							num = 3;
						}
						continue;
					case 4:
						ᜁ = null;
						num = 7;
						continue;
					case 3:
						num = 6;
						continue;
					case 8:
						this.m_ᜀ = null;
						num = 5;
						continue;
					case 5:
					case 6:
					case 7:
						return flag;
					}
					break;
				}
			}
		}

		public CTableSheet GetTableSheet(string tableName)
		{
			//Discarded unreachable code: IL_005a
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((this.m_ᜀ == null) ? 2 : 3);
					break;
				case 2:
					if (ᜁ != null)
					{
						num = 1;
						break;
					}
					return null;
				case 1:
					if (true)
					{
					}
					return ᜁ.GetTableSheet(tableName);
				case 3:
					return this.m_ᜀ.GetTableSheet(tableName);
				}
			}
		}

		public CTableSheet GetTableSheet(int tableNum)
		{
			//Discarded unreachable code: IL_005a
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((this.m_ᜀ == null) ? 2 : 3);
					break;
				case 2:
					if (ᜁ != null)
					{
						num = 0;
						break;
					}
					return null;
				case 0:
					if (true)
					{
					}
					return ᜁ.GetTableSheet(tableNum);
				case 3:
					return this.m_ᜀ.GetTableSheet(tableNum);
				}
			}
		}

		private EExcelExt ᜀ(string A_0)
		{
			//Discarded unreachable code: IL_005e
			int a_ = 10;
			while (true)
			{
				string extension = Path.GetExtension(A_0);
				int num = 3;
				while (true)
				{
					switch (num)
					{
					case 3:
						num = (extension.Equals(CSimpleThreadPool.b("桅ぇ❉⁋", a_), StringComparison.CurrentCultureIgnoreCase) ? 1 : 2);
						continue;
					case 0:
						return EExcelExt.EE_CSV;
					case 2:
						if (true)
						{
						}
						if (extension.Equals(CSimpleThreadPool.b("桅⭇㥉㩋", a_), StringComparison.CurrentCultureIgnoreCase))
						{
							num = 0;
							continue;
						}
						return EExcelExt.EE_NOT_SUPPORT;
					case 1:
						return EExcelExt.EE_XML;
					}
					break;
				}
			}
		}
	}
}
