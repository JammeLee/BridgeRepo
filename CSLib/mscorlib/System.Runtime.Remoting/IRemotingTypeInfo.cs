using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting
{
	[ComVisible(true)]
	public interface IRemotingTypeInfo
	{
		string TypeName
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get;
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			set;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		bool CanCastTo(Type fromType, object o);
	}
}
