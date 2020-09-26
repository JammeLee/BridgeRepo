using System;
using System.Security.Permissions;

namespace Microsoft.Win32
{
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class PowerModeChangedEventArgs : EventArgs
	{
		private readonly PowerModes mode;

		public PowerModes Mode => mode;

		public PowerModeChangedEventArgs(PowerModes mode)
		{
			this.mode = mode;
		}
	}
}
