using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace System
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class AppDomainManager : MarshalByRefObject
	{
		private AppDomainManagerInitializationOptions m_flags;

		private ApplicationActivator m_appActivator;

		private Assembly m_entryAssembly;

		public AppDomainManagerInitializationOptions InitializationFlags
		{
			get
			{
				return m_flags;
			}
			set
			{
				m_flags = value;
			}
		}

		public virtual ApplicationActivator ApplicationActivator
		{
			get
			{
				if (m_appActivator == null)
				{
					m_appActivator = new ApplicationActivator();
				}
				return m_appActivator;
			}
		}

		public virtual HostSecurityManager HostSecurityManager => null;

		public virtual HostExecutionContextManager HostExecutionContextManager => HostExecutionContextManager.GetInternalHostExecutionContextManager();

		public virtual Assembly EntryAssembly
		{
			get
			{
				if (m_entryAssembly == null)
				{
					AppDomain currentDomain = AppDomain.CurrentDomain;
					if (currentDomain.IsDefaultAppDomain() && currentDomain.ActivationContext != null)
					{
						ManifestRunner manifestRunner = new ManifestRunner(currentDomain, currentDomain.ActivationContext);
						m_entryAssembly = manifestRunner.EntryAssembly;
					}
					else
					{
						m_entryAssembly = nGetEntryAssembly();
					}
				}
				return m_entryAssembly;
			}
		}

		internal static AppDomainManager CurrentAppDomainManager => AppDomain.CurrentDomain.DomainManager;

		public virtual AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
		{
			return CreateDomainHelper(friendlyName, securityInfo, appDomainInfo);
		}

		[SecurityPermission(SecurityAction.Demand, ControlAppDomain = true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		protected static AppDomain CreateDomainHelper(string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
		{
			if (friendlyName == null)
			{
				throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"));
			}
			if (securityInfo != null)
			{
				new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
			}
			return AppDomain.nCreateDomain(friendlyName, appDomainInfo, securityInfo, (securityInfo == null) ? AppDomain.CurrentDomain.InternalEvidence : null, AppDomain.CurrentDomain.GetSecurityDescriptor());
		}

		public virtual void InitializeNewDomain(AppDomainSetup appDomainInfo)
		{
		}

		public virtual bool CheckSecuritySettings(SecurityState state)
		{
			return false;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void nRegisterWithHost();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Assembly nGetEntryAssembly();
	}
}
