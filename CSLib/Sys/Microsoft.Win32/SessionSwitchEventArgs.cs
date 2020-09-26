using System;
using System.Security.Permissions;

namespace Microsoft.Win32
{
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class SessionSwitchEventArgs : EventArgs
	{
		private readonly SessionSwitchReason reason;

		public SessionSwitchReason Reason => reason;

		public SessionSwitchEventArgs(SessionSwitchReason reason)
		{
			this.reason = reason;
		}
	}
}
