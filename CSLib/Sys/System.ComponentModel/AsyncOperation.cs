using System.Security.Permissions;
using System.Threading;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public sealed class AsyncOperation
	{
		private SynchronizationContext syncContext;

		private object userSuppliedState;

		private bool alreadyCompleted;

		public object UserSuppliedState => userSuppliedState;

		public SynchronizationContext SynchronizationContext => syncContext;

		private AsyncOperation(object userSuppliedState, SynchronizationContext syncContext)
		{
			this.userSuppliedState = userSuppliedState;
			this.syncContext = syncContext;
			alreadyCompleted = false;
			this.syncContext.OperationStarted();
		}

		~AsyncOperation()
		{
			if (!alreadyCompleted && syncContext != null)
			{
				syncContext.OperationCompleted();
			}
		}

		public void Post(SendOrPostCallback d, object arg)
		{
			VerifyNotCompleted();
			VerifyDelegateNotNull(d);
			syncContext.Post(d, arg);
		}

		public void PostOperationCompleted(SendOrPostCallback d, object arg)
		{
			Post(d, arg);
			OperationCompletedCore();
		}

		public void OperationCompleted()
		{
			VerifyNotCompleted();
			OperationCompletedCore();
		}

		private void OperationCompletedCore()
		{
			try
			{
				syncContext.OperationCompleted();
			}
			finally
			{
				alreadyCompleted = true;
				GC.SuppressFinalize(this);
			}
		}

		private void VerifyNotCompleted()
		{
			if (alreadyCompleted)
			{
				throw new InvalidOperationException(SR.GetString("Async_OperationAlreadyCompleted"));
			}
		}

		private void VerifyDelegateNotNull(SendOrPostCallback d)
		{
			if (d == null)
			{
				throw new ArgumentNullException(SR.GetString("Async_NullDelegate"), "d");
			}
		}

		internal static AsyncOperation CreateOperation(object userSuppliedState, SynchronizationContext syncContext)
		{
			return new AsyncOperation(userSuppliedState, syncContext);
		}
	}
}
