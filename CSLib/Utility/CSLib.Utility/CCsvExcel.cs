using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSLib.Utility
{
	public class CCsvExcel
	{
		public delegate void HandleFinishLoad(bool result, string error);

		private List<CCsvExcelSheet> m_ᜀ = new List<CCsvExcelSheet>();

		private Dictionary<string, int> ᜁ = new Dictionary<string, int>();

		private static readonly Encoding ᜂ = Encoding.UTF8;

		public bool LoadFile(string fileName)
		{
			//Discarded unreachable code: IL_002c
			int a_ = 6;
			int num = 3;
			CCsvReader cCsvReader = default(CCsvReader);
			while (true)
			{
				switch (num)
				{
				case 2:
					CDebugOut.LogError(CSimpleThreadPool.b("сⵃ⩅ⵇ灉汋牍", a_) + fileName + CSimpleThreadPool.b("籁摃⩅❇⭉⡋湍㙏㍑㵓㩕㵗㹙絛", a_));
					return false;
				case 1:
					if (!cCsvReader.Load(fileName))
					{
						num = 2;
						continue;
					}
					return _LoadCsvExcel(cCsvReader);
				case 0:
					CDebugOut.LogError(CSimpleThreadPool.b("сⵃ⩅ⵇ灉汋牍", a_) + fileName + CSimpleThreadPool.b("籁摃≅❇⽉㽋⁍睏♑瑓㍕⁗㍙⽛⩝䅟", a_));
					return false;
				}
				if (true)
				{
				}
				if (!File.Exists(fileName))
				{
					num = 0;
					continue;
				}
				cCsvReader = new CCsvReader(ᜂ);
				num = 1;
			}
		}

		public bool LoadString(string csvString)
		{
			//Discarded unreachable code: IL_0017
			CCsvReader cCsvReader = new CCsvReader(ᜂ);
			if (!cCsvReader.LoadCsv(csvString))
			{
				if (true)
				{
				}
				return false;
			}
			return _LoadCsvExcel(cCsvReader);
		}

		public bool _LoadCsvExcel(CCsvReader reader)
		{
			//Discarded unreachable code: IL_008a
			int a_ = 14;
			int num = 2;
			string fileNameWithoutExtension = default(string);
			while (true)
			{
				switch (num)
				{
				case 1:
					if (!ᜀ(fileNameWithoutExtension, reader.ColCount).ReadFromFile(reader))
					{
						num = 0;
						continue;
					}
					return true;
				case 0:
					if (true)
					{
					}
					CDebugOut.LogError(CSimpleThreadPool.b("ᩉⵋ㱍⍏㝑瑓ፕ⁗㥙㥛㉝䡟šᝣၥ䅧䩩⩫ݭᱯ\u1771味㍵\u0a77\u0879፻౽멿ꊁ", a_) + fileNameWithoutExtension.ToString());
					return false;
				case 3:
					return false;
				}
				if (reader == null)
				{
					num = 3;
					continue;
				}
				fileNameWithoutExtension = Path.GetFileNameWithoutExtension(reader.FileName);
				num = 1;
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
			//Discarded unreachable code: IL_0013
			if (tableNum >= this.m_ᜀ.Count)
			{
				if (true)
				{
				}
				return null;
			}
			return this.m_ᜀ[tableNum];
		}

		private CCsvExcelSheet ᜀ(string A_0, int A_1)
		{
			//Discarded unreachable code: IL_0046
			CCsvExcelSheet cCsvExcelSheet = default(CCsvExcelSheet);
			while (true)
			{
				int num = 0;
				int num2 = 4;
				while (true)
				{
					switch (num2)
					{
					case 6:
						cCsvExcelSheet = new CCsvExcelSheet(A_1);
						num2 = 5;
						continue;
					case 5:
						if (true)
						{
						}
						if (cCsvExcelSheet == null)
						{
							num2 = 1;
							continue;
						}
						cCsvExcelSheet.Name = A_0;
						ᜁ.Add(A_0, this.m_ᜀ.Count);
						this.m_ᜀ.Add(cCsvExcelSheet);
						return cCsvExcelSheet;
					case 2:
						if (!(this.m_ᜀ[num].Name == A_0))
						{
							num++;
							num2 = 0;
						}
						else
						{
							num2 = 7;
						}
						continue;
					case 1:
						return null;
					case 7:
						return null;
					case 0:
					case 4:
						num2 = 3;
						continue;
					case 3:
						num2 = ((num >= this.m_ᜀ.Count) ? 6 : 2);
						continue;
					}
					break;
				}
			}
		}
	}
}
