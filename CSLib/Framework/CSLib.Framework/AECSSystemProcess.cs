using System;
using CSLib.Utility;

namespace CSLib.Framework
{
	public abstract class AECSSystemProcess
	{
		protected CECSEntityList m_entityList;

		public CECSEntityList EntityList => m_entityList;

		public virtual void AddEntity(CECSEntity entity)
		{
			throw new NotImplementedException();
		}

		public virtual void DelEntity(CECSEntity entity)
		{
			throw new NotImplementedException();
		}

		public virtual void Update(float detlaTime)
		{
			throw new NotImplementedException();
		}

		public virtual void Destroy()
		{
			//Discarded unreachable code: IL_000c
			int a_ = 6;
			if (true)
			{
			}
			CDebugOut.LogError(CMessageLabel.b("战⬚怜\uda41媸୵畿屚띳魙ⶹ\uee45衕\ue65e㧋䅨㪴ፏ丼䘾㉀㝂⁄⩆杈ཊ⡌㱎═⅒㩔\u2e56硘", a_), GetType().Name);
		}
	}
}
