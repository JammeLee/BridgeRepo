using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	internal class ServerObjectTerminatorSink : InternalSink, IMessageSink
	{
		internal StackBuilderSink _stackBuilderSink;

		public IMessageSink NextSink => null;

		internal ServerObjectTerminatorSink(MarshalByRefObject srvObj)
		{
			_stackBuilderSink = new StackBuilderSink(srvObj);
		}

		public virtual IMessage SyncProcessMessage(IMessage reqMsg)
		{
			IMessage message = InternalSink.ValidateMessage(reqMsg);
			if (message != null)
			{
				return message;
			}
			ServerIdentity serverIdentity = InternalSink.GetServerIdentity(reqMsg);
			ArrayWithSize serverSideDynamicSinks = serverIdentity.ServerSideDynamicSinks;
			if (serverSideDynamicSinks != null)
			{
				DynamicPropertyHolder.NotifyDynamicSinks(reqMsg, serverSideDynamicSinks, bCliSide: false, bStart: true, bAsync: false);
			}
			IMessageSink messageSink = _stackBuilderSink.ServerObject as IMessageSink;
			IMessage message2 = ((messageSink == null) ? _stackBuilderSink.SyncProcessMessage(reqMsg) : messageSink.SyncProcessMessage(reqMsg));
			if (serverSideDynamicSinks != null)
			{
				DynamicPropertyHolder.NotifyDynamicSinks(message2, serverSideDynamicSinks, bCliSide: false, bStart: false, bAsync: false);
			}
			return message2;
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
		{
			IMessageCtrl result = null;
			IMessage message = InternalSink.ValidateMessage(reqMsg);
			if (message != null)
			{
				replySink?.SyncProcessMessage(message);
			}
			else
			{
				IMessageSink messageSink = _stackBuilderSink.ServerObject as IMessageSink;
				result = ((messageSink == null) ? _stackBuilderSink.AsyncProcessMessage(reqMsg, replySink) : messageSink.AsyncProcessMessage(reqMsg, replySink));
			}
			return result;
		}
	}
}
