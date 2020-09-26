using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting
{
	internal class ComRedirectionProxy : MarshalByRefObject, IMessageSink
	{
		private MarshalByRefObject _comObject;

		private Type _serverType;

		public IMessageSink NextSink => null;

		internal ComRedirectionProxy(MarshalByRefObject comObject, Type serverType)
		{
			_comObject = comObject;
			_serverType = serverType;
		}

		public virtual IMessage SyncProcessMessage(IMessage msg)
		{
			IMethodCallMessage reqMsg = (IMethodCallMessage)msg;
			IMethodReturnMessage methodReturnMessage = null;
			methodReturnMessage = RemotingServices.ExecuteMessage(_comObject, reqMsg);
			if (methodReturnMessage != null)
			{
				COMException ex = methodReturnMessage.Exception as COMException;
				if (ex != null && (ex._HResult == -2147023174 || ex._HResult == -2147023169))
				{
					_comObject = (MarshalByRefObject)Activator.CreateInstance(_serverType, nonPublic: true);
					methodReturnMessage = RemotingServices.ExecuteMessage(_comObject, reqMsg);
				}
			}
			return methodReturnMessage;
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
		{
			IMessage message = null;
			message = SyncProcessMessage(msg);
			replySink?.SyncProcessMessage(message);
			return null;
		}
	}
}
