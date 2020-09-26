using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public sealed class ComponentChangingEventArgs : EventArgs
	{
		private object component;

		private MemberDescriptor member;

		public object Component => component;

		public MemberDescriptor Member => member;

		public ComponentChangingEventArgs(object component, MemberDescriptor member)
		{
			this.component = component;
			this.member = member;
		}
	}
}
