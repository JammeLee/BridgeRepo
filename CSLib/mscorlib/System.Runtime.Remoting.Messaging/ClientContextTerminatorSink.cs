using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	internal class ClientContextTerminatorSink : InternalSink, IMessageSink
	{
		private static ClientContextTerminatorSink messageSink;

		private static object staticSyncObject = new object();

		internal static IMessageSink MessageSink
		{
			get
			{
				if (messageSink == null)
				{
					ClientContextTerminatorSink clientContextTerminatorSink = new ClientContextTerminatorSink();
					lock (staticSyncObject)
					{
						if (messageSink == null)
						{
							messageSink = clientContextTerminatorSink;
						}
					}
				}
				return messageSink;
			}
		}

		public IMessageSink NextSink => null;

		internal static object SyncProcessMessageCallback(object[] args)
		{
			IMessage msg = (IMessage)args[0];
			IMessageSink messageSink = (IMessageSink)args[1];
			return messageSink.SyncProcessMessage(msg);
		}

		public virtual IMessage SyncProcessMessage(IMessage reqMsg)
		{
			IMessage message = InternalSink.ValidateMessage(reqMsg);
			if (message != null)
			{
				return message;
			}
			Context currentContext = Thread.CurrentContext;
			bool flag = currentContext.NotifyDynamicSinks(reqMsg, bCliSide: true, bStart: true, bAsync: false, bNotifyGlobals: true);
			IMessage message2;
			if (reqMsg is IConstructionCallMessage)
			{
				message = currentContext.NotifyActivatorProperties(reqMsg, bServerSide: false);
				if (message != null)
				{
					return message;
				}
				message2 = ((IConstructionCallMessage)reqMsg).Activator.Activate((IConstructionCallMessage)reqMsg);
				message = currentContext.NotifyActivatorProperties(message2, bServerSide: false);
				if (message != null)
				{
					return message;
				}
			}
			else
			{
				message2 = null;
				ChannelServices.NotifyProfiler(reqMsg, RemotingProfilerEvent.ClientSend);
				object[] array = new object[2];
				object[] array2 = array;
				IMessageSink channelSink = GetChannelSink(reqMsg);
				array2[0] = reqMsg;
				array2[1] = channelSink;
				InternalCrossContextDelegate internalCrossContextDelegate = SyncProcessMessageCallback;
				message2 = ((channelSink == CrossContextChannel.MessageSink) ? ((IMessage)internalCrossContextDelegate(array2)) : ((IMessage)Thread.CurrentThread.InternalCrossContextCallback(Context.DefaultContext, internalCrossContextDelegate, array2)));
				ChannelServices.NotifyProfiler(message2, RemotingProfilerEvent.ClientReceive);
			}
			if (flag)
			{
				currentContext.NotifyDynamicSinks(reqMsg, bCliSide: true, bStart: false, bAsync: false, bNotifyGlobals: true);
			}
			return message2;
		}

		internal static object AsyncProcessMessageCallback(object[] args)
		{
			IMessage msg = (IMessage)args[0];
			IMessageSink replySink = (IMessageSink)args[1];
			IMessageSink messageSink = (IMessageSink)args[2];
			return messageSink.AsyncProcessMessage(msg, replySink);
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
		{
			IMessage message = InternalSink.ValidateMessage(reqMsg);
			IMessageCtrl result = null;
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
				if (RemotingServices.CORProfilerTrackRemotingAsync())
				{
					RemotingServices.CORProfilerRemotingClientSendingMessage(out var id, fIsAsync: true);
					if (RemotingServices.CORProfilerTrackRemotingCookie())
					{
						reqMsg.Properties["CORProfilerCookie"] = id;
					}
					if (replySink != null)
					{
						IMessageSink messageSink = new ClientAsyncReplyTerminatorSink(replySink);
						replySink = messageSink;
					}
				}
				Context currentContext = Thread.CurrentContext;
				currentContext.NotifyDynamicSinks(reqMsg, bCliSide: true, bStart: true, bAsync: true, bNotifyGlobals: true);
				if (replySink != null)
				{
					replySink = new AsyncReplySink(replySink, currentContext);
				}
				object[] array = new object[3];
				object[] array2 = array;
				InternalCrossContextDelegate internalCrossContextDelegate = AsyncProcessMessageCallback;
				IMessageSink channelSink = GetChannelSink(reqMsg);
				array2[0] = reqMsg;
				array2[1] = replySink;
				array2[2] = channelSink;
				result = ((channelSink == CrossContextChannel.MessageSink) ? ((IMessageCtrl)internalCrossContextDelegate(array2)) : ((IMessageCtrl)Thread.CurrentThread.InternalCrossContextCallback(Context.DefaultContext, internalCrossContextDelegate, array2)));
			}
			return result;
		}

		private IMessageSink GetChannelSink(IMessage reqMsg)
		{
			Identity identity = InternalSink.GetIdentity(reqMsg);
			return identity.ChannelSink;
		}
	}
}
