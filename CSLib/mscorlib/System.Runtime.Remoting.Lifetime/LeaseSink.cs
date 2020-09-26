using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Lifetime
{
	internal class LeaseSink : IMessageSink
	{
		private Lease lease;

		private IMessageSink nextSink;

		public IMessageSink NextSink => nextSink;

		public LeaseSink(Lease lease, IMessageSink nextSink)
		{
			this.lease = lease;
			this.nextSink = nextSink;
		}

		public IMessage SyncProcessMessage(IMessage msg)
		{
			lease.RenewOnCall();
			return nextSink.SyncProcessMessage(msg);
		}

		public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
		{
			lease.RenewOnCall();
			return nextSink.AsyncProcessMessage(msg, replySink);
		}
	}
}
