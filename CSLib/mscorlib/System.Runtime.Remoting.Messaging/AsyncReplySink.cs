using System.Runtime.Remoting.Contexts;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	internal class AsyncReplySink : IMessageSink
	{
		private IMessageSink _replySink;

		private Context _cliCtx;

		public IMessageSink NextSink => _replySink;

		internal AsyncReplySink(IMessageSink replySink, Context cliCtx)
		{
			_replySink = replySink;
			_cliCtx = cliCtx;
		}

		internal static object SyncProcessMessageCallback(object[] args)
		{
			IMessage msg = (IMessage)args[0];
			IMessageSink messageSink = (IMessageSink)args[1];
			Thread.CurrentContext.NotifyDynamicSinks(msg, bCliSide: true, bStart: false, bAsync: true, bNotifyGlobals: true);
			return messageSink.SyncProcessMessage(msg);
		}

		public virtual IMessage SyncProcessMessage(IMessage reqMsg)
		{
			IMessage result = null;
			if (_replySink != null)
			{
				object[] args = new object[2]
				{
					reqMsg,
					_replySink
				};
				InternalCrossContextDelegate ftnToCall = SyncProcessMessageCallback;
				result = (IMessage)Thread.CurrentThread.InternalCrossContextCallback(_cliCtx, ftnToCall, args);
			}
			return result;
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
		{
			throw new NotSupportedException();
		}
	}
}
