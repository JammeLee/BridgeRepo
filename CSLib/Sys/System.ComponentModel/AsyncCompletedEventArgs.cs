using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class AsyncCompletedEventArgs : EventArgs
	{
		private readonly Exception error;

		private readonly bool cancelled;

		private readonly object userState;

		[SRDescription("Async_AsyncEventArgs_Cancelled")]
		public bool Cancelled => cancelled;

		[SRDescription("Async_AsyncEventArgs_Error")]
		public Exception Error => error;

		[SRDescription("Async_AsyncEventArgs_UserState")]
		public object UserState => userState;

		public AsyncCompletedEventArgs(Exception error, bool cancelled, object userState)
		{
			this.error = error;
			this.cancelled = cancelled;
			this.userState = userState;
		}

		protected void RaiseExceptionIfNecessary()
		{
			if (Error != null)
			{
				throw new TargetInvocationException(SR.GetString("Async_ExceptionOccurred"), Error);
			}
			if (Cancelled)
			{
				throw new InvalidOperationException(SR.GetString("Async_OperationCancelled"));
			}
		}
	}
}
