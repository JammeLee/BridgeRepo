using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Contexts
{
	internal class SynchronizedServerContextSink : InternalSink, IMessageSink
	{
		internal IMessageSink _nextSink;

		internal SynchronizationAttribute _property;

		public IMessageSink NextSink => _nextSink;

		internal SynchronizedServerContextSink(SynchronizationAttribute prop, IMessageSink nextSink)
		{
			_property = prop;
			_nextSink = nextSink;
		}

		~SynchronizedServerContextSink()
		{
			_property.Dispose();
		}

		public virtual IMessage SyncProcessMessage(IMessage reqMsg)
		{
			WorkItem workItem = new WorkItem(reqMsg, _nextSink, null);
			_property.HandleWorkRequest(workItem);
			return workItem.ReplyMessage;
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
		{
			WorkItem workItem = new WorkItem(reqMsg, _nextSink, replySink);
			workItem.SetAsync();
			_property.HandleWorkRequest(workItem);
			return null;
		}
	}
}
