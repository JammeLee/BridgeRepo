using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace CSLib.Utility
{
	public class CSimpleThreadPool : CSingleton<CSimpleThreadPool>
	{
		private bool ᜀ;

		private CCollectionContainerDictionaryType<string, Thread> ᜁ = new CCollectionContainerDictionaryType<string, Thread>();

		private CCollectionContainerListType<System.Timers.Timer> ᜂ = new CCollectionContainerListType<System.Timers.Timer>();

		private CCollectionContainerDictionaryType<int, System.Threading.Timer> ᜃ = new CCollectionContainerDictionaryType<int, System.Threading.Timer>();

		public bool Exit
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

		public string NewThread(WaitCallback callBack, object paramaters, bool STAMode)
		{
			//Discarded unreachable code: IL_002d
			while (true)
			{
				Thread thread = new Thread(callBack.Invoke);
				if (true)
				{
				}
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (STAMode)
						{
							num = 1;
							continue;
						}
						thread.SetApartmentState(ApartmentState.MTA);
						num = 2;
						continue;
					case 1:
						thread.SetApartmentState(ApartmentState.STA);
						num = 3;
						continue;
					case 2:
					case 3:
					{
						string text = Guid.NewGuid().ToString();
						ᜁ.Add(text, thread);
						thread.Start(paramaters);
						return text;
					}
					}
					break;
				}
			}
		}

		public void DelThread(string key)
		{
			//Discarded unreachable code: IL_0034
			while (true)
			{
				Thread thread = ᜁ.Get(key);
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (true)
						{
						}
						if (thread != null)
						{
							num = 0;
							continue;
						}
						return;
					case 4:
						thread.Abort();
						ᜁ.Remove(key);
						num = 3;
						continue;
					case 3:
						return;
					case 0:
						num = 1;
						continue;
					case 1:
						if (thread.IsAlive)
						{
							num = 4;
							continue;
						}
						return;
					}
					break;
				}
			}
		}

		public void AwakeThread(string key)
		{
			ᜁ.Get(key)?.Join();
		}

		public bool HaveAliveThread()
		{
			//Discarded unreachable code: IL_00a8
			bool result = default(bool);
			using (Dictionary<string, Thread>.ValueCollection.Enumerator enumerator = ᜁ.Values.GetEnumerator())
			{
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 4:
						break;
					default:
						num = 0;
						continue;
					case 0:
						num = (enumerator.MoveNext() ? 3 : 5);
						continue;
					case 3:
						if (enumerator.Current.IsAlive)
						{
							num = 1;
							continue;
						}
						goto default;
					case 1:
						result = true;
						num = 6;
						continue;
					case 6:
						goto end_IL_0015;
					case 5:
						num = 4;
						continue;
					}
					break;
				}
				goto IL_0013;
				end_IL_0015:;
			}
			if (true)
			{
			}
			return result;
			IL_0013:
			return false;
		}

		public System.Timers.Timer NewTimersTimer()
		{
			System.Timers.Timer timer = new System.Timers.Timer();
			ᜂ.Add(timer);
			return timer;
		}

		public System.Threading.Timer NewThreadingTimer(TimerCallback timerDelegate, object state, int dueTime, int period, int key)
		{
			//Discarded unreachable code: IL_0050
			while (true)
			{
				System.Threading.Timer timer = new System.Threading.Timer(timerDelegate, state, dueTime, period);
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (key >= 0)
						{
							num = 2;
							continue;
						}
						goto IL_0057;
					case 2:
						ᜃ.Add(key, timer);
						num = 1;
						continue;
					case 1:
						{
							if (1 == 0)
							{
							}
							goto IL_0057;
						}
						IL_0057:
						return timer;
					}
					break;
				}
			}
		}

		public void DelThreadingTimer(int key)
		{
			//Discarded unreachable code: IL_0039
			while (true)
			{
				System.Threading.Timer timer = ᜃ.Get(key);
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (timer != null)
						{
							num = 0;
							continue;
						}
						return;
					case 0:
						if (true)
						{
						}
						timer.Dispose();
						ᜃ.Remove(key);
						num = 1;
						continue;
					case 1:
						return;
					}
					break;
				}
			}
		}

		public void FinishAll()
		{
			//Discarded unreachable code: IL_003b
			switch (0)
			{
			}
			Dictionary<int, System.Threading.Timer>.ValueCollection.Enumerator enumerator2 = default(Dictionary<int, System.Threading.Timer>.ValueCollection.Enumerator);
			Dictionary<string, Thread>.ValueCollection.Enumerator enumerator3 = default(Dictionary<string, Thread>.ValueCollection.Enumerator);
			Thread current2 = default(Thread);
			while (true)
			{
				IEnumerator<System.Timers.Timer> enumerator = ᜂ.GetEnumerator();
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (true)
						{
						}
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
									if (enumerator.MoveNext())
									{
										System.Timers.Timer current = enumerator.Current;
										current.Stop();
										current.Close();
										num = 4;
									}
									else
									{
										num = 3;
									}
									continue;
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
							num = 0;
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
						ᜂ.Clear();
						enumerator2 = ᜃ.Values.GetEnumerator();
						num = 0;
						continue;
					case 1:
						try
						{
							num = 2;
							while (true)
							{
								switch (num)
								{
								default:
									num = 4;
									continue;
								case 4:
									if (!enumerator3.MoveNext())
									{
										num = 6;
										continue;
									}
									current2 = enumerator3.Current;
									num = 0;
									continue;
								case 0:
									if (current2.IsAlive)
									{
										num = 1;
										continue;
									}
									goto default;
								case 1:
									current2.Abort();
									num = 5;
									continue;
								case 6:
									num = 3;
									continue;
								case 3:
									break;
								}
								break;
							}
						}
						finally
						{
							((IDisposable)enumerator3).Dispose();
						}
						ᜁ.Clear();
						return;
					case 0:
						try
						{
							num = 4;
							while (true)
							{
								switch (num)
								{
								default:
									num = 1;
									continue;
								case 1:
									if (enumerator2.MoveNext())
									{
										enumerator2.Current.Dispose();
										num = 3;
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
						finally
						{
							((IDisposable)enumerator2).Dispose();
						}
						ᜃ.Clear();
						enumerator3 = ᜁ.Values.GetEnumerator();
						num = 1;
						continue;
					}
					break;
				}
			}
		}

		internal static string b(string A_0, int A_1)
		{
			char[] array = A_0.ToCharArray();
			int num = 1340473659 + A_1;
			int num2 = 0;
			if (num2 >= 1)
			{
				goto IL_0014;
			}
			goto IL_0047;
			IL_0047:
			if (num2 >= array.Length)
			{
				return string.Intern(new string(array));
			}
			goto IL_0014;
			IL_0014:
			int num3 = num2;
			char num4 = array[num3];
			byte b = (byte)((num4 & 0xFFu) ^ (uint)num++);
			byte b2 = (byte)(((int)num4 >> 8) ^ num++);
			byte num5 = b2;
			b2 = b;
			b = num5;
			array[num3] = (char)((b2 << 8) | b);
			num2++;
			goto IL_0047;
		}
	}
}
