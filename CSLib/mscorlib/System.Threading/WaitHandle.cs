using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
	[ComVisible(true)]
	public abstract class WaitHandle : MarshalByRefObject, IDisposable
	{
		public const int WaitTimeout = 258;

		private const int MAX_WAITHANDLES = 64;

		private const int WAIT_OBJECT_0 = 0;

		private const int WAIT_ABANDONED = 128;

		private const int WAIT_FAILED = int.MaxValue;

		private const int ERROR_TOO_MANY_POSTS = 298;

		private IntPtr waitHandle;

		internal SafeWaitHandle safeWaitHandle;

		internal bool hasThreadAffinity;

		protected static readonly IntPtr InvalidHandle = Win32Native.INVALID_HANDLE_VALUE;

		[Obsolete("Use the SafeWaitHandle property instead.")]
		public virtual IntPtr Handle
		{
			get
			{
				if (safeWaitHandle != null)
				{
					return safeWaitHandle.DangerousGetHandle();
				}
				return InvalidHandle;
			}
			[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				if (value == InvalidHandle)
				{
					safeWaitHandle.SetHandleAsInvalid();
					safeWaitHandle = null;
				}
				else
				{
					safeWaitHandle = new SafeWaitHandle(value, ownsHandle: true);
				}
				waitHandle = value;
			}
		}

		public SafeWaitHandle SafeWaitHandle
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				if (safeWaitHandle == null)
				{
					safeWaitHandle = new SafeWaitHandle(InvalidHandle, ownsHandle: false);
				}
				return safeWaitHandle;
			}
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					if (value == null)
					{
						safeWaitHandle = null;
						waitHandle = InvalidHandle;
					}
					else
					{
						safeWaitHandle = value;
						waitHandle = safeWaitHandle.DangerousGetHandle();
					}
				}
			}
		}

		protected WaitHandle()
		{
			safeWaitHandle = null;
			waitHandle = InvalidHandle;
			hasThreadAffinity = false;
		}

		internal void SetHandleInternal(SafeWaitHandle handle)
		{
			safeWaitHandle = handle;
			waitHandle = handle.DangerousGetHandle();
		}

		public virtual bool WaitOne(int millisecondsTimeout, bool exitContext)
		{
			if (millisecondsTimeout < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return WaitOne((long)millisecondsTimeout, exitContext);
		}

		public virtual bool WaitOne(TimeSpan timeout, bool exitContext)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (-1 > num || int.MaxValue < num)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return WaitOne(num, exitContext);
		}

		public virtual bool WaitOne()
		{
			return WaitOne(-1, exitContext: false);
		}

		public virtual bool WaitOne(int millisecondsTimeout)
		{
			return WaitOne(millisecondsTimeout, exitContext: false);
		}

		public virtual bool WaitOne(TimeSpan timeout)
		{
			return WaitOne(timeout, exitContext: false);
		}

		private bool WaitOne(long timeout, bool exitContext)
		{
			if (safeWaitHandle == null)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
			}
			int num = WaitOneNative(safeWaitHandle, (uint)timeout, hasThreadAffinity, exitContext);
			if (num == 128)
			{
				throw new AbandonedMutexException();
			}
			return num != 258;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int WaitOneNative(SafeWaitHandle waitHandle, uint millisecondsTimeout, bool hasThreadAffinity, bool exitContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern int WaitMultiple(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext, bool WaitAll);

		public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
		{
			if (waitHandles == null || waitHandles.Length == 0)
			{
				throw new ArgumentNullException("waitHandles");
			}
			if (waitHandles.Length > 64)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_MaxWaitHandles"));
			}
			if (-1 > millisecondsTimeout)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			WaitHandle[] array = new WaitHandle[waitHandles.Length];
			for (int i = 0; i < waitHandles.Length; i++)
			{
				WaitHandle waitHandle = waitHandles[i];
				if (waitHandle == null)
				{
					throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_ArrayElement"));
				}
				if (RemotingServices.IsTransparentProxy(waitHandle))
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WaitOnTransparentProxy"));
				}
				array[i] = waitHandle;
			}
			int num = WaitMultiple(array, millisecondsTimeout, exitContext, WaitAll: true);
			if (128 <= num && 128 + array.Length > num)
			{
				throw new AbandonedMutexException();
			}
			for (int j = 0; j < array.Length; j++)
			{
				GC.KeepAlive(array[j]);
			}
			return num != 258;
		}

		public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (-1 > num || int.MaxValue < num)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return WaitAll(waitHandles, (int)num, exitContext);
		}

		public static bool WaitAll(WaitHandle[] waitHandles)
		{
			return WaitAll(waitHandles, -1, exitContext: true);
		}

		public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout)
		{
			return WaitAll(waitHandles, millisecondsTimeout, exitContext: true);
		}

		public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout)
		{
			return WaitAll(waitHandles, timeout, exitContext: true);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
		{
			if (waitHandles == null)
			{
				throw new ArgumentNullException("waitHandles");
			}
			if (64 < waitHandles.Length)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_MaxWaitHandles"));
			}
			if (-1 > millisecondsTimeout)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			WaitHandle[] array = new WaitHandle[waitHandles.Length];
			for (int i = 0; i < waitHandles.Length; i++)
			{
				WaitHandle waitHandle = waitHandles[i];
				if (waitHandle == null)
				{
					throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_ArrayElement"));
				}
				if (RemotingServices.IsTransparentProxy(waitHandle))
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WaitOnTransparentProxy"));
				}
				array[i] = waitHandle;
			}
			int num = WaitMultiple(array, millisecondsTimeout, exitContext, WaitAll: false);
			for (int j = 0; j < array.Length; j++)
			{
				GC.KeepAlive(array[j]);
			}
			if (128 <= num && 128 + array.Length > num)
			{
				int num2 = num - 128;
				if (0 <= num2 && num2 < array.Length)
				{
					throw new AbandonedMutexException(num2, array[num2]);
				}
				throw new AbandonedMutexException();
			}
			return num;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (-1 > num || int.MaxValue < num)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return WaitAny(waitHandles, (int)num, exitContext);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles)
		{
			return WaitAny(waitHandles, -1, exitContext: true);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout)
		{
			return WaitAny(waitHandles, millisecondsTimeout, exitContext: true);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout)
		{
			return WaitAny(waitHandles, timeout, exitContext: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int SignalAndWaitOne(SafeWaitHandle waitHandleToSignal, SafeWaitHandle waitHandleToWaitOn, int millisecondsTimeout, bool hasThreadAffinity, bool exitContext);

		public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn)
		{
			return SignalAndWait(toSignal, toWaitOn, -1, exitContext: false);
		}

		public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, TimeSpan timeout, bool exitContext)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (-1 > num || int.MaxValue < num)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return SignalAndWait(toSignal, toWaitOn, (int)num, exitContext);
		}

		public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, int millisecondsTimeout, bool exitContext)
		{
			if ((Environment.OSInfo & Environment.OSName.Win9x) != 0)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
			}
			if (toSignal == null)
			{
				throw new ArgumentNullException("toSignal");
			}
			if (toWaitOn == null)
			{
				throw new ArgumentNullException("toWaitOn");
			}
			if (-1 > millisecondsTimeout)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			int num = SignalAndWaitOne(toSignal.safeWaitHandle, toWaitOn.safeWaitHandle, millisecondsTimeout, toWaitOn.hasThreadAffinity, exitContext);
			if (int.MaxValue != num && toSignal.hasThreadAffinity)
			{
				Thread.EndCriticalRegion();
				Thread.EndThreadAffinity();
			}
			if (128 == num)
			{
				throw new AbandonedMutexException();
			}
			if (298 == num)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Threading.WaitHandleTooManyPosts"));
			}
			if (num == 0)
			{
				return true;
			}
			return false;
		}

		public virtual void Close()
		{
			Dispose(explicitDisposing: true);
			GC.nativeSuppressFinalize(this);
		}

		protected virtual void Dispose(bool explicitDisposing)
		{
			if (safeWaitHandle != null)
			{
				safeWaitHandle.Close();
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(explicitDisposing: true);
			GC.nativeSuppressFinalize(this);
		}
	}
}
