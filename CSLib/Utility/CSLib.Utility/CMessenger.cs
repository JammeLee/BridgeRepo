using System;
using System.Collections.Generic;

namespace CSLib.Utility
{
	public static class CMessenger
	{
		public class BroadcastException : Exception
		{
			public BroadcastException(string msg)
				: base(msg)
			{
			}
		}

		public class ListenerException : Exception
		{
			public ListenerException(string msg)
				: base(msg)
			{
			}
		}

		public static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

		public static List<string> permanentMessages = new List<string>();

		public static void MarkAsPermanent(string eventType)
		{
			permanentMessages.Add(eventType);
		}

		public static void Cleanup()
		{
			//Discarded unreachable code: IL_004d
			switch (0)
			{
			default:
			{
				List<string> list = new List<string>();
				using (Dictionary<string, Delegate>.Enumerator enumerator = eventTable.GetEnumerator())
				{
					int num = 0;
					bool flag = default(bool);
					KeyValuePair<string, Delegate> current = default(KeyValuePair<string, Delegate>);
					string current2 = default(string);
					List<string>.Enumerator enumerator2 = default(List<string>.Enumerator);
					while (true)
					{
						switch (num)
						{
						case 3:
							if (!flag)
							{
								num = 1;
								continue;
							}
							goto default;
						case 2:
							try
							{
								num = 0;
								while (true)
								{
									switch (num)
									{
									case 3:
										if (current.Key == current2)
										{
											num = 6;
											continue;
										}
										goto default;
									case 6:
										flag = true;
										num = 2;
										continue;
									case 2:
										break;
									default:
										num = 4;
										continue;
									case 4:
										if (enumerator2.MoveNext())
										{
											current2 = enumerator2.Current;
											num = 3;
										}
										else
										{
											num = 5;
										}
										continue;
									case 5:
										num = 1;
										continue;
									case 1:
										break;
									}
									break;
								}
							}
							finally
							{
								((IDisposable)enumerator2).Dispose();
							}
							num = 3;
							continue;
						default:
							num = 7;
							continue;
						case 7:
							if (!enumerator.MoveNext())
							{
								num = 6;
								continue;
							}
							current = enumerator.Current;
							flag = false;
							enumerator2 = permanentMessages.GetEnumerator();
							num = 2;
							continue;
						case 1:
							list.Add(current.Key);
							num = 4;
							continue;
						case 6:
							num = 5;
							continue;
						case 5:
							break;
						}
						break;
					}
				}
				using List<string>.Enumerator enumerator3 = list.GetEnumerator();
				int num = 4;
				while (true)
				{
					switch (num)
					{
					default:
						if (1 == 0)
						{
						}
						goto case 1;
					case 1:
						num = 2;
						break;
					case 2:
					{
						if (!enumerator3.MoveNext())
						{
							num = 3;
							break;
						}
						string current3 = enumerator3.Current;
						eventTable.Remove(current3);
						num = 1;
						break;
					}
					case 3:
						num = 0;
						break;
					case 0:
						return;
					}
				}
			}
			}
		}

		public static void PrintEventTable()
		{
			//Discarded unreachable code: IL_00e3
			int a_ = 7;
			CDebugOut.Log(CSimpleThreadPool.b("䩂䱄乆瑈癊灌潎᱐ᙒ\u0654і᱘ᕚ\u1a5c\u1a5e㍠䍢㕤ᕦhժᥬ⩮ݰᙲ᭴Ͷ\u2d78\u1a7aὼ\u137e\ue480ꎂ뢄몆뒈", a_));
			using (Dictionary<string, Delegate>.Enumerator enumerator = eventTable.GetEnumerator())
			{
				int num = 3;
				while (true)
				{
					switch (num)
					{
					default:
						num = 4;
						continue;
					case 4:
						if (enumerator.MoveNext())
						{
							KeyValuePair<string, Delegate> current = enumerator.Current;
							CDebugOut.LogError(CSimpleThreadPool.b("䩂䱄乆", a_) + current.Key + CSimpleThreadPool.b("䩂䱄", a_) + current.Value);
							num = 1;
						}
						else
						{
							num = 2;
						}
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
			if (1 == 0)
			{
			}
		}

		public static void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
		{
			//Discarded unreachable code: IL_009c
			int a_ = 10;
			int num = 5;
			Delegate @delegate = default(Delegate);
			while (true)
			{
				switch (num)
				{
				default:
					if (!eventTable.ContainsKey(eventType))
					{
						num = 6;
						break;
					}
					goto case 4;
				case 0:
					num = 2;
					break;
				case 2:
					if (@delegate.GetType() != listenerBeingAdded.GetType())
					{
						num = 1;
						break;
					}
					return;
				case 4:
					@delegate = eventTable[eventType];
					num = 3;
					break;
				case 3:
					if ((object)@delegate != null)
					{
						num = 0;
						break;
					}
					return;
				case 6:
					eventTable.Add(eventType, null);
					if (true)
					{
					}
					num = 4;
					break;
				case 1:
					throw new ListenerException(string.Format(CSimpleThreadPool.b("\u0745㱇㹉⥋⍍⁏♑㵓㡕㽗穙⡛ㅝ䁟\u0361cɥ䡧٩իᵭѯ\u1771\u1a73፵\u0a77婹\u0b7b\u177d\uf47f\uea81ꒃ\uef85\ue687\ue989\ue38b\ue08d\ue38fﮑ\ue793\ue295ﶗ\uf499\ue89b뺝펟쮡쎣좥즧\udea9\ud9ab\udcad햯銱튳\ud9b5쪷骹\ud9bb좽ꖿ곁냃\ue6c5볇돉볋ꯍ\uf0cf꧑\ue4d3ꯕ\uf6d7龎\u9fdbꯝ鋟郡臣裥鳧쫩胫蟭華蛱釳飵鷷裹迻\udefd棿持爃挅⠇縉甋縍甏㈑漓✕攗㨙紛瀝䐟ȡ䠣伥嬧帩䤫䀭唯䀱ᐳ吵崷匹刻夽怿⍁⁃≅ⵇ\u2e49汋♍ㅏ⅑瑓≕⅗⩙㥛繝᭟偡ᥣ", a_), eventType, @delegate.GetType().Name, listenerBeingAdded.GetType().Name));
				}
			}
		}

		public static void OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
		{
			//Discarded unreachable code: IL_00ad
			int a_ = 8;
			int num = 2;
			Delegate @delegate = default(Delegate);
			while (true)
			{
				switch (num)
				{
				default:
					if (eventTable.ContainsKey(eventType))
					{
						num = 5;
						break;
					}
					throw new ListenerException(string.Format(CSimpleThreadPool.b("Ճ㉅㱇⽉⅋㹍\u244f㭑㩓ㅕ硗\u2e59㍛繝\u125fݡ\u0963॥ṧཀྵ䱫ɭ\u196fűs፵ᙷό\u0e7b幽\ue67f\ued81\uf683ꚅﲇ\uf389ﲋ\ueb8d낏낑\uef93ꚕ\ue597뢙벛ﲝ햟횡蒣\ueba5춧\ud9a9\udfab쮭\udeaf햱톳쒵颷\udeb9펻\udbbd뎿곁\ue3c3닅\ue8c7ꇉꋋꇍ\ua7cf\uf2d1뗓듕럗꿙\ua8dbﻝ铟諡跣闥죧迩髫语黯蛱퓳苵臷諹駻탽", a_), eventType));
				case 4:
					throw new ListenerException(string.Format(CSimpleThreadPool.b("Ճ㉅㱇⽉⅋㹍\u244f㭑㩓ㅕ硗\u2e59㍛繝\u125fݡ\u0963॥ṧཀྵ䱫ɭ\u196fűs፵ᙷό\u0e7b幽\uf77f\ueb81\uf083\uee85ꢇ\ue389\ue28b\ued8dﾏﲑ\ue793ﾕ\ueb97\uee99鍊\uf09d풟芡힣쾥쾧쒩춫\udaad얯삱톳隵\udeb7햹캻麽ꖿ듁ꇃ\ua8c5볇\ueac9룋럍ꃏ럑\uf4d3귕\ue8d7\ua7d9\uf2dbﻝꏟ韡難铥跧蓩飫컭鳯鯱蟳苵鷷铹駻賽珿∁氃朅縇漉Ⰻ稍椏我焓㘕挗⬙愛㸝䄟䰡䀣إ䐧䌩弫娭唯就儳䐵ᠷ堹夻圽⸿╁摃㑅ⵇ❉⍋㡍㕏㙑瑓㹕㥗⥙籛⩝ᥟቡţ䙥፧塩ᅫ", a_), eventType, @delegate.GetType().Name, listenerBeingRemoved.GetType().Name));
				case 1:
					if (@delegate.GetType() != listenerBeingRemoved.GetType())
					{
						num = 4;
						break;
					}
					return;
				case 5:
					@delegate = eventTable[eventType];
					if (true)
					{
					}
					num = 3;
					break;
				case 3:
					num = (((object)@delegate != null) ? 1 : 0);
					break;
				case 0:
					throw new ListenerException(string.Format(CSimpleThreadPool.b("Ճ㉅㱇⽉⅋㹍\u244f㭑㩓ㅕ硗\u2e59㍛繝\u125fݡ\u0963॥ṧཀྵ䱫ɭ\u196fűs፵ᙷό\u0e7b幽\uf77f\ueb81\uf083\uee85ꢇ\uec89\ue38bﲍ낏\uf791\ue293\uf395\uf697\uee99벛\uea9d\ud99f튡솣蚥誧톩鲫펭銯銱횳쎵첷骹\udfbb쮽늿냁ꇃ\ua8c5볇\ueac9ꃋ\ua7cdꏏꛑ뇓룕뷗꣙ﳛ럝鏟싡諣鏥蓧蛩싫", a_), eventType));
				}
			}
		}

		public static void OnListenerRemoved(string eventType)
		{
			if ((object)eventTable[eventType] == null)
			{
				eventTable.Remove(eventType);
			}
		}

		public static void OnBroadcasting(string eventType)
		{
		}

		public static BroadcastException CreateBroadcastSignatureException(string eventType)
		{
			int a_ = 12;
			return new BroadcastException(string.Format(CSimpleThreadPool.b("\u0a47㡉⍋⽍㑏ㅑ㕓╕ⱗ㍙㉛㥝䁟ཡţᕥ᭧୩୫୭偯偱\u0f73䙵շ塹屻ᱽ\uf57f\uf681ꒃ\uea85\ue187黎\uf88b\ueb8dﺏ\uf791\ue693\ue595뢗\uf299ﶛ\ue89d얟芡얣蚥첧쎩쪫좭햯삱톳\ud8b5첷骹쾻ힽ\ua7bf곁ꗃ닅뷇룉꧋\ueecd\ua4cf뫑뗓룕\uf8d7껙듛믝샟胡難觥觧軩迫迭華蛱釳蓵훷", a_), eventType));
		}

		public static void AddListener(string eventType, Callback handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerAdding(eventType, handler);
			eventTable[eventType] = (Callback)Delegate.Combine((Callback)eventTable[eventType], handler);
		}

		public static void AddListener<T>(string eventType, Callback<T> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerAdding(eventType, handler);
			eventTable[eventType] = (Callback<T>)Delegate.Combine((Callback<T>)eventTable[eventType], handler);
		}

		public static void AddListener<T, U>(string eventType, Callback<T, U> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerAdding(eventType, handler);
			eventTable[eventType] = (Callback<T, U>)Delegate.Combine((Callback<T, U>)eventTable[eventType], handler);
		}

		public static void AddListener<T, U, V>(string eventType, Callback<T, U, V> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerAdding(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V>)Delegate.Combine((Callback<T, U, V>)eventTable[eventType], handler);
		}

		public static void AddListener<T, U, V, W>(string eventType, Callback<T, U, V, W> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerAdding(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V, W>)Delegate.Combine((Callback<T, U, V, W>)eventTable[eventType], handler);
		}

		public static void AddListener<T, U, V, W, N>(string eventType, Callback<T, U, V, W, N> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerAdding(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V, W, N>)Delegate.Combine((Callback<T, U, V, W, N>)eventTable[eventType], handler);
		}

		public static void RemoveListener(string eventType, Callback handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback)Delegate.Remove((Callback)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}

		public static void RemoveListener<T>(string eventType, Callback<T> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback<T>)Delegate.Remove((Callback<T>)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}

		public static void RemoveListener<T, U>(string eventType, Callback<T, U> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback<T, U>)Delegate.Remove((Callback<T, U>)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}

		public static void RemoveListener<T, U, V>(string eventType, Callback<T, U, V> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V>)Delegate.Remove((Callback<T, U, V>)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}

		public static void RemoveListener<T, U, V, W>(string eventType, Callback<T, U, V, W> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V, W>)Delegate.Remove((Callback<T, U, V, W>)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}

		public static void RemoveListener<T, U, V, W, N>(string eventType, Callback<T, U, V, W, N> handler)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V, W, N>)Delegate.Remove((Callback<T, U, V, W, N>)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}

		public static void Broadcast(string eventType)
		{
			//Discarded unreachable code: IL_0029
			Delegate value = default(Delegate);
			Callback callback = default(Callback);
			while (true)
			{
				OnBroadcasting(eventType);
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (true)
						{
						}
						if (eventTable.TryGetValue(eventType, out value))
						{
							num = 1;
							continue;
						}
						return;
					case 2:
						callback();
						return;
					case 1:
						callback = value as Callback;
						num = 3;
						continue;
					case 3:
						if (callback == null)
						{
							throw CreateBroadcastSignatureException(eventType);
						}
						num = 2;
						continue;
					}
					break;
				}
			}
		}

		public static void Broadcast<T>(string eventType, T arg1)
		{
			//Discarded unreachable code: IL_006d
			Delegate value = default(Delegate);
			Callback<T> callback = default(Callback<T>);
			while (true)
			{
				OnBroadcasting(eventType);
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 1:
						if (eventTable.TryGetValue(eventType, out value))
						{
							num = 0;
							continue;
						}
						if (1 == 0)
						{
						}
						return;
					case 3:
						callback(arg1);
						return;
					case 0:
						callback = value as Callback<T>;
						num = 2;
						continue;
					case 2:
						if (callback == null)
						{
							throw CreateBroadcastSignatureException(eventType);
						}
						num = 3;
						continue;
					}
					break;
				}
			}
		}

		public static void Broadcast<T, U>(string eventType, T arg1, U arg2)
		{
			//Discarded unreachable code: IL_001b
			Delegate value = default(Delegate);
			Callback<T, U> callback = default(Callback<T, U>);
			while (true)
			{
				if (true)
				{
				}
				OnBroadcasting(eventType);
				int num = 3;
				while (true)
				{
					switch (num)
					{
					case 3:
						if (eventTable.TryGetValue(eventType, out value))
						{
							num = 0;
							continue;
						}
						return;
					case 1:
						callback(arg1, arg2);
						return;
					case 0:
						callback = value as Callback<T, U>;
						num = 2;
						continue;
					case 2:
						if (callback == null)
						{
							throw CreateBroadcastSignatureException(eventType);
						}
						num = 1;
						continue;
					}
					break;
				}
			}
		}

		public static void Broadcast<T, U, V>(string eventType, T arg1, U arg2, V arg3)
		{
			//Discarded unreachable code: IL_0053
			Delegate value = default(Delegate);
			Callback<T, U, V> callback = default(Callback<T, U, V>);
			while (true)
			{
				OnBroadcasting(eventType);
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (eventTable.TryGetValue(eventType, out value))
						{
							num = 1;
							continue;
						}
						return;
					case 3:
						callback(arg1, arg2, arg3);
						return;
					case 1:
						if (true)
						{
						}
						callback = value as Callback<T, U, V>;
						num = 2;
						continue;
					case 2:
						if (callback == null)
						{
							throw CreateBroadcastSignatureException(eventType);
						}
						num = 3;
						continue;
					}
					break;
				}
			}
		}

		public static void Broadcast<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
		{
			//Discarded unreachable code: IL_0067
			Delegate value = default(Delegate);
			Callback<T, U, V, W> callback = default(Callback<T, U, V, W>);
			while (true)
			{
				OnBroadcasting(eventType);
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (eventTable.TryGetValue(eventType, out value))
						{
							num = 3;
							continue;
						}
						return;
					case 1:
						callback(arg1, arg2, arg3, arg4);
						return;
					case 3:
						callback = value as Callback<T, U, V, W>;
						num = 2;
						continue;
					case 2:
						if (callback == null)
						{
							throw CreateBroadcastSignatureException(eventType);
						}
						if (true)
						{
						}
						num = 1;
						continue;
					}
					break;
				}
			}
		}

		public static void Broadcast<T, U, V, W, N>(string eventType, T arg1, U arg2, V arg3, W arg4, N arg5)
		{
			//Discarded unreachable code: IL_0005
			Delegate value = default(Delegate);
			Callback<T, U, V, W, N> callback = default(Callback<T, U, V, W, N>);
			while (true)
			{
				OnBroadcasting(eventType);
				int num = 2;
				while (true)
				{
					if (true)
					{
					}
					switch (num)
					{
					case 2:
						if (eventTable.TryGetValue(eventType, out value))
						{
							num = 3;
							continue;
						}
						return;
					case 1:
						callback(arg1, arg2, arg3, arg4, arg5);
						return;
					case 3:
						callback = value as Callback<T, U, V, W, N>;
						num = 0;
						continue;
					case 0:
						if (callback == null)
						{
							throw CreateBroadcastSignatureException(eventType);
						}
						num = 1;
						continue;
					}
					break;
				}
			}
		}
	}
}
