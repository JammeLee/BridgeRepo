using System.Deployment.Internal.Isolation.Manifest;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Util;

namespace System.Security.Policy
{
	[ComVisible(true)]
	public static class ApplicationSecurityManager
	{
		private static IApplicationTrustManager m_appTrustManager = null;

		private static string s_machineConfigFile = Config.MachineDirectory + "applicationtrust.config";

		public static ApplicationTrustCollection UserApplicationTrusts
		{
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
			get
			{
				return new ApplicationTrustCollection(storeBounded: true);
			}
		}

		public static IApplicationTrustManager ApplicationTrustManager
		{
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
			get
			{
				if (m_appTrustManager == null)
				{
					m_appTrustManager = DecodeAppTrustManager();
					if (m_appTrustManager == null)
					{
						throw new PolicyException(Environment.GetResourceString("Policy_NoTrustManager"));
					}
				}
				return m_appTrustManager;
			}
		}

		[SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public static bool DetermineApplicationTrust(ActivationContext activationContext, TrustManagerContext context)
		{
			if (activationContext == null)
			{
				throw new ArgumentNullException("activationContext");
			}
			ApplicationTrust applicationTrust = null;
			AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
			if (domainManager != null)
			{
				HostSecurityManager hostSecurityManager = domainManager.HostSecurityManager;
				if (hostSecurityManager != null && (hostSecurityManager.Flags & HostSecurityManagerOptions.HostDetermineApplicationTrust) == HostSecurityManagerOptions.HostDetermineApplicationTrust)
				{
					return hostSecurityManager.DetermineApplicationTrust(CmsUtils.MergeApplicationEvidence(null, activationContext.Identity, activationContext, null), null, context)?.IsApplicationTrustedToRun ?? false;
				}
			}
			return DetermineApplicationTrustInternal(activationContext, context)?.IsApplicationTrustedToRun ?? false;
		}

		internal static ApplicationTrust DetermineApplicationTrustInternal(ActivationContext activationContext, TrustManagerContext context)
		{
			ApplicationTrust applicationTrust = null;
			ApplicationTrustCollection applicationTrustCollection = new ApplicationTrustCollection(storeBounded: true);
			if (context == null || !context.IgnorePersistedDecision)
			{
				applicationTrust = applicationTrustCollection[activationContext.Identity.FullName];
				if (applicationTrust != null)
				{
					return applicationTrust;
				}
			}
			applicationTrust = ApplicationTrustManager.DetermineApplicationTrust(activationContext, context);
			if (applicationTrust == null)
			{
				applicationTrust = new ApplicationTrust(activationContext.Identity);
			}
			applicationTrust.ApplicationIdentity = activationContext.Identity;
			if (applicationTrust.Persist)
			{
				applicationTrustCollection.Add(applicationTrust);
			}
			return applicationTrust;
		}

		private static IApplicationTrustManager DecodeAppTrustManager()
		{
			if (File.InternalExists(s_machineConfigFile))
			{
				FileStream stream = new FileStream(s_machineConfigFile, FileMode.Open, FileAccess.Read);
				SecurityElement securityElement = SecurityElement.FromString(new StreamReader(stream).ReadToEnd());
				SecurityElement securityElement2 = securityElement.SearchForChildByTag("mscorlib");
				if (securityElement2 != null)
				{
					SecurityElement securityElement3 = securityElement2.SearchForChildByTag("security");
					if (securityElement3 != null)
					{
						SecurityElement securityElement4 = securityElement3.SearchForChildByTag("policy");
						if (securityElement4 != null)
						{
							SecurityElement securityElement5 = securityElement4.SearchForChildByTag("ApplicationSecurityManager");
							if (securityElement5 != null)
							{
								SecurityElement securityElement6 = securityElement5.SearchForChildByTag("IApplicationTrustManager");
								if (securityElement6 != null)
								{
									IApplicationTrustManager applicationTrustManager = DecodeAppTrustManagerFromElement(securityElement6);
									if (applicationTrustManager != null)
									{
										return applicationTrustManager;
									}
								}
							}
						}
					}
				}
			}
			return DecodeAppTrustManagerFromElement(CreateDefaultApplicationTrustManagerElement());
		}

		private static SecurityElement CreateDefaultApplicationTrustManagerElement()
		{
			SecurityElement securityElement = new SecurityElement("IApplicationTrustManager");
			securityElement.AddAttribute("class", string.Concat("System.Security.Policy.TrustManager, System.Windows.Forms, Version=", Assembly.GetExecutingAssembly().GetVersion(), ", Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			securityElement.AddAttribute("version", "1");
			return securityElement;
		}

		private static IApplicationTrustManager DecodeAppTrustManagerFromElement(SecurityElement elTrustManager)
		{
			new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
			string typeName = elTrustManager.Attribute("class");
			Type type = Type.GetType(typeName, throwOnError: false, ignoreCase: false);
			if (type == null)
			{
				return null;
			}
			IApplicationTrustManager applicationTrustManager = Activator.CreateInstance(type) as IApplicationTrustManager;
			applicationTrustManager?.FromXml(elTrustManager);
			return applicationTrustManager;
		}
	}
}
