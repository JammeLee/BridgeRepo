using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Channels
{
	[ComVisible(true)]
	public interface IServerResponseChannelSinkStack
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		void AsyncProcessResponse(IMessage msg, ITransportHeaders headers, Stream stream);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		Stream GetResponseStream(IMessage msg, ITransportHeaders headers);
	}
}
