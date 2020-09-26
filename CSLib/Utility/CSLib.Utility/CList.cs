using System;
using System.Collections.Generic;

namespace CSLib.Utility
{
	public class CList<OBJECT>
	{
		public delegate void CbTraversalHandler(OBJECT obj);

		private ushort ᜀ;

		private List<OBJECT> ᜁ = new List<OBJECT>();

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

		public List<OBJECT> Objects => ᜁ;

		public bool AddObject(OBJECT obj)
		{
			//Discarded unreachable code: IL_0034
			int num = 5;
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
						break;
					}
					goto IL_0043;
				case 4:
					if (ᜁ.Contains(obj))
					{
						num = 3;
						break;
					}
					ᜁ.Add(obj);
					return true;
				case 2:
					return false;
				case 0:
					num = 1;
					break;
				case 1:
					if (Limit <= ᜁ.Count)
					{
						num = 2;
						break;
					}
					goto IL_0043;
				case 3:
					{
						return false;
					}
					IL_0043:
					num = 4;
					break;
				}
			}
		}

		public bool DelObject(OBJECT obj)
		{
			//Discarded unreachable code: IL_0011
			if (ᜁ.Contains(obj))
			{
				if (true)
				{
				}
				ᜁ.Remove(obj);
				return true;
			}
			return false;
		}

		public void Traversal(CbTraversalHandler cbHandler)
		{
			//Discarded unreachable code: IL_00dd
			int num = 1;
			List<OBJECT>.Enumerator enumerator = default(List<OBJECT>.Enumerator);
			OBJECT current = default(OBJECT);
			while (true)
			{
				switch (num)
				{
				default:
					if (cbHandler == null)
					{
						num = 0;
						break;
					}
					enumerator = ᜁ.GetEnumerator();
					num = 2;
					break;
				case 0:
					return;
				case 2:
					if (true)
					{
					}
					try
					{
						num = 0;
						while (true)
						{
							switch (num)
							{
							case 5:
								if (current != null)
								{
									num = 6;
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
									num = 5;
								}
								else
								{
									num = 2;
								}
								break;
							case 6:
								cbHandler(current);
								num = 1;
								break;
							case 2:
								num = 4;
								break;
							case 4:
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
