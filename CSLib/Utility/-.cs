using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CSLib.Utility;

internal class _1752
{
	private string ᜀ = "";

	private string m_ᜁ = "";

	private string m_ᜂ = "";

	private StreamWriter ᜃ;

	private int ᜄ;

	private int ᜅ = 100000;

	public _1752(string A_0, string A_1, string A_2)
	{
		ᜀ = A_0;
		this.m_ᜁ = A_1;
		this.m_ᜂ = A_2;
	}

	public void ᜁ(int A_0)
	{
		ᜅ = A_0;
	}

	private bool ᜁ()
	{
		//Discarded unreachable code: IL_0041
		int a_ = 6;
		switch (0)
		{
		}
		bool result = default(bool);
		string text = default(string);
		while (true)
		{
			ᜂ();
			DirectoryInfo directoryInfo = new DirectoryInfo(ᜀ);
			if (true)
			{
			}
			int num = 1;
			while (true)
			{
				string text2;
				switch (num)
				{
				case 1:
					if (!directoryInfo.Exists)
					{
						num = 0;
						continue;
					}
					goto IL_0065;
				case 0:
					try
					{
						directoryInfo.Create();
					}
					catch (IOException value2)
					{
						CConsole.WriteLine(value2);
					}
					goto IL_0065;
				case 2:
					{
						try
						{
							while (true)
							{
								FileStream fileStream = File.Open(text, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
								num = 1;
								while (true)
								{
									switch (num)
									{
									case 1:
										if (fileStream == null)
										{
											num = 3;
											continue;
										}
										ᜃ = new StreamWriter(fileStream);
										CConsole.WriteLine(CSimpleThreadPool.b("ŁC⍅⩇㽉⭋ࡍ㥏㹑ㅓ癕扗穙", a_) + text);
										num = 0;
										continue;
									case 3:
										result = false;
										num = 2;
										continue;
									case 0:
										goto end_IL_0163;
									case 2:
										return result;
									}
									break;
								}
							}
							end_IL_0163:;
						}
						catch (Exception value)
						{
							ᜃ = null;
							CConsole.WriteLine(value);
						}
						ᜄ = 0;
						return true;
					}
					IL_0065:
					text2 = DateTime.Now.ToString(CSimpleThreadPool.b("㭁㵃歅Շ\u0749態⩍㑏牑᱓ṕ扗㝙ㅛ摝\u135fᅡ", a_));
					text2 = text2.Replace(CSimpleThreadPool.b("潁", a_), "");
					text2 = text2.Replace(CSimpleThreadPool.b("扁", a_), CSimpleThreadPool.b("ᵁ", a_));
					text2 = text2.Replace(CSimpleThreadPool.b("硁", a_), "");
					text = this.m_ᜁ + CSimpleThreadPool.b("ᵁ", a_) + text2 + CSimpleThreadPool.b("ᵁ", a_) + this.m_ᜂ;
					text = ᜀ + text + CSimpleThreadPool.b("汁ࡃ⥅⽇", a_);
					num = 2;
					continue;
				}
				break;
			}
		}
	}

	public void ᜁ(string A_0)
	{
		//Discarded unreachable code: IL_009f
		int a_ = 5;
		int num = 4;
		while (true)
		{
			switch (num)
			{
			default:
				if (ᜃ == null)
				{
					num = 2;
					break;
				}
				goto IL_010d;
			case 7:
				try
				{
					ᜃ.WriteLine(DateTime.Now.ToString(CSimpleThreadPool.b("㡀㩂㱄㹆摈يL扎㕐㝒畔ὖᅘ慚ぜ㉞孠\u1062ᙤ", a_)) + CSimpleThreadPool.b("慀", a_) + A_0);
					ᜃ.Flush();
				}
				catch (Exception value)
				{
					CConsole.WriteLine(value);
				}
				ᜄ++;
				num = 5;
				break;
			case 6:
				if (1 == 0)
				{
				}
				return;
			case 0:
				ᜁ();
				num = 3;
				break;
			case 3:
				return;
			case 2:
				num = 1;
				break;
			case 1:
				if (!ᜁ())
				{
					num = 6;
					break;
				}
				goto IL_010d;
			case 5:
				{
					if (ᜄ >= ᜅ)
					{
						num = 0;
						break;
					}
					return;
				}
				IL_010d:
				num = 7;
				break;
			}
		}
	}

	public void ᜂ()
	{
		if (ᜃ != null)
		{
			ᜃ.Close();
		}
	}
}
internal class _1713<ᜀ, ᜁ, ᜂ>
{
	public delegate void ᜀ(ᜀ A_0, ᜁ A_1, ᜂ A_2);

	private CDictionary<ᜀ, CDictionary<ᜁ, ᜂ>> m_ᜀ = new CDictionary<ᜀ, CDictionary<ᜁ, ᜂ>>();

	public void ᜀ(ᜀ A_0, ᜁ A_1, ᜂ A_2)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		CDictionary<ᜁ, ᜂ> @object = this.ᜀ.GetObject(A_0);
		if (@object == null)
		{
			this.ᜀ.NewObject(A_0);
		}
		@object.AddObject(A_1, A_2);
	}

	public void ᜁ(ᜀ A_0, ᜁ A_1)
	{
		//Discarded unreachable code: IL_0034
		while (true)
		{
			CDictionary<ᜁ, ᜂ> @object = this.ᜀ.GetObject(A_0);
			int num = 3;
			while (true)
			{
				switch (num)
				{
				case 3:
					if (true)
					{
					}
					if (@object == null)
					{
						num = 0;
						continue;
					}
					@object.DelObject(A_1);
					num = 4;
					continue;
				case 0:
					return;
				case 2:
					this.ᜀ.DelObject(A_0);
					num = 1;
					continue;
				case 1:
					return;
				case 4:
					if (@object.Objects.Count <= 0)
					{
						num = 2;
						continue;
					}
					return;
				}
				break;
			}
		}
	}

	public ᜂ ᜀ(ᜀ A_0, ᜁ A_1)
	{
		//Discarded unreachable code: IL_0067
		ᜂ object2 = default(ᜂ);
		while (true)
		{
			CDictionary<ᜁ, ᜂ> @object = this.ᜀ.GetObject(A_0);
			int num = 3;
			while (true)
			{
				switch (num)
				{
				case 3:
					if (@object == null)
					{
						num = 2;
						continue;
					}
					object2 = @object.GetObject(A_1);
					num = 0;
					continue;
				case 0:
					if (object2 == null)
					{
						num = 1;
						continue;
					}
					return object2;
				case 1:
					if (true)
					{
					}
					return default(ᜂ);
				case 2:
					return default(ᜂ);
				}
				break;
			}
		}
	}

	public void ᜀ(global::_1713<ᜀ, ᜁ, ᜂ>.ᜀ A_0)
	{
		//Discarded unreachable code: IL_01a6
		switch (0)
		{
		default:
		{
			int num = 0;
			Dictionary<ᜀ, CDictionary<ᜁ, ᜂ>>.Enumerator enumerator2 = default(Dictionary<ᜀ, CDictionary<ᜁ, ᜂ>>.Enumerator);
			KeyValuePair<ᜀ, CDictionary<ᜁ, ᜂ>> current = default(KeyValuePair<ᜀ, CDictionary<ᜁ, ᜂ>>);
			Dictionary<ᜁ, ᜂ>.Enumerator enumerator = default(Dictionary<ᜁ, ᜂ>.Enumerator);
			while (true)
			{
				switch (num)
				{
				default:
					if (A_0 == null)
					{
						num = 1;
						break;
					}
					enumerator2 = this.ᜀ.Objects.GetEnumerator();
					num = 2;
					break;
				case 1:
					return;
				case 2:
					if (true)
					{
					}
					try
					{
						num = 1;
						while (true)
						{
							switch (num)
							{
							case 4:
								if (current.Value != null)
								{
									num = 6;
									break;
								}
								goto default;
							default:
								num = 3;
								break;
							case 3:
								if (enumerator2.MoveNext())
								{
									current = enumerator2.Current;
									num = 4;
								}
								else
								{
									num = 5;
								}
								break;
							case 0:
								try
								{
									num = 1;
									while (true)
									{
										switch (num)
										{
										default:
											num = 2;
											continue;
										case 2:
										{
											if (!enumerator.MoveNext())
											{
												num = 3;
												continue;
											}
											KeyValuePair<ᜁ, ᜂ> current2 = enumerator.Current;
											A_0(current.Key, current2.Key, current2.Value);
											num = 4;
											continue;
										}
										case 3:
											num = 0;
											continue;
										case 0:
											break;
										}
										break;
									}
								}
								finally
								{
									((IDisposable)enumerator).Dispose();
								}
								goto default;
							case 6:
								enumerator = current.Value.Objects.GetEnumerator();
								num = 0;
								break;
							case 5:
								num = 2;
								break;
							case 2:
								return;
							}
						}
					}
					finally
					{
						((IDisposable)enumerator2).Dispose();
					}
				}
			}
		}
		}
	}
}
internal class _1755
{
	private _1755 ᜀ;

	private _1755 ᜁ;

	public _1755 ᜅ(_1755 A_0)
	{
		//Discarded unreachable code: IL_0017
		while (true)
		{
			if (true)
			{
			}
			A_0.ᜁ = this;
			A_0.ᜀ = ᜀ;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 2:
					if (ᜀ != null)
					{
						num = 1;
						continue;
					}
					goto case 0;
				case 1:
					ᜀ.ᜁ = A_0;
					num = 0;
					continue;
				case 0:
					ᜀ = A_0;
					return this;
				}
				break;
			}
		}
	}

	public _1755 ᜇ(_1755 A_0)
	{
		//Discarded unreachable code: IL_0017
		while (true)
		{
			if (true)
			{
			}
			A_0.ᜀ = this;
			A_0.ᜁ = ᜁ;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 0:
					if (ᜁ != null)
					{
						num = 1;
						continue;
					}
					goto case 2;
				case 1:
					ᜁ.ᜀ = A_0;
					num = 2;
					continue;
				case 2:
					ᜁ = A_0;
					return this;
				}
				break;
			}
		}
	}

	public _1755 ᜂ(_1755 A_0)
	{
		//Discarded unreachable code: IL_0050
		while (true)
		{
			ᜀ = A_0;
			ᜁ = A_0.ᜁ;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 2:
					if (ᜁ != null)
					{
						num = 1;
						continue;
					}
					goto case 0;
				case 1:
					ᜁ.ᜀ = this;
					if (true)
					{
					}
					num = 0;
					continue;
				case 0:
					A_0.ᜁ = this;
					return this;
				}
				break;
			}
		}
	}

	public _1755 ᜉ(_1755 A_0)
	{
		//Discarded unreachable code: IL_0058
		while (true)
		{
			ᜁ = A_0;
			ᜀ = A_0.ᜀ;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 0:
					if (ᜀ != null)
					{
						num = 1;
						continue;
					}
					goto IL_005f;
				case 1:
					ᜀ.ᜁ = this;
					num = 2;
					continue;
				case 2:
					{
						if (1 == 0)
						{
						}
						goto IL_005f;
					}
					IL_005f:
					A_0.ᜀ = this;
					return this;
				}
				break;
			}
		}
	}

	public _1755 ᜄ(_1755 A_0)
	{
		//Discarded unreachable code: IL_003d
		while (true)
		{
			ᜀ = A_0.ᜀ;
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 1:
					if (ᜀ != null)
					{
						num = 2;
						continue;
					}
					goto case 0;
				case 2:
					if (true)
					{
					}
					ᜀ.ᜁ = this;
					num = 0;
					continue;
				case 0:
					return this;
				}
				break;
			}
		}
	}

	public _1755 ᜈ(_1755 A_0)
	{
		//Discarded unreachable code: IL_0017
		while (true)
		{
			if (true)
			{
			}
			ᜁ = A_0.ᜁ;
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 1:
					if (ᜁ != null)
					{
						num = 0;
						continue;
					}
					goto case 2;
				case 0:
					ᜁ.ᜀ = this;
					num = 2;
					continue;
				case 2:
					return this;
				}
				break;
			}
		}
	}

	public _1755 ᜂ()
	{
		//Discarded unreachable code: IL_0033
		int num = 4;
		while (true)
		{
			switch (num)
			{
			default:
				if (ᜁ != null)
				{
					if (true)
					{
					}
					num = 2;
					continue;
				}
				goto case 5;
			case 1:
				ᜀ.ᜁ = ᜁ;
				num = 0;
				continue;
			case 5:
				num = 3;
				continue;
			case 3:
				if (ᜀ != null)
				{
					num = 1;
					continue;
				}
				break;
			case 2:
				ᜁ.ᜀ = ᜀ;
				num = 5;
				continue;
			case 0:
				break;
			}
			break;
		}
		return this;
	}

	public _1755 ᜃ()
	{
		return ᜀ;
	}

	public void ᜆ(_1755 A_0)
	{
		ᜀ = A_0;
	}

	public _1755 ᜄ()
	{
		return ᜁ;
	}

	public void ᜃ(_1755 A_0)
	{
		ᜁ = A_0;
	}
}
internal class _1716
{
	private class ᜀ : ᜃ
	{
		public object ᜁ;
	}

	private ᜀ m_ᜀ;

	private ᜀ m_ᜁ;

	private int m_ᜂ;

	public _1716()
	{
		this.m_ᜀ = new ᜀ();
		this.m_ᜁ = this.m_ᜀ;
	}

	public object ᜁ()
	{
		//Discarded unreachable code: IL_0010
		if (this.m_ᜀ.ᜂ() == null)
		{
			if (true)
			{
			}
			return null;
		}
		this.m_ᜂ--;
		object result = this.m_ᜀ.ᜁ;
		this.m_ᜀ = (ᜀ)this.m_ᜀ.ᜂ();
		return result;
	}

	public bool ᜁ(object A_0)
	{
		//Discarded unreachable code: IL_000c
		ᜀ ᜀ = new ᜀ();
		if (ᜀ == null)
		{
			if (true)
			{
			}
			return false;
		}
		this.m_ᜁ.ᜁ = A_0;
		this.m_ᜁ.ᜄ(ᜀ);
		this.m_ᜁ = ᜀ;
		this.m_ᜂ++;
		return true;
	}

	public object ᜄ()
	{
		if (this.m_ᜀ.ᜂ() == null)
		{
			return null;
		}
		return this.m_ᜀ.ᜁ;
	}

	public void ᜂ()
	{
		this.m_ᜀ = this.m_ᜁ;
		this.m_ᜂ = 0;
	}

	public int ᜃ()
	{
		return this.m_ᜂ;
	}
}
[CompilerGenerated]
internal sealed class _171C
{
	[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
	private struct ᜀ
	{
	}

	internal static readonly ᜀ ᜀ/* Not supported: data(3C 00 3E 00 2F 00 22 00 7C 00 3A 00) */;
}
