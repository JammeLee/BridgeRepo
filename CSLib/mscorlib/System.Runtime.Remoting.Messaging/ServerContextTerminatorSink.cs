using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	internal class ServerContextTerminatorSink : InternalSink, IMessageSink
	{
		private static ServerContextTerminatorSink messageSink;

		private static object staticSyncObject = new object();

		internal static IMessageSink MessageSink
		{
			get
			{
				if (messageSink == null)
				{
					ServerContextTerminatorSink serverContextTerminatorSink = new ServerContextTerminatorSink();
					lock (staticSyncObject)
					{
						if (messageSink == null)
						{
							messageSink = serverContextTerminatorSink;
						}
					}
				}
				return messageSink;
			}
		}

		public IMessageSink NextSink => null;

		public virtual IMessage SyncProcessMessage(IMessage reqMsg)
		{
			IMessage message = InternalSink.ValidateMessage(reqMsg);
			if (message != null)
			{
				return message;
			}
			Context currentContext = Thread.CurrentContext;
			if (reqMsg is IConstructionCallMessage)
			{
				message = currentContext.NotifyActivatorProperties(reqMsg, bServerSide: true);
				if (message != null)
				{
					return message;
				}
				IMessage message2 = ((IConstructionCallMessage)reqMsg).Activator.Activate((IConstructionCallMessage)reqMsg);
				message = currentContext.NotifyActivatorProperties(message2, bServerSide: true);
				if (message != null)
				{
					return message;
				}
				return message2;
			}
			MarshalByRefObject obj = null;
			try
			{
				return GetObjectChain(reqMsg, out obj).SyncProcessMessage(reqMsg);
			}
			finally
			{
				IDisposable disposable = null;
				if (obj != null && (disposable = obj as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
		{
			IMessageCtrl result = null;
			IMessage message = InternalSink.ValidateMessage(reqMsg);
			if (message == null)
			{
				message = InternalSink.DisallowAsyncActivation(reqMsg);
			}
			if (message != null)
			{
				replySink?.SyncProcessMessage(message);
			}
			else
			{
				MarshalByRefObject obj;
				IMessageSink objectChain = GetObjectChain(reqMsg, out obj);
				IDisposable iDis;
				if (obj != null && (iDis = obj as IDisposable) != null)
				{
					DisposeSink disposeSink = new DisposeSink(iDis, replySink);
					replySink = disposeSink;
				}
				result = objectChain.AsyncProcessMessage(reqMsg, replySink);
			}
			return result;
		}

		internal virtual IMessageSink GetObjectChain(IMessage reqMsg, out MarshalByRefObject obj)
		{
			ServerIdentity serverIdentity = InternalSink.GetServerIdentity(reqMsg);
			return serverIdentity.GetServerObjectChain(out obj);
		}
	}
}
