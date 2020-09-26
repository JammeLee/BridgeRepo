using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting
{
	[ComVisible(true)]
	public interface IChannelInfo
	{
		object[] ChannelData
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get;
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			set;
		}
	}
}
