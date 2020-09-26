using System;
using System.Security.Permissions;

namespace Microsoft.Win32
{
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public class SessionEndingEventArgs : EventArgs
	{
		private bool cancel;

		private readonly SessionEndReasons reason;

		public bool Cancel
		{
			get
			{
				return cancel;
			}
			set
			{
				cancel = value;
			}
		}

		public SessionEndReasons Reason => reason;

		public SessionEndingEventArgs(SessionEndReasons reason)
		{
			this.reason = reason;
		}
	}
}
