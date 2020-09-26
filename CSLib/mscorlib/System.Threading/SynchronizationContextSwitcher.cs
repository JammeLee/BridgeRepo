using System.Runtime.ConstrainedExecution;

namespace System.Threading
{
	internal struct SynchronizationContextSwitcher : IDisposable
	{
		internal SynchronizationContext savedSC;

		internal SynchronizationContext currSC;

		internal ExecutionContext _ec;

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is SynchronizationContextSwitcher))
			{
				return false;
			}
			SynchronizationContextSwitcher synchronizationContextSwitcher = (SynchronizationContextSwitcher)obj;
			if (savedSC == synchronizationContextSwitcher.savedSC && currSC == synchronizationContextSwitcher.currSC)
			{
				return _ec == synchronizationContextSwitcher._ec;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public static bool operator ==(SynchronizationContextSwitcher c1, SynchronizationContextSwitcher c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(SynchronizationContextSwitcher c1, SynchronizationContextSwitcher c2)
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
			if (_ec == null)
			{
				return true;
			}
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
			if (_ec != null)
			{
				ExecutionContext executionContextNoCreate = Thread.CurrentThread.GetExecutionContextNoCreate();
				if (_ec != executionContextNoCreate)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
				}
				if (currSC != _ec.SynchronizationContext)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
				}
				executionContextNoCreate.SynchronizationContext = savedSC;
				_ec = null;
			}
		}
	}
}
