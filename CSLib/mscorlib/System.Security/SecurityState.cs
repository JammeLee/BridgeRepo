using System.Security.Permissions;

namespace System.Security
{
	[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
	public abstract class SecurityState
	{
		public bool IsStateAvailable()
		{
			return AppDomainManager.CurrentAppDomainManager?.CheckSecuritySettings(this) ?? false;
		}

		public abstract void EnsureState();
	}
}
