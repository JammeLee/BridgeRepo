using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization
{
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class ResolveNameEventArgs : EventArgs
	{
		private string name;

		private object value;

		public string Name => name;

		public object Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		public ResolveNameEventArgs(string name)
		{
			this.name = name;
			value = null;
		}
	}
}
