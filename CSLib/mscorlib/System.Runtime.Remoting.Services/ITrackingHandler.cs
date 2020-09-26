using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Services
{
	[ComVisible(true)]
	public interface ITrackingHandler
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		void MarshaledObject(object obj, ObjRef or);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		void UnmarshaledObject(object obj, ObjRef or);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		void DisconnectedObject(object obj);
	}
}
