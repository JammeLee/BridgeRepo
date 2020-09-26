using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

namespace System.Threading
{
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
	public class SynchronizationContext
	{
		private SynchronizationContextProperties _props;

		public static SynchronizationContext Current => Thread.CurrentThread.GetExecutionContextNoCreate()?.SynchronizationContext;

		protected void SetWaitNotificationRequired()
		{
			RuntimeHelpers.PrepareDelegate(new WaitDelegate(Wait));
			_props |= SynchronizationContextProperties.RequireWaitNotification;
		}

		public bool IsWaitNotificationRequired()
		{
			return (_props & SynchronizationContextProperties.RequireWaitNotification) != 0;
		}

		public virtual void Send(SendOrPostCallback d, object state)
		{
			d(state);
		}

		public virtual void Post(SendOrPostCallback d, object state)
		{
			ThreadPool.QueueUserWorkItem(d.Invoke, state);
		}

		public virtual void OperationStarted()
		{
		}

		public virtual void OperationCompleted()
		{
		}

		[CLSCompliant(false)]
		[PrePrepareMethod]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public virtual int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			if (waitHandles == null)
			{
				throw new ArgumentNullException("waitHandles");
			}
			return WaitHelper(waitHandles, waitAll, millisecondsTimeout);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[PrePrepareMethod]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[CLSCompliant(false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		protected static extern int WaitHelper(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public static void SetSynchronizationContext(SynchronizationContext syncContext)
		{
			SetSynchronizationContext(syncContext, Thread.CurrentThread.ExecutionContext.SynchronizationContext);
		}

		internal static SynchronizationContextSwitcher SetSynchronizationContext(SynchronizationContext syncContext, SynchronizationContext prevSyncContext)
		{
			ExecutionContext executionContext = Thread.CurrentThread.ExecutionContext;
			SynchronizationContextSwitcher result = default(SynchronizationContextSwitcher);
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				result._ec = executionContext;
				result.savedSC = prevSyncContext;
				result.currSC = syncContext;
				executionContext.SynchronizationContext = syncContext;
				return result;
			}
			catch
			{
				result.UndoNoThrow();
				throw;
			}
		}

		public virtual SynchronizationContext CreateCopy()
		{
			return new SynchronizationContext();
		}

		private static int InvokeWaitMethodHelper(SynchronizationContext syncContext, IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			return syncContext.Wait(waitHandles, waitAll, millisecondsTimeout);
		}
	}
}
