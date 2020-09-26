using System;
using System.Collections.Generic;
using System.Threading;
using CSLib.Framework;
using CSLib.Utility;

internal class _173D
{
	private object m_ᜀ = new object();

	private bool ᜁ = true;

	private Dictionary<ushort, DMsgExecFunc> m_ᜂ = new Dictionary<ushort, DMsgExecFunc>();

	private Dictionary<uint, DMsgExecFunc> ᜃ = new Dictionary<uint, DMsgExecFunc>();

	private CMsgExecFuncFactory ᜄ;

	internal bool ᜂ()
	{
		return ᜁ;
	}

	internal void ᜀ(bool A_0)
	{
		ᜁ = A_0;
	}

	public bool ᜀ(CMessageLabel A_0, byte[] A_1, int A_2)
	{
		//Discarded unreachable code: IL_001d
		int a_ = 18;
		CStream cStream = new CStream();
		if (!cStream.Write(A_1, 0, A_2))
		{
			if (true)
			{
			}
			ᝋ.ᜁ(CMessageLabel.b("怤弦䰨䠪堬嬮吰縲倴䐶䨸娺娼娾慀ɂ敄絆楈橊⁌㱎㙐R⅔╖㱘㩚ぜ煞㙠ᅢ\u0c64፦౨䍪lᱮᙰㅲtᅶὸ坺嵼佾궀ꎂ\ue884\uf486\uee88\ud88a\ue48c\uf58e\uf490몒", a_));
			return false;
		}
		return ᜀ(A_0, cStream);
	}

	public bool ᜀ(CMessageLabel A_0, CStream A_1)
	{
		//Discarded unreachable code: IL_00ba
		int a_ = 6;
		CMessage msg = default(CMessage);
		while (true)
		{
			ushort sVal = 0;
			ushort sVal2 = 0;
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 1:
					if (!A_1.Read(ref sVal))
					{
						num = 4;
						continue;
					}
					if (true)
					{
					}
					num = 3;
					continue;
				case 0:
					ᝋ.ᜁ(CMessageLabel.b("尘挚砜簞吠圢䀤樦䰨堪帬丮嘰嘲ᔴ甶\u1938ĺᴼ刾㉀⑂敄穆瑈歊⍌㩎㵐㽒", a_));
					return false;
				case 8:
					ᝋ.ᜁ(CMessageLabel.b("尘挚砜簞吠圢䀤樦䰨堪帬丮嘰嘲ᔴ甶\u1938ĺᴼḾⱀ᱂⡄㑆\u2e48\u0d4aⱌⱎ═㱒❔\u2e56睘ᡚ⽜㩞`ᝢd佦ᵨቪᵬ੮嵰卲ᱴ፶啸孺\u0f7c\u1a7e\ue780ꎂ\ue884\uf486\uee88ꊊ권떎놐\ue792\uec94\ue796ﲘ뮚ꂜ뾞", a_) + sVal + CMessageLabel.b("㤘ℚ㴜瘞䔠\u0322ᠤܦ", a_) + sVal2);
					return false;
				case 3:
					if (!A_1.Read(ref sVal2))
					{
						num = 2;
						continue;
					}
					msg = null;
					num = 7;
					continue;
				case 6:
					if (!msg.Deserialize(A_1))
					{
						num = 5;
						continue;
					}
					A_0.MsgObject = msg;
					A_0.UniqueID = msg.UniqueID;
					A_0.ReqIndex = msg.GetReqIndex();
					A_0.ResIndex = msg.GetResIndex();
					return ᜀ(A_0, msg);
				case 4:
					ᝋ.ᜁ(CMessageLabel.b("尘挚砜簞吠圢䀤樦䰨堪帬丮嘰嘲ᔴ甶\u1938ĺᴼḾⱀあ≄ᑆ㵈㥊⡌\u2e4e㱐絒ݔ㉖㡘㽚畜ⵞѠբ䕤፦\u1068᭪\u086c䙮", a_));
					return false;
				case 9:
					if (msg != null)
					{
						msg.UniqueID = CBitHelper.MergeUInt16(sVal, sVal2);
						num = 6;
					}
					else
					{
						num = 0;
					}
					continue;
				case 5:
					ᝋ.ᜁ(CMessageLabel.b("尘挚砜簞吠圢䀤樦䰨堪帬丮嘰嘲ᔴ甶\u1938ĺᴼḾⱀあ≄楆\u0d48\u2e4a㹌⩎⍐㩒㑔㭖じ⅚㡜睞ౠ\u1062ɤ㑦ᵨᥪ\u086c\u0e6eᱰ婲", a_));
					return false;
				case 7:
					num = ((!CSingleton<CMsgFactory>.Instance.CreateMsg(sVal, sVal2, ref msg)) ? 8 : 9);
					continue;
				case 2:
					ᝋ.ᜁ(CMessageLabel.b("尘挚砜簞吠圢䀤樦䰨堪帬丮嘰嘲ᔴ甶\u1938ĺᴼḾⱀあ≄ᑆ㵈㥊⡌\u2e4e㱐絒ݔ㉖㡘㽚畜ⵞѠբ䕤\u0e66൨䉪", a_));
					return false;
				}
				break;
			}
		}
	}

	public bool ᜀ(CMessageLabel A_0, CMessage A_1)
	{
		//Discarded unreachable code: IL_04e6
		int a_ = 10;
		switch (0)
		{
		}
		Dictionary<uint, DMsgExecFunc>.Enumerator enumerator = default(Dictionary<uint, DMsgExecFunc>.Enumerator);
		Dictionary<ushort, DMsgExecFunc>.Enumerator enumerator2 = default(Dictionary<ushort, DMsgExecFunc>.Enumerator);
		DMsgExecFunc msgExecut = default(DMsgExecFunc);
		DMsgExecFunc msgExecut2 = default(DMsgExecFunc);
		while (true)
		{
			byte highUInt = CBitHelper.GetHighUInt8(A_1.MsgType);
			byte lowUInt = CBitHelper.GetLowUInt8(A_1.MsgType);
			int num = 15;
			while (true)
			{
				switch (num)
				{
				case 15:
					num = (ᜃ.ContainsKey(A_1.UniqueID) ? 1 : 5);
					continue;
				case 3:
					try
					{
						num = 2;
						while (true)
						{
							switch (num)
							{
							default:
								num = 1;
								continue;
							case 1:
							{
								if (!enumerator.MoveNext())
								{
									num = 0;
									continue;
								}
								uint key2 = enumerator.Current.Key;
								ushort highUInt3 = CBitHelper.GetHighUInt16(key2);
								ushort lowUInt2 = CBitHelper.GetLowUInt16(key2);
								byte highUInt4 = CBitHelper.GetHighUInt8(highUInt3);
								byte lowUInt3 = CBitHelper.GetLowUInt8(highUInt3);
								ᝋ.ᜁ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀㝂⡄㝆ᩈ\u2e4a㽌㥎㑐⅒畔橖祘", a_) + highUInt4 + CMessageLabel.b("☜㼞唠丢唤愦尨䔪丬༮రጲ", a_) + lowUInt3 + CMessageLabel.b("☜㼞唠丢唤渦洨ପ\u102c༮", a_) + lowUInt2);
								num = 3;
								continue;
							}
							case 0:
								num = 4;
								continue;
							case 4:
								break;
							}
							break;
						}
					}
					finally
					{
						((IDisposable)enumerator).Dispose();
					}
					ᝋ.ᜁ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀漹줊洈ḓ㌛怂쬸ᄲ娵\udd39㠶祘慚絜", a_));
					enumerator2 = this.m_ᜂ.GetEnumerator();
					num = 13;
					continue;
				case 10:
					if (ᜁ)
					{
						num = 14;
						continue;
					}
					goto case 0;
				case 13:
					try
					{
						num = 3;
						while (true)
						{
							switch (num)
							{
							default:
								num = 2;
								continue;
							case 2:
								if (enumerator2.MoveNext())
								{
									ushort key = enumerator2.Current.Key;
									byte highUInt2 = CBitHelper.GetHighUInt8(key);
									ᝋ.ᜁ(string.Concat(str3: CBitHelper.GetLowUInt8(key).ToString(), str0: CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀㝂⡄㝆ᩈ\u2e4a㽌㥎㑐⅒畔橖祘", a_), str1: highUInt2.ToString(), str2: CMessageLabel.b("☜㼞唠丢唤愦尨䔪丬༮రጲ", a_)));
									num = 1;
								}
								else
								{
									num = 4;
								}
								continue;
							case 4:
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
						((IDisposable)enumerator2).Dispose();
					}
					ᝋ.ᜁ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀ꌧ逨․׀쌧∬䎱\ue536저囘苝訦н睜畞䭠䥢佤䵦䍨", a_));
					num = 0;
					continue;
				case 5:
					if (ᜄ != null)
					{
						num = 6;
						continue;
					}
					goto IL_038a;
				case 2:
					msgExecut = null;
					num = 4;
					continue;
				case 4:
					if (ᜄ.Create(A_1.MsgType, ref msgExecut))
					{
						num = 9;
						continue;
					}
					goto IL_01b6;
				case 11:
					num = ((!this.m_ᜂ.ContainsKey(A_1.MsgType)) ? 16 : 12);
					continue;
				case 12:
					ᝋ.ᜂ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀欕畄瑆䴑嬨툞⠬\u1dd8\udb3f㨴睖捘筚⡜\u0c5eѠᅢ፤ɦ᭨䭪偬佮", a_) + highUInt + CMessageLabel.b("☜㼞吠攢値䤦䨨ପ\u102c༮", a_) + lowUInt + CMessageLabel.b("☜㼞吠樢愤ܦᐨପ", a_) + A_1.Id);
					this.m_ᜂ[A_1.MsgType](A_0, A_1);
					return true;
				case 1:
					ᜃ[A_1.UniqueID](A_0, A_1);
					return true;
				case 6:
					msgExecut2 = null;
					num = 8;
					continue;
				case 8:
					if (ᜄ.Create(A_1.MsgType, A_1.Id, ref msgExecut2))
					{
						num = 7;
						continue;
					}
					goto IL_038a;
				case 7:
					if (true)
					{
					}
					ᝋ.ᜂ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀欕畄畆䴑嬨툞⠬\u1dd8\udb3f㨴睖捘筚⡜\u0c5eѠᅢ፤ɦ᭨䭪偬佮", a_) + highUInt + CMessageLabel.b("☜㼞吠攢値䤦䨨ପ\u102c༮", a_) + lowUInt + CMessageLabel.b("☜㼞吠樢愤ܦᐨପ", a_) + A_1.Id);
					ᜃ.Add(A_1.UniqueID, msgExecut2);
					msgExecut2(A_0, A_1);
					return true;
				case 16:
					if (ᜄ != null)
					{
						num = 2;
						continue;
					}
					goto IL_01b6;
				case 9:
					ᝋ.ᜂ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀欕畄獆䴑嬨툞⠬\u1dd8\udb3f㨴睖捘筚⡜\u0c5eѠᅢ፤ɦ᭨䭪偬佮", a_) + highUInt + CMessageLabel.b("☜㼞吠攢値䤦䨨ପ\u102c༮", a_) + lowUInt + CMessageLabel.b("☜㼞吠樢愤ܦᐨପ", a_) + A_1.Id);
					this.m_ᜂ.Add(A_1.MsgType, msgExecut);
					msgExecut(A_0, A_1);
					return true;
				case 14:
					ᝋ.ᜁ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀甧甖䰈\u2bdf鋅Ⱛ윣㸰獒潔睖", a_));
					ᝋ.ᜁ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀㙂ᙄ≆㭈㵊⡌㵎煐湒畔", a_) + highUInt + CMessageLabel.b("☜㼞吠攢値䤦䨨ପ\u102c༮", a_) + lowUInt + CMessageLabel.b("☜㼞吠樢愤ܦᐨପ", a_) + A_1.Id);
					ᝋ.ᜁ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀ꌧ逨․׀쌧∬䎱\ue536저囘苝备逃睜畞䭠䥢佤䵦䍨", a_));
					ᝋ.ᜁ(CMessageLabel.b("堜朞䐠䀢値匦䰨昪䠬尮䈰刲刴制\u1938砺ᴼԾ慀漹䔊洈ḓ㌛怂쬸ᄲ娵\udd39㠶祘慚絜", a_));
					enumerator = ᜃ.GetEnumerator();
					num = 3;
					continue;
				case 0:
					{
						return false;
					}
					IL_038a:
					num = 11;
					continue;
					IL_01b6:
					num = 10;
					continue;
				}
				break;
			}
		}
	}

	internal void ᜀ(byte A_0, byte A_1, ushort A_2)
	{
		//Discarded unreachable code: IL_021f
		int a_ = 6;
		switch (0)
		{
		default:
		{
			ᝋ.ᜁ(CMessageLabel.b("娘嘚渜砞搠嬢䀤䐦尨弪䠬Į田嘲圴䈶常爺匼夾⹀捂罄杆攳䬄朂᠕⤁縜턢\u1734倿팷㈼罞孠䍢", a_));
			Dictionary<uint, DMsgExecFunc>.Enumerator enumerator = ᜃ.GetEnumerator();
			try
			{
				int num = 1;
				while (true)
				{
					switch (num)
					{
					default:
						num = 4;
						continue;
					case 4:
					{
						if (!enumerator.MoveNext())
						{
							num = 2;
							continue;
						}
						uint key = enumerator.Current.Key;
						ushort highUInt = CBitHelper.GetHighUInt16(key);
						ushort lowUInt = CBitHelper.GetLowUInt16(key);
						byte highUInt2 = CBitHelper.GetHighUInt8(highUInt);
						byte lowUInt2 = CBitHelper.GetLowUInt8(highUInt);
						ᝋ.ᜁ(CMessageLabel.b("娘嘚渜砞搠嬢䀤䐦尨弪䠬Į田嘲圴䈶常爺匼夾⹀捂罄杆㵈♊㵌ᱎ㑐⅒⍔㉖⭘筚恜罞", a_) + highUInt2 + CMessageLabel.b("∘㬚検爞儠攢値䤦䨨ପ\u102c༮", a_) + lowUInt2 + CMessageLabel.b("∘㬚検爞儠樢愤ܦᐨପ", a_) + lowUInt);
						num = 3;
						continue;
					}
					case 2:
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
				if (true)
				{
				}
				((IDisposable)enumerator).Dispose();
			}
			ᝋ.ᜁ(CMessageLabel.b("娘嘚渜砞搠嬢䀤䐦尨弪䠬Į田嘲圴䈶常爺匼夾⹀捂罄杆攳위朂᠕⤁縜턢\u1734倿팷㈼罞孠䍢", a_));
			using Dictionary<ushort, DMsgExecFunc>.Enumerator enumerator2 = this.m_ᜂ.GetEnumerator();
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					num = 0;
					break;
				case 0:
				{
					if (!enumerator2.MoveNext())
					{
						num = 1;
						break;
					}
					ushort key2 = enumerator2.Current.Key;
					byte highUInt3 = CBitHelper.GetHighUInt8(key2);
					ᝋ.ᜁ(string.Concat(str3: CBitHelper.GetLowUInt8(key2).ToString(), str0: CMessageLabel.b("娘嘚渜砞搠嬢䀤䐦尨弪䠬Į田嘲圴䈶常爺匼夾⹀捂罄杆㵈♊㵌ᱎ㑐⅒⍔㉖⭘筚恜罞", a_), str1: highUInt3.ToString(), str2: CMessageLabel.b("∘㬚検爞儠攢値䤦䨨ପ\u102c༮", a_)));
					num = 4;
					break;
				}
				case 1:
					num = 3;
					break;
				case 3:
					return;
				}
			}
		}
		}
	}

	public bool ᜀ(ushort A_0, DMsgExecFunc A_1)
	{
		//Discarded unreachable code: IL_0023
		int num = 3;
		while (true)
		{
			switch (num)
			{
			case 0:
				this.m_ᜂ[A_0] = A_1;
				num = 1;
				continue;
			case 1:
			case 2:
				return true;
			}
			if (true)
			{
			}
			if (this.m_ᜂ.ContainsKey(A_0))
			{
				num = 0;
				continue;
			}
			this.m_ᜂ.Add(A_0, A_1);
			num = 2;
		}
	}

	public bool ᜀ(ushort A_0, ushort A_1, DMsgExecFunc A_2)
	{
		//Discarded unreachable code: IL_0023
		while (true)
		{
			uint key = CBitHelper.MergeUInt16(A_0, A_1);
			if (true)
			{
			}
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 0:
					if (ᜃ.ContainsKey(key))
					{
						num = 2;
						continue;
					}
					ᜃ.Add(key, A_2);
					num = 3;
					continue;
				case 2:
					ᜃ[key] = A_2;
					num = 1;
					continue;
				case 1:
				case 3:
					return true;
				}
				break;
			}
		}
	}

	public bool ᜀ(byte A_0, byte A_1, ushort A_2, DMsgExecFunc A_3)
	{
		//Discarded unreachable code: IL_0057
		while (true)
		{
			uint key = CBitHelper.MergeUInt16(CBitHelper.MergeUInt8(A_0, A_1), A_2);
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 0:
					if (ᜃ.ContainsKey(key))
					{
						num = 2;
						continue;
					}
					ᜃ.Add(key, A_3);
					if (true)
					{
					}
					num = 3;
					continue;
				case 2:
					ᜃ[key] = A_3;
					num = 1;
					continue;
				case 1:
				case 3:
					return true;
				}
				break;
			}
		}
	}

	public void ᜀ(ushort A_0, ushort A_1)
	{
		uint key = CBitHelper.MergeUInt16(A_0, A_1);
		ᜃ.Remove(key);
	}

	public void ᜀ(ushort A_0)
	{
		this.m_ᜂ.Remove(A_0);
	}

	public CMsgExecFuncFactory ᜀ()
	{
		return ᜄ;
	}

	public void ᜀ(CMsgExecFuncFactory A_0)
	{
		ᜄ = A_0;
	}
}
internal class _1715
{
	private Queue<CMessageBlock>[] ᜀ;

	private Queue<CMessageBlock> ᜁ;

	private Queue<CMessageBlock> ᜂ;

	private AutoResetEvent m_ᜃ;

	private AutoResetEvent m_ᜄ;

	private ᜅ m_ᜅ;

	private int m_ᜆ = -1;

	private int m_ᜇ;

	private int m_ᜈ = -1;

	private bool m_ᜉ;

	private bool m_ᜊ;

	public _1715()
	{
		ᜀ = new Queue<CMessageBlock>[2];
		ᜀ[0] = new Queue<CMessageBlock>();
		ᜀ[1] = new Queue<CMessageBlock>();
		ᜁ = ᜀ[0];
		ᜂ = ᜀ[1];
		this.m_ᜈ = Thread.CurrentThread.ManagedThreadId;
		this.m_ᜃ = new AutoResetEvent(initialState: true);
		this.m_ᜄ = new AutoResetEvent(initialState: true);
	}

	public void ᜃ(CMessageBlock A_0)
	{
		//Discarded unreachable code: IL_00c1
		int a_ = 9;
		int num = 0;
		int managedThreadId = default(int);
		while (true)
		{
			switch (num)
			{
			default:
				if (this.m_ᜊ)
				{
					num = 1;
					break;
				}
				managedThreadId = Thread.CurrentThread.ManagedThreadId;
				num = 15;
				break;
			case 6:
				CSingleton<CLogInfoList>.Instance.WriteLine(CMessageLabel.b("缛缝丟ȡ䨣䤥尧\u0a29弫䬭帯嘱ᐳ嬵䬷崹᰻䨽⼿扁⁃⽅\u2e47ⱉ⥋㱍㕏㱑⁓癕ⱗ㉙\u2e5b㭝ş١䑣ཥ٧䩩Ὣݭṯᕱᡳ፵塷\u0e79ᑻ౽\ue57f\ue381\ue083ꚅ\ue587黎\ueb8b꺍\ue18f\ue791\uf193\ue395ﶗ몙\uf19b\uf19d쒟잡", a_));
				return;
			case 12:
				this.m_ᜃ.Set();
				return;
			case 14:
				num = 2;
				break;
			case 2:
				if (managedThreadId == this.m_ᜈ)
				{
					num = 13;
					break;
				}
				this.m_ᜃ.WaitOne();
				ᜁ.Enqueue(A_0);
				num = 4;
				break;
			case 5:
			case 7:
				if (true)
				{
				}
				num = 11;
				break;
			case 11:
				if (this.m_ᜅ != null)
				{
					num = 3;
					break;
				}
				return;
			case 15:
				num = ((!this.m_ᜉ) ? 8 : 14);
				break;
			case 8:
				if (managedThreadId != this.m_ᜈ)
				{
					num = 6;
					break;
				}
				ᜁ.Enqueue(A_0);
				num = 5;
				break;
			case 3:
				this.m_ᜅ.ᜂ(this.m_ᜆ);
				num = 9;
				break;
			case 9:
				return;
			case 4:
				if (this.m_ᜅ != null)
				{
					num = 10;
					break;
				}
				goto case 12;
			case 1:
				CSingleton<CLogInfoList>.Instance.WriteLine(CMessageLabel.b("儛笝匟儡䔣䄥䴧\u0a29䔫䀭\u102f䀱儳嬵圷䰹夻ḽ㌿㙁╃㉅㵇㥉", a_));
				return;
			case 10:
				this.m_ᜅ.ᜂ(this.m_ᜆ);
				num = 12;
				break;
			case 13:
				ᜁ.Enqueue(A_0);
				num = 7;
				break;
			}
		}
	}

	public CMessageBlock ᜈ()
	{
		//Discarded unreachable code: IL_0017
		while (true)
		{
			if (true)
			{
			}
			CMessageBlock result = null;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 2:
					if (ᜂ.Count > 0)
					{
						num = 1;
						continue;
					}
					goto case 0;
				case 1:
					result = ᜂ.Dequeue();
					num = 0;
					continue;
				case 0:
					return result;
				}
				break;
			}
		}
	}

	public void ᜃ(ref CMessageBlock A_0)
	{
		A_0 = ᜈ();
	}

	public void ᜅ()
	{
		//Discarded unreachable code: IL_007e
		int num3 = default(int);
		int num2 = default(int);
		while (true)
		{
			this.m_ᜃ.WaitOne();
			int num = 5;
			while (true)
			{
				switch (num)
				{
				case 5:
					if (this.m_ᜅ != null)
					{
						num = 2;
						continue;
					}
					goto case 3;
				case 0:
				case 1:
					num = 4;
					continue;
				case 4:
					if (num3 >= num2)
					{
						num = 3;
						continue;
					}
					this.m_ᜅ.ᜂ(this.m_ᜆ);
					num3++;
					num = 0;
					continue;
				case 2:
					if (true)
					{
					}
					num2 = ᜂ.Count + ᜁ.Count;
					num3 = 0;
					num = 1;
					continue;
				case 3:
					this.m_ᜃ.Set();
					return;
				}
				break;
			}
		}
	}

	public void ᜆ()
	{
		this.m_ᜃ.WaitOne();
	}

	public void ᜇ()
	{
		this.m_ᜃ.Set();
	}

	public int ᜃ()
	{
		return ᜂ.Count;
	}

	public void ᜄ()
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		ᜁ = ᜀ[1 - this.m_ᜇ];
		ᜂ = ᜀ[this.m_ᜇ];
		this.m_ᜇ = 1 - this.m_ᜇ;
	}

	public void ᜌ()
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		this.m_ᜄ.WaitOne();
		this.m_ᜈ = Thread.CurrentThread.ManagedThreadId;
		this.m_ᜄ.Set();
	}

	public ᜅ ᜉ()
	{
		return this.m_ᜅ;
	}

	public void ᜃ(ᜅ A_0)
	{
		this.m_ᜅ = A_0;
	}

	public int ᜊ()
	{
		return this.m_ᜆ;
	}

	public void ᜃ(int A_0)
	{
		this.m_ᜆ = A_0;
	}

	public bool ᜋ()
	{
		return this.m_ᜉ;
	}

	public void ᜃ(bool A_0)
	{
		this.m_ᜉ = A_0;
	}
}
internal delegate void _1754(CMessageBlock A_0);
internal class _1734 : CSingleton<_1734>
{
	private CCollectionContainerListType<ᜅ> ᜀ;

	private CCollectionContainerDictionaryType<uint, CCollectionContainerSetType<_1754>> m_ᜁ;

	private AutoResetEvent m_ᜂ;

	private bool m_ᜃ = true;

	private int m_ᜄ;

	private int m_ᜅ = 1;

	public _1734()
	{
		ᜀ = new CCollectionContainerListType<ᜅ>();
		this.m_ᜁ = new CCollectionContainerDictionaryType<uint, CCollectionContainerSetType<_1754>>();
		this.m_ᜂ = new AutoResetEvent(initialState: true);
	}

	public void ᜁ(int A_0)
	{
		//Discarded unreachable code: IL_003f
		int num2 = default(int);
		while (true)
		{
			this.m_ᜅ = A_0;
			int num = 5;
			while (true)
			{
				switch (num)
				{
				case 5:
					if (this.m_ᜅ <= 0)
					{
						if (true)
						{
						}
						num = 4;
						continue;
					}
					goto case 3;
				case 3:
					num2 = 0;
					num = 1;
					continue;
				case 4:
					this.m_ᜅ = 1;
					num = 3;
					continue;
				case 0:
				case 1:
					num = 2;
					continue;
				case 2:
					if (num2 < this.m_ᜅ)
					{
						ᜅ value = new ᜅ();
						ᜀ.Add(value);
						num2++;
						num = 0;
					}
					else
					{
						num = 6;
					}
					continue;
				case 6:
					return;
				}
				break;
			}
		}
	}

	public void ᜁ()
	{
		//Discarded unreachable code: IL_00a1
		this.m_ᜃ = false;
		IEnumerator<ᜅ> enumerator = ᜀ.GetEnumerator();
		try
		{
			int num = 1;
			ᜅ current = default(ᜅ);
			while (true)
			{
				switch (num)
				{
				case 6:
					if (!current.ᜂ())
					{
						num = 3;
						break;
					}
					goto default;
				default:
					num = 5;
					break;
				case 5:
					if (enumerator.MoveNext())
					{
						current = enumerator.Current;
						num = 6;
					}
					else
					{
						num = 2;
					}
					break;
				case 3:
					current.ᜂ(A_0: true);
					CSingleton<CSimpleThreadPool>.Instance.NewThread(current.ᜂ, null, STAMode: false);
					if (true)
					{
					}
					num = 4;
					break;
				case 2:
					num = 0;
					break;
				case 0:
					return;
				}
			}
		}
		finally
		{
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (enumerator != null)
					{
						num = 2;
						continue;
					}
					break;
				case 2:
					enumerator.Dispose();
					num = 1;
					continue;
				case 1:
					break;
				}
				break;
			}
		}
	}

	public void ᜃ()
	{
		//Discarded unreachable code: IL_0187
		this.m_ᜃ = true;
		IEnumerator<ᜅ> enumerator = ᜀ.GetEnumerator();
		try
		{
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					num = 1;
					continue;
				case 1:
					if (!enumerator.MoveNext())
					{
						num = 2;
						continue;
					}
					enumerator.Current.ᜂ(A_0: false);
					num = 4;
					continue;
				case 2:
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
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					if (enumerator != null)
					{
						num = 0;
						continue;
					}
					break;
				case 0:
					if (true)
					{
					}
					enumerator.Dispose();
					num = 2;
					continue;
				case 2:
					break;
				}
				break;
			}
		}
		enumerator = ᜀ.GetEnumerator();
		try
		{
			int num = 1;
			ᜅ current = default(ᜅ);
			while (true)
			{
				switch (num)
				{
				case 3:
				case 5:
					num = 7;
					break;
				case 7:
					if (current.ᜃ())
					{
						num = 2;
						break;
					}
					Thread.Sleep(100);
					num = 5;
					break;
				default:
					num = 4;
					break;
				case 4:
					if (!enumerator.MoveNext())
					{
						num = 0;
						break;
					}
					current = enumerator.Current;
					num = 3;
					break;
				case 0:
					num = 6;
					break;
				case 6:
					return;
				}
			}
		}
		finally
		{
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (enumerator != null)
					{
						num = 1;
						continue;
					}
					break;
				case 1:
					enumerator.Dispose();
					num = 0;
					continue;
				case 0:
					break;
				}
				break;
			}
		}
	}

	public bool ᜅ()
	{
		return this.m_ᜃ;
	}

	public void ᜂ()
	{
		this.m_ᜂ.WaitOne();
	}

	public void ᜄ()
	{
		this.m_ᜂ.Set();
	}

	public void ᜃ(ushort A_0, ushort A_1, _1754 A_2)
	{
		//Discarded unreachable code: IL_0038
		int a_ = 0;
		int num = 1;
		uint key = default(uint);
		CCollectionContainerSetType<_1754> cCollectionContainerSetType = default(CCollectionContainerSetType<_1754>);
		while (true)
		{
			switch (num)
			{
			default:
				if (!this.m_ᜃ)
				{
					if (true)
					{
					}
					num = 0;
				}
				else
				{
					key = CBitHelper.MergeUInt16(A_0, A_1);
					cCollectionContainerSetType = this.m_ᜁ.Get(key);
					num = 4;
				}
				continue;
			case 2:
				cCollectionContainerSetType = new CCollectionContainerSetType<_1754>();
				this.m_ᜁ.Add(key, cCollectionContainerSetType);
				num = 3;
				continue;
			case 4:
				if (cCollectionContainerSetType == null)
				{
					num = 2;
					continue;
				}
				break;
			case 0:
				CSingleton<CLogInfoList>.Instance.WriteLine(CMessageLabel.b("帒瀔搖樘稚稜稞Ġ猢䨤䠦䔨ପ䐬尮ᄰ愲䀴夶圸刺匼堾浀㍂⥄≆⡈㡊⡌潎≐❒㩔❖祘㥚㡜㥞\u0e60ᅢd䝦᭨\u0e6a੬ٮɰݲ\u1074ն奸ᙺ\u0e7c\u187e", a_));
				return;
			case 3:
				break;
			}
			break;
		}
		cCollectionContainerSetType.Add(A_2);
	}

	public void ᜂ(ushort A_0, ushort A_1, _1754 A_2)
	{
		//Discarded unreachable code: IL_0014
		int a_ = 1;
		if (!this.m_ᜃ)
		{
			if (true)
			{
			}
			CSingleton<CLogInfoList>.Instance.WriteLine(CMessageLabel.b("夓猕欗椙紛礝䔟ȡ琣䤥䜧䘩ఫ䜭䌯ሱ昳䌵嘷吹唻倽✿湁㑃⩅ⵇ⭉㽋⭍灏⅑⁓㥕⡗穙㹛㭝\u065fൡᙣ\u0365䡧ᡩ५७\u196fűs፵\u0a77婹ᅻൽ\ue77f", a_));
		}
		else
		{
			this.m_ᜂ.WaitOne();
			ᜃ(A_0, A_1, A_2);
			this.m_ᜂ.Set();
		}
	}

	public void ᜄ(ushort A_0, ushort A_1, _1754 A_2)
	{
		//Discarded unreachable code: IL_0054
		int a_ = 1;
		int num = 0;
		CCollectionContainerSetType<_1754> cCollectionContainerSetType = default(CCollectionContainerSetType<_1754>);
		while (true)
		{
			switch (num)
			{
			case 3:
				cCollectionContainerSetType.Remove(A_2);
				num = 4;
				continue;
			case 4:
				return;
			case 1:
				if (cCollectionContainerSetType != null)
				{
					num = 3;
					continue;
				}
				return;
			case 2:
				CSingleton<CLogInfoList>.Instance.WriteLine(CMessageLabel.b("夓猕欗椙紛礝䔟ȡ琣䤥䜧䘩ఫ䜭䌯ሱ昳䌵嘷吹唻倽✿湁㑃⩅ⵇ⭉㽋⭍灏⅑⁓㥕⡗穙㹛㭝\u065fൡᙣ\u0365䡧ᡩ५७\u196fűs፵\u0a77婹ᅻൽ\ue77f", a_));
				return;
			}
			if (!this.m_ᜃ)
			{
				num = 2;
				continue;
			}
			if (true)
			{
			}
			uint key = CBitHelper.MergeUInt16(A_0, A_1);
			cCollectionContainerSetType = this.m_ᜁ.Get(key);
			num = 1;
		}
	}

	public void ᜁ(ushort A_0, ushort A_1, _1754 A_2)
	{
		//Discarded unreachable code: IL_0014
		int a_ = 1;
		if (!this.m_ᜃ)
		{
			if (true)
			{
			}
			CSingleton<CLogInfoList>.Instance.WriteLine(CMessageLabel.b("夓猕欗椙紛礝䔟ȡ琣䤥䜧䘩ఫ䜭䌯ሱ昳䌵嘷吹唻倽✿湁㑃⩅ⵇ⭉㽋⭍灏⅑⁓㥕⡗穙㹛㭝\u065fൡᙣ\u0365䡧ᡩ५७\u196fűs፵\u0a77婹ᅻൽ\ue77f", a_));
		}
		else
		{
			this.m_ᜂ.WaitOne();
			ᜄ(A_0, A_1, A_2);
			this.m_ᜂ.Set();
		}
	}

	internal void ᜁ(CMessageBlock A_0)
	{
		//Discarded unreachable code: IL_00bf
		int num = 8;
		List<_1754>.Enumerator enumerator = default(List<_1754>.Enumerator);
		CCollectionContainerSetType<_1754> cCollectionContainerSetType = default(CCollectionContainerSetType<_1754>);
		_1754 current = default(_1754);
		while (true)
		{
			switch (num)
			{
			default:
				if (this.m_ᜃ)
				{
					num = 4;
					break;
				}
				goto IL_005c;
			case 1:
				enumerator = cCollectionContainerSetType.Sets.GetEnumerator();
				num = 5;
				break;
			case 3:
				if (cCollectionContainerSetType != null)
				{
					num = 1;
					break;
				}
				return;
			case 7:
				num = 0;
				break;
			case 0:
				if (A_0.Msg != null)
				{
					num = 6;
					break;
				}
				goto IL_005c;
			case 6:
				return;
			case 4:
				num = 2;
				break;
			case 2:
				if (A_0 != null)
				{
					if (true)
					{
					}
					num = 7;
					break;
				}
				goto IL_005c;
			case 5:
				{
					try
					{
						num = 2;
						while (true)
						{
							switch (num)
							{
							case 4:
								current(A_0);
								num = 0;
								break;
							case 3:
								if (current != null)
								{
									num = 4;
									break;
								}
								goto default;
							default:
								num = 6;
								break;
							case 6:
								if (enumerator.MoveNext())
								{
									current = enumerator.Current;
									num = 3;
								}
								else
								{
									num = 1;
								}
								break;
							case 1:
								num = 5;
								break;
							case 5:
								return;
							}
						}
					}
					finally
					{
						((IDisposable)enumerator).Dispose();
					}
				}
				IL_005c:
				cCollectionContainerSetType = this.m_ᜁ.Get(A_0.Msg.UniqueID);
				num = 3;
				break;
			}
		}
	}

	internal void ᜁ(ᜈ A_0)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		while (true)
		{
			ᜅ ᜅ = ᜀ[this.m_ᜄ++ % this.m_ᜅ];
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 0:
					if (ᜅ != null)
					{
						num = 2;
						continue;
					}
					return;
				case 2:
					ᜅ.ᜂ(A_0.ᜂ());
					num = 1;
					continue;
				case 1:
					return;
				}
				break;
			}
		}
	}
}
