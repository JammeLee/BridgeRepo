using System.Collections.Generic;
using System.Threading;
using CSLib.Framework;
using CSLib.Utility;

internal class ᜅ
{
	private CCollectionContainerListType<_1715> ᜀ;

	private Queue<int> ᜁ;

	private bool m_ᜂ;

	private AutoResetEvent m_ᜃ;

	private AutoResetEvent ᜄ;

	private AutoResetEvent m_ᜅ;

	private AutoResetEvent ᜆ;

	private bool ᜇ = true;

	private CMessageBlock[] ᜈ;

	private bool ᜉ;

	public ᜅ()
	{
		ᜀ = new CCollectionContainerListType<_1715>();
		ᜁ = new Queue<int>();
		this.m_ᜃ = new AutoResetEvent(initialState: true);
		ᜄ = new AutoResetEvent(initialState: false);
		m_ᜅ = new AutoResetEvent(initialState: true);
		ᜆ = new AutoResetEvent(initialState: true);
		ᜈ = new CMessageBlock[5];
	}

	public void ᜂ(object A_0)
	{
		//Discarded unreachable code: IL_0139
		switch (0)
		{
		}
		int num2 = default(int);
		_1715 obj = default(_1715);
		int num3 = default(int);
		int num4 = default(int);
		while (true)
		{
			ᜇ = false;
			int num = 22;
			while (true)
			{
				switch (num)
				{
				case 26:
					num2++;
					num = 4;
					continue;
				case 17:
					m_ᜅ.Set();
					ᜉ = false;
					num2 = 0;
					num = 9;
					continue;
				case 20:
				{
					int i = ᜁ.Dequeue();
					obj = ᜀ[i];
					num = 1;
					continue;
				}
				case 1:
					if (obj != null)
					{
						num = 21;
						continue;
					}
					goto case 3;
				case 25:
					ᜆ.Set();
					num = 19;
					continue;
				case 23:
					if (ᜁ.Count > 0)
					{
						num = 20;
						continue;
					}
					ᜈ[num3] = null;
					num = 3;
					continue;
				case 14:
					CSingleton<_1734>.Instance.ᜁ(ᜈ[num2]);
					num = 26;
					continue;
				case 10:
					ᜄ.WaitOne();
					num = 12;
					continue;
				case 0:
				case 11:
					num = 7;
					continue;
				case 7:
					if (num3 < num4)
					{
						if (true)
						{
						}
						num = 23;
					}
					else
					{
						num = 17;
					}
					continue;
				case 21:
					num = 15;
					continue;
				case 15:
					if (obj.ᜃ() <= 0)
					{
						num = 13;
						continue;
					}
					goto case 8;
				case 16:
				case 19:
					num = 6;
					continue;
				case 6:
					if (ᜁ.Count <= 0)
					{
						num = 10;
						continue;
					}
					ᜉ = true;
					m_ᜅ.WaitOne();
					num3 = 0;
					num = 0;
					continue;
				case 12:
				case 22:
					num = 24;
					continue;
				case 24:
					if (!this.m_ᜂ)
					{
						num = 5;
						continue;
					}
					num4 = ᜈ.Length;
					num = 16;
					continue;
				case 8:
					ᜈ[num3] = obj.ᜈ();
					num = 18;
					continue;
				case 13:
					obj.ᜄ();
					num = 8;
					continue;
				case 4:
				case 9:
					num = 27;
					continue;
				case 27:
					num = ((num2 < num4) ? 2 : 25);
					continue;
				case 2:
					if (ᜈ[num2] != null)
					{
						num = 14;
						continue;
					}
					goto case 26;
				case 3:
				case 18:
					num3++;
					num = 11;
					continue;
				case 5:
					ᜇ = true;
					return;
				}
				break;
			}
		}
	}

	public bool ᜂ(_1715 A_0)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		this.m_ᜃ.WaitOne();
		A_0.ᜃ(this);
		A_0.ᜃ(ᜀ.Count);
		ᜀ.Add(A_0);
		this.m_ᜃ.Set();
		return true;
	}

	public bool ᜃ(int A_0)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		ᜆ.WaitOne();
		this.m_ᜃ.WaitOne();
		ᜀ[A_0] = null;
		this.m_ᜃ.Set();
		return true;
	}

	public void ᜂ(int A_0)
	{
		//Discarded unreachable code: IL_003e
		if (ᜉ)
		{
			m_ᜅ.WaitOne();
			ᜁ.Enqueue(A_0);
			m_ᜅ.Set();
			ᜄ.Set();
			return;
		}
		if (true)
		{
		}
		ᜁ.Enqueue(A_0);
		ᜄ.Set();
	}

	public bool ᜂ()
	{
		return this.m_ᜂ;
	}

	public void ᜂ(bool A_0)
	{
		this.m_ᜂ = A_0;
		ᜄ.Set();
	}

	public bool ᜃ()
	{
		return ᜇ;
	}
}
