using System;
using System.Collections.Generic;
using System.Threading;
using CSLib.Utility;

namespace CSLib.Framework
{
	public class CMsgFactory : CSingleton<CMsgFactory>
	{
		private object m_ᜁ = new object();

		private Dictionary<uint, CMessage> m_ᜂ = new Dictionary<uint, CMessage>();

		private Dictionary<uint, DMsgCreater> m_ᜃ = new Dictionary<uint, DMsgCreater>();

		private Dictionary<ushort, DMsgCreater> ᜄ = new Dictionary<ushort, DMsgCreater>();

		private DMsgCreater ᜅ;

		private static Dictionary<uint, object> ᜆ = new Dictionary<uint, object>();

		public DMsgCreater MsgCreater
		{
			get
			{
				return ᜅ;
			}
			set
			{
				ᜅ = value;
			}
		}

		protected CMsgFactory()
		{
		}

		public CMessageLabel CreateMsg(byte[] msgBuff, int msgSize)
		{
			//Discarded unreachable code: IL_0012
			lock (this.m_ᜁ)
			{
				if (true)
				{
				}
				return _CreateMsg(msgBuff, msgSize);
			}
		}

		public CMessageLabel _CreateMsg(byte[] msgBuff, int msgSize)
		{
			//Discarded unreachable code: IL_0114
			switch (0)
			{
			}
			ushort sVal2 = default(ushort);
			ushort sVal = default(ushort);
			CMessage A_ = default(CMessage);
			while (true)
			{
				CStream cStream = new CStream();
				int num = 8;
				while (true)
				{
					switch (num)
					{
					case 8:
						if (!cStream.Write(msgBuff, 0, msgSize))
						{
							num = 3;
							continue;
						}
						sVal2 = 0;
						sVal = 0;
						num = 9;
						continue;
					case 6:
						if (A_ == null)
						{
							num = 10;
							continue;
						}
						A_.UniqueID = CBitHelper.MergeUInt16(sVal2, sVal);
						num = 11;
						continue;
					case 5:
						num = ((!ᜁ(sVal2, sVal, ref A_)) ? 4 : 6);
						continue;
					case 9:
						num = ((!cStream.Read(ref sVal2)) ? 7 : 0);
						continue;
					case 11:
						if (!A_.Deserialize(cStream))
						{
							num = 2;
							continue;
						}
						return new CMessageLabel
						{
							MsgObject = A_,
							UniqueID = A_.UniqueID,
							ReqIndex = A_.GetReqIndex(),
							ResIndex = A_.GetResIndex()
						};
					case 3:
						return null;
					case 10:
						return null;
					case 2:
						return null;
					case 1:
						return null;
					case 7:
						if (true)
						{
						}
						return null;
					case 4:
						return null;
					case 0:
						if (cStream.Read(ref sVal))
						{
							A_ = null;
							num = 5;
						}
						else
						{
							num = 1;
						}
						continue;
					}
					break;
				}
			}
		}

		public bool CreateMsg(ushort type, ushort id, ref CMessage msg)
		{
			//Discarded unreachable code: IL_0025
			bool result;
			lock (this.m_ᜁ)
			{
				result = ᜁ(type, id, ref msg);
			}
			if (true)
			{
			}
			return result;
		}

		private bool ᜁ(ushort A_0, ushort A_1, ref CMessage A_2)
		{
			//Discarded unreachable code: IL_00f8
			while (true)
			{
				uint key = CBitHelper.MergeUInt16(A_0, 16) + A_1;
				int num = 10;
				while (true)
				{
					switch (num)
					{
					case 10:
						num = ((!this.m_ᜂ.ContainsKey(key)) ? 7 : 2);
						continue;
					case 5:
						return true;
					case 11:
						if (ᜄ.ContainsKey(A_0))
						{
							num = 9;
							continue;
						}
						goto IL_00b1;
					case 7:
						if (this.m_ᜃ.ContainsKey(key))
						{
							num = 1;
							continue;
						}
						goto IL_0065;
					case 4:
						if (ᜅ == null)
						{
							num = 6;
							continue;
						}
						return ᜅ(A_0, A_1, ref A_2);
					case 2:
					{
						CMessage cMessage = this.m_ᜂ[key];
						A_2 = (CMessage)Activator.CreateInstance(cMessage.GetType());
						return true;
					}
					case 9:
						if (true)
						{
						}
						num = 3;
						continue;
					case 3:
						if (ᜄ[A_0](A_0, A_1, ref A_2))
						{
							num = 5;
							continue;
						}
						goto IL_00b1;
					case 6:
						return false;
					case 0:
						return true;
					case 1:
						num = 8;
						continue;
					case 8:
						{
							if (this.m_ᜃ[key](A_0, A_1, ref A_2))
							{
								num = 0;
								continue;
							}
							goto IL_0065;
						}
						IL_0065:
						num = 11;
						continue;
						IL_00b1:
						num = 4;
						continue;
					}
					break;
				}
			}
		}

		public bool AddMsgObject(ushort type, ushort id, CMessage msgObject)
		{
			//Discarded unreachable code: IL_0012
			lock (this.m_ᜁ)
			{
				if (true)
				{
				}
				return ᜁ(type, id, msgObject);
			}
		}

		private bool ᜁ(ushort A_0, ushort A_1, CMessage A_2)
		{
			//Discarded unreachable code: IL_0090
			int a_ = 4;
			while (true)
			{
				uint key = CBitHelper.MergeUInt16(A_0, 16) + A_1;
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (this.m_ᜂ.ContainsKey(key))
						{
							num = 1;
							continue;
						}
						ᝋ.ᜂ(CMessageLabel.b("䠖堘缚礜刞删䐢樤䔦䌨个丬嬮ᄰलᔴ❔Ꙫ쁗鵮Ἶ筀捂ㅄ㹆㥈\u2e4a浌牎煐", a_) + A_0 + CMessageLabel.b("Ⱆ㤘爚礜㼞ᰠ\u0322", a_) + A_1);
						this.m_ᜂ.Add(key, A_2);
						if (true)
						{
						}
						num = 2;
						continue;
					case 1:
						ᝋ.ᜀ(CMessageLabel.b("䠖堘缚礜刞删䐢樤䔦䌨个丬嬮ᄰलᔴ놿\uef4e쁗鵮Ἶ筀捂ㅄ㹆㥈\u2e4a浌牎煐", a_) + A_0 + CMessageLabel.b("Ⱆ㤘爚礜㼞ᰠ\u0322", a_) + A_1);
						this.m_ᜂ[key] = A_2;
						num = 3;
						continue;
					case 2:
					case 3:
						return true;
					}
					break;
				}
			}
		}

		public void DelMsgObject(ushort type, ushort id)
		{
			//Discarded unreachable code: IL_0012
			lock (this.m_ᜁ)
			{
				if (true)
				{
				}
				ᜂ(type, id);
			}
		}

		private void ᜂ(ushort A_0, ushort A_1)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			uint key = CBitHelper.MergeUInt16(A_0, 16) + A_1;
			this.m_ᜂ.Remove(key);
		}

		public bool AddMsgObject(byte server, byte func, ushort id, CMessage msgObject)
		{
			//Discarded unreachable code: IL_0012
			lock (this.m_ᜁ)
			{
				if (true)
				{
				}
				return ᜁ(server, func, id, msgObject);
			}
		}

		private bool ᜁ(byte A_0, byte A_1, ushort A_2, CMessage A_3)
		{
			//Discarded unreachable code: IL_0083
			int a_ = 9;
			switch (0)
			{
			default:
			{
				int num = 9;
				byte highUInt = default(byte);
				byte lowUInt = default(byte);
				ushort id = default(ushort);
				uint key = default(uint);
				while (true)
				{
					switch (num)
					{
					default:
						if (A_3 == null)
						{
							num = 4;
							break;
						}
						highUInt = CBitHelper.GetHighUInt8(A_3.MsgType);
						lowUInt = CBitHelper.GetLowUInt8(A_3.MsgType);
						id = A_3.Id;
						num = 3;
						break;
					case 1:
						if (true)
						{
						}
						if (this.m_ᜂ.ContainsKey(key))
						{
							num = 2;
							break;
						}
						ᝋ.ᜂ(CMessageLabel.b("䌛弝䐟䘡椣唥伧攩丫䐭唯儱䀳ᘵȷᨹⱙꅯ뭒\ue213摃籅桇㥉⥋㱍♏㝑♓癕敗穙", a_) + A_0 + CMessageLabel.b("✛㸝䘟圡䨣䔥\u0827ᜩఫ", a_) + A_1 + CMessageLabel.b("✛㸝䤟䘡Уᬥ\u0827", a_) + A_2);
						this.m_ᜂ.Add(key, A_3);
						num = 8;
						break;
					case 6:
						ᝋ.ᜁ(CMessageLabel.b("䌛弝䐟䘡椣唥伧攩丫䐭唯儱䀳ᘵȷᨹ䰻弽㈿⍁⥃E㵇⑉⽋湍煏潑瑓㥕㩗す\u1a5b⭝\u0e5fš䑣履䡧\u1a69൫ᱭᅯά\u2073\u0f75\u0877ό屻䍽ꁿ", a_) + A_0 + CMessageLabel.b("✛㸝借䌡嘣䜥䔧氩夫䀭匯ሱळᘵ", a_) + A_1 + CMessageLabel.b("✛㸝借䌡嘣䜥䔧挩栫อയሱ", a_) + A_2 + CMessageLabel.b("✛㸝伟䀡丣甥䴧堩娫䬭䈯ሱळᘵ", a_) + highUInt + CMessageLabel.b("✛㸝伟䀡丣急崧䐩伫อയሱ", a_) + lowUInt + CMessageLabel.b("✛㸝伟䀡丣漥氧\u0a29ᄫอ", a_) + id);
						return false;
					case 3:
						num = ((A_0 == highUInt) ? 7 : 5);
						break;
					case 4:
						return false;
					case 2:
						ᝋ.ᜀ(CMessageLabel.b("䌛弝䐟䘡椣唥伧攩丫䐭唯儱䀳ᘵȷᨹ몲\ue84b뭒\ue213摃籅桇㥉⥋㱍♏㝑♓癕敗穙", a_) + A_0 + CMessageLabel.b("✛㸝䘟圡䨣䔥\u0827ᜩఫ", a_) + A_1 + CMessageLabel.b("✛㸝䤟䘡Уᬥ\u0827", a_) + A_2);
						this.m_ᜂ[key] = A_3;
						num = 0;
						break;
					case 7:
						if (A_1 == lowUInt)
						{
							key = CBitHelper.MergeUInt16(CBitHelper.MergeUInt8(A_0, A_1), 16) + A_2;
							num = 1;
						}
						else
						{
							num = 6;
						}
						break;
					case 5:
						ᝋ.ᜁ(CMessageLabel.b("䌛弝䐟䘡椣唥伧攩丫䐭唯儱䀳ᘵȷᨹ䰻弽㈿⍁⥃ᕅⵇ㡉㩋⭍≏牑畓歕硗㕙㹛㑝㍟ݡᙣၥ൧ᡩ䱫呭偯ɱᕳѵ\u1977\u1779⡻ݽ\uf07f\ue781ꒃ뮅ꢇ", a_) + A_0 + CMessageLabel.b("✛㸝借䌡嘣䜥䔧氩夫䀭匯ሱळᘵ", a_) + A_1 + CMessageLabel.b("✛㸝借䌡嘣䜥䔧挩栫อയሱ", a_) + A_2 + CMessageLabel.b("✛㸝伟䀡丣甥䴧堩娫䬭䈯ሱळᘵ", a_) + highUInt + CMessageLabel.b("✛㸝伟䀡丣急崧䐩伫อയሱ", a_) + lowUInt + CMessageLabel.b("✛㸝伟䀡丣漥氧\u0a29ᄫอ", a_) + id);
						return false;
					case 0:
					case 8:
						return true;
					}
				}
			}
			}
		}

		public void DelMsgObject(byte server, byte func, ushort id)
		{
			//Discarded unreachable code: IL_0012
			lock (this.m_ᜁ)
			{
				if (true)
				{
				}
				ᜃ(server, func, id);
			}
		}

		private void ᜃ(byte A_0, byte A_1, ushort A_2)
		{
			//Discarded unreachable code: IL_0055
			while (true)
			{
				uint key = CBitHelper.MergeUInt16(CBitHelper.MergeUInt8(A_0, A_1), 16) + A_2;
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 1:
						if (this.m_ᜂ.ContainsKey(key))
						{
							num = 0;
							continue;
						}
						return;
					case 0:
						this.m_ᜂ.Remove(key);
						if (true)
						{
						}
						num = 2;
						continue;
					case 2:
						return;
					}
					break;
				}
			}
		}

		public bool AddIgnoreTrace(byte server, byte func, ushort id)
		{
			//Discarded unreachable code: IL_0012
			lock (this.m_ᜁ)
			{
				if (true)
				{
				}
				return ᜂ(server, func, id);
			}
		}

		private bool ᜂ(byte A_0, byte A_1, ushort A_2)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			uint key = CBitHelper.MergeUInt16(CBitHelper.MergeUInt8(A_0, A_1), A_2);
			if (ᜆ.ContainsKey(key))
			{
				return false;
			}
			ᜆ[key] = null;
			return true;
		}

		public bool IsIgnoreTrace(byte server, byte func, ushort id)
		{
			//Discarded unreachable code: IL_001e
			object obj = this.m_ᜁ;
			Monitor.Enter(obj);
			try
			{
				return ᜁ(server, func, id);
			}
			finally
			{
				if (true)
				{
				}
				Monitor.Exit(obj);
			}
		}

		private bool ᜁ(byte A_0, byte A_1, ushort A_2)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			uint key = CBitHelper.MergeUInt16(CBitHelper.MergeUInt8(A_0, A_1), A_2);
			if (ᜆ.ContainsKey(key))
			{
				return true;
			}
			return false;
		}

		public bool AddMsgCreater(ushort type, ushort id, DMsgCreater msgCreate)
		{
			//Discarded unreachable code: IL_0025
			bool result;
			lock (this.m_ᜁ)
			{
				result = ᜁ(type, id, msgCreate);
			}
			if (true)
			{
			}
			return result;
		}

		private bool ᜁ(ushort A_0, ushort A_1, DMsgCreater A_2)
		{
			//Discarded unreachable code: IL_0020
			uint key = CBitHelper.MergeUInt16(A_0, 16) + A_1;
			if (this.m_ᜃ.ContainsKey(key))
			{
				return false;
			}
			if (true)
			{
			}
			this.m_ᜃ.Add(key, A_2);
			return true;
		}

		public bool AddMsgCreater(ushort type, DMsgCreater msgCreate)
		{
			//Discarded unreachable code: IL_0024
			bool result;
			lock (this.m_ᜁ)
			{
				result = ᜁ(type, msgCreate);
			}
			if (true)
			{
			}
			return result;
		}

		private bool ᜁ(ushort A_0, DMsgCreater A_1)
		{
			//Discarded unreachable code: IL_0011
			if (ᜄ.ContainsKey(A_0))
			{
				if (true)
				{
				}
				return false;
			}
			ᜄ.Add(A_0, A_1);
			return true;
		}

		public void DelMsgCreater(ushort type, ushort id)
		{
			//Discarded unreachable code: IL_001c
			object obj = this.m_ᜁ;
			Monitor.Enter(obj);
			try
			{
				ᜁ(type, id);
			}
			finally
			{
				if (true)
				{
				}
				Monitor.Exit(obj);
			}
		}

		private void ᜁ(ushort A_0, ushort A_1)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			uint key = CBitHelper.MergeUInt16(A_0, 16) + A_1;
			this.m_ᜃ.Remove(key);
		}

		public void DelMsgCreater(ushort type)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			lock (this.m_ᜁ)
			{
				ᜁ(type);
			}
		}

		private void ᜁ(ushort A_0)
		{
			ᜄ.Remove(A_0);
		}
	}
}
