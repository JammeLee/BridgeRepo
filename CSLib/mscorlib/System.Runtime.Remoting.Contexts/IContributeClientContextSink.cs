using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Contexts
{
	[ComVisible(true)]
	public interface IContributeClientContextSink
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		IMessageSink GetClientContextSink(IMessageSink nextSink);
	}
}
