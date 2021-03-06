using System.Security.Permissions;

namespace System.ComponentModel.Design
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public delegate void ActiveDesignerEventHandler(object sender, ActiveDesignerEventArgs e);
}
