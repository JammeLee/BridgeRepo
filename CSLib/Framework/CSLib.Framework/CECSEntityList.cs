using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CSLib.Utility;

namespace CSLib.Framework
{
	public class CECSEntityList
	{
		public CECSEntityNode head;

		private CECSEntityNode m_ᜀ;

		[CompilerGenerated]
		private uint m_ᜁ;

		private static CStack<CECSEntityNode> m_ᜂ = new CStack<CECSEntityNode>();

		public uint Count
		{
			[CompilerGenerated]
			get
			{
				return this.m_ᜁ;
			}
			[CompilerGenerated]
			private set
			{
				this.m_ᜁ = value;
			}
		}

		public CECSEntityList()
		{
			Count = 0u;
		}

		public void AddEntity(CECSEntity entity)
		{
			//Discarded unreachable code: IL_0050
			while (true)
			{
				CECSEntityNode cECSEntityNode = CECSEntityList.m_ᜂ.Pop();
				cECSEntityNode.Entity = entity;
				cECSEntityNode.pre = this.m_ᜀ;
				cECSEntityNode.next = null;
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (true)
						{
						}
						if (this.m_ᜀ != null)
						{
							num = 4;
							continue;
						}
						goto case 3;
					case 0:
						head = cECSEntityNode;
						num = 1;
						continue;
					case 3:
						this.m_ᜀ = cECSEntityNode;
						num = 5;
						continue;
					case 5:
						if (head == null)
						{
							num = 0;
							continue;
						}
						goto case 1;
					case 4:
						this.m_ᜀ.next = cECSEntityNode;
						num = 3;
						continue;
					case 1:
						Count++;
						return;
					}
					break;
				}
			}
		}

		public bool HasEntity(CECSEntity entity)
		{
			//Discarded unreachable code: IL_0034
			while (true)
			{
				CECSEntityNode next = head;
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 2:
						return true;
					case 3:
						if (next.Entity != entity)
						{
							if (true)
							{
							}
							next = next.next;
							num = 5;
						}
						else
						{
							num = 2;
						}
						continue;
					case 1:
					case 5:
						num = 0;
						continue;
					case 0:
						num = ((next == null) ? 4 : 3);
						continue;
					case 4:
						return false;
					}
					break;
				}
			}
		}

		public bool DelEntity(CECSEntity entity)
		{
			//Discarded unreachable code: IL_0071
			while (true)
			{
				CECSEntityNode next = head;
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 2:
						ᜂ(next);
						return true;
					case 4:
						if (next.Entity != entity)
						{
							next = next.next;
							num = 1;
						}
						else
						{
							num = 2;
						}
						continue;
					case 0:
					case 1:
						num = 5;
						continue;
					case 5:
						if (true)
						{
						}
						num = ((next == null) ? 3 : 4);
						continue;
					case 3:
						return false;
					}
					break;
				}
			}
		}

		// 大概意思是先检测entity是否已经存在于list（CECSEntityNode）中，如果已经存在，就先删除（flag=true）。
		// 然后判断当前entity是否已经挂载全部的component，如果挂载，那么就在CECSEntityNode的尾部插入entity
		// 如果entity是第一次被add（flag2=true且flag=false），那么会触发CbAddEntity方法，否则就不会触发
		// 如果entity不是第一次被add（flag2=false且flag=true），并且entity上又没有挂载全部的component；那么会触发CbDelEntity方法
		// 如果entity不是第一次被add（flag2=true且flag=true），但entity上挂载了全部的component；那么不会触发CbAddEntity方法，并直接return
		public void RefreshEntity(CECSEntity entity, List<Type> matchTypes, AECSSystem system)
		{
			//Discarded unreachable code: IL_00f2
			while (true)
			{
				bool flag = false;
				bool flag2 = false;
				flag = DelEntity(entity);
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (entity.HasComponents(matchTypes))
						{
							num = 9;
							continue;
						}
						goto case 0;
					case 1:
						system.CbDelEntity(entity);
						num = 3;
						continue;
					case 4:
						num = 12;
						continue;
					case 12:
						if (!flag2)
						{
							num = 1;
							continue;
						}
						goto case 3;
					case 0:
						num = 7;
						continue;
					case 7:
						if (system != null)
						{
							num = 5;
							continue;
						}
						return;
					case 6:
						return;
					case 9:
						AddEntity(entity);
						flag2 = true;
						num = 0;
						continue;
					case 3:
						num = 11;
						continue;
					case 11:
						// entity第一次被添加时（即flag=false,flag2=true），调用CbAddEntity
						if (!flag && flag2)
						{
							num = 10;
							continue;
						}
						return;
					case 10:
						if (true)
						{
						}
						system.CbAddEntity(entity);
						num = 6;
						continue;
					case 5:
						num = 8;
						continue;
					case 8:
						if (flag)
						{
							num = 4;
							continue;
						}
						goto case 3;
					}
					break;
				}
			}
		}

		// 大概意思是从list中删除传进来的node
		private void ᜂ(CECSEntityNode A_0)
		{
			//Discarded unreachable code: IL_00c2
			while (true)
			{
				CECSEntityNode pre = A_0.pre;
				CECSEntityNode next = A_0.next;
				int num = 4;
				while (true)
				{
					switch (num)
					{
					case 4:
						if (head == A_0)
						{
							num = 11;
							continue;
						}
						goto case 9;
					case 3:
						pre.next = next;
						num = 0;
						continue;
					case 6:
						num = 1;
						continue;
					case 1:
						if (pre != null)
						{
							num = 3;
							continue;
						}
						goto case 0;
					case 9:
						num = 2;
						continue;
					case 2:
						if (this.m_ᜀ == A_0)
						{
							num = 8;
							continue;
						}
						goto case 6;
					case 7:
						next.pre = pre;
						num = 5;
						continue;
					case 11:
						if (true)
						{
						}
						head = next;
						num = 9;
						continue;
					case 0:
						num = 10;
						continue;
					case 10:
						if (next != null)
						{
							num = 7;
							continue;
						}
						goto case 5;
					case 8:
						this.m_ᜀ = pre;
						num = 6;
						continue;
					case 5:
						ᜁ(A_0);
						Count--;
						return;
					}
					break;
				}
			}
		}

		private void ᜁ(CECSEntityNode A_0)
		{
			A_0.Dispose();
			CECSEntityList.m_ᜂ.Push(A_0);
		}

		private bool ᜀ(CECSEntityNode A_0)
		{
			//Discarded unreachable code: IL_0034
			while (true)
			{
				CECSEntityNode next = head;
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 5:
						return true;
					case 2:
						if (next != A_0)
						{
							if (true)
							{
							}
							next = next.next;
							num = 0;
						}
						else
						{
							num = 5;
						}
						continue;
					case 0:
					case 1:
						num = 3;
						continue;
					case 3:
						num = ((next == null) ? 4 : 2);
						continue;
					case 4:
						return false;
					}
					break;
				}
			}
		}

		public bool IsEmpty()
		{
			return head == null;
		}

		public void ForEach(System.Action<CECSEntity> action)
		{
			//Discarded unreachable code: IL_0022
			while (true)
			{
				CECSEntityNode next = head;
				if (true)
				{
				}
				int num = 0;
				while (true)
				{
					switch (num)
					{
					case 0:
					case 1:
						num = 3;
						continue;
					case 3:
						if (next == null)
						{
							num = 2;
							continue;
						}
						action(next.Entity);
						next = next.next;
						num = 1;
						continue;
					case 2:
						return;
					}
					break;
				}
			}
		}

		public void Clear()
		{
			//Discarded unreachable code: IL_003d
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					num = 2;
					break;
				case 2:
				{
					if (head == null)
					{
						num = 3;
						break;
					}
					CECSEntityNode cECSEntityNode = head;
					head = head.next;
					cECSEntityNode.pre = null;
					cECSEntityNode.next = null;
					num = 0;
					break;
				}
				case 3:
					if (true)
					{
					}
					this.m_ᜀ = null;
					Count = 0u;
					return;
				}
			}
		}
	}
}
