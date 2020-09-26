using System;
using System.Collections.Generic;
using CSLib.Utility;

namespace CSLib.Framework
{
	public class CMsgExecuteMgr : CSingleton<CMsgExecuteMgr>
	{
		private Dictionary<ushort, _173D> ᜀ = new Dictionary<ushort, _173D>();

		public bool AddMsgExecute(ushort ownerID)
		{
			//Discarded unreachable code: IL_0011
			if (ᜀ.ContainsKey(ownerID))
			{
				if (true)
				{
				}
				return false;
			}
			_173D value = new _173D();
			ᜀ.Add(ownerID, value);
			return true;
		}

		public void DelMsgExecute(ushort ownerID)
		{
			//Discarded unreachable code: IL_001f
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 2:
					ᜀ.Remove(ownerID);
					num = 0;
					continue;
				case 0:
					return;
				}
				if (true)
				{
				}
				if (ᜀ.ContainsKey(ownerID))
				{
					num = 2;
					continue;
				}
				return;
			}
		}

		public bool AddMsgExecFunc(ushort ownerID, ushort type, ushort id, DMsgExecFunc msgExecObj)
		{
			return ᜁ(ownerID, type, id, msgExecObj);
		}

		private bool ᜁ(ushort A_0, ushort A_1, ushort A_2, DMsgExecFunc A_3)
		{
			//Discarded unreachable code: IL_0052
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (!ᜀ.ContainsKey(A_0))
					{
						num = 2;
						continue;
					}
					break;
				case 2:
				{
					_173D value = new _173D();
					ᜀ.Add(A_0, value);
					num = 1;
					continue;
				}
				case 1:
					if (1 == 0)
					{
					}
					break;
				}
				break;
			}
			return ᜀ[A_0].ᜀ(A_1, A_2, A_3);
		}

		public bool AddMsgExecFunc(ushort ownerID, byte server, byte func, ushort id, DMsgExecFunc msgExecObj)
		{
			return ᜁ(ownerID, server, func, id, msgExecObj);
		}

		private bool ᜁ(ushort A_0, byte A_1, byte A_2, ushort A_3, DMsgExecFunc A_4)
		{
			//Discarded unreachable code: IL_0052
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (!ᜀ.ContainsKey(A_0))
					{
						num = 0;
						continue;
					}
					break;
				case 0:
				{
					_173D value = new _173D();
					ᜀ.Add(A_0, value);
					num = 1;
					continue;
				}
				case 1:
					if (1 == 0)
					{
					}
					break;
				}
				break;
			}
			return ᜀ[A_0].ᜀ(A_1, A_2, A_3, A_4);
		}

		public bool ExecuteMessage(CMessageLabel msgLabel, byte[] msgBuff, int msgSize)
		{
			return ᜁ(msgLabel, msgBuff, msgSize);
		}

		private bool ᜁ(CMessageLabel A_0, byte[] A_1, int A_2)
		{
			//Discarded unreachable code: IL_0182
			int a_ = 15;
			switch (0)
			{
			}
			CStream cStream = default(CStream);
			ushort sVal = default(ushort);
			byte highUInt = default(byte);
			ushort sVal2 = default(ushort);
			byte lowUInt = default(byte);
			_173D value2 = default(_173D);
			bool result = default(bool);
			KeyValuePair<ushort, _173D> current = default(KeyValuePair<ushort, _173D>);
			_173D value = default(_173D);
			while (true)
			{
				Dictionary<ushort, _173D>.Enumerator enumerator = ᜀ.GetEnumerator();
				int num = 4;
				while (true)
				{
					switch (num)
					{
					case 5:
						if (cStream.Read(ref sVal))
						{
							highUInt = CBitHelper.GetHighUInt8(sVal2);
							lowUInt = CBitHelper.GetLowUInt8(sVal2);
							ᝋ.ᜁ(CMessageLabel.b("愡椣唥伧漩含䬭匯䜱䀳匵男崹主\u103dἿ\u0741㱃⍅⭇㽉㡋⭍ᵏ㝑❓╕㥗㵙㥛繝婟䉡分嘷挩\u08fe뗤༊\uf802ᴑ味䱵塷", a_));
							ᝋ.ᜁ(CMessageLabel.b("愡椣唥伧漩含䬭匯䜱䀳匵男崹主\u103dἿ\u0741㱃⍅⭇㽉㡋⭍ᵏ㝑❓╕㥗㵙㥛繝婟䉡ᅣ㕥൧ᡩ\u1a6b୭ɯ剱䥳噵", a_) + highUInt + CMessageLabel.b("\u1921У匥渧弩䈫䴭\u102f༱ᐳ", a_) + lowUInt + CMessageLabel.b("\u1921У匥愧温ఫጭ\u102f", a_) + sVal);
							ᝋ.ᜁ(CMessageLabel.b("愡椣唥伧漩含䬭匯䜱䀳匵男崹主\u103dἿ\u0741㱃⍅⭇㽉㡋⭍ᵏ㝑❓╕㥗㵙㥛繝婟䉡萆댉༅⛡\ue406č粐옗\uef21痹근稦뜢命ꩿ\ua881꺃겅ꊇꂉ", a_));
							enumerator = ᜀ.GetEnumerator();
							num = 0;
						}
						else
						{
							num = 2;
						}
						continue;
					case 1:
						if (!cStream.Write(A_1, 0, A_2))
						{
							num = 3;
							continue;
						}
						sVal2 = 0;
						sVal = 0;
						if (true)
						{
						}
						num = 7;
						continue;
					case 2:
						return false;
					case 7:
						num = ((!cStream.Read(ref sVal2)) ? 6 : 5);
						continue;
					case 4:
						try
						{
							num = 5;
							while (true)
							{
								switch (num)
								{
								case 6:
									if (value2 != null)
									{
										num = 3;
										continue;
									}
									goto default;
								case 3:
									value2.ᜀ(A_0: false);
									num = 7;
									continue;
								case 7:
									if (value2.ᜀ(A_0, A_1, A_2))
									{
										num = 8;
										continue;
									}
									goto default;
								case 8:
									result = true;
									num = 0;
									continue;
								default:
									num = 1;
									continue;
								case 1:
									if (enumerator.MoveNext())
									{
										value2 = enumerator.Current.Value;
										num = 6;
									}
									else
									{
										num = 4;
									}
									continue;
								case 4:
									num = 2;
									continue;
								case 2:
									break;
								case 0:
									return result;
								}
								break;
							}
						}
						finally
						{
							((IDisposable)enumerator).Dispose();
						}
						cStream = new CStream();
						num = 1;
						continue;
					case 0:
						try
						{
							num = 0;
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
										num = 6;
										continue;
									}
									current = enumerator.Current;
									value = current.Value;
									num = 3;
									continue;
								case 5:
									ᝋ.ᜁ(CMessageLabel.b("愡椣唥伧漩含䬭匯䜱䀳匵男崹主\u103dἿ\u0741㱃⍅⭇㽉㡋⭍ᵏ㝑❓╕㥗㵙㥛繝婟䉡搼괼◷\uec3a霗넓偯㵱ͳᡵᵷ\u0879㕻㩽ꁿ뾁ꒃ", a_) + current.Key);
									value.ᜀ(highUInt, lowUInt, sVal);
									num = 2;
									continue;
								case 3:
									if (value != null)
									{
										num = 5;
										continue;
									}
									goto default;
								case 6:
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
						ᝋ.ᜁ(CMessageLabel.b("愡椣唥伧漩含䬭匯䜱䀳匵男崹主\u103dἿ\u0741㱃⍅⭇㽉㡋⭍ᵏ㝑❓╕㥗㵙㥛繝婟䉡萆댉༅⛡\ue406č粐옗\uef21痹근꤇⌜命ꩿ\ua881꺃겅ꊇꂉ", a_));
						return false;
					case 6:
						return false;
					case 3:
						return false;
					}
					break;
				}
			}
		}

		public bool ExecuteMessage(CMessageLabel msgLabel, byte[] msgBuff, int msgSize, ushort ownerID)
		{
			return ᜁ(msgLabel, msgBuff, msgSize);
		}

		private bool ᜁ(CMessageLabel A_0, byte[] A_1, int A_2, ushort A_3)
		{
			//Discarded unreachable code: IL_0023
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					num = (ᜀ.ContainsKey(A_3) ? 1 : 2);
					break;
				case 0:
					return true;
				case 1:
					if (ᜀ[A_3].ᜀ(A_0, A_1, A_2))
					{
						num = 0;
						break;
					}
					return false;
				case 2:
					return false;
				}
			}
		}

		internal bool ᜁ(CMessageLabel A_0, CStream A_1)
		{
			//Discarded unreachable code: IL_0011
			switch (0)
			{
			default:
			{
				if (true)
				{
				}
				using (Dictionary<ushort, _173D>.Enumerator enumerator = ᜀ.GetEnumerator())
				{
					int num = 4;
					_173D value = default(_173D);
					bool result = default(bool);
					while (true)
					{
						switch (num)
						{
						default:
							num = 8;
							continue;
						case 8:
							if (!enumerator.MoveNext())
							{
								num = 0;
								continue;
							}
							value = enumerator.Current.Value;
							num = 3;
							continue;
						case 3:
							if (value != null)
							{
								num = 7;
								continue;
							}
							goto default;
						case 5:
							result = true;
							num = 6;
							continue;
						case 7:
							num = 1;
							continue;
						case 1:
							if (value.ᜀ(A_0, A_1))
							{
								num = 5;
								continue;
							}
							goto default;
						case 0:
							num = 2;
							continue;
						case 2:
							break;
						case 6:
							return result;
						}
						break;
					}
				}
				return false;
			}
			}
		}

		internal bool ᜁ(CMessageLabel A_0, CMessage A_1)
		{
			//Discarded unreachable code: IL_0062
			switch (0)
			{
			default:
			{
				using (Dictionary<ushort, _173D>.Enumerator enumerator = ᜀ.GetEnumerator())
				{
					int num = 5;
					_173D value = default(_173D);
					bool result = default(bool);
					while (true)
					{
						switch (num)
						{
						default:
							num = 3;
							continue;
						case 3:
							if (true)
							{
							}
							if (!enumerator.MoveNext())
							{
								num = 8;
								continue;
							}
							value = enumerator.Current.Value;
							num = 6;
							continue;
						case 6:
							if (value != null)
							{
								num = 1;
								continue;
							}
							goto default;
						case 2:
							result = true;
							num = 0;
							continue;
						case 1:
							num = 4;
							continue;
						case 4:
							if (value.ᜀ(A_0, A_1))
							{
								num = 2;
								continue;
							}
							goto default;
						case 8:
							num = 7;
							continue;
						case 7:
							break;
						case 0:
							return result;
						}
						break;
					}
				}
				return false;
			}
			}
		}
	}
}
