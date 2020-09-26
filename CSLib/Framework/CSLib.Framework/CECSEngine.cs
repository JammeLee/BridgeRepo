using System;
using System.Collections;
using System.Collections.Generic;
using CSLib.Utility;

namespace CSLib.Framework
{
	public class CECSEngine : CSingleton<CECSEngine>
	{
		private ulong id;

		public System.Action<IEnumerator> StartCoroutine;

		private List<AECSSystem> m_ᜁ = new List<AECSSystem>();

		private List<AECSSystem> m_ᜂ = new List<AECSSystem>();

		private Dictionary<ulong, CECSEntity> ᜃ = new Dictionary<ulong, CECSEntity>();

		private CObjectPool<CECSEntity> entityPool = new CObjectPool<CECSEntity>();

		public void FixedUpdate(float detlaTime)
		{
			//Discarded unreachable code: IL_001d
			while (true)
			{
				int num = 0;
				if (true)
				{
				}
				int num2 = 3;
				while (true)
				{
					switch (num2)
					{
					case 2:
					case 3:
						num2 = 1;
						continue;
					case 1:
						if (num >= this.m_ᜁ.Count)
						{
							num2 = 0;
							continue;
						}
						this.m_ᜁ[num].SystemProcess.Update(detlaTime);
						num++;
						num2 = 2;
						continue;
					case 0:
						return;
					}
					break;
				}
			}
		}

		public void Update(float detlaTime)
		{
			//Discarded unreachable code: IL_001d
			while (true)
			{
				int num = 0;
				if (true)
				{
				}
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					case 1:
					case 2:
						num2 = 3;
						continue;
					case 3:
						if (num >= this.m_ᜂ.Count)
						{
							num2 = 0;
							continue;
						}
						this.m_ᜂ[num].SystemProcess.Update(detlaTime);
						num++;
						num2 = 2;
						continue;
					case 0:
						return;
					}
					break;
				}
			}
		}

		public CECSEntity CreateEntity(List<AECSComponent> componentList)
		{
			//Discarded unreachable code: IL_00ff
			switch (0)
			{
			}
			int num3 = default(int);
			while (true)
			{
				CECSEntity cECSEntity = entityPool.Obtain();
				cECSEntity.BaseId = ++id;
				cECSEntity.InitComponents(componentList);
				int num = 0;
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					case 6:
						num3 = 0;
						num2 = 4;
						continue;
					case 0:
					case 4:
						num2 = 3;
						continue;
					case 3:
						if (num3 < this.m_ᜂ.Count)
						{
							this.m_ᜂ[num3].SystemProcess.AddEntity(cECSEntity);
							num3++;
							num2 = 0;
						}
						else
						{
							num2 = 7;
						}
						continue;
					case 1:
					case 5:
						if (true)
						{
						}
						num2 = 2;
						continue;
					case 2:
						if (num < this.m_ᜁ.Count)
						{
							this.m_ᜁ[num].SystemProcess.AddEntity(cECSEntity);
							num++;
							num2 = 5;
						}
						else
						{
							num2 = 6;
						}
						continue;
					case 7:
						ᜃ.Add(cECSEntity.BaseId, cECSEntity);
						return cECSEntity;
					}
					break;
				}
			}
		}

		public CECSEntity CreateEntity(object[] componentList)
		{
			//Discarded unreachable code: IL_017f
			switch (0)
			{
			}
			int num4 = default(int);
			int num3 = default(int);
			while (true)
			{
				CECSEntity cECSEntity = entityPool.Obtain();
				cECSEntity.BaseId = ++id;
				List<AECSComponent> list = new List<AECSComponent>();
				int num = 0;
				int num2 = 9;
				while (true)
				{
					switch (num2)
					{
					case 3:
						num4 = 0;
						num2 = 1;
						continue;
					case 4:
					case 10:
						num2 = 7;
						continue;
					case 7:
						if (num3 >= this.m_ᜁ.Count)
						{
							num2 = 3;
							continue;
						}
						this.m_ᜁ[num3].SystemProcess.AddEntity(cECSEntity);
						num3++;
						if (true)
						{
						}
						num2 = 4;
						continue;
					case 2:
					case 9:
						num2 = 6;
						continue;
					case 6:
					{
						if (num >= componentList.Length)
						{
							num2 = 11;
							continue;
						}
						object obj = componentList[num];
						list.Add((AECSComponent)obj);
						num++;
						num2 = 2;
						continue;
					}
					case 1:
					case 5:
						num2 = 0;
						continue;
					case 0:
						if (num4 >= this.m_ᜂ.Count)
						{
							num2 = 8;
							continue;
						}
						this.m_ᜂ[num4].SystemProcess.AddEntity(cECSEntity);
						num4++;
						num2 = 5;
						continue;
					case 11:
						cECSEntity.InitComponents(list);
						num3 = 0;
						num2 = 10;
						continue;
					case 8:
						ᜃ.Add(cECSEntity.BaseId, cECSEntity);
						return cECSEntity;
					}
					break;
				}
			}
		}

		public void RemoveEntity(CECSEntity entity)
		{
			//Discarded unreachable code: IL_009e
			int num3 = default(int);
			while (true)
			{
				int num = 0;
				int num2 = 3;
				while (true)
				{
					switch (num2)
					{
					case 5:
						num3 = 0;
						num2 = 2;
						continue;
					case 0:
					case 2:
						num2 = 4;
						continue;
					case 4:
						if (true)
						{
						}
						if (num3 < this.m_ᜂ.Count)
						{
							this.m_ᜂ[num3].SystemProcess.DelEntity(entity);
							num3++;
							num2 = 0;
						}
						else
						{
							num2 = 6;
						}
						continue;
					case 3:
					case 7:
						num2 = 1;
						continue;
					case 1:
						if (num < this.m_ᜁ.Count)
						{
							this.m_ᜁ[num].SystemProcess.DelEntity(entity);
							num++;
							num2 = 7;
						}
						else
						{
							num2 = 5;
						}
						continue;
					case 6:
						entity.Reset();
						ᜃ.Remove(entity.BaseId);
						return;
					}
					break;
				}
			}
		}

		public void RemoveEntities()
		{
			//Discarded unreachable code: IL_0073
			switch (0)
			{
			}
			CECSEntity value = default(CECSEntity);
			int num2 = default(int);
			while (true)
			{
				List<ulong> list = new List<ulong>(ᜃ.Keys);
				Dictionary<ulong, CECSEntity>.Enumerator enumerator = ᜃ.GetEnumerator();
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 5:
						RemoveEntity(value);
						num = 2;
						continue;
					case 2:
						if (1 == 0)
						{
						}
						goto IL_0154;
					case 7:
						if (ᜃ.TryGetValue(list[num2], out value))
						{
							num = 5;
							continue;
						}
						goto IL_0154;
					case 3:
					case 6:
						num = 0;
						continue;
					case 0:
						if (num2 < list.Count)
						{
							value = null;
							num = 7;
						}
						else
						{
							num = 4;
						}
						continue;
					case 4:
						return;
					case 1:
						{
							try
							{
								num = 2;
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
											num = 0;
											continue;
										}
										list.Add(enumerator.Current.Key);
										num = 1;
										continue;
									case 0:
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
							num2 = 0;
							num = 3;
							continue;
						}
						IL_0154:
						num2++;
						num = 6;
						continue;
					}
					break;
				}
			}
		}

		internal void ᜁ(CECSEntity entity)
		{
			//Discarded unreachable code: IL_0055
			int num3 = default(int);
			while (true)
			{
				int num = 0;
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					case 7:
						num3 = 0;
						num2 = 5;
						continue;
					case 5:
					case 6:
						num2 = 2;
						continue;
					case 2:
						if (num3 < this.m_ᜂ.Count)
						{
							this.m_ᜂ[num3].SystemProcess.AddEntity(entity);
							num3++;
							if (true)
							{
							}
							num2 = 6;
						}
						else
						{
							num2 = 4;
						}
						continue;
					case 4:
						return;
					case 0:
					case 1:
						num2 = 3;
						continue;
					case 3:
						if (num < this.m_ᜁ.Count)
						{
							this.m_ᜁ[num].SystemProcess.AddEntity(entity);
							num++;
							num2 = 0;
						}
						else
						{
							num2 = 7;
						}
						continue;
					}
					break;
				}
			}
		}

		private bool ᜂ(AECSSystem system)
		{
			//Discarded unreachable code: IL_00a0
			List<AECSSystem>.Enumerator enumerator = this.m_ᜁ.GetEnumerator();
			try
			{
				int num = 0;
				bool result = default(bool);
				while (true)
				{
					switch (num)
					{
					default:
						num = 5;
						continue;
					case 5:
						num = (enumerator.MoveNext() ? 6 : 2);
						continue;
					case 6:
						if (enumerator.Current.FullName == system.FullName)
						{
							num = 4;
							continue;
						}
						goto default;
					case 4:
						result = true;
						num = 1;
						continue;
					case 2:
						num = 3;
						continue;
					case 3:
						break;
					case 1:
						return result;
					}
					break;
				}
			}
			finally
			{
				if (true)
				{
				}
				((IDisposable)enumerator).Dispose();
			}
			return false;
		}

		public void AddLogicSystem(AECSSystem system)
		{
			//Discarded unreachable code: IL_00e8
			int a_ = 0;
			int num = 1;
			Dictionary<ulong, CECSEntity>.Enumerator enumerator = default(Dictionary<ulong, CECSEntity>.Enumerator);
			while (true)
			{
				switch (num)
				{
				default:
					if (ᜂ(system))
					{
						num = 0;
						break;
					}
					system.Engine = this;
					this.m_ᜁ.Add(system);
					enumerator = ᜃ.GetEnumerator();
					num = 2;
					break;
				case 2:
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
							default:
								num = 0;
								break;
							case 0:
								if (enumerator.MoveNext())
								{
									KeyValuePair<ulong, CECSEntity> current = enumerator.Current;
									system.SystemProcess.AddEntity(current.Value);
									num = 3;
								}
								else
								{
									num = 2;
								}
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
				case 0:
					CDebugOut.LogError(CMessageLabel.b("\ude83ᡍ띄籉\ue066쉢ס娠ጢ堤", a_), system.FullName);
					return;
				}
			}
		}

		private bool ᜁ(AECSSystem system)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			using (List<AECSSystem>.Enumerator enumerator = this.m_ᜂ.GetEnumerator())
			{
				int num = 5;
				bool result = default(bool);
				while (true)
				{
					switch (num)
					{
					default:
						num = 1;
						continue;
					case 1:
						num = (enumerator.MoveNext() ? 4 : 6);
						continue;
					case 4:
						if (enumerator.Current.FullName == system.FullName)
						{
							num = 2;
							continue;
						}
						goto default;
					case 2:
						result = true;
						num = 3;
						continue;
					case 6:
						num = 0;
						continue;
					case 0:
						break;
					case 3:
						return result;
					}
					break;
				}
			}
			return false;
		}

		public void AddFrameSystem(AECSSystem system)
		{
			//Discarded unreachable code: IL_0028
			int a_ = 13;
			int num = 2;
			Dictionary<ulong, CECSEntity>.Enumerator enumerator = default(Dictionary<ulong, CECSEntity>.Enumerator);
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (ᜁ(system))
					{
						num = 1;
						break;
					}
					system.Engine = this;
					this.m_ᜂ.Add(system);
					enumerator = ᜃ.GetEnumerator();
					num = 0;
					break;
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
								break;
							case 1:
								if (enumerator.MoveNext())
								{
									KeyValuePair<ulong, CECSEntity> current = enumerator.Current;
									system.SystemProcess.AddEntity(current.Value);
									num = 3;
								}
								else
								{
									num = 2;
								}
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
				case 1:
					CDebugOut.LogError(CMessageLabel.b("\ued8e⽸葱䍴퍛\uf557㛔唭/伱", a_), system.FullName);
					return;
				}
			}
		}

		public void InsertFrameSystem(AECSSystem system, AECSSystem searchSystem)
		{
			//Discarded unreachable code: IL_004e
			int a_ = 15;
			int num = 4;
			int num2 = default(int);
			Dictionary<ulong, CECSEntity>.Enumerator enumerator = default(Dictionary<ulong, CECSEntity>.Enumerator);
			while (true)
			{
				switch (num)
				{
				default:
					if (ᜁ(system))
					{
						num = 6;
						break;
					}
					system.Engine = this;
					num2 = this.m_ᜂ.IndexOf(searchSystem);
					num = 3;
					break;
				case 1:
				case 2:
					if (true)
					{
					}
					enumerator = ᜃ.GetEnumerator();
					num = 0;
					break;
				case 3:
					if (num2 < 0)
					{
						this.m_ᜂ.Add(system);
						CDebugOut.LogWarning(CMessageLabel.b("儡崣唥尧伩䄫ጭ䬯ȱ䤳㭻恬ቮ㷄", a_), searchSystem.FullName);
						num = 1;
					}
					else
					{
						num = 5;
					}
					break;
				case 6:
					CDebugOut.LogError(CMessageLabel.b("\uefb0⥺虷䵶텕\uf355㓒䬯ȱ䤳", a_), system.FullName);
					return;
				case 0:
					try
					{
						num = 2;
						while (true)
						{
							switch (num)
							{
							default:
								num = 1;
								break;
							case 1:
							{
								if (!enumerator.MoveNext())
								{
									num = 3;
									break;
								}
								KeyValuePair<ulong, CECSEntity> current = enumerator.Current;
								system.SystemProcess.AddEntity(current.Value);
								num = 0;
								break;
							}
							case 3:
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
				case 5:
					this.m_ᜂ.Insert(num2, system);
					num = 2;
					break;
				}
			}
		}

		public T GetSystem<T>() where T : AECSSystem
		{
			//Discarded unreachable code: IL_006e
			switch (0)
			{
			}
			T val2 = default(T);
			int num3 = default(int);
			T val = default(T);
			while (true)
			{
				int num = 0;
				int num2 = 10;
				while (true)
				{
					switch (num2)
					{
					case 7:
						return val2;
					case 2:
						num3 = 0;
						num2 = 3;
						continue;
					case 9:
						return val;
					case 0:
						if (val != null)
						{
							num2 = 9;
							continue;
						}
						num++;
						num2 = 8;
						continue;
					case 6:
						if (val2 == null)
						{
							if (true)
							{
							}
							num3++;
							num2 = 4;
						}
						else
						{
							num2 = 7;
						}
						continue;
					case 3:
					case 4:
						num2 = 11;
						continue;
					case 11:
						if (num3 < this.m_ᜂ.Count)
						{
							val2 = this.m_ᜂ[num3] as T;
							num2 = 6;
						}
						else
						{
							num2 = 1;
						}
						continue;
					case 8:
					case 10:
						num2 = 5;
						continue;
					case 5:
						if (num < this.m_ᜁ.Count)
						{
							val = this.m_ᜁ[num] as T;
							num2 = 0;
						}
						else
						{
							num2 = 2;
						}
						continue;
					case 1:
						return null;
					}
					break;
				}
			}
		}

		public bool GetSystem<T>(out T system) where T : AECSSystem
		{
			//Discarded unreachable code: IL_009c
			int num3 = default(int);
			while (true)
			{
				system = null;
				int num = 0;
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					case 7:
						return true;
					case 8:
						num3 = 0;
						num2 = 10;
						continue;
					case 6:
						return true;
					case 5:
						if (true)
						{
						}
						if (system != null)
						{
							num2 = 6;
							continue;
						}
						num++;
						num2 = 0;
						continue;
					case 4:
						if (system == null)
						{
							num3++;
							num2 = 2;
						}
						else
						{
							num2 = 7;
						}
						continue;
					case 2:
					case 10:
						num2 = 3;
						continue;
					case 3:
						if (num3 < this.m_ᜂ.Count)
						{
							system = this.m_ᜂ[num3] as T;
							num2 = 4;
						}
						else
						{
							num2 = 11;
						}
						continue;
					case 0:
					case 1:
						num2 = 9;
						continue;
					case 9:
						if (num < this.m_ᜁ.Count)
						{
							system = this.m_ᜁ[num] as T;
							num2 = 5;
						}
						else
						{
							num2 = 8;
						}
						continue;
					case 11:
						return false;
					}
					break;
				}
			}
		}

		public bool GetSystem(string name, out AECSSystem system, bool fullName = false)
		{
			//Discarded unreachable code: IL_005b
			int num3 = default(int);
			while (true)
			{
				if (true)
				{
				}
				system = null;
				int num = 0;
				int num2 = 13;
				while (true)
				{
					switch (num2)
					{
					case 16:
						system = this.m_ᜂ[num3];
						return true;
					case 15:
						if (fullName)
						{
							num2 = 18;
							continue;
						}
						goto IL_01cb;
					case 9:
						system = this.m_ᜁ[num];
						return true;
					case 19:
						system = this.m_ᜂ[num3];
						return true;
					case 1:
						num2 = 5;
						continue;
					case 5:
						if (this.m_ᜁ[num].Name.Contains(name))
						{
							num2 = 9;
							continue;
						}
						goto IL_0183;
					case 18:
						num2 = 0;
						continue;
					case 0:
						if (this.m_ᜂ[num3].Name.Contains(name))
						{
							num2 = 16;
							continue;
						}
						goto IL_01cb;
					case 3:
					case 10:
						num2 = 8;
						continue;
					case 8:
						num2 = ((num3 >= this.m_ᜂ.Count) ? 14 : 15);
						continue;
					case 11:
					case 13:
						num2 = 6;
						continue;
					case 6:
						num2 = ((num < this.m_ᜁ.Count) ? 2 : 4);
						continue;
					case 17:
						if (this.m_ᜁ[num].Name.Equals(name))
						{
							num2 = 12;
							continue;
						}
						num++;
						num2 = 11;
						continue;
					case 7:
						if (!this.m_ᜂ[num3].Name.Equals(name))
						{
							num3++;
							num2 = 10;
						}
						else
						{
							num2 = 19;
						}
						continue;
					case 12:
						system = this.m_ᜁ[num];
						return true;
					case 2:
						if (fullName)
						{
							num2 = 1;
							continue;
						}
						goto IL_0183;
					case 4:
						num3 = 0;
						num2 = 3;
						continue;
					case 14:
						{
							return false;
						}
						IL_01cb:
						num2 = 7;
						continue;
						IL_0183:
						num2 = 17;
						continue;
					}
					break;
				}
			}
		}

		public AECSSystem GetSystem(string name, bool fullName = false)
		{
			//Discarded unreachable code: IL_0203
			int num3 = default(int);
			while (true)
			{
				int num = 0;
				int num2 = 19;
				while (true)
				{
					switch (num2)
					{
					case 11:
						return this.m_ᜂ[num3];
					case 5:
						if (fullName)
						{
							num2 = 13;
							continue;
						}
						goto IL_01b4;
					case 2:
						return this.m_ᜁ[num];
					case 7:
						return this.m_ᜂ[num3];
					case 6:
						num2 = 12;
						continue;
					case 12:
						if (this.m_ᜂ[num].Name.Contains(name))
						{
							num2 = 2;
							continue;
						}
						goto IL_016c;
					case 13:
						num2 = 8;
						continue;
					case 8:
						if (this.m_ᜂ[num3].Name.Contains(name))
						{
							num2 = 11;
							continue;
						}
						goto IL_01b4;
					case 3:
					case 18:
						num2 = 0;
						continue;
					case 0:
						num2 = ((num3 >= this.m_ᜂ.Count) ? 4 : 5);
						continue;
					case 10:
					case 19:
						num2 = 14;
						continue;
					case 14:
						num2 = ((num < this.m_ᜁ.Count) ? 15 : 16);
						continue;
					case 9:
						if (this.m_ᜁ[num].Name.Equals(name))
						{
							num2 = 1;
							continue;
						}
						num++;
						num2 = 10;
						continue;
					case 17:
						if (!this.m_ᜂ[num3].Name.Equals(name))
						{
							num3++;
							num2 = 18;
						}
						else
						{
							num2 = 7;
						}
						continue;
					case 1:
						return this.m_ᜁ[num];
					case 15:
						if (true)
						{
						}
						if (fullName)
						{
							num2 = 6;
							continue;
						}
						goto IL_016c;
					case 16:
						num3 = 0;
						num2 = 3;
						continue;
					case 4:
						{
							return null;
						}
						IL_01b4:
						num2 = 17;
						continue;
						IL_016c:
						num2 = 9;
						continue;
					}
					break;
				}
			}
		}

		public bool DelSystem(AECSSystem system)
		{
			//Discarded unreachable code: IL_00a5
			int num3 = default(int);
			while (true)
			{
				int num = 0;
				int num2 = 6;
				while (true)
				{
					switch (num2)
					{
					case 10:
						this.m_ᜂ[num3].SystemProcess.Destroy();
						this.m_ᜂ.RemoveAt(num3);
						return true;
					case 7:
						num3 = 0;
						num2 = 3;
						continue;
					case 1:
						this.m_ᜁ[num].SystemProcess.Destroy();
						this.m_ᜁ.RemoveAt(num);
						return true;
					case 5:
						if (this.m_ᜁ[num] == system)
						{
							num2 = 1;
							continue;
						}
						num++;
						num2 = 4;
						continue;
					case 8:
						if (this.m_ᜂ[num3] != system)
						{
							num3++;
							if (true)
							{
							}
							num2 = 2;
						}
						else
						{
							num2 = 10;
						}
						continue;
					case 2:
					case 3:
						num2 = 9;
						continue;
					case 9:
						num2 = ((num3 < this.m_ᜂ.Count) ? 8 : 0);
						continue;
					case 4:
					case 6:
						num2 = 11;
						continue;
					case 11:
						num2 = ((num >= this.m_ᜁ.Count) ? 7 : 5);
						continue;
					case 0:
						return false;
					}
					break;
				}
			}
		}

		public bool DelSystem<T>() where T : AECSSystem
		{
			//Discarded unreachable code: IL_0171
			int num3 = default(int);
			while (true)
			{
				int num = 0;
				int num2 = 8;
				while (true)
				{
					switch (num2)
					{
					case 3:
						this.m_ᜂ[num3].SystemProcess.Destroy();
						this.m_ᜂ.RemoveAt(num3);
						return true;
					case 6:
						num3 = 0;
						num2 = 1;
						continue;
					case 11:
						this.m_ᜁ[num].SystemProcess.Destroy();
						this.m_ᜁ.RemoveAt(num);
						return true;
					case 4:
						if (this.m_ᜁ[num] is T)
						{
							num2 = 11;
							continue;
						}
						num++;
						num2 = 2;
						continue;
					case 10:
						if (!(this.m_ᜂ[num3] is T))
						{
							num3++;
							num2 = 9;
						}
						else
						{
							num2 = 3;
						}
						continue;
					case 1:
					case 9:
						num2 = 5;
						continue;
					case 5:
						num2 = ((num3 < this.m_ᜂ.Count) ? 10 : 0);
						continue;
					case 2:
					case 8:
						num2 = 7;
						continue;
					case 7:
						num2 = ((num >= this.m_ᜁ.Count) ? 6 : 4);
						continue;
					case 0:
						if (true)
						{
						}
						return false;
					}
					break;
				}
			}
		}
	}
}
