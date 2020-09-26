using System;
using System.Collections.Generic;

namespace CSLib.Framework
{
	public class CECSSystemProcess : AECSSystemProcess
	{
		protected AECSSystem m_system;

		protected List<Type> m_entityTypes;

		public CECSSystemProcess(AECSSystem sys, params Type[] compatibleTypes)
		{
			m_system = sys;
			m_entityList = new CECSEntityList();
			m_entityTypes = new List<Type>();
			m_entityTypes.AddRange(compatibleTypes);
		}

		public override void AddEntity(CECSEntity entity)
		{
			m_entityList.RefreshEntity(entity, m_entityTypes, m_system);
		}

		public override void DelEntity(CECSEntity entity)
		{
			if (m_entityList.DelEntity(entity))
			{
				m_system.CbDelEntity(entity);
			}
		}

		public override void Update(float deltaTime)
		{
			//Discarded unreachable code: IL_003e
			while (true)
			{
				m_system.deltaTime = deltaTime;
				m_system.Before();
				CECSEntityNode cECSEntityNode = m_entityList.head;
				if (true)
				{
				}
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 0:
					case 2:
						num = 3;
						continue;
					case 3:
						if (cECSEntityNode == null)
						{
							num = 1;
							continue;
						}
						m_system.Update(cECSEntityNode.Entity);
						cECSEntityNode = cECSEntityNode.next;
						num = 0;
						continue;
					case 1:
						m_system.After();
						return;
					}
					break;
				}
			}
		}

		public override void Destroy()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			m_system.Destroy();
			m_system = null;
			m_entityList.Clear();
			m_entityList = null;
			m_entityTypes.Clear();
			m_entityTypes = null;
		}
	}
}
