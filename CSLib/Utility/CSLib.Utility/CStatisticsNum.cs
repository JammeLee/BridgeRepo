using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSLib.Utility
{
	public class CStatisticsNum<KEY> : CSingleton<CStatisticsNum<KEY>>
	{
		[CompilerGenerated]
		private sealed class ᜀ
		{
			public StreamWriter ᜀ;

			internal void ᜁ(global::ᜌ<KEY> A_0)
			{
				//Discarded unreachable code: IL_000c
				int a_ = 1;
				if (true)
				{
				}
				CDebugOut.Log(string.Concat(A_0.ᜀ, CSimpleThreadPool.b("ᴼԾ慀", a_), A_0.ᜁ));
				ᜀ.WriteLine(string.Concat(A_0.ᜀ, CSimpleThreadPool.b("ᴼԾ慀", a_), A_0.ᜁ));
			}
		}

		private ushort m_ᜀ;

		private Dictionary<KEY, int> ᜁ = new Dictionary<KEY, int>();

		public ushort Limit
		{
			get
			{
				return this.ᜀ;
			}
			set
			{
				this.ᜀ = value;
			}
		}

		public Dictionary<KEY, int> Objects => ᜁ;

		public int TotalNum
		{
			get
			{
				//Discarded unreachable code: IL_0037
				int num = 0;
				using Dictionary<KEY, int>.Enumerator enumerator = ᜁ.GetEnumerator();
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					default:
						if (1 == 0)
						{
						}
						goto case 0;
					case 0:
						num2 = 3;
						break;
					case 3:
						if (enumerator.MoveNext())
						{
							num += enumerator.Current.Value;
							num2 = 0;
						}
						else
						{
							num2 = 2;
						}
						break;
					case 2:
						num2 = 4;
						break;
					case 4:
						return num;
					}
				}
			}
		}

		public bool AddNum(KEY key)
		{
			//Discarded unreachable code: IL_0081
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					num = (ᜁ.ContainsKey(key) ? 1 : 2);
					continue;
				case 3:
					if (Limit <= ᜁ.Count)
					{
						num = 4;
						continue;
					}
					break;
				case 2:
					if (Limit > 0)
					{
						num = 5;
						continue;
					}
					break;
				case 5:
					if (true)
					{
					}
					num = 3;
					continue;
				case 1:
					ᜁ[key]++;
					return true;
				case 4:
					return false;
				}
				break;
			}
			ᜁ.Add(key, 1);
			return true;
		}

		public bool AddNum(KEY key, int num)
		{
			//Discarded unreachable code: IL_00b3
			int num2 = 7;
			while (true)
			{
				switch (num2)
				{
				default:
					num2 = ((num <= 0) ? 2 : 0);
					continue;
				case 6:
					num2 = 3;
					continue;
				case 3:
					if (Limit <= ᜁ.Count)
					{
						num2 = 1;
						continue;
					}
					break;
				case 4:
					ᜁ[key] += num;
					return true;
				case 2:
					return false;
				case 1:
					return false;
				case 0:
					num2 = ((!ᜁ.ContainsKey(key)) ? 5 : 4);
					continue;
				case 5:
					if (true)
					{
					}
					if (Limit > 0)
					{
						num2 = 6;
						continue;
					}
					break;
				}
				break;
			}
			ᜁ.Add(key, num);
			return true;
		}

		public int GetNum(KEY key)
		{
			//Discarded unreachable code: IL_0013
			if (!ᜁ.ContainsKey(key))
			{
				if (true)
				{
				}
				return 0;
			}
			return ᜁ[key];
		}

		public bool SubNum(KEY key)
		{
			//Discarded unreachable code: IL_0011
			if (ᜁ.ContainsKey(key))
			{
				if (true)
				{
				}
				ᜁ[key]--;
				return true;
			}
			return false;
		}

		public bool SubNum(KEY key, int num)
		{
			//Discarded unreachable code: IL_0023
			int num2 = 0;
			while (true)
			{
				switch (num2)
				{
				default:
					if (true)
					{
					}
					num2 = ((num > 0) ? 1 : 3);
					break;
				case 2:
					ᜁ[key] -= num;
					return true;
				case 1:
					if (ᜁ.ContainsKey(key))
					{
						num2 = 2;
						break;
					}
					return false;
				case 3:
					return false;
				}
			}
		}

		public void ClearEmpty()
		{
			//Discarded unreachable code: IL_00a7
			Dictionary<KEY, int>.Enumerator enumerator = ᜁ.GetEnumerator();
			try
			{
				int num = 4;
				KeyValuePair<KEY, int> current = default(KeyValuePair<KEY, int>);
				while (true)
				{
					switch (num)
					{
					case 1:
						if (current.Value == 0)
						{
							num = 6;
							break;
						}
						goto default;
					default:
						num = 3;
						break;
					case 3:
						if (enumerator.MoveNext())
						{
							current = enumerator.Current;
							num = 1;
						}
						else
						{
							num = 5;
						}
						break;
					case 6:
						ᜁ.Remove(current.Key);
						num = 2;
						break;
					case 5:
						num = 0;
						break;
					case 0:
						return;
					}
				}
			}
			finally
			{
				if (true)
				{
				}
				((IDisposable)enumerator).Dispose();
			}
		}

		public void SaveToFile(string fileName)
		{
			//Discarded unreachable code: IL_000c
			int a_ = 11;
			if (true)
			{
			}
			switch (0)
			{
			}
			List<global::ᜌ<KEY>> list = new List<global::ᜌ<KEY>>();
			using (Dictionary<KEY, int>.Enumerator enumerator = ᜁ.GetEnumerator())
			{
				int num = 2;
				while (true)
				{
					switch (num)
					{
					default:
						num = 1;
						continue;
					case 1:
						if (enumerator.MoveNext())
						{
							KeyValuePair<KEY, int> current = enumerator.Current;
							global::ᜌ<KEY> item = new global::ᜌ<KEY>
							{
								ᜀ = current.Key,
								ᜁ = current.Value
							};
							list.Add(item);
							num = 4;
						}
						else
						{
							num = 0;
						}
						continue;
					case 0:
						num = 3;
						continue;
					case 3:
						break;
					}
					break;
				}
			}
			list.Sort((global::ᜌ<KEY> A_0, global::ᜌ<KEY> A_1) => A_0.ᜁ.CompareTo(A_1.ᜁ));
			FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			StreamWriter ᜀ = new StreamWriter(stream, Encoding.GetEncoding(CSimpleThreadPool.b("F\u0b48祊繌繎捐", a_)));
			CDebugOut.Log(CSimpleThreadPool.b("ᑆ⡈㵊⡌\u1b4e㹐ᕒ㱔㭖㱘筚崃鐇", a_));
			list.ForEach(delegate(global::ᜌ<KEY> A_0)
			{
				//Discarded unreachable code: IL_000c
				int a_2 = 1;
				if (true)
				{
				}
				CDebugOut.Log(string.Concat(A_0.ᜀ, CSimpleThreadPool.b("ᴼԾ慀", a_2), A_0.ᜁ));
				ᜀ.WriteLine(string.Concat(A_0.ᜀ, CSimpleThreadPool.b("ᴼԾ慀", a_2), A_0.ᜁ));
			});
			CDebugOut.Log(CSimpleThreadPool.b("ᑆ⡈㵊⡌\u1b4e㹐ᕒ㱔㭖㱘筚踢9", a_));
			ᜀ.Close();
		}
	}
}
