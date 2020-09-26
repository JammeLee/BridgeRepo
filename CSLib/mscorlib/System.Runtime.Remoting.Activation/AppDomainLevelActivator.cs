using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Activation
{
	internal class AppDomainLevelActivator : IActivator
	{
		private IActivator m_NextActivator;

		private string m_RemActivatorURL;

		public virtual IActivator NextActivator
		{
			get
			{
				return m_NextActivator;
			}
			set
			{
				m_NextActivator = value;
			}
		}

		public virtual ActivatorLevel Level => ActivatorLevel.AppDomain;

		internal AppDomainLevelActivator(string remActivatorURL)
		{
			m_RemActivatorURL = remActivatorURL;
		}

		internal AppDomainLevelActivator(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			m_NextActivator = (IActivator)info.GetValue("m_NextActivator", typeof(IActivator));
		}

		[ComVisible(true)]
		public virtual IConstructionReturnMessage Activate(IConstructionCallMessage ctorMsg)
		{
			ctorMsg.Activator = m_NextActivator;
			return ActivationServices.GetActivator().Activate(ctorMsg);
		}
	}
}
