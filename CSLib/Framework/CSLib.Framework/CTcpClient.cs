using System;
using System.Collections.Generic;
using CSLib.Network;
using CSLib.Utility;

namespace CSLib.Framework
{
	public class CTcpClient : CTcpClient
	{
		private bool ᜀ;

		private Dictionary<ushort, CMsgBuffInfoQueue> ᜁ = new Dictionary<ushort, CMsgBuffInfoQueue>();

		public bool UseEchoID
		{
			get
			{
				return ᜀ;
			}
			set
			{
				ᜀ = value;
			}
		}

		public CTcpClient()
			: this()
		{
			ᜁ[0] = new CMsgBuffInfoQueue();
		}

		protected override bool _CbParseMsg(byte[] msgBuff, int msgSize)
		{
			//Discarded unreachable code: IL_011b
			int a_ = 14;
			switch (0)
			{
			}
			ushort num2 = default(ushort);
			while (true)
			{
				byte server = (byte)BitConverter.ToChar(msgBuff, 1);
				byte b = (byte)BitConverter.ToChar(msgBuff, 0);
				ushort id = BitConverter.ToUInt16(msgBuff, 2);
				int num = 3;
				while (true)
				{
					byte[] array;
					switch (num)
					{
					case 3:
						num = ((!ᜀ) ? 7 : 12);
						continue;
					case 9:
						ᝋ.ᜂ(CMessageLabel.b("縠怢䜤眦䠨太帬䨮簰䀲刴᜶\u0338\u1b3aⵞꁬ\ue423甧級菏섥\u242a浌畎煐㵒\u0654㉖⭘ⵚ㡜ⵞ⡠❢䕤婦䥨", a_) + server + CMessageLabel.b("ᨠ\u0322䬤愦尨䔪丬昮田ጲ࠴᜶", a_) + b + CMessageLabel.b("ᨠ\u0322䬤樦䰨堪帬丮嘰嘲簴猶\u1938غᴼ", a_) + id + CMessageLabel.b("ᨠ\u0322䠤否丨砪䐬售吰ጲ࠴᜶", a_) + msgSize);
						num = 5;
						continue;
					case 5:
						if (1 == 0)
						{
						}
						goto IL_02e7;
					case 11:
						num = 2;
						continue;
					case 2:
						if (!CSingleton<CMsgFactory>.Instance.IsIgnoreTrace(server, b, id))
						{
							num = 9;
							continue;
						}
						goto IL_02e7;
					case 4:
					{
						ᝋ.ᜂ(CMessageLabel.b("縠怢䜤眦䠨太帬䨮簰䀲刴᜶\u0338\u1b3aⵞꁬ\ue423甧D⑆ⅈ⑊씡\u202e煐楒畔㥖\u0a58㹚⽜⥞ѠᅢⱤ⍦䥨噪䵬", a_) + server + CMessageLabel.b("ᨠ\u0322䬤愦尨䔪丬昮田ጲ࠴᜶", a_) + b + CMessageLabel.b("ᨠ\u0322䬤樦䰨堪帬丮嘰嘲簴猶\u1938غᴼ", a_) + id + CMessageLabel.b("ᨠ\u0322䠤否丨砪䐬售吰ጲ࠴᜶", a_) + msgSize + CMessageLabel.b("ᨠ\u0322䬤戦䨨䌪䈬昮田ጲ࠴᜶", a_) + num2);
						byte[] array2 = new byte[msgSize];
						Array.Copy(msgBuff, array2, msgSize);
						ᜁ[num2].Enqueue(num2, array2, msgSize);
						num = 8;
						continue;
					}
					case 12:
						num2 = BitConverter.ToUInt16(msgBuff, 4);
						num = 1;
						continue;
					case 1:
						if (ᜁ.ContainsKey(num2))
						{
							num = 4;
							continue;
						}
						ᝋ.ᜁ(CMessageLabel.b("縠怢䜤眦䠨太帬䨮簰䀲刴᜶\u0338\u1b3a\udd59\uea52\ue423甧D⑆ⅈ⑊씡\u202e煐楒畔㥖\u0a58㹚⽜⥞ѠᅢⱤ⍦䥨噪䵬", a_) + server + CMessageLabel.b("ᨠ\u0322䬤愦尨䔪丬昮田ጲ࠴᜶", a_) + b + CMessageLabel.b("ᨠ\u0322䬤樦䰨堪帬丮嘰嘲簴猶\u1938غᴼ", a_) + id + CMessageLabel.b("ᨠ\u0322䠤否丨砪䐬售吰ጲ࠴᜶", a_) + msgSize + CMessageLabel.b("ᨠ\u0322䬤戦䨨䌪䈬昮田ጲ࠴᜶", a_) + num2);
						num = 6;
						continue;
					case 7:
						if (!ᜁ.ContainsKey(b))
						{
							ᝋ.ᜁ(CMessageLabel.b("縠怢䜤眦䠨太帬䨮簰䀲刴᜶\u0338\u1b3a\udd59\uea52\ue423甧級菏섥\u242a浌畎煐㵒\u0654㉖⭘ⵚ㡜ⵞ⡠❢䕤婦䥨", a_) + server + CMessageLabel.b("ᨠ\u0322䬤愦尨䔪丬昮田ጲ࠴᜶", a_) + b + CMessageLabel.b("ᨠ\u0322䬤樦䰨堪帬丮嘰嘲簴猶\u1938غᴼ", a_) + id + CMessageLabel.b("ᨠ\u0322䠤否丨砪䐬售吰ጲ࠴᜶", a_) + msgSize);
							num = 10;
						}
						else
						{
							num = 11;
						}
						continue;
					case 0:
					case 6:
					case 8:
					case 10:
						{
							return true;
						}
						IL_02e7:
						array = new byte[msgSize];
						Array.Copy(msgBuff, array, msgSize);
						ᜁ[0].Enqueue(b, array, msgSize);
						num = 0;
						continue;
					}
					break;
				}
			}
		}

		public void RegisterOwnerID(ushort ownerID)
		{
			//Discarded unreachable code: IL_001f
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 2:
					ᜁ[ownerID] = new CMsgBuffInfoQueue();
					num = 1;
					continue;
				case 1:
					return;
				}
				if (true)
				{
				}
				if (!ᜁ.ContainsKey(ownerID))
				{
					num = 2;
					continue;
				}
				return;
			}
		}

		public CMsgBuffInfoQueue GetMsgBuffInfoQueue(ushort ownerID)
		{
			//Discarded unreachable code: IL_0020
			if (ᜁ.ContainsKey(ownerID))
			{
				return ᜁ[ownerID];
			}
			if (true)
			{
			}
			return null;
		}
	}
}
