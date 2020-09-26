namespace CSLib.Framework
{
	public abstract class CFSMStateProcess
	{
		private CFSMManager ᜀ;

		private object ᜁ;

		public CFSMManager FSMManager => ᜀ;

		public object Owner
		{
			get
			{
				return ᜁ;
			}
			set
			{
				ᜁ = value;
			}
		}

		public CFSMStateProcess(CFSMManager fsmManager)
		{
			ᜀ = fsmManager;
		}

		public virtual void processIn()
		{
		}

		public virtual void processInput(int iInputID)
		{
		}

		public virtual void processState(float fDeltaTime)
		{
		}

		public virtual void processOut()
		{
		}

		public virtual void onDestory()
		{
		}

		protected int stateTransition(int iInputID)
		{
			if (ᜀ != null)
			{
				return ᜀ.stateTransition(iInputID);
			}
			return 0;
		}
	}
}
