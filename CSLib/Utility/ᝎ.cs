using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using CSLib.Utility;

internal class ᝎ : CSingleton<ᝎ>
{
	private class ᜀ
	{
		public ELogLevel ᜀ;

		public string ᜁ;

		public int ᜂ;

		public string ᜃ;

		public object[] ᜄ;
	}

	private ArrayList m_ᜀ = new ArrayList();

	private FileInfo m_ᜁ;

	private FileStream m_ᜂ;

	private long m_ᜃ = 8388608L;

	private EFileLogTime ᜄ = EFileLogTime.HOUR;

	private ᝄ ᜅ = new ᝄ();

	private LocalDataStoreSlot ᜆ;

	private int ᜇ = 10;

	private Timer ᜈ;

	private ᝎ()
	{
		ᜆ = Thread.AllocateDataSlot();
	}

	~ᝎ()
	{
	}

	public bool ᜁ(string A_0)
	{
		return ᜀ(A_0, EFileLogTime.HOUR);
	}

	public bool ᜀ(string A_0, EFileLogTime A_1)
	{
		//Discarded unreachable code: IL_005d
		while (true)
		{
			this.m_ᜁ = new FileInfo(A_0);
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 2:
					if (this.m_ᜁ.Exists)
					{
						num = 1;
						continue;
					}
					this.m_ᜂ = this.m_ᜁ.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
					num = 0;
					continue;
				case 7:
					if (true)
					{
					}
					num = 4;
					continue;
				case 4:
					if (!ᜂ())
					{
						num = 8;
						continue;
					}
					return ᜅ.ᜄ();
				case 0:
				case 3:
					ᜄ = A_1;
					ᜅ.ᜂ(ᜀ);
					num = 6;
					continue;
				case 6:
					if (this.m_ᜁ.Length > 0)
					{
						num = 5;
						continue;
					}
					goto case 7;
				case 1:
					this.m_ᜂ = this.m_ᜁ.Open(FileMode.Open, FileAccess.Write, FileShare.Read);
					num = 3;
					continue;
				case 8:
					return false;
				case 5:
					ᜀ(this.m_ᜁ.LastWriteTime);
					num = 7;
					continue;
				}
				break;
			}
		}
	}

	public bool ᜀ(string A_0, uint A_1)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		this.m_ᜁ = new FileInfo(A_0);
		this.m_ᜂ = (this.m_ᜂ = this.m_ᜁ.Open(FileMode.Append, FileAccess.Write, FileShare.Read));
		this.m_ᜃ = A_1 * 1024;
		ᜅ.ᜂ(ᜁ);
		return ᜅ.ᜄ();
	}

	public void ᜃ()
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		IntPtr a_ = new IntPtr(0);
		IntPtr a_2 = new IntPtr(0);
		ᜅ.ᜃ(0u, a_, a_2);
		ᜅ.ᜈ();
	}

	public void ᜀ(ELogLevel A_0, string A_1, int A_2, string A_3, params object[] A_4)
	{
		//Discarded unreachable code: IL_0224
		switch (0)
		{
		default:
		{
			int num = 0;
			object data = default(object);
			int num2 = default(int);
			_1716 obj2 = default(_1716);
			ᜀ ᜀ = default(ᜀ);
			_1716 value = default(_1716);
			ArrayList obj = default(ArrayList);
			IntPtr a_ = default(IntPtr);
			IntPtr a_2 = default(IntPtr);
			while (true)
			{
				switch (num)
				{
				default:
					if (!ᜅ.ᜅ())
					{
						num = 1;
						break;
					}
					data = Thread.GetData(ᜆ);
					num2 = 0;
					num = 7;
					break;
				case 1:
					return;
				case 16:
					num = ((obj2 != null) ? 2 : 17);
					break;
				case 17:
					return;
				case 4:
					if (ᜀ != null)
					{
						ᜀ.ᜀ = A_0;
						ᜀ.ᜁ = A_1;
						ᜀ.ᜂ = A_2;
						ᜀ.ᜃ = A_3;
						ᜀ.ᜄ = A_4;
						obj2 = (_1716)this.m_ᜀ[num2];
						num = 16;
					}
					else
					{
						num = 13;
					}
					break;
				case 13:
					return;
				case 3:
					try
					{
						num2 = this.m_ᜀ.Add(value);
					}
					finally
					{
						Monitor.Exit(obj);
					}
					Thread.SetData(ᜆ, num2);
					num = 12;
					break;
				case 9:
					return;
				case 7:
					if (data == null)
					{
						num = 10;
						break;
					}
					num2 = (int)data;
					num = 15;
					break;
				case 11:
					if (ᜅ.ᜅ())
					{
						num = 8;
						break;
					}
					return;
				case 12:
				case 15:
					num = 5;
					break;
				case 5:
					if (num2 < this.m_ᜀ.Count)
					{
						ᜀ = new ᜀ();
						num = 4;
					}
					else
					{
						num = 6;
					}
					break;
				case 6:
					return;
				case 2:
					if (obj2.ᜁ(ᜀ))
					{
						a_ = new IntPtr(num2);
						a_2 = new IntPtr(0);
						num = 11;
					}
					else
					{
						num = 9;
					}
					break;
				case 8:
					ᜅ.ᜃ(1u, a_, a_2);
					num = 14;
					break;
				case 14:
					return;
				case 10:
					if (true)
					{
					}
					value = new _1716();
					obj = this.m_ᜀ;
					Monitor.Enter(obj);
					num = 3;
					break;
				}
			}
		}
		}
	}

	private bool ᜂ()
	{
		//Discarded unreachable code: IL_00ca
		switch (0)
		{
		}
		while (true)
		{
			int dueTime = 0;
			int num = 0;
			DateTime now = DateTime.Now;
			EFileLogTime eFileLogTime = ᜄ;
			int num2 = 1;
			while (true)
			{
				switch (num2)
				{
				case 1:
					switch (eFileLogTime)
					{
					default:
						num2 = 2;
						continue;
					case EFileLogTime.SECOND:
						num = 1000;
						dueTime = num - now.Millisecond;
						num2 = 0;
						continue;
					case EFileLogTime.MINUTE:
						num = 60000;
						dueTime = num - (now.Second * 1000 + now.Millisecond);
						num2 = 6;
						continue;
					case EFileLogTime.HOUR:
						break;
					case EFileLogTime.DAY:
						num = 86400000;
						dueTime = num - (now.Hour * 60 * 60 * 1000 + now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond);
						num2 = 4;
						continue;
					}
					goto IL_00d1;
				case 2:
					num2 = 5;
					continue;
				case 5:
					if (1 == 0)
					{
					}
					goto IL_00d1;
				case 0:
				case 3:
				case 4:
				case 6:
					{
						ᜈ = new Timer(ᜀ, this, dueTime, num);
						return true;
					}
					IL_00d1:
					num = 3600000;
					dueTime = num - (now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond);
					num2 = 3;
					continue;
				}
				break;
			}
		}
	}

	private void ᜀ(object A_0)
	{
		//Discarded unreachable code: IL_0017
		while (true)
		{
			if (true)
			{
			}
			IntPtr a_ = new IntPtr(0);
			IntPtr a_2 = new IntPtr(0);
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 2:
					if (ᜅ.ᜅ())
					{
						num = 0;
						continue;
					}
					return;
				case 0:
					ᜅ.ᜃ(2u, a_, a_2);
					num = 1;
					continue;
				case 1:
					return;
				}
				break;
			}
		}
	}

	private bool ᜁ(uint A_0, IntPtr A_1, IntPtr A_2)
	{
		//Discarded unreachable code: IL_004e
		int num = 6;
		while (true)
		{
			switch (num)
			{
			default:
				if (A_0 == 0)
				{
					num = 2;
					continue;
				}
				goto IL_004b;
			case 9:
				return false;
			case 8:
				num = ((A_0 == 1) ? 1 : 9);
				continue;
			case 10:
				num = 5;
				continue;
			case 5:
				if (A_2.ToInt32() == 0)
				{
					num = 4;
					continue;
				}
				goto IL_004b;
			case 0:
				ᜁ();
				num = 7;
				continue;
			case 2:
				num = 3;
				continue;
			case 3:
				if (A_1.ToInt32() == 0)
				{
					num = 10;
					continue;
				}
				goto IL_004b;
			case 1:
				if (this.m_ᜁ.Length >= this.m_ᜃ)
				{
					num = 0;
					continue;
				}
				break;
			case 4:
				return false;
			case 7:
				break;
				IL_004b:
				if (true)
				{
				}
				num = 8;
				continue;
			}
			break;
		}
		int a_ = A_1.ToInt32();
		ᜁ(a_);
		return true;
	}

	private bool ᜀ(uint A_0, IntPtr A_1, IntPtr A_2)
	{
		//Discarded unreachable code: IL_0160
		switch (0)
		{
		default:
		{
			int num = 3;
			DateTime now = default(DateTime);
			TimeSpan t = default(TimeSpan);
			EFileLogTime eFileLogTime = default(EFileLogTime);
			while (true)
			{
				switch (num)
				{
				default:
					if (A_0 == 0)
					{
						num = 10;
						break;
					}
					goto IL_0141;
				case 9:
					return false;
				case 6:
				case 13:
				case 15:
				case 17:
					ᜀ(now - t);
					return true;
				case 2:
					num = 14;
					break;
				case 14:
					if (A_2.ToInt32() == 0)
					{
						num = 9;
						break;
					}
					goto IL_0141;
				case 11:
					return false;
				case 4:
					switch (eFileLogTime)
					{
					case EFileLogTime.MINUTE:
						t = new TimeSpan(0, 0, 1, 0, 0);
						num = 6;
						continue;
					case EFileLogTime.DAY:
						t = new TimeSpan(1, 0, 0, 0, 0);
						num = 13;
						continue;
					case EFileLogTime.HOUR:
						t = new TimeSpan(0, 1, 0, 0, 0);
						num = 15;
						continue;
					default:
						num = 5;
						continue;
					case EFileLogTime.SECOND:
						break;
					}
					goto case 18;
				case 7:
					num = ((A_0 != 2) ? 1 : 12);
					break;
				case 12:
					if (true)
					{
					}
					num = 16;
					break;
				case 1:
				{
					if (A_0 != 1)
					{
						num = 11;
						break;
					}
					int a_ = A_1.ToInt32();
					ᜁ(a_);
					return true;
				}
				case 18:
					t = new TimeSpan(0, 0, 0, 1, 0);
					num = 17;
					break;
				case 16:
					if (this.m_ᜁ.Length > 0)
					{
						now = DateTime.Now;
						eFileLogTime = ᜄ;
						num = 4;
					}
					else
					{
						num = 8;
					}
					break;
				case 10:
					num = 0;
					break;
				case 0:
					if (A_1.ToInt32() == 0)
					{
						num = 2;
						break;
					}
					goto IL_0141;
				case 8:
					return true;
				case 5:
					{
						num = 18;
						break;
					}
					IL_0141:
					num = 7;
					break;
				}
			}
		}
		}
	}

	private string ᜀ(string A_0)
	{
		//Discarded unreachable code: IL_002d
		while (true)
		{
			int num = 0;
			int num2 = 1;
			while (true)
			{
				switch (num2)
				{
				case 1:
					if (1 == 0)
					{
					}
					goto case 4;
				case 5:
					return A_0.Substring(0, num);
				case 3:
					if (A_0[num] != '.')
					{
						num++;
						num2 = 4;
					}
					else
					{
						num2 = 5;
					}
					continue;
				case 4:
					num2 = 2;
					continue;
				case 2:
					num2 = ((num < A_0.Length) ? 3 : 0);
					continue;
				case 0:
					return "";
				}
				break;
			}
		}
	}

	private void ᜀ(DateTime A_0)
	{
		//Discarded unreachable code: IL_019a
		int a_ = 16;
		switch (0)
		{
		}
		while (true)
		{
			string directoryName = this.m_ᜁ.DirectoryName;
			string str = ᜀ(this.m_ᜁ.Name);
			string extension = this.m_ᜁ.Extension;
			string text = directoryName + CSimpleThreadPool.b("။", a_) + str + CSimpleThreadPool.b("ፋ", a_);
			EFileLogTime eFileLogTime = ᜄ;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 0:
					switch (eFileLogTime)
					{
					default:
						num = 2;
						continue;
					case EFileLogTime.SECOND:
						break;
					case EFileLogTime.DAY:
						text += A_0.ToString(CSimpleThreadPool.b("㕋㝍⥏⭑祓᭕ᕗ睙㡛㩝", a_));
						num = 3;
						continue;
					case EFileLogTime.MINUTE:
						if (true)
						{
						}
						text += A_0.ToString(CSimpleThreadPool.b("㕋㝍⥏⭑祓᭕ᕗ睙㡛㩝㽟⩡Ᵽ䭥էݩ", a_));
						num = 4;
						continue;
					case EFileLogTime.HOUR:
						text += A_0.ToString(CSimpleThreadPool.b("㕋㝍⥏⭑祓᭕ᕗ睙㡛㩝㽟⩡Ᵽ", a_));
						num = 1;
						continue;
					}
					goto case 6;
				case 8:
					try
					{
						this.m_ᜂ.Flush();
						this.m_ᜁ.CopyTo(text, overwrite: true);
					}
					catch (Exception)
					{
						return;
					}
					this.m_ᜂ.SetLength(0L);
					this.m_ᜂ.Flush();
					this.m_ᜁ.Refresh();
					num = 5;
					continue;
				case 6:
					text += A_0.ToString(CSimpleThreadPool.b("㕋㝍⥏⭑祓᭕ᕗ睙㡛㩝㽟⩡Ᵽ䭥էݩ䅫ᵭ\u036f", a_));
					num = 7;
					continue;
				case 2:
					num = 6;
					continue;
				case 1:
				case 3:
				case 4:
				case 7:
					text += extension;
					num = 8;
					continue;
				case 5:
					return;
				}
				break;
			}
		}
	}

	private void ᜁ()
	{
		//Discarded unreachable code: IL_001c
		int a_ = 1;
		switch (0)
		{
		}
		string path = default(string);
		int num3 = default(int);
		string destFileName = default(string);
		while (true)
		{
			string directoryName = this.m_ᜁ.DirectoryName;
			string str = ᜀ(this.m_ᜁ.Name);
			string extension = this.m_ᜁ.Extension;
			string str2 = directoryName + CSimpleThreadPool.b("愼", a_) + str + CSimpleThreadPool.b("戼", a_);
			int num = 1;
			int num2 = 13;
			while (true)
			{
				if (true)
				{
				}
				switch (num2)
				{
				case 0:
					path = str2 + num3 + extension;
					num2 = 5;
					continue;
				case 11:
					try
					{
						this.m_ᜂ.Flush();
						this.m_ᜁ.CopyTo(destFileName, overwrite: true);
					}
					catch (Exception)
					{
						return;
					}
					num3 = (num + 1) % ᜇ;
					num2 = 3;
					continue;
				case 10:
				case 13:
					num2 = 8;
					continue;
				case 8:
					num2 = ((num > ᜇ) ? 1 : 12);
					continue;
				case 9:
					num++;
					num2 = 10;
					continue;
				case 3:
					if (num != num3)
					{
						num2 = 0;
						continue;
					}
					goto IL_0168;
				case 2:
					return;
				case 4:
					destFileName = str2 + num + extension;
					num2 = 11;
					continue;
				case 1:
					num2 = 6;
					continue;
				case 6:
					if (num > ᜇ)
					{
						num2 = 7;
						continue;
					}
					goto case 4;
				case 5:
					try
					{
						num2 = 3;
						while (true)
						{
							switch (num2)
							{
							default:
								if (File.Exists(path))
								{
									num2 = 1;
									continue;
								}
								break;
							case 1:
								File.Delete(path);
								num2 = 2;
								continue;
							case 2:
								break;
							case 0:
								goto end_IL_01e3;
							}
							num2 = 0;
						}
						end_IL_01e3:;
					}
					catch (Exception)
					{
					}
					goto IL_0168;
				case 12:
					if (File.Exists(str2 + num + extension))
					{
						num2 = 9;
						continue;
					}
					goto case 1;
				case 7:
					{
						num = 1;
						num2 = 4;
						continue;
					}
					IL_0168:
					this.m_ᜂ.SetLength(0L);
					this.m_ᜂ.Flush();
					this.m_ᜁ.Refresh();
					num2 = 2;
					continue;
				}
				break;
			}
		}
	}

	private void ᜁ(int A_0)
	{
		//Discarded unreachable code: IL_0081
		int a_ = 19;
		switch (0)
		{
		}
		while (true)
		{
			_1716 obj = (_1716)this.m_ᜀ[A_0];
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(DateTime.Now.ToString(CSimpleThreadPool.b("ᑎ⡐⩒ⱔ\u2e56瑘ᙚၜ牞\u0560ݢ䕤⽦Ⅸ兪lɮ䭰rٴ⩶", a_)));
			int length = stringBuilder.Length;
			ᜀ ᜀ = (ᜀ)obj.ᜁ();
			if (true)
			{
			}
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 1:
				case 2:
					num = 0;
					continue;
				case 0:
				{
					if (ᜀ == null)
					{
						num = 3;
						continue;
					}
					stringBuilder.Append(CSimpleThreadPool.b("ᑎ", a_) + ᜀ.ᜀ.ToString() + CSimpleThreadPool.b("\u124e", a_));
					stringBuilder.AppendFormat(ᜀ.ᜃ, ᜀ.ᜄ);
					stringBuilder.Append(CSimpleThreadPool.b("ᑎ", a_) + ᜀ.ᜁ.ToString() + CSimpleThreadPool.b("李", a_) + ᜀ.ᜂ + CSimpleThreadPool.b("晎\u0c50", a_));
					CConsole.WriteLine(stringBuilder.ToString());
					stringBuilder.Append(CSimpleThreadPool.b("䉎子", a_));
					byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
					this.m_ᜂ.Write(bytes, 0, bytes.Length);
					stringBuilder.Remove(length, stringBuilder.Length - length);
					ᜀ = (ᜀ)obj.ᜁ();
					num = 2;
					continue;
				}
				case 3:
					this.m_ᜂ.Flush();
					this.m_ᜁ.Refresh();
					return;
				}
				break;
			}
		}
	}
}
