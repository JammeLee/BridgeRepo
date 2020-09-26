using System.Runtime.ConstrainedExecution;
using System.Security.Principal;
using System.Threading;

namespace System.Security
{
	internal struct SecurityContextSwitcher : IDisposable
	{
		internal SecurityContext prevSC;

		internal SecurityContext currSC;

		internal ExecutionContext currEC;

		internal CompressedStackSwitcher cssw;

		internal WindowsImpersonationContext wic;

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is SecurityContextSwitcher))
			{
				return false;
			}
			SecurityContextSwitcher securityContextSwitcher = (SecurityContextSwitcher)obj;
			if (prevSC == securityContextSwitcher.prevSC && currSC == securityContextSwitcher.currSC && currEC == securityContextSwitcher.currEC && cssw == securityContextSwitcher.cssw)
			{
				return wic == securityContextSwitcher.wic;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public static bool operator ==(SecurityContextSwitcher c1, SecurityContextSwitcher c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(SecurityContextSwitcher c1, SecurityContextSwitcher c2)
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
			if (currEC == null)
			{
				return;
			}
			if (currEC != Thread.CurrentThread.GetExecutionContextNoCreate())
			{
				Environment.FailFast(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
			}
			if (currSC != currEC.SecurityContext)
			{
				Environment.FailFast(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
			}
			currEC.SecurityContext = prevSC;
			currEC = null;
			bool flag = true;
			try
			{
				if (wic != null)
				{
					flag &= wic.UndoNoThrow();
				}
			}
			catch
			{
				flag &= cssw.UndoNoThrow();
				Environment.FailFast(Environment.GetResourceString("ExecutionContext_UndoFailed"));
			}
			if (!(flag & cssw.UndoNoThrow()))
			{
				Environment.FailFast(Environment.GetResourceString("ExecutionContext_UndoFailed"));
			}
		}
	}
}
