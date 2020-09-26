using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class InternalMessageWrapper
	{
		protected IMessage WrappedMessage;

		public InternalMessageWrapper(IMessage msg)
		{
			WrappedMessage = msg;
		}

		internal object GetIdentityObject()
		{
			IInternalMessage internalMessage = WrappedMessage as IInternalMessage;
			if (internalMessage != null)
			{
				return internalMessage.IdentityObject;
			}
			return (WrappedMessage as InternalMessageWrapper)?.GetIdentityObject();
		}

		internal object GetServerIdentityObject()
		{
			IInternalMessage internalMessage = WrappedMessage as IInternalMessage;
			if (internalMessage != null)
			{
				return internalMessage.ServerIdentityObject;
			}
			return (WrappedMessage as InternalMessageWrapper)?.GetServerIdentityObject();
		}
	}
}
