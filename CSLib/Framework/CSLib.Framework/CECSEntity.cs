using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CSLib.Utility;

namespace CSLib.Framework
{
	public class CECSEntity : CObjectPool<CECSEntity>.IPoolable
	{
		[CompilerGenerated]
		private ulong _baseId;

		private List<AECSComponent> compoenets = new List<AECSComponent>();

		public ulong BaseId
		{
			[CompilerGenerated]
			get
			{
				return _baseId;
			}
			[CompilerGenerated]
			set
			{
				_baseId = value;
			}
		}

		public void InitComponents(List<AECSComponent> componentList)
		{
			//Discarded unreachable code: IL_001f
			int num = 2;
			List<AECSComponent>.Enumerator enumerator = default(List<AECSComponent>.Enumerator);
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (componentList == null)
					{
						num = 1;
						break;
					}
					compoenets = componentList;
					enumerator = compoenets.GetEnumerator();
					num = 0;
					break;
				case 1:
					return;
				case 0:
					try
					{
						num = 3;
						while (true)
						{
							switch (num)
							{
							default:
								num = 4;
								break;
							case 4:
								if (enumerator.MoveNext())
								{
									enumerator.Current.Entity = this;
									num = 0;
								}
								else
								{
									num = 1;
								}
								break;
							case 1:
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

		public void AddComponent(AECSComponent component)
		{
			//Discarded unreachable code: IL_001f
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 0:
					compoenets.Add(component);
					component.Entity = this;
					CSingleton<CECSEngine>.Instance.ᜁ(this);
					num = 1;
					continue;
				case 1:
					return;
				}
				if (true)
				{
				}
				if (!compoenets.Contains(component))
				{
					num = 0;
					continue;
				}
				return;
			}
		}

		public void DelComponent(AECSComponent component)
		{
			//Discarded unreachable code: IL_005e
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (compoenets.Contains(component))
					{
						num = 1;
						break;
					}
					return;
				case 1:
					compoenets.Remove(component);
					component.Entity = null;
					CSingleton<CECSEngine>.Instance.ᜁ(this);
					num = 0;
					break;
				case 0:
					if (1 == 0)
					{
					}
					return;
				}
			}
		}

		public void AddComponents(List<AECSComponent> components)
		{
			//Discarded unreachable code: IL_00e0
			int num = 5;
			int num2 = default(int);
			while (true)
			{
				switch (num)
				{
				default:
					if (components != null)
					{
						num = 7;
						continue;
					}
					return;
				case 4:
				case 9:
					num = 0;
					continue;
				case 0:
					num = ((num2 < components.Count) ? 3 : 6);
					continue;
				case 7:
					num2 = 0;
					num = 4;
					continue;
				case 6:
					CSingleton<CECSEngine>.Instance.ᜁ(this);
					num = 8;
					continue;
				case 8:
					return;
				case 3:
					if (!compoenets.Contains(components[num2]))
					{
						num = 1;
						continue;
					}
					break;
				case 1:
					if (true)
					{
					}
					compoenets.Add(components[num2]);
					components[num2].Entity = this;
					num = 2;
					continue;
				case 2:
					break;
				}
				num2++;
				num = 9;
			}
		}

		public void DelComponents(List<AECSComponent> components)
		{
			//Discarded unreachable code: IL_00b8
			int num = 9;
			int num2 = default(int);
			while (true)
			{
				switch (num)
				{
				default:
					if (components != null)
					{
						num = 5;
						continue;
					}
					return;
				case 1:
				case 3:
					num = 2;
					continue;
				case 2:
					num = ((num2 < components.Count) ? 8 : 6);
					continue;
				case 0:
					compoenets.Remove(components[num2]);
					components[num2].Entity = null;
					num = 7;
					continue;
				case 5:
					num2 = 0;
					num = 1;
					continue;
				case 6:
					CSingleton<CECSEngine>.Instance.ᜁ(this);
					num = 4;
					continue;
				case 4:
					if (1 == 0)
					{
					}
					return;
				case 8:
					if (!compoenets.Contains(components[num2]))
					{
						num = 0;
						continue;
					}
					break;
				case 7:
					break;
				}
				num2++;
				num = 3;
			}
		}

		public bool HasComponent<T>() where T : AECSComponent
		{
			return GetComponent<T>() != null;
		}

		public bool HasComponent(Type type)
		{
			//Discarded unreachable code: IL_002f
			while (true)
			{
				int num = 0;
				int num2 = 2;
				while (true)
				{
					switch (num2)
					{
					case 4:
						return true;
					case 3:
						if (compoenets[num].GetType() != type)
						{
							if (true)
							{
							}
							num++;
							num2 = 1;
						}
						else
						{
							num2 = 4;
						}
						continue;
					case 1:
					case 2:
						num2 = 0;
						continue;
					case 0:
						num2 = ((num >= compoenets.Count) ? 5 : 3);
						continue;
					case 5:
						return false;
					}
					break;
				}
			}
		}

		public bool HasComponents(IEnumerable<Type> types)
		{
			//Discarded unreachable code: IL_000c
			IEnumerator<Type> enumerator = types.GetEnumerator();
			try
			{
				int num = 5;
				Type current = default(Type);
				bool result = default(bool);
				while (true)
				{
					switch (num)
					{
					default:
						num = 3;
						continue;
					case 3:
						if (!enumerator.MoveNext())
						{
							num = 4;
							continue;
						}
						current = enumerator.Current;
						num = 6;
						continue;
					case 6:
						if (!HasComponent(current))
						{
							num = 0;
							continue;
						}
						goto default;
					case 0:
						result = false;
						num = 1;
						continue;
					case 4:
						num = 2;
						continue;
					case 2:
						break;
					case 1:
						return result;
					}
					break;
				}
			}
			finally
			{
				int num = 1;
				while (true)
				{
					switch (num)
					{
					default:
						if (enumerator != null)
						{
							num = 0;
							continue;
						}
						break;
					case 0:
						enumerator.Dispose();
						num = 2;
						continue;
					case 2:
						break;
					}
					break;
				}
			}
			if (true)
			{
			}
			return true;
		}

		public T GetComponent<T>() where T : AECSComponent
		{
			//Discarded unreachable code: IL_005e
			T val = default(T);
			while (true)
			{
				int num = 0;
				int num2 = 0;
				while (true)
				{
					switch (num2)
					{
					case 3:
						return val;
					case 5:
						if (true)
						{
						}
						if (val == null)
						{
							num++;
							num2 = 1;
						}
						else
						{
							num2 = 3;
						}
						continue;
					case 0:
					case 1:
						num2 = 4;
						continue;
					case 4:
						if (num < compoenets.Count)
						{
							val = compoenets[num] as T;
							num2 = 5;
						}
						else
						{
							num2 = 2;
						}
						continue;
					case 2:
						return null;
					}
					break;
				}
			}
		}

		public AECSComponent GetComponent(Type componentType)
		{
			//Discarded unreachable code: IL_002d
			while (true)
			{
				int num = 0;
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					case 1:
						if (1 == 0)
						{
						}
						goto case 0;
					case 4:
						return compoenets[num];
					case 3:
						if (compoenets[num].GetType() != componentType)
						{
							num++;
							num2 = 0;
						}
						else
						{
							num2 = 4;
						}
						continue;
					case 0:
						num2 = 2;
						continue;
					case 2:
						num2 = ((num >= compoenets.Count) ? 5 : 3);
						continue;
					case 5:
						return null;
					}
					break;
				}
			}
		}

		public AECSComponent GetComponent(string name)
		{
			//Discarded unreachable code: IL_007d
			while (true)
			{
				int num = 0;
				int num2 = 5;
				while (true)
				{
					switch (num2)
					{
					case 3:
						return compoenets[num];
					case 0:
						if (!compoenets[num].Name.Equals(name))
						{
							num++;
							num2 = 4;
						}
						else
						{
							num2 = 3;
						}
						continue;
					case 4:
					case 5:
						num2 = 2;
						continue;
					case 2:
						if (true)
						{
						}
						num2 = ((num >= compoenets.Count) ? 1 : 0);
						continue;
					case 1:
						return null;
					}
					break;
				}
			}
		}

		public bool GetComponent<T>(out T component) where T : AECSComponent
		{
			//Discarded unreachable code: IL_0005
			while (true)
			{
				component = null;
				int num = 0;
				int num2 = 2;
				while (true)
				{
					if (true)
					{
					}
					switch (num2)
					{
					case 5:
						return true;
					case 1:
						if (component == null)
						{
							num++;
							num2 = 4;
						}
						else
						{
							num2 = 5;
						}
						continue;
					case 2:
					case 4:
						num2 = 3;
						continue;
					case 3:
						if (num < compoenets.Count)
						{
							component = compoenets[num] as T;
							num2 = 1;
						}
						else
						{
							num2 = 0;
						}
						continue;
					case 0:
						return false;
					}
					break;
				}
			}
		}

		public virtual void DebugLogOut()
		{
			int a_ = 0;
			CDebugOut.Log(CMessageLabel.b("娒㔔瘖琘㬚昜⼞尠Ȣ", a_) + BaseId);
		}

		public void Destroy()
		{
			CSingleton<CECSEngine>.Instance.RemoveEntity(this);
		}

		public void Reset()
		{
			//Discarded unreachable code: IL_0035
			using (List<AECSComponent>.Enumerator enumerator = compoenets.GetEnumerator())
			{
				int num = 4;
				while (true)
				{
					switch (num)
					{
					default:
						if (1 == 0)
						{
						}
						goto case 0;
					case 0:
						num = 1;
						continue;
					case 1:
						if (enumerator.MoveNext())
						{
							enumerator.Current.Clear();
							num = 0;
						}
						else
						{
							num = 2;
						}
						continue;
					case 2:
						num = 3;
						continue;
					case 3:
						break;
					}
					break;
				}
			}
			compoenets.Clear();
		}
	}
}
