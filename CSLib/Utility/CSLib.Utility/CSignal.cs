using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CSLib.Utility
{
	public class CSignal
	{
		private struct ᜀ
		{
			public readonly object ᜀ;

			public readonly Action ᜁ;

			public readonly int ᜂ;

			public readonly float ᜃ;

			public ᜀ(Action A_0, object A_1, int A_2)
			{
				ᜁ = A_0;
				ᜀ = A_1;
				ᜂ = A_2;
				ᜃ = DateTime.Now.Ticks;
			}

			public void Invoke()
			{
				if (ᜁ != null)
				{
					ᜁ();
				}
			}
		}

		[CompilerGenerated]
		private sealed class ᜂ
		{
			public Action ᜀ;

			internal bool ᜁ(ᜀ A_0)
			{
				return A_0.ᜁ == ᜀ;
			}
		}

		[CompilerGenerated]
		private sealed class ᜁ
		{
			public object ᜀ;

			internal bool ᜁ(ᜀ A_0)
			{
				return A_0.ᜀ == ᜀ;
			}
		}

		private const int m_ᜀ = 0;

		private readonly List<ᜀ> m_ᜁ;

		public CSignal()
		{
			this.m_ᜁ = new List<ᜀ>();
		}

		public void AddListener(Action callback, object context = null, int priority = 0)
		{
			//Discarded unreachable code: IL_0023
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (callback != null)
					{
						num = 0;
						break;
					}
					return;
				case 0:
					this.m_ᜁ.Add(new ᜀ(callback, context, priority));
					num = 1;
					break;
				case 1:
					this.m_ᜁ.Sort(delegate(ᜀ A_0, ᜀ A_1)
					{
						//Discarded unreachable code: IL_0023
						int num2 = 3;
						float num3;
						while (true)
						{
							switch (num2)
							{
							default:
								if (true)
								{
								}
								num2 = ((A_0.ᜂ != A_1.ᜂ) ? 2 : 0);
								continue;
							case 0:
								num3 = A_0.ᜃ - A_1.ᜃ;
								break;
							case 2:
								num2 = 1;
								continue;
							case 1:
								num3 = A_1.ᜂ - A_0.ᜂ;
								break;
							}
							break;
						}
						return (int)num3;
					});
					num = 3;
					break;
				case 3:
					return;
				}
			}
		}

		public void RemoveListener(Action callback)
		{
			//Discarded unreachable code: IL_0017
			while (true)
			{
				if (true)
				{
				}
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (callback != null)
						{
							num = 1;
							continue;
						}
						return;
					case 1:
						this.m_ᜁ.RemoveAll((ᜀ A_0) => A_0.ᜁ == callback);
						num = 2;
						continue;
					case 2:
						return;
					}
					break;
				}
			}
		}

		public void RemoveListenerByContext(object context)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			this.m_ᜁ.RemoveAll((ᜀ A_0) => A_0.ᜀ == context);
		}

		public void Dispatch()
		{
			//Discarded unreachable code: IL_005e
			while (true)
			{
				ᜀ[] array = this.m_ᜁ.ToArray();
				int num = 0;
				int num2 = 2;
				while (true)
				{
					switch (num2)
					{
					case 2:
					case 3:
						num2 = 1;
						continue;
					case 1:
					{
						if (num >= array.Length)
						{
							num2 = 0;
							continue;
						}
						ᜀ ᜀ = array[num];
						ᜀ.Invoke();
						num++;
						if (true)
						{
						}
						num2 = 3;
						continue;
					}
					case 0:
						return;
					}
					break;
				}
			}
		}

		public void Clear()
		{
			this.m_ᜁ.Clear();
		}
	}
	public class CSignal<T>
	{
		private struct ᜁ
		{
			public readonly object ᜀ;

			public readonly Action ᜁ;

			public readonly Action<T> ᜂ;

			public readonly int ᜃ;

			public readonly float ᜄ;

			public ᜁ(Action A_0, object A_1, int A_2)
			{
				ᜁ = A_0;
				ᜂ = null;
				ᜀ = A_1;
				ᜃ = A_2;
				ᜄ = DateTime.Now.Ticks;
			}

			public ᜁ(Action<T> A_0, object A_1, int A_2)
			{
				ᜁ = null;
				ᜂ = A_0;
				ᜀ = A_1;
				ᜃ = A_2;
				ᜄ = DateTime.Now.Ticks;
			}

			public void Invoke(T A_0)
			{
				//Discarded unreachable code: IL_0033
				int num = 3;
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
							break;
						}
						goto case 5;
					case 4:
						ᜂ(A_0);
						num = 0;
						break;
					case 0:
						return;
					case 5:
						num = 1;
						break;
					case 1:
						if (ᜂ != null)
						{
							num = 4;
							break;
						}
						return;
					case 2:
						ᜁ();
						num = 5;
						break;
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class ᜂ
		{
			public Action ᜀ;

			internal bool ᜁ(ᜁ A_0)
			{
				return A_0.ᜁ == ᜀ;
			}
		}

		[CompilerGenerated]
		private sealed class ᜃ
		{
			public Action<T> ᜀ;

			internal bool ᜁ(ᜁ A_0)
			{
				return A_0.ᜂ == ᜀ;
			}
		}

		[CompilerGenerated]
		private sealed class ᜀ
		{
			public object ᜀ;

			internal bool ᜁ(ᜁ A_0)
			{
				return A_0.ᜀ == ᜀ;
			}
		}

		private const int m_ᜀ = 0;

		private readonly List<ᜁ> m_ᜁ;

		public CSignal()
		{
			this.ᜁ = new List<ᜁ>();
		}

		public void AddListener(Action callback, object context = null, int priority = 0)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			this.ᜁ.Add(new ᜁ(callback, context, priority));
			this.ᜁ.Sort(delegate(ᜁ A_0, ᜁ A_1)
			{
				//Discarded unreachable code: IL_0023
				int num = 0;
				float num2;
				while (true)
				{
					switch (num)
					{
					default:
						if (true)
						{
						}
						num = ((A_0.ᜃ != A_1.ᜃ) ? 1 : 3);
						continue;
					case 3:
						num2 = A_0.ᜄ - A_1.ᜄ;
						break;
					case 1:
						num = 2;
						continue;
					case 2:
						num2 = A_1.ᜃ - A_0.ᜃ;
						break;
					}
					break;
				}
				return (int)num2;
			});
		}

		public void AddListener(Action<T> callback, object context = null, int priority = 0)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			this.ᜁ.Add(new ᜁ(callback, context, priority));
			this.ᜁ.Sort(delegate(ᜁ A_0, ᜁ A_1)
			{
				//Discarded unreachable code: IL_0023
				int num = 2;
				float num2;
				while (true)
				{
					switch (num)
					{
					default:
						if (true)
						{
						}
						num = ((A_0.ᜃ != A_1.ᜃ) ? 1 : 3);
						continue;
					case 3:
						num2 = A_0.ᜄ - A_1.ᜄ;
						break;
					case 1:
						num = 0;
						continue;
					case 0:
						num2 = A_1.ᜃ - A_0.ᜃ;
						break;
					}
					break;
				}
				return (int)num2;
			});
		}

		public void RemoveListener(Action callback)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			this.ᜁ.RemoveAll((ᜁ A_0) => A_0.ᜁ == callback);
		}

		public void RemoveListener(Action<T> callback)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			this.ᜁ.RemoveAll((ᜁ A_0) => A_0.ᜂ == callback);
		}

		public void RemoveListenerByContext(object context)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			this.ᜁ.RemoveAll((ᜁ A_0) => A_0.ᜀ == context);
		}

		public void Dispatch(T param1)
		{
			//Discarded unreachable code: IL_0029
			while (true)
			{
				ᜁ[] array = this.ᜁ.ToArray();
				int num = 0;
				if (true)
				{
				}
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					case 0:
					case 1:
						num2 = 2;
						continue;
					case 2:
					{
						if (num >= array.Length)
						{
							num2 = 3;
							continue;
						}
						ᜁ ᜁ = array[num];
						ᜁ.Invoke(param1);
						num++;
						num2 = 0;
						continue;
					}
					case 3:
						return;
					}
					break;
				}
			}
		}

		public void Clear()
		{
			this.ᜁ.Clear();
		}
	}
	public class CSignal<T1, T2>
	{
		private struct ᜀ
		{
			public readonly object ᜀ;

			public readonly Action ᜁ;

			public readonly Action<T1, T2> ᜂ;

			public readonly int ᜃ;

			public readonly float ᜄ;

			public ᜀ(Action A_0, object A_1, int A_2)
			{
				ᜁ = A_0;
				ᜂ = null;
				ᜀ = A_1;
				ᜃ = A_2;
				ᜄ = DateTime.Now.Ticks;
			}

			public ᜀ(Action<T1, T2> A_0, object A_1, int A_2)
			{
				ᜁ = null;
				ᜂ = A_0;
				ᜀ = A_1;
				ᜃ = A_2;
				ᜄ = DateTime.Now.Ticks;
			}

			public void Invoke(T1 A_0, T2 A_1)
			{
				//Discarded unreachable code: IL_0033
				int num = 3;
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
							num = 5;
							break;
						}
						goto case 0;
					case 2:
						ᜂ(A_0, A_1);
						num = 1;
						break;
					case 1:
						return;
					case 0:
						num = 4;
						break;
					case 4:
						if (ᜂ != null)
						{
							num = 2;
							break;
						}
						return;
					case 5:
						ᜁ();
						num = 0;
						break;
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class ᜁ
		{
			public Action ᜀ;

			internal bool ᜁ(ᜀ A_0)
			{
				return A_0.ᜁ == ᜀ;
			}
		}

		[CompilerGenerated]
		private sealed class ᜃ
		{
			public Action<T1, T2> ᜀ;

			internal bool ᜁ(ᜀ A_0)
			{
				return A_0.ᜂ == ᜀ;
			}
		}

		[CompilerGenerated]
		private sealed class ᜂ
		{
			public object ᜀ;

			internal bool ᜁ(ᜀ A_0)
			{
				return A_0.ᜀ == ᜀ;
			}
		}

		private const int m_ᜀ = 0;

		private readonly List<ᜀ> m_ᜁ;

		public CSignal()
		{
			this.ᜁ = new List<ᜀ>();
		}

		public void AddListener(Action callback, object context = null, int priority = 0)
		{
			//Discarded unreachable code: IL_0023
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (callback != null)
					{
						num = 1;
						break;
					}
					return;
				case 1:
					this.ᜁ.Add(new ᜀ(callback, context, priority));
					num = 0;
					break;
				case 0:
					this.ᜁ.Sort(delegate(ᜀ A_0, ᜀ A_1)
					{
						//Discarded unreachable code: IL_0023
						int num2 = 3;
						float num3;
						while (true)
						{
							switch (num2)
							{
							default:
								if (true)
								{
								}
								num2 = ((A_0.ᜃ != A_1.ᜃ) ? 2 : 0);
								continue;
							case 0:
								num3 = A_0.ᜄ - A_1.ᜄ;
								break;
							case 2:
								num2 = 1;
								continue;
							case 1:
								num3 = A_1.ᜃ - A_0.ᜃ;
								break;
							}
							break;
						}
						return (int)num3;
					});
					num = 3;
					break;
				case 3:
					return;
				}
			}
		}

		public void AddListener(Action<T1, T2> callback, object context = null, int priority = 0)
		{
			//Discarded unreachable code: IL_0026
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 3:
					this.ᜁ.Add(new ᜀ(callback, context, priority));
					num = 2;
					continue;
				case 2:
					this.ᜁ.Sort(delegate(ᜀ A_0, ᜀ A_1)
					{
						//Discarded unreachable code: IL_0023
						int num2 = 3;
						float num3;
						while (true)
						{
							switch (num2)
							{
							default:
								if (true)
								{
								}
								num2 = ((A_0.ᜃ != A_1.ᜃ) ? 2 : 0);
								continue;
							case 0:
								num3 = A_0.ᜄ - A_1.ᜄ;
								break;
							case 2:
								num2 = 1;
								continue;
							case 1:
								num3 = A_1.ᜃ - A_0.ᜃ;
								break;
							}
							break;
						}
						return (int)num3;
					});
					num = 0;
					continue;
				case 0:
					return;
				}
				if (callback != null)
				{
					if (true)
					{
					}
					num = 3;
					continue;
				}
				return;
			}
		}

		public void RemoveListener(Action callback)
		{
			//Discarded unreachable code: IL_0056
			while (true)
			{
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (callback != null)
						{
							num = 2;
							continue;
						}
						return;
					case 2:
						this.ᜁ.RemoveAll((ᜀ A_0) => A_0.ᜁ == callback);
						if (true)
						{
						}
						num = 1;
						continue;
					case 1:
						return;
					}
					break;
				}
			}
		}

		public void RemoveListener(Action<T1, T2> callback)
		{
			//Discarded unreachable code: IL_005e
			while (true)
			{
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 1:
						if (callback != null)
						{
							num = 0;
							continue;
						}
						return;
					case 0:
						this.ᜁ.RemoveAll((ᜀ A_0) => A_0.ᜂ == callback);
						num = 2;
						continue;
					case 2:
						if (1 == 0)
						{
						}
						return;
					}
					break;
				}
			}
		}

		public void RemoveListenerByContext(object context)
		{
			//Discarded unreachable code: IL_003e
			while (true)
			{
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
						if (context != null)
						{
							num = 1;
							continue;
						}
						return;
					case 1:
						if (true)
						{
						}
						this.ᜁ.RemoveAll((ᜀ A_0) => A_0.ᜀ == context);
						num = 2;
						continue;
					case 2:
						return;
					}
					break;
				}
			}
		}

		public void Dispatch(T1 param1, T2 param2)
		{
			//Discarded unreachable code: IL_0029
			while (true)
			{
				ᜀ[] array = this.ᜁ.ToArray();
				int num = 0;
				if (true)
				{
				}
				int num2 = 2;
				while (true)
				{
					switch (num2)
					{
					case 1:
					case 2:
						num2 = 0;
						continue;
					case 0:
					{
						if (num >= array.Length)
						{
							num2 = 3;
							continue;
						}
						ᜀ ᜀ = array[num];
						ᜀ.Invoke(param1, param2);
						num++;
						num2 = 1;
						continue;
					}
					case 3:
						return;
					}
					break;
				}
			}
		}

		public void Clear()
		{
			this.ᜁ.Clear();
		}
	}
	public class CSignal<T1, T2, T3>
	{
		private struct ᜃ
		{
			public readonly object ᜀ;

			public readonly Action ᜁ;

			public readonly Action<T1, T2, T3> ᜂ;

			public readonly int ᜃ;

			public readonly float ᜄ;

			public ᜃ(Action A_0, object A_1, int A_2)
			{
				ᜁ = A_0;
				ᜂ = null;
				ᜀ = A_1;
				ᜃ = A_2;
				ᜄ = DateTime.Now.Ticks;
			}

			public ᜃ(Action<T1, T2, T3> A_0, object A_1, int A_2)
			{
				ᜁ = null;
				ᜂ = A_0;
				ᜀ = A_1;
				ᜃ = A_2;
				ᜄ = DateTime.Now.Ticks;
			}

			public void Invoke(T1 A_0, T2 A_1, T3 A_2)
			{
				//Discarded unreachable code: IL_0033
				int num = 2;
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
							num = 0;
							break;
						}
						goto case 4;
					case 5:
						ᜂ(A_0, A_1, A_2);
						num = 3;
						break;
					case 3:
						return;
					case 4:
						num = 1;
						break;
					case 1:
						if (ᜂ != null)
						{
							num = 5;
							break;
						}
						return;
					case 0:
						ᜁ();
						num = 4;
						break;
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class ᜀ
		{
			public Action ᜀ;

			internal bool ᜁ(ᜃ A_0)
			{
				return A_0.ᜁ == ᜀ;
			}
		}

		[CompilerGenerated]
		private sealed class ᜂ
		{
			public Action<T1, T2, T3> ᜀ;

			internal bool ᜁ(ᜃ A_0)
			{
				return A_0.ᜂ == ᜀ;
			}
		}

		[CompilerGenerated]
		private sealed class ᜁ
		{
			public object ᜀ;

			internal bool ᜁ(ᜃ A_0)
			{
				return A_0.ᜀ == ᜀ;
			}
		}

		private const int m_ᜀ = 0;

		private readonly List<ᜃ> m_ᜁ;

		public CSignal()
		{
			this.ᜁ = new List<ᜃ>();
		}

		public void AddListener(Action callback, object context = null, int priority = 0)
		{
			//Discarded unreachable code: IL_0023
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (callback != null)
					{
						num = 0;
						break;
					}
					return;
				case 0:
					this.ᜁ.Add(new ᜃ(callback, context, priority));
					num = 3;
					break;
				case 3:
					this.ᜁ.Sort(delegate(ᜃ A_0, ᜃ A_1)
					{
						//Discarded unreachable code: IL_0023
						int num2 = 1;
						float num3;
						while (true)
						{
							switch (num2)
							{
							default:
								if (true)
								{
								}
								num2 = ((A_0.ᜃ != A_1.ᜃ) ? 2 : 0);
								continue;
							case 0:
								num3 = A_0.ᜄ - A_1.ᜄ;
								break;
							case 2:
								num2 = 3;
								continue;
							case 3:
								num3 = A_1.ᜃ - A_0.ᜃ;
								break;
							}
							break;
						}
						return (int)num3;
					});
					num = 1;
					break;
				case 1:
					return;
				}
			}
		}

		public void AddListener(Action<T1, T2, T3> callback, object context = null, int priority = 0)
		{
			//Discarded unreachable code: IL_0026
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 3:
					this.ᜁ.Add(new ᜃ(callback, context, priority));
					num = 0;
					continue;
				case 0:
					this.ᜁ.Sort(delegate(ᜃ A_0, ᜃ A_1)
					{
						//Discarded unreachable code: IL_0023
						int num2 = 3;
						float num3;
						while (true)
						{
							switch (num2)
							{
							default:
								if (true)
								{
								}
								num2 = ((A_0.ᜃ == A_1.ᜃ) ? 2 : 0);
								continue;
							case 2:
								num3 = A_0.ᜄ - A_1.ᜄ;
								break;
							case 0:
								num2 = 1;
								continue;
							case 1:
								num3 = A_1.ᜃ - A_0.ᜃ;
								break;
							}
							break;
						}
						return (int)num3;
					});
					num = 2;
					continue;
				case 2:
					return;
				}
				if (callback != null)
				{
					if (true)
					{
					}
					num = 3;
					continue;
				}
				return;
			}
		}

		public void RemoveListener(Action callback)
		{
			//Discarded unreachable code: IL_0056
			while (true)
			{
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (callback != null)
						{
							num = 1;
							continue;
						}
						return;
					case 1:
						this.ᜁ.RemoveAll((ᜃ A_0) => A_0.ᜁ == callback);
						if (true)
						{
						}
						num = 0;
						continue;
					case 0:
						return;
					}
					break;
				}
			}
		}

		public void RemoveListener(Action<T1, T2, T3> callback)
		{
			//Discarded unreachable code: IL_005e
			while (true)
			{
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (callback != null)
						{
							num = 1;
							continue;
						}
						return;
					case 1:
						this.ᜁ.RemoveAll((ᜃ A_0) => A_0.ᜂ == callback);
						num = 0;
						continue;
					case 0:
						if (1 == 0)
						{
						}
						return;
					}
					break;
				}
			}
		}

		public void RemoveListenerByContext(object context)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			this.ᜁ.RemoveAll((ᜃ A_0) => A_0.ᜀ == context);
		}

		public void Dispatch(T1 param1, T2 param2, T3 param3)
		{
			//Discarded unreachable code: IL_0029
			while (true)
			{
				ᜃ[] array = this.ᜁ.ToArray();
				int num = 0;
				if (true)
				{
				}
				int num2 = 2;
				while (true)
				{
					switch (num2)
					{
					case 0:
					case 2:
						num2 = 1;
						continue;
					case 1:
					{
						if (num >= array.Length)
						{
							num2 = 3;
							continue;
						}
						ᜃ ᜃ = array[num];
						ᜃ.Invoke(param1, param2, param3);
						num++;
						num2 = 0;
						continue;
					}
					case 3:
						return;
					}
					break;
				}
			}
		}

		public void Clear()
		{
			this.ᜁ.Clear();
		}
	}
}
