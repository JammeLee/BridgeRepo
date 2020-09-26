using System;
using System.Collections.Generic;

namespace CSLib.Utility
{
	public class CDictionary<KEY, OBJECT>
	{
		public delegate void CbTraversalHandler(KEY key, OBJECT obj);

		private ushort ᜀ;

		private Dictionary<KEY, OBJECT> ᜁ = new Dictionary<KEY, OBJECT>();

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

		public Dictionary<KEY, OBJECT> Objects => ᜁ;

		public OBJECT NewObject(KEY key)
		{
			//Discarded unreachable code: IL_0039
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (ᜁ.ContainsKey(key))
					{
						if (true)
						{
						}
						num = 1;
					}
					else
					{
						num = 4;
					}
					continue;
				case 5:
					num = 3;
					continue;
				case 3:
					if (Limit <= ᜁ.Count)
					{
						num = 2;
						continue;
					}
					break;
				case 4:
					if (Limit > 0)
					{
						num = 5;
						continue;
					}
					break;
				case 1:
					return ᜁ[key];
				case 2:
					return default(OBJECT);
				}
				break;
			}
			OBJECT val = (OBJECT)Activator.CreateInstance(typeof(OBJECT), nonPublic: true);
			ᜁ.Add(key, val);
			return val;
		}

		public bool AddObject(KEY key, OBJECT obj)
		{
			//Discarded unreachable code: IL_0034
			int num = 0;
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
						num = 4;
						break;
					}
					goto IL_0043;
				case 1:
					if (ᜁ.ContainsKey(key))
					{
						num = 3;
						break;
					}
					ᜁ.Add(key, obj);
					return true;
				case 5:
					return false;
				case 4:
					num = 2;
					break;
				case 2:
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
					num = 1;
					break;
				}
			}
		}

		public bool SetObject(KEY key, OBJECT obj)
		{
			//Discarded unreachable code: IL_0022
			if (ᜁ.ContainsKey(key))
			{
				ᜁ[key] = obj;
				return true;
			}
			if (true)
			{
			}
			return false;
		}

		public bool ContainsKey(KEY key)
		{
			return ᜁ.ContainsKey(key);
		}

		public OBJECT GetObject(KEY key)
		{
			//Discarded unreachable code: IL_001f
			OBJECT value = default(OBJECT);
			if (ᜁ.TryGetValue(key, out value))
			{
				return value;
			}
			if (true)
			{
			}
			return default(OBJECT);
		}

		public bool DelObject(KEY key)
		{
			//Discarded unreachable code: IL_0013
			if (ᜁ.ContainsKey(key))
			{
				if (true)
				{
				}
				ᜁ.Remove(key);
				return true;
			}
			return false;
		}

		public void Traversal(CbTraversalHandler cbHandler)
		{
			//Discarded unreachable code: IL_001f
			int num = 1;
			Dictionary<KEY, OBJECT>.Enumerator enumerator = default(Dictionary<KEY, OBJECT>.Enumerator);
			KeyValuePair<KEY, OBJECT> current = default(KeyValuePair<KEY, OBJECT>);
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (cbHandler == null)
					{
						num = 2;
						break;
					}
					enumerator = ᜁ.GetEnumerator();
					num = 0;
					break;
				case 2:
					return;
				case 0:
					try
					{
						num = 4;
						while (true)
						{
							switch (num)
							{
							case 6:
								if (current.Value != null)
								{
									num = 5;
									break;
								}
								goto default;
							default:
								num = 0;
								break;
							case 0:
								if (enumerator.MoveNext())
								{
									current = enumerator.Current;
									num = 6;
								}
								else
								{
									num = 2;
								}
								break;
							case 5:
								cbHandler(current.Key, current.Value);
								num = 3;
								break;
							case 2:
								num = 1;
								break;
							case 1:
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
