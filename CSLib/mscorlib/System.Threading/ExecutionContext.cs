using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.Threading
{
	[Serializable]
	public sealed class ExecutionContext : ISerializable
	{
		internal class ExecutionContextRunData
		{
			internal ExecutionContext ec;

			internal ContextCallback callBack;

			internal object state;

			internal ExecutionContextSwitcher ecsw;

			internal ExecutionContextRunData(ExecutionContext executionContext, ContextCallback cb, object state)
			{
				ec = executionContext;
				callBack = cb;
				this.state = state;
				ecsw = default(ExecutionContextSwitcher);
			}
		}

		private HostExecutionContext _hostExecutionContext;

		private SynchronizationContext _syncContext;

		private SecurityContext _securityContext;

		private LogicalCallContext _logicalCallContext;

		private IllogicalCallContext _illogicalCallContext;

		private Thread _thread;

		internal bool isNewCapture;

		internal bool isFlowSuppressed;

		internal static RuntimeHelpers.TryCode tryCode;

		internal static RuntimeHelpers.CleanupCode cleanupCode;

		internal LogicalCallContext LogicalCallContext
		{
			get
			{
				if (_logicalCallContext == null)
				{
					_logicalCallContext = new LogicalCallContext();
				}
				return _logicalCallContext;
			}
			set
			{
				_logicalCallContext = value;
			}
		}

		internal IllogicalCallContext IllogicalCallContext
		{
			get
			{
				if (_illogicalCallContext == null)
				{
					_illogicalCallContext = new IllogicalCallContext();
				}
				return _illogicalCallContext;
			}
			set
			{
				_illogicalCallContext = value;
			}
		}

		internal Thread Thread
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			set
			{
				_thread = value;
			}
		}

		internal SynchronizationContext SynchronizationContext
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return _syncContext;
			}
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			set
			{
				_syncContext = value;
			}
		}

		internal HostExecutionContext HostExecutionContext
		{
			get
			{
				return _hostExecutionContext;
			}
			set
			{
				_hostExecutionContext = value;
			}
		}

		internal SecurityContext SecurityContext
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return _securityContext;
			}
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			set
			{
				_securityContext = value;
				if (value != null)
				{
					_securityContext.ExecutionContext = this;
				}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal ExecutionContext()
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static void Run(ExecutionContext executionContext, ContextCallback callback, object state)
		{
			if (executionContext == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NullContext"));
			}
			if (!executionContext.isNewCapture)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotNewCaptureContext"));
			}
			executionContext.isNewCapture = false;
			ExecutionContext executionContextNoCreate = Thread.CurrentThread.GetExecutionContextNoCreate();
			if ((executionContextNoCreate == null || executionContextNoCreate.IsDefaultFTContext()) && SecurityContext.CurrentlyInDefaultFTSecurityContext(executionContextNoCreate) && executionContext.IsDefaultFTContext())
			{
				callback(state);
			}
			else
			{
				RunInternal(executionContext, callback, state);
			}
		}

		internal static void RunInternal(ExecutionContext executionContext, ContextCallback callback, object state)
		{
			if (cleanupCode == null)
			{
				tryCode = runTryCode;
				cleanupCode = runFinallyCode;
			}
			ExecutionContextRunData userData = new ExecutionContextRunData(executionContext, callback, state);
			RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(tryCode, cleanupCode, userData);
		}

		internal static void runTryCode(object userData)
		{
			ExecutionContextRunData executionContextRunData = (ExecutionContextRunData)userData;
			executionContextRunData.ecsw = SetExecutionContext(executionContextRunData.ec);
			executionContextRunData.callBack(executionContextRunData.state);
		}

		[PrePrepareMethod]
		internal static void runFinallyCode(object userData, bool exceptionThrown)
		{
			ExecutionContextRunData executionContextRunData = (ExecutionContextRunData)userData;
			executionContextRunData.ecsw.Undo();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static ExecutionContextSwitcher SetExecutionContext(ExecutionContext executionContext)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			ExecutionContextSwitcher result = default(ExecutionContextSwitcher);
			result.thread = Thread.CurrentThread;
			result.prevEC = Thread.CurrentThread.GetExecutionContextNoCreate();
			result.currEC = executionContext;
			Thread.CurrentThread.SetExecutionContext(executionContext);
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				if (executionContext != null)
				{
					SecurityContext securityContext = executionContext.SecurityContext;
					if (securityContext != null)
					{
						SecurityContext prevSecurityContext = ((result.prevEC != null) ? result.prevEC.SecurityContext : null);
						result.scsw = SecurityContext.SetSecurityContext(securityContext, prevSecurityContext, ref stackMark);
					}
					else if (!SecurityContext.CurrentlyInDefaultFTSecurityContext(result.prevEC))
					{
						SecurityContext prevSecurityContext2 = ((result.prevEC != null) ? result.prevEC.SecurityContext : null);
						result.scsw = SecurityContext.SetSecurityContext(SecurityContext.FullTrustSecurityContext, prevSecurityContext2, ref stackMark);
					}
					SynchronizationContext synchronizationContext = executionContext.SynchronizationContext;
					if (synchronizationContext != null)
					{
						SynchronizationContext prevSyncContext = ((result.prevEC != null) ? result.prevEC.SynchronizationContext : null);
						result.sysw = SynchronizationContext.SetSynchronizationContext(synchronizationContext, prevSyncContext);
					}
					HostExecutionContext hostExecutionContext = executionContext.HostExecutionContext;
					if (hostExecutionContext != null)
					{
						result.hecsw = HostExecutionContextManager.SetHostExecutionContextInternal(hostExecutionContext);
						return result;
					}
					return result;
				}
				return result;
			}
			catch
			{
				result.UndoNoThrow();
				throw;
			}
		}

		public ExecutionContext CreateCopy()
		{
			if (!isNewCapture)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotCopyUsedContext"));
			}
			ExecutionContext executionContext = new ExecutionContext();
			executionContext.isNewCapture = true;
			executionContext._syncContext = ((_syncContext == null) ? null : _syncContext.CreateCopy());
			executionContext._hostExecutionContext = ((_hostExecutionContext == null) ? null : _hostExecutionContext.CreateCopy());
			if (_securityContext != null)
			{
				executionContext._securityContext = _securityContext.CreateCopy();
				executionContext._securityContext.ExecutionContext = executionContext;
			}
			if (_logicalCallContext != null)
			{
				LogicalCallContext logicalCallContext = LogicalCallContext;
				executionContext.LogicalCallContext = (LogicalCallContext)logicalCallContext.Clone();
			}
			if (_illogicalCallContext != null)
			{
				IllogicalCallContext illogicalCallContext = IllogicalCallContext;
				executionContext.IllogicalCallContext = (IllogicalCallContext)illogicalCallContext.Clone();
			}
			return executionContext;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static AsyncFlowControl SuppressFlow()
		{
			if (IsFlowSuppressed())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotSupressFlowMultipleTimes"));
			}
			AsyncFlowControl result = default(AsyncFlowControl);
			result.Setup();
			return result;
		}

		public static void RestoreFlow()
		{
			ExecutionContext executionContextNoCreate = Thread.CurrentThread.GetExecutionContextNoCreate();
			if (executionContextNoCreate == null || !executionContextNoCreate.isFlowSuppressed)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotRestoreUnsupressedFlow"));
			}
			executionContextNoCreate.isFlowSuppressed = false;
		}

		public static bool IsFlowSuppressed()
		{
			return Thread.CurrentThread.GetExecutionContextNoCreate()?.isFlowSuppressed ?? false;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ExecutionContext Capture()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Capture(ref stackMark);
		}

		internal static ExecutionContext Capture(ref StackCrawlMark stackMark)
		{
			if (IsFlowSuppressed())
			{
				return null;
			}
			ExecutionContext executionContextNoCreate = Thread.CurrentThread.GetExecutionContextNoCreate();
			ExecutionContext executionContext = new ExecutionContext();
			executionContext.isNewCapture = true;
			executionContext.SecurityContext = SecurityContext.Capture(executionContextNoCreate, ref stackMark);
			if (executionContext.SecurityContext != null)
			{
				executionContext.SecurityContext.ExecutionContext = executionContext;
			}
			executionContext._hostExecutionContext = HostExecutionContextManager.CaptureHostExecutionContext();
			if (executionContextNoCreate != null)
			{
				executionContext._syncContext = ((executionContextNoCreate._syncContext == null) ? null : executionContextNoCreate._syncContext.CreateCopy());
				if (executionContextNoCreate._logicalCallContext != null)
				{
					LogicalCallContext logicalCallContext = executionContextNoCreate.LogicalCallContext;
					executionContext.LogicalCallContext = (LogicalCallContext)logicalCallContext.Clone();
				}
			}
			return executionContext;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (_logicalCallContext != null)
			{
				info.AddValue("LogicalCallContext", _logicalCallContext, typeof(LogicalCallContext));
			}
		}

		private ExecutionContext(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Name.Equals("LogicalCallContext"))
				{
					_logicalCallContext = (LogicalCallContext)enumerator.Value;
				}
			}
			Thread = Thread.CurrentThread;
		}

		internal static void ClearSyncContext(ExecutionContext ec)
		{
			if (ec != null)
			{
				ec.SynchronizationContext = null;
			}
		}

		internal bool IsDefaultFTContext()
		{
			if (_hostExecutionContext != null)
			{
				return false;
			}
			if (_syncContext != null)
			{
				return false;
			}
			if (_securityContext != null && !_securityContext.IsDefaultFTSecurityContext())
			{
				return false;
			}
			if (_logicalCallContext != null && _logicalCallContext.HasInfo)
			{
				return false;
			}
			if (_illogicalCallContext != null && _illogicalCallContext.HasUserData)
			{
				return false;
			}
			return true;
		}
	}
}
