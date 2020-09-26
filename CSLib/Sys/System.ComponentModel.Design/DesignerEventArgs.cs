using System.Security.Permissions;

namespace System.ComponentModel.Design
{
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class DesignerEventArgs : EventArgs
	{
		private readonly IDesignerHost host;

		public IDesignerHost Designer => host;

		public DesignerEventArgs(IDesignerHost host)
		{
			this.host = host;
		}
	}
}
