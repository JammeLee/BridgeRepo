using System.Runtime.InteropServices;

namespace System.Runtime.Hosting
{
	[Serializable]
	[ComVisible(true)]
	public sealed class ActivationArguments
	{
		private bool m_useFusionActivationContext;

		private bool m_activateInstance;

		private string m_appFullName;

		private string[] m_appManifestPaths;

		private string[] m_activationData;

		internal bool UseFusionActivationContext => m_useFusionActivationContext;

		internal bool ActivateInstance
		{
			get
			{
				return m_activateInstance;
			}
			set
			{
				m_activateInstance = value;
			}
		}

		internal string ApplicationFullName => m_appFullName;

		internal string[] ApplicationManifestPaths => m_appManifestPaths;

		public ApplicationIdentity ApplicationIdentity => new ApplicationIdentity(m_appFullName);

		public ActivationContext ActivationContext
		{
			get
			{
				if (!UseFusionActivationContext)
				{
					return null;
				}
				if (m_appManifestPaths == null)
				{
					return new ActivationContext(new ApplicationIdentity(m_appFullName));
				}
				return new ActivationContext(new ApplicationIdentity(m_appFullName), m_appManifestPaths);
			}
		}

		public string[] ActivationData => m_activationData;

		private ActivationArguments()
		{
		}

		public ActivationArguments(ApplicationIdentity applicationIdentity)
			: this(applicationIdentity, null)
		{
		}

		public ActivationArguments(ApplicationIdentity applicationIdentity, string[] activationData)
		{
			if (applicationIdentity == null)
			{
				throw new ArgumentNullException("applicationIdentity");
			}
			m_appFullName = applicationIdentity.FullName;
			m_activationData = activationData;
		}

		public ActivationArguments(ActivationContext activationData)
			: this(activationData, null)
		{
		}

		public ActivationArguments(ActivationContext activationContext, string[] activationData)
		{
			if (activationContext == null)
			{
				throw new ArgumentNullException("activationContext");
			}
			m_appFullName = activationContext.Identity.FullName;
			m_appManifestPaths = activationContext.ManifestPaths;
			m_activationData = activationData;
			m_useFusionActivationContext = true;
		}

		internal ActivationArguments(string appFullName, string[] appManifestPaths, string[] activationData)
		{
			if (appFullName == null)
			{
				throw new ArgumentNullException("appFullName");
			}
			m_appFullName = appFullName;
			m_appManifestPaths = appManifestPaths;
			m_activationData = activationData;
			m_useFusionActivationContext = true;
		}
	}
}
