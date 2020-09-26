using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Activation
{
	[Serializable]
	internal class ConstructionLevelActivator : IActivator
	{
		public virtual IActivator NextActivator
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public virtual ActivatorLevel Level => ActivatorLevel.Construction;

		internal ConstructionLevelActivator()
		{
		}

		[ComVisible(true)]
		public virtual IConstructionReturnMessage Activate(IConstructionCallMessage ctorMsg)
		{
			ctorMsg.Activator = ctorMsg.Activator.NextActivator;
			return ActivationServices.DoServerContextActivation(ctorMsg);
		}
	}
}
