using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security.Permissions;

namespace System.Threading
{
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public static class ThreadPool
	{
		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public static bool SetMaxThreads(int workerThreads, int completionPortThreads)
		{
			return SetMaxThreadsNative(workerThreads, completionPortThreads);
		}

		public static void GetMaxThreads(out int workerThreads, out int completionPortThreads)
		{
			GetMaxThreadsNative(out workerThreads, out completionPortThreads);
		}

		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public static bool SetMinThreads(int workerThreads, int completionPortThreads)
		{
			ThreadPoolGlobals.tpWarmupCount = Math.Max(ThreadPoolGlobals.GetProcessorCount() * 2, workerThreads);
			return SetMinThreadsNative(workerThreads, completionPortThreads);
		}

		public static void GetMinThreads(out int workerThreads, out int completionPortThreads)
		{
			GetMinThreadsNative(out workerThreads, out completionPortThreads);
		}

		public static void GetAvailableThreads(out int workerThreads, out int completionPortThreads)
		{
			GetAvailableThreadsNative(out workerThreads, out completionPortThreads);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RegisterWaitForSingleObject(waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, compressStack: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RegisterWaitForSingleObject(waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, compressStack: false);
		}

		private static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce, ref StackCrawlMark stackMark, bool compressStack)
		{
			if (RemotingServices.IsTransparentProxy(waitObject))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WaitOnTransparentProxy"));
			}
			RegisteredWaitHandle registeredWaitHandle = new RegisteredWaitHandle();
			if (callBack != null)
			{
				_ThreadPoolWaitOrTimerCallback threadPoolWaitOrTimerCallback = new _ThreadPoolWaitOrTimerCallback(callBack, state, compressStack, ref stackMark);
				state = threadPoolWaitOrTimerCallback;
				registeredWaitHandle.SetWaitObject(waitObject);
				IntPtr handle = RegisterWaitForSingleObjectNative(waitObject, state, millisecondsTimeOutInterval, executeOnlyOnce, registeredWaitHandle, ref stackMark, compressStack);
				registeredWaitHandle.SetHandle(handle);
				return registeredWaitHandle;
			}
			throw new ArgumentNullException("WaitOrTimerCallback");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce)
		{
			if (millisecondsTimeOutInterval < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, compressStack: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce)
		{
			if (millisecondsTimeOutInterval < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, compressStack: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce)
		{
			if (millisecondsTimeOutInterval < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, compressStack: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce)
		{
			if (millisecondsTimeOutInterval < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, compressStack: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, TimeSpan timeout, bool executeOnlyOnce)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			if (num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_LessEqualToIntegerMaxVal"));
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)num, executeOnlyOnce, ref stackMark, compressStack: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, TimeSpan timeout, bool executeOnlyOnce)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			if (num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_LessEqualToIntegerMaxVal"));
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)num, executeOnlyOnce, ref stackMark, compressStack: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static bool QueueUserWorkItem(WaitCallback callBack, object state)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return QueueUserWorkItemHelper(callBack, state, ref stackMark, compressStack: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static bool QueueUserWorkItem(WaitCallback callBack)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return QueueUserWorkItemHelper(callBack, null, ref stackMark, compressStack: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public static bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return QueueUserWorkItemHelper(callBack, state, ref stackMark, compressStack: false);
		}

		private static bool QueueUserWorkItemHelper(WaitCallback callBack, object state, ref StackCrawlMark stackMark, bool compressStack)
		{
			bool result = true;
			if (callBack != null)
			{
				_ThreadPoolWaitCallback tpcallBack = new _ThreadPoolWaitCallback(callBack, state, compressStack, ref stackMark);
				if (!ThreadPoolGlobals.vmTpInitialized)
				{
					InitializeVMTp();
					ThreadPoolGlobals.vmTpInitialized = true;
				}
				uint num = ThreadPoolGlobals.tpQueue.EnQueue(tpcallBack);
				if (ThreadPoolGlobals.tpHosted || num < ThreadPoolGlobals.tpWarmupCount)
				{
					result = AdjustThreadsInPool(ThreadPoolGlobals.tpQueue.GetQueueCount());
				}
				else
				{
					UpdateNativeTpCount(ThreadPoolGlobals.tpQueue.GetQueueCount());
				}
				return result;
			}
			throw new ArgumentNullException("WaitCallback");
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool AdjustThreadsInPool(uint QueueLength);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void UpdateNativeTpCount(uint QueueLength);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern bool PostQueuedCompletionStatus(NativeOverlapped* overlapped);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void InitializeVMTp();

		[CLSCompliant(false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		public unsafe static bool UnsafeQueueNativeOverlapped(NativeOverlapped* overlapped)
		{
			return PostQueuedCompletionStatus(overlapped);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool SetMinThreadsNative(int workerThreads, int completionPortThreads);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool SetMaxThreadsNative(int workerThreads, int completionPortThreads);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetMinThreadsNative(out int workerThreads, out int completionPortThreads);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetMaxThreadsNative(out int workerThreads, out int completionPortThreads);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetAvailableThreadsNative(out int workerThreads, out int completionPortThreads);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void CompleteThreadPoolRequest(uint QueueLength);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool ShouldReturnToVm();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern bool SetAppDomainRequestActive();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern void ClearAppDomainRequestActive();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool IsThreadPoolHosted();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern void SetNativeTpEvent();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr RegisterWaitForSingleObjectNative(WaitHandle waitHandle, object state, uint timeOutInterval, bool executeOnlyOnce, RegisteredWaitHandle registeredWaitHandle, ref StackCrawlMark stackMark, bool compressStack);

		[Obsolete("ThreadPool.BindHandle(IntPtr) has been deprecated.  Please use ThreadPool.BindHandle(SafeHandle) instead.", false)]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static bool BindHandle(IntPtr osHandle)
		{
			return BindIOCompletionCallbackNative(osHandle);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static bool BindHandle(SafeHandle osHandle)
		{
			if (osHandle == null)
			{
				throw new ArgumentNullException("osHandle");
			}
			bool flag = false;
			bool success = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				osHandle.DangerousAddRef(ref success);
				return BindIOCompletionCallbackNative(osHandle.DangerousGetHandle());
			}
			finally
			{
				if (success)
				{
					osHandle.DangerousRelease();
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern bool BindIOCompletionCallbackNative(IntPtr fileHandle);
	}
}
