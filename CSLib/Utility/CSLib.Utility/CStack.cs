using System;
using System.Collections.Generic;

namespace CSLib.Utility
{
	public class CStack<OBJECT> where OBJECT : new()
	{
		public delegate void CbTraversalHandler(OBJECT obj);

		private ushort ᜀ;

		private Stack<OBJECT> ᜁ = new Stack<OBJECT>();

		public ushort Limit
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

		public Stack<OBJECT> Objects => ᜁ;

		public bool Push(OBJECT obj)
		{
			//Discarded unreachable code: IL_0038
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (Limit > 0)
					{
						if (true)
						{
						}
						num = 0;
						continue;
					}
					goto IL_0047;
				case 6:
					if (obj != null)
					{
						num = 4;
						continue;
					}
					break;
				case 1:
					return false;
				case 0:
					num = 5;
					continue;
				case 5:
					if (Limit <= ᜁ.Count)
					{
						num = 1;
						continue;
					}
					goto IL_0047;
				case 4:
					ᜁ.Push(obj);
					num = 2;
					continue;
				case 2:
					break;
					IL_0047:
					num = 6;
					continue;
				}
				break;
			}
			return true;
		}

		public OBJECT Pop()
		{
			//Discarded unreachable code: IL_0011
			if (ᜁ.Count > 0)
			{
				if (true)
				{
				}
				return ᜁ.Pop();
			}
			return new OBJECT();
		}

		public void Traversal(CbTraversalHandler cbHandler)
		{
			//Discarded unreachable code: IL_00dd
			int num = 2;
			Stack<OBJECT>.Enumerator enumerator = default(Stack<OBJECT>.Enumerator);
			OBJECT current = default(OBJECT);
			while (true)
			{
				switch (num)
				{
				default:
					if (cbHandler == null)
					{
						num = 1;
						break;
					}
					enumerator = ᜁ.GetEnumerator();
					num = 0;
					break;
				case 1:
					return;
				case 0:
					if (true)
					{
					}
					try
					{
						num = 4;
						while (true)
						{
							switch (num)
							{
							case 6:
								if (current != null)
								{
									num = 0;
									break;
								}
								goto default;
							default:
								num = 3;
								break;
							case 3:
								if (enumerator.MoveNext())
								{
									current = enumerator.Current;
									num = 6;
								}
								else
								{
									num = 5;
								}
								break;
							case 0:
								cbHandler(current);
								num = 1;
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
						((IDisposable)enumerator).Dispose();
					}
				}
			}
		}
	}
}
