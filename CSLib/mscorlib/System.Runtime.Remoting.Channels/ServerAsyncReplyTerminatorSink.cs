using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	internal class ServerAsyncReplyTerminatorSink : IMessageSink
	{
		internal IMessageSink _nextSink;

		public IMessageSink NextSink => _nextSink;

		internal ServerAsyncReplyTerminatorSink(IMessageSink nextSink)
		{
			_nextSink = nextSink;
		}

		public virtual IMessage SyncProcessMessage(IMessage replyMsg)
		{
			RemotingServices.CORProfilerRemotingServerSendingReply(out var id, fIsAsync: true);
			if (RemotingServices.CORProfilerTrackRemotingCookie())
			{
				replyMsg.Properties["CORProfilerCookie"] = id;
			}
			return _nextSink.SyncProcessMessage(replyMsg);
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage replyMsg, IMessageSink replySink)
		{
			return null;
		}
	}
}
