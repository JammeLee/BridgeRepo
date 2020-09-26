using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.CompilerServices
{
	public static class RuntimeHelpers
	{
		public delegate void TryCode(object userData);

		public delegate void CleanupCode(object userData, bool exceptionThrown);

		private class ExecuteWithLockHelper
		{
			internal object m_lockObject;

			internal bool m_tookLock;

			internal TryCode m_userCode;

			internal object m_userState;

			internal ExecuteWithLockHelper(object lockObject, TryCode userCode, object userState)
			{
				m_lockObject = lockObject;
				m_userCode = userCode;
				m_userState = userState;
			}
		}

		private static TryCode s_EnterMonitor = EnterMonitorAndTryCode;

		private static CleanupCode s_ExitMonitor = ExitMonitorOnBackout;

		public static int OffsetToStringData => 12;

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void InitializeArray(Array array, RuntimeFieldHandle fldHandle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern object GetObjectValue(object obj);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _RunClassConstructor(IntPtr type);

		public static void RunClassConstructor(RuntimeTypeHandle type)
		{
			_RunClassConstructor(type.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _RunModuleConstructor(IntPtr module);

		public unsafe static void RunModuleConstructor(ModuleHandle module)
		{
			_RunModuleConstructor(new IntPtr(module.Value));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _PrepareMethod(IntPtr method, RuntimeTypeHandle[] instantiation);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _CompileMethod(IntPtr method);

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void PrepareMethod(RuntimeMethodHandle method)
		{
			_PrepareMethod(method.Value, null);
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void PrepareMethod(RuntimeMethodHandle method, RuntimeTypeHandle[] instantiation)
		{
			_PrepareMethod(method.Value, instantiation);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static extern void PrepareDelegate(Delegate d);

		public static int GetHashCode(object o)
		{
			return object.InternalGetHashCode(o);
		}

		public new static bool Equals(object o1, object o2)
		{
			return object.InternalEquals(o1, o2);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static extern void ProbeForSufficientStack();

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void PrepareConstrainedRegions()
		{
			ProbeForSufficientStack();
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void PrepareConstrainedRegionsNoOP()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static extern void ExecuteCodeWithGuaranteedCleanup(TryCode code, CleanupCode backoutCode, object userData);

		[PrePrepareMethod]
		internal static void ExecuteBackoutCodeHelper(object backoutCode, object userData, bool exceptionThrown)
		{
			((CleanupCode)backoutCode)(userData, exceptionThrown);
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal static void ExecuteCodeWithLock(object lockObject, TryCode code, object userState)
		{
			ExecuteWithLockHelper userData = new ExecuteWithLockHelper(lockObject, code, userState);
			ExecuteCodeWithGuaranteedCleanup(s_EnterMonitor, s_ExitMonitor, userData);
		}

		private static void EnterMonitorAndTryCode(object helper)
		{
			ExecuteWithLockHelper executeWithLockHelper = (ExecuteWithLockHelper)helper;
			Monitor.ReliableEnter(executeWithLockHelper.m_lockObject, ref executeWithLockHelper.m_tookLock);
			executeWithLockHelper.m_userCode(executeWithLockHelper.m_userState);
		}

		[PrePrepareMethod]
		private static void ExitMonitorOnBackout(object helper, bool exceptionThrown)
		{
			ExecuteWithLockHelper executeWithLockHelper = (ExecuteWithLockHelper)helper;
			if (executeWithLockHelper.m_tookLock)
			{
				Monitor.Exit(executeWithLockHelper.m_lockObject);
			}
		}
	}
}
