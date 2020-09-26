using System;
using System.Collections.Generic;

namespace CSLib.Utility
{
	public class CQueue<OBJECT>
	{
		public delegate void CbTraversalHandler(OBJECT obj);

		private ushort ᜀ;

		private Queue<OBJECT> ᜁ = new Queue<OBJECT>();

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

		public Queue<OBJECT> Objects => ᜁ;

		public bool AddObject(OBJECT obj)
		{
			//Discarded unreachable code: IL_0034
			int num = 2;
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
						num = 1;
						break;
					}
					goto IL_0043;
				case 0:
					if (ᜁ.Contains(obj))
					{
						num = 3;
						break;
					}
					ᜁ.Enqueue(obj);
					return true;
				case 5:
					return false;
				case 1:
					num = 4;
					break;
				case 4:
					if (Limit <= ᜁ.Count)
					{
						num = 5;
						break;
					}
					goto IL_0043;
				case 3:
					{
						return false;
					}
					IL_0043:
					num = 0;
					break;
				}
			}
		}

		public bool DelObject(OBJECT obj)
		{
			//Discarded unreachable code: IL_0011
			switch (0)
			{
			default:
			{
				if (true)
				{
				}
				bool result = false;
				Queue<OBJECT> queue = new Queue<OBJECT>();
				using (Queue<OBJECT>.Enumerator enumerator = ᜁ.GetEnumerator())
				{
					int num = 0;
					OBJECT current = default(OBJECT);
					while (true)
					{
						switch (num)
						{
						case 4:
							if (!current.Equals(obj))
							{
								num = 5;
								continue;
							}
							result = true;
							num = 7;
							continue;
						default:
							num = 2;
							continue;
						case 2:
							if (enumerator.MoveNext())
							{
								current = enumerator.Current;
								num = 4;
							}
							else
							{
								num = 6;
							}
							continue;
						case 5:
							queue.Enqueue(current);
							num = 3;
							continue;
						case 6:
							num = 1;
							continue;
						case 1:
							break;
						}
						break;
					}
				}
				ᜁ = queue;
				return result;
			}
			}
		}

		public void Traversal(CbTraversalHandler cbHandler)
		{
			//Discarded unreachable code: IL_00dd
			int num = 2;
			Queue<OBJECT>.Enumerator enumerator = default(Queue<OBJECT>.Enumerator);
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
							case 2:
								if (current != null)
								{
									num = 5;
									break;
								}
								goto default;
							default:
								num = 1;
								break;
							case 1:
								if (enumerator.MoveNext())
								{
									current = enumerator.Current;
									num = 2;
								}
								else
								{
									num = 3;
								}
								break;
							case 5:
								cbHandler(current);
								num = 6;
								break;
							case 3:
								num = 0;
								break;
							case 0:
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
