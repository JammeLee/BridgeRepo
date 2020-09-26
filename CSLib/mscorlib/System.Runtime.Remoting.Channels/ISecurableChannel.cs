using System.Security.Permissions;

namespace System.Runtime.Remoting.Channels
{
	public interface ISecurableChannel
	{
		bool IsSecured
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get;
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			set;
		}
	}
}
