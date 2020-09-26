using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CSLib.Utility
{
	public class CObjectPool<T> where T : class, new()
	{
		public interface IPoolable
		{
			void Reset();
		}

		public readonly int max;

		private readonly Stack<T> ᜀ;

		[CompilerGenerated]
		private int ᜁ;

		public int Count => ᜀ.Count;

		public int Peak
		{
			[CompilerGenerated]
			get
			{
				return ᜁ;
			}
			[CompilerGenerated]
			private set
			{
				ᜁ = value;
			}
		}

		public CObjectPool(int initialCapacity = 16, int max = int.MaxValue)
		{
			ᜀ = new Stack<T>(initialCapacity);
			this.max = max;
		}

		public T Obtain()
		{
			if (ᜀ.Count != 0)
			{
				return ᜀ.Pop();
			}
			return new T();
		}

		public void Free(T obj)
		{
			//Discarded unreachable code: IL_0030
			int a_ = 10;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					num = ((obj != null) ? 4 : 0);
					continue;
				case 3:
					ᜀ.Push(obj);
					Peak = Math.Max(Peak, ᜀ.Count);
					num = 1;
					continue;
				case 4:
					if (ᜀ.Count < max)
					{
						num = 3;
						continue;
					}
					break;
				case 0:
					throw new ArgumentNullException(CSimpleThreadPool.b("⥅⩇⁉", a_), CSimpleThreadPool.b("⥅⩇⁉汋ⵍㅏ㱑㩓㥕ⱗ穙㹛㭝䁟ౡᅣ\u0a65ѧ", a_));
				case 1:
					break;
				}
				break;
			}
			Reset(obj);
		}

		public void Clear()
		{
			ᜀ.Clear();
		}

		protected void Reset(T obj)
		{
			(obj as IPoolable)?.Reset();
		}
	}
}
