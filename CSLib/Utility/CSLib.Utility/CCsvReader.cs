using System;
using System.Collections;
using System.IO;
using System.Text;

namespace CSLib.Utility
{
	public class CCsvReader
	{
		private ArrayList m_ᜀ;

		private string m_ᜁ;

		private Encoding m_ᜂ;

		public string FileName => this.m_ᜁ;

		public Encoding FileEncoding
		{
			set
			{
				this.m_ᜂ = value;
			}
		}

		public int RowCount => this.m_ᜀ.Count;

		public int ColCount
		{
			get
			{
				//Discarded unreachable code: IL_0039
				ArrayList arrayList = default(ArrayList);
				while (true)
				{
					int num = 0;
					int num2 = 0;
					int num3 = 7;
					while (true)
					{
						int num4;
						switch (num3)
						{
						case 1:
						case 7:
							if (true)
							{
							}
							num3 = 6;
							continue;
						case 6:
							if (num2 >= this.m_ᜀ.Count)
							{
								num3 = 4;
								continue;
							}
							arrayList = (ArrayList)this.m_ᜀ[num2];
							num3 = 2;
							continue;
						case 0:
							num3 = 5;
							continue;
						case 5:
							num4 = arrayList.Count;
							goto IL_00af;
						case 2:
							num3 = ((num > arrayList.Count) ? 3 : 0);
							continue;
						case 3:
							num4 = num;
							goto IL_00af;
						case 4:
							{
								return num;
							}
							IL_00af:
							num = num4;
							num2++;
							num3 = 1;
							continue;
						}
						break;
					}
				}
			}
		}

		public string this[int row, int col]
		{
			get
			{
				//Discarded unreachable code: IL_002e
				ᜃ(row);
				ᜁ(col);
				ArrayList arrayList = (ArrayList)this.m_ᜀ[row - 1];
				if (arrayList.Count < col)
				{
					if (true)
					{
					}
					return "";
				}
				return arrayList[col - 1].ToString();
			}
		}

		public ArrayList this[int row]
		{
			get
			{
				//Discarded unreachable code: IL_0003
				if (true)
				{
				}
				ᜃ(row);
				return (ArrayList)this.m_ᜀ[row - 1];
			}
		}

		public CCsvReader()
		{
			this.m_ᜀ = new ArrayList();
			this.m_ᜁ = "";
			this.m_ᜂ = Encoding.Default;
		}

		public CCsvReader(Encoding encoding)
		{
			this.m_ᜀ = new ArrayList();
			this.m_ᜁ = "";
			this.m_ᜂ = encoding;
		}

		public virtual bool Load(string fileName)
		{
			//Discarded unreachable code: IL_00b6
			int a_ = 10;
			while (true)
			{
				this.m_ᜁ = fileName;
				int num = 4;
				while (true)
				{
					switch (num)
					{
					case 4:
						num = ((this.m_ᜁ != null) ? 2 : 6);
						continue;
					case 3:
						if (this.m_ᜂ == null)
						{
							num = 0;
							continue;
						}
						goto case 5;
					case 1:
						throw new Exception(CSimpleThreadPool.b("䄦툜츿ཋᵍ\u064f픴ꈝ嬛\f爎", a_));
					case 6:
						throw new Exception(CSimpleThreadPool.b("뇎伤퀒췂㏂㔞혧\u1753Օ๗\udd3cꨕ匉", a_));
					case 0:
						this.m_ᜂ = Encoding.Default;
						num = 5;
						continue;
					case 2:
						if (true)
						{
						}
						num = ((!File.Exists(this.m_ᜁ)) ? 1 : 3);
						continue;
					case 5:
					{
						FileStream a_2 = new FileStream(this.m_ᜁ, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
						return ᜀ(a_2);
					}
					}
					break;
				}
			}
		}

		public virtual bool LoadCsv(string csv)
		{
			//Discarded unreachable code: IL_0031
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (this.m_ᜂ == null)
					{
						num = 2;
						continue;
					}
					break;
				case 2:
					if (true)
					{
					}
					this.m_ᜂ = Encoding.Default;
					num = 1;
					continue;
				case 1:
					break;
				}
				break;
			}
			MemoryStream a_ = new MemoryStream(this.m_ᜂ.GetBytes(csv));
			return ᜀ(a_);
		}

		private void ᜃ(int A_0)
		{
			//Discarded unreachable code: IL_002c
			int a_ = 18;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					num = ((A_0 <= 0) ? 1 : 2);
					break;
				case 3:
					throw new Exception(CSimpleThreadPool.b("\uef21夨Ďᤁ\u1add\udc21⨼㈸", a_));
				case 2:
					if (A_0 > RowCount)
					{
						num = 3;
						break;
					}
					return;
				case 1:
					throw new Exception(CSimpleThreadPool.b("\u02c5\u202a弟꧓変혙橙", a_));
				}
			}
		}

		private void ᜂ(int A_0)
		{
			//Discarded unreachable code: IL_0038
			int a_ = 17;
			int num = 4;
			while (true)
			{
				switch (num)
				{
				default:
					if (A_0 <= 0)
					{
						if (true)
						{
						}
						num = 3;
						break;
					}
					goto IL_0047;
				case 1:
					if (A_0 > RowCount)
					{
						num = 5;
						break;
					}
					return;
				case 0:
					throw new Exception(CSimpleThreadPool.b("Ǆ㼫尞껒\u1c2f\ud918楘䴸刀턐䱠剢", a_));
				case 3:
					num = 2;
					break;
				case 2:
					if (A_0 != -1)
					{
						num = 0;
						break;
					}
					goto IL_0047;
				case 5:
					{
						throw new Exception(CSimpleThreadPool.b("\uec20䘩ȏḀ\u19dc팠⤽㔹", a_));
					}
					IL_0047:
					num = 1;
					break;
				}
			}
		}

		private void ᜁ(int A_0)
		{
			//Discarded unreachable code: IL_004e
			int a_ = 17;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 1:
					throw new Exception(CSimpleThreadPool.b("\uec20䘩ȏḀ䈆팠⤽㔹", a_));
				case 0:
					if (A_0 > ColCount)
					{
						num = 1;
						continue;
					}
					return;
				case 3:
					throw new Exception(CSimpleThreadPool.b("娞㼫尞껒娈\ud918楘", a_));
				}
				if (A_0 <= 0)
				{
					num = 3;
					continue;
				}
				if (true)
				{
				}
				num = 0;
			}
		}

		private void ᜀ(int A_0)
		{
			//Discarded unreachable code: IL_0034
			int a_ = 17;
			int num = 4;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (A_0 <= 0)
					{
						num = 0;
						break;
					}
					goto IL_0047;
				case 3:
					if (A_0 > ColCount)
					{
						num = 5;
						break;
					}
					return;
				case 2:
					throw new Exception(CSimpleThreadPool.b("娞㼫尞껒\u1c2f\ud918楘䴸刀턐䱠剢", a_));
				case 0:
					num = 1;
					break;
				case 1:
					if (A_0 != -1)
					{
						num = 2;
						break;
					}
					goto IL_0047;
				case 5:
					{
						throw new Exception(CSimpleThreadPool.b("\uec20䘩ȏḀ䈆팠⤽㔹", a_));
					}
					IL_0047:
					num = 3;
					break;
				}
			}
		}

		private bool ᜀ(Stream A_0)
		{
			//Discarded unreachable code: IL_010d
			int a_ = 10;
			string text2 = default(string);
			while (true)
			{
				StreamReader streamReader = new StreamReader(A_0);
				string text = string.Empty;
				int num = 8;
				while (true)
				{
					switch (num)
					{
					case 2:
						ᜁ(text);
						text = "";
						num = 9;
						continue;
					case 4:
					case 7:
						num = 1;
						continue;
					case 1:
						if (!ᜄ(text))
						{
							num = 2;
							continue;
						}
						goto case 8;
					case 3:
						num = 5;
						continue;
					case 5:
						if (string.IsNullOrEmpty(text))
						{
							num = 6;
							continue;
						}
						if (true)
						{
						}
						text = text + CSimpleThreadPool.b("䭅䉇", a_) + text2;
						num = 4;
						continue;
					case 0:
						throw new Exception(CSimpleThreadPool.b("Յᭇ᱉쬮렃퐹渹嬌弲䇂뗒", a_));
					case 8:
					case 9:
						text2 = streamReader.ReadLine();
						num = 10;
						continue;
					case 10:
						if (text2 != null)
						{
							num = 3;
							continue;
						}
						streamReader.Close();
						num = 11;
						continue;
					case 11:
						if (text.Length > 0)
						{
							num = 0;
							continue;
						}
						return true;
					case 6:
						text = text2;
						num = 7;
						continue;
					}
					break;
				}
			}
		}

		private string ᜅ(string A_0)
		{
			int a_ = 8;
			return A_0.Replace(CSimpleThreadPool.b("晃摅", a_), CSimpleThreadPool.b("晃", a_));
		}

		private bool ᜄ(string A_0)
		{
			//Discarded unreachable code: IL_0005
			while (true)
			{
				int num = 0;
				bool result = false;
				int num2 = 0;
				int num3 = 0;
				while (true)
				{
					if (true)
					{
					}
					switch (num3)
					{
					case 9:
						num3 = 6;
						continue;
					case 6:
						if (num % 2 == 1)
						{
							num3 = 4;
							continue;
						}
						goto case 7;
					case 2:
						num2++;
						num3 = 5;
						continue;
					case 1:
						if (A_0[num2] == '"')
						{
							num3 = 8;
							continue;
						}
						goto case 2;
					case 4:
						result = true;
						num3 = 7;
						continue;
					case 8:
						num++;
						num3 = 2;
						continue;
					case 0:
					case 5:
						num3 = 3;
						continue;
					case 3:
						num3 = ((num2 < A_0.Length) ? 1 : 9);
						continue;
					case 7:
						return result;
					}
					break;
				}
			}
		}

		private bool ᜃ(string A_0)
		{
			//Discarded unreachable code: IL_0047
			bool result = default(bool);
			while (true)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 7;
				while (true)
				{
					switch (num3)
					{
					case 8:
						result = false;
						num3 = 0;
						continue;
					case 0:
						if (true)
						{
						}
						if (num % 2 == 1)
						{
							num3 = 6;
							continue;
						}
						goto case 1;
					case 5:
					case 7:
						num3 = 2;
						continue;
					case 2:
						num3 = ((num2 < A_0.Length) ? 3 : 8);
						continue;
					case 3:
						if (A_0[num2] == '"')
						{
							num3 = 4;
							continue;
						}
						goto case 8;
					case 6:
						result = true;
						num3 = 1;
						continue;
					case 4:
						num++;
						num2++;
						num3 = 5;
						continue;
					case 1:
						return result;
					}
					break;
				}
			}
		}

		private bool ᜂ(string A_0)
		{
			//Discarded unreachable code: IL_004e
			while (true)
			{
				int num = 0;
				bool result = false;
				int num2 = A_0.Length - 1;
				int num3 = 2;
				while (true)
				{
					switch (num3)
					{
					case 7:
						num3 = 6;
						continue;
					case 6:
						if (true)
						{
						}
						if (num % 2 == 1)
						{
							num3 = 0;
							continue;
						}
						goto case 4;
					case 2:
					case 5:
						num3 = 1;
						continue;
					case 1:
						num3 = ((num2 >= 0) ? 8 : 7);
						continue;
					case 8:
						if (A_0[num2] == '"')
						{
							num3 = 3;
							continue;
						}
						goto case 7;
					case 0:
						result = true;
						num3 = 4;
						continue;
					case 3:
						num++;
						num2--;
						num3 = 5;
						continue;
					case 4:
						return result;
					}
					break;
				}
			}
		}

		private void ᜁ(string A_0)
		{
			//Discarded unreachable code: IL_0129
			int a_ = 4;
			switch (0)
			{
			}
			while (true)
			{
				ArrayList arrayList = new ArrayList();
				string[] array = A_0.Split(',');
				bool flag = false;
				string text = string.Empty;
				int num = 0;
				int num2 = 17;
				while (true)
				{
					switch (num2)
					{
					case 21:
						arrayList.Add(ᜀ(array[num]));
						flag = false;
						num2 = 7;
						continue;
					case 11:
						arrayList.Add(ᜀ(text));
						flag = false;
						num2 = 3;
						continue;
					case 17:
					case 20:
						num2 = 15;
						continue;
					case 15:
						if (num >= array.Length)
						{
							if (true)
							{
							}
							num2 = 14;
						}
						else
						{
							num2 = 1;
						}
						continue;
					case 2:
						text = text + CSimpleThreadPool.b("氿", a_) + array[num];
						num2 = 16;
						continue;
					case 16:
						if (ᜂ(array[num]))
						{
							num2 = 11;
							continue;
						}
						goto case 0;
					case 0:
					case 3:
					case 7:
					case 18:
						num++;
						num2 = 20;
						continue;
					case 13:
						throw new Exception(CSimpleThreadPool.b("ずⰢ砫䤚䄠ꓜ퓓", a_));
					case 5:
						num2 = 9;
						continue;
					case 9:
						if (!ᜄ(array[num]))
						{
							num2 = 21;
							continue;
						}
						goto IL_0221;
					case 6:
						if (!ᜃ(array[num]))
						{
							arrayList.Add(ᜀ(array[num]));
							num2 = 18;
						}
						else
						{
							num2 = 19;
						}
						continue;
					case 14:
						num2 = 10;
						continue;
					case 10:
						if (flag)
						{
							num2 = 13;
							continue;
						}
						this.m_ᜀ.Add(arrayList);
						return;
					case 19:
						num2 = 8;
						continue;
					case 8:
						if (ᜂ(array[num]))
						{
							num2 = 12;
							continue;
						}
						goto IL_0221;
					case 1:
						num2 = (flag ? 2 : 6);
						continue;
					case 12:
						num2 = 4;
						continue;
					case 4:
						{
							if (array[num].Length > 2)
							{
								num2 = 5;
								continue;
							}
							goto IL_0221;
						}
						IL_0221:
						flag = true;
						text = array[num];
						num2 = 0;
						continue;
					}
					break;
				}
			}
		}

		private string ᜀ(string A_0)
		{
			//Discarded unreachable code: IL_00ac
			int a_ = 13;
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((!string.IsNullOrEmpty(A_0)) ? 2 : 7);
					continue;
				case 6:
					if (A_0.Length > 2)
					{
						num = 8;
						continue;
					}
					break;
				case 2:
					if (!ᜃ(A_0))
					{
						num = 6;
						continue;
					}
					if (true)
					{
					}
					num = 5;
					continue;
				case 1:
					A_0 = A_0.Substring(1, A_0.Length - 2).Replace(CSimpleThreadPool.b("歈楊", a_), CSimpleThreadPool.b("歈", a_));
					num = 9;
					continue;
				case 7:
					return string.Empty;
				case 8:
					num = 0;
					continue;
				case 0:
					if (A_0[0] == '"')
					{
						num = 1;
						continue;
					}
					break;
				case 4:
					return A_0.Substring(1, A_0.Length - 2).Replace(CSimpleThreadPool.b("歈楊", a_), CSimpleThreadPool.b("歈", a_));
				case 5:
					num = 10;
					continue;
				case 10:
					if (!ᜂ(A_0))
					{
						throw new Exception(CSimpleThreadPool.b("㤭┩堓렝넵蘾氇\u1ac7", a_) + A_0);
					}
					num = 4;
					continue;
				case 9:
					break;
				}
				break;
			}
			return A_0;
		}
	}
}
