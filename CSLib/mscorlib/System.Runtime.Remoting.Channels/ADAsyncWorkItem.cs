using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	internal class ADAsyncWorkItem
	{
		private IMessageSink _replySink;

		private IMessageSink _nextSink;

		private LogicalCallContext _callCtx;

		private IMessage _reqMsg;

		internal ADAsyncWorkItem(IMessage reqMsg, IMessageSink nextSink, IMessageSink replySink)
		{
			_reqMsg = reqMsg;
			_nextSink = nextSink;
			_replySink = replySink;
			_callCtx = CallContext.GetLogicalCallContext();
		}

		internal virtual void FinishAsyncWork(object stateIgnored)
		{
			LogicalCallContext logicalCallContext = CallContext.SetLogicalCallContext(_callCtx);
			IMessage msg = _nextSink.SyncProcessMessage(_reqMsg);
			if (_replySink != null)
			{
				_replySink.SyncProcessMessage(msg);
			}
			CallContext.SetLogicalCallContext(logicalCallContext);
		}
	}
}
