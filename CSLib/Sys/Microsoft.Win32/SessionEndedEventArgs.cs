using System;
using System.Security.Permissions;

namespace Microsoft.Win32
{
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public class SessionEndedEventArgs : EventArgs
	{
		private readonly SessionEndReasons reason;

		public SessionEndReasons Reason => reason;

		public SessionEndedEventArgs(SessionEndReasons reason)
		{
			this.reason = reason;
		}
	}
}
