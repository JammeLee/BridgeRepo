using System;
using System.Security.Permissions;

namespace Microsoft.Win32
{
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class UserPreferenceChangedEventArgs : EventArgs
	{
		private readonly UserPreferenceCategory category;

		public UserPreferenceCategory Category => category;

		public UserPreferenceChangedEventArgs(UserPreferenceCategory category)
		{
			this.category = category;
		}
	}
}
