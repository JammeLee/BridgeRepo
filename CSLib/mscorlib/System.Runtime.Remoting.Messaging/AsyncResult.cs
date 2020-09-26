using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	[ComVisible(true)]
	public class AsyncResult : IAsyncResult, IMessageSink
	{
		private IMessageCtrl _mc;

		private AsyncCallback _acbd;

		private IMessage _replyMsg;

		private bool _isCompleted;

		private bool _endInvokeCalled;

		private ManualResetEvent _AsyncWaitHandle;

		private Delegate _asyncDelegate;

		private object _asyncState;

		public virtual bool IsCompleted => _isCompleted;

		public virtual object AsyncDelegate => _asyncDelegate;

		public virtual object AsyncState => _asyncState;

		public virtual bool CompletedSynchronously => false;

		public bool EndInvokeCalled
		{
			get
			{
				return _endInvokeCalled;
			}
			set
			{
				_endInvokeCalled = value;
			}
		}

		public virtual WaitHandle AsyncWaitHandle
		{
			get
			{
				FaultInWaitHandle();
				return _AsyncWaitHandle;
			}
		}

		public IMessageSink NextSink
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				return null;
			}
		}

		internal AsyncResult(Message m)
		{
			m.GetAsyncBeginInfo(out _acbd, out _asyncState);
			_asyncDelegate = (Delegate)m.GetThisPtr();
		}

		private void FaultInWaitHandle()
		{
			lock (this)
			{
				if (_AsyncWaitHandle == null)
				{
					_AsyncWaitHandle = new ManualResetEvent(_isCompleted);
				}
			}
		}

		public virtual void SetMessageCtrl(IMessageCtrl mc)
		{
			_mc = mc;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual IMessage SyncProcessMessage(IMessage msg)
		{
			if (msg == null)
			{
				_replyMsg = new ReturnMessage(new RemotingException(Environment.GetResourceString("Remoting_NullMessage")), new ErrorMessage());
			}
			else if (!(msg is IMethodReturnMessage))
			{
				_replyMsg = new ReturnMessage(new RemotingException(Environment.GetResourceString("Remoting_Message_BadType")), new ErrorMessage());
			}
			else
			{
				_replyMsg = msg;
			}
			lock (this)
			{
				_isCompleted = true;
				if (_AsyncWaitHandle != null)
				{
					_AsyncWaitHandle.Set();
				}
			}
			if (_acbd != null)
			{
				_acbd(this);
			}
			return null;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
		}

		public virtual IMessage GetReplyMessage()
		{
			return _replyMsg;
		}
	}
}
