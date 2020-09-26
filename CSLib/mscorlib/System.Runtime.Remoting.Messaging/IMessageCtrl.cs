using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[ComVisible(true)]
	public interface IMessageCtrl
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		void Cancel(int msToCancel);
	}
}
