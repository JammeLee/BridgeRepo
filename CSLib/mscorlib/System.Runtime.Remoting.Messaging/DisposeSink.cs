namespace System.Runtime.Remoting.Messaging
{
	internal class DisposeSink : IMessageSink
	{
		private IDisposable _iDis;

		private IMessageSink _replySink;

		public IMessageSink NextSink => _replySink;

		internal DisposeSink(IDisposable iDis, IMessageSink replySink)
		{
			_iDis = iDis;
			_replySink = replySink;
		}

		public virtual IMessage SyncProcessMessage(IMessage reqMsg)
		{
			IMessage result = null;
			try
			{
				if (_replySink != null)
				{
					return _replySink.SyncProcessMessage(reqMsg);
				}
				return result;
			}
			finally
			{
				_iDis.Dispose();
			}
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
		{
			throw new NotSupportedException();
		}
	}
}
