using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization
{
	[ComVisible(true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public abstract class DesignerLoader
	{
		public virtual bool Loading => false;

		public abstract void BeginLoad(IDesignerLoaderHost host);

		public abstract void Dispose();

		public virtual void Flush()
		{
		}
	}
}
