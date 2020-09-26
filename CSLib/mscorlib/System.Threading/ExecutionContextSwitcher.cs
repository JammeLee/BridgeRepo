using System.Runtime.ConstrainedExecution;
using System.Security;

namespace System.Threading
{
	internal struct ExecutionContextSwitcher : IDisposable
	{
		internal ExecutionContext prevEC;

		internal ExecutionContext currEC;

		internal SecurityContextSwitcher scsw;

		internal SynchronizationContextSwitcher sysw;

		internal object hecsw;

		internal Thread thread;

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ExecutionContextSwitcher))
			{
				return false;
			}
			ExecutionContextSwitcher executionContextSwitcher = (ExecutionContextSwitcher)obj;
			if (prevEC == executionContextSwitcher.prevEC && currEC == executionContextSwitcher.currEC && scsw == executionContextSwitcher.scsw && sysw == executionContextSwitcher.sysw && hecsw == executionContextSwitcher.hecsw)
			{
				return thread == executionContextSwitcher.thread;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public static bool operator ==(ExecutionContextSwitcher c1, ExecutionContextSwitcher c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(ExecutionContextSwitcher c1, ExecutionContextSwitcher c2)
		{
			return !c1.Equals(c2);
		}

		void IDisposable.Dispose()
		{
			Undo();
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal bool UndoNoThrow()
		{
			try
			{
				Undo();
			}
			catch
			{
				return false;
			}
			return true;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public void Undo()
		{
			if (thread != null)
			{
				if (thread != Thread.CurrentThread)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseSwitcherOtherThread"));
				}
				if (currEC != Thread.CurrentThread.GetExecutionContextNoCreate())
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
				}
				scsw.Undo();
				try
				{
					HostExecutionContextSwitcher.Undo(hecsw);
				}
				finally
				{
					sysw.Undo();
				}
				Thread.CurrentThread.SetExecutionContext(prevEC);
				thread = null;
			}
		}
	}
}
