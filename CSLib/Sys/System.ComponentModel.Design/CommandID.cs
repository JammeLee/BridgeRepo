using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class CommandID
	{
		private readonly Guid menuGroup;

		private readonly int commandID;

		public virtual int ID => commandID;

		public virtual Guid Guid => menuGroup;

		public CommandID(Guid menuGroup, int commandID)
		{
			this.menuGroup = menuGroup;
			this.commandID = commandID;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CommandID))
			{
				return false;
			}
			CommandID commandID = (CommandID)obj;
			if (commandID.menuGroup.Equals(menuGroup))
			{
				return commandID.commandID == this.commandID;
			}
			return false;
		}

		public override int GetHashCode()
		{
			Guid guid = menuGroup;
			return (guid.GetHashCode() << 2) | commandID;
		}

		public override string ToString()
		{
			Guid guid = menuGroup;
			return guid.ToString() + " : " + commandID.ToString(CultureInfo.CurrentCulture);
		}
	}
}
