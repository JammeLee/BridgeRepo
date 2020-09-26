using System.Collections;
using System.Deployment.Internal.Isolation.Manifest;
using System.Reflection;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Security
{
	[Serializable]
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class HostSecurityManager
	{
		public virtual HostSecurityManagerOptions Flags => HostSecurityManagerOptions.AllFlags;

		public virtual PolicyLevel DomainPolicy => null;

		public virtual Evidence ProvideAppDomainEvidence(Evidence inputEvidence)
		{
			return inputEvidence;
		}

		public virtual Evidence ProvideAssemblyEvidence(Assembly loadedAssembly, Evidence inputEvidence)
		{
			return inputEvidence;
		}

		[SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
		public virtual ApplicationTrust DetermineApplicationTrust(Evidence applicationEvidence, Evidence activatorEvidence, TrustManagerContext context)
		{
			if (applicationEvidence == null)
			{
				throw new ArgumentNullException("applicationEvidence");
			}
			IEnumerator hostEnumerator = applicationEvidence.GetHostEnumerator();
			ActivationArguments activationArguments = null;
			ApplicationTrust applicationTrust = null;
			while (hostEnumerator.MoveNext())
			{
				if (activationArguments == null)
				{
					activationArguments = hostEnumerator.Current as ActivationArguments;
				}
				if (applicationTrust == null)
				{
					applicationTrust = hostEnumerator.Current as ApplicationTrust;
				}
				if (activationArguments != null && applicationTrust != null)
				{
					break;
				}
			}
			if (activationArguments == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Policy_MissingActivationContextInAppEvidence"));
			}
			ActivationContext activationContext = activationArguments.ActivationContext;
			if (activationContext == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Policy_MissingActivationContextInAppEvidence"));
			}
			if (applicationTrust != null && !CmsUtils.CompareIdentities(applicationTrust.ApplicationIdentity, activationArguments.ApplicationIdentity, ApplicationVersionMatch.MatchExactVersion))
			{
				applicationTrust = null;
			}
			if (applicationTrust == null)
			{
				applicationTrust = ((AppDomain.CurrentDomain.ApplicationTrust == null || !CmsUtils.CompareIdentities(AppDomain.CurrentDomain.ApplicationTrust.ApplicationIdentity, activationArguments.ApplicationIdentity, ApplicationVersionMatch.MatchExactVersion)) ? ApplicationSecurityManager.DetermineApplicationTrustInternal(activationContext, context) : AppDomain.CurrentDomain.ApplicationTrust);
			}
			ApplicationSecurityInfo applicationSecurityInfo = new ApplicationSecurityInfo(activationContext);
			if (applicationTrust != null && applicationTrust.IsApplicationTrustedToRun && !applicationSecurityInfo.DefaultRequestSet.IsSubsetOf(applicationTrust.DefaultGrantSet.PermissionSet))
			{
				throw new InvalidOperationException(Environment.GetResourceString("Policy_AppTrustMustGrantAppRequest"));
			}
			return applicationTrust;
		}

		public virtual PermissionSet ResolvePolicy(Evidence evidence)
		{
			return SecurityManager.PolicyManager.ResolveHelper(evidence);
		}
	}
}
