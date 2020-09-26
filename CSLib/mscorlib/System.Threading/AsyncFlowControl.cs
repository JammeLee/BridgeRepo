using System.Security;

namespace System.Threading
{
	public struct AsyncFlowControl : IDisposable
	{
		private bool useEC;

		private ExecutionContext _ec;

		private SecurityContext _sc;

		private Thread _thread;

		internal void Setup(SecurityContextDisableFlow flags)
		{
			useEC = false;
			_sc = Thread.CurrentThread.ExecutionContext.SecurityContext;
			_sc._disableFlow = flags;
			_thread = Thread.CurrentThread;
		}

		internal void Setup()
		{
			useEC = true;
			_ec = Thread.CurrentThread.ExecutionContext;
			_ec.isFlowSuppressed = true;
			_thread = Thread.CurrentThread;
		}

		void IDisposable.Dispose()
		{
			Undo();
		}

		public void Undo()
		{
			if (_thread == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseAFCMultiple"));
			}
			if (_thread != Thread.CurrentThread)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseAFCOtherThread"));
			}
			if (useEC)
			{
				if (Thread.CurrentThread.ExecutionContext != _ec)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AsyncFlowCtrlCtxMismatch"));
				}
				ExecutionContext.RestoreFlow();
			}
			else
			{
				if (Thread.CurrentThread.ExecutionContext.SecurityContext != _sc)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AsyncFlowCtrlCtxMismatch"));
				}
				SecurityContext.RestoreFlow();
			}
			_thread = null;
		}

		public override int GetHashCode()
		{
			if (_thread != null)
			{
				return _thread.GetHashCode();
			}
			return ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is AsyncFlowControl)
			{
				return Equals((AsyncFlowControl)obj);
			}
			return false;
		}

		public bool Equals(AsyncFlowControl obj)
		{
			if (obj.useEC == useEC && obj._ec == _ec && obj._sc == _sc)
			{
				return obj._thread == _thread;
			}
			return false;
		}

		public static bool operator ==(AsyncFlowControl a, AsyncFlowControl b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(AsyncFlowControl a, AsyncFlowControl b)
		{
			return !(a == b);
		}
	}
}
