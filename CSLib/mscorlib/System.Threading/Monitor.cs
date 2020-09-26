using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	[ComVisible(true)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public static class Monitor
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void Enter(object obj);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void ReliableEnter(object obj, ref bool tookLock);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern void Exit(object obj);

		public static bool TryEnter(object obj)
		{
			return TryEnterTimeout(obj, 0);
		}

		public static bool TryEnter(object obj, int millisecondsTimeout)
		{
			return TryEnterTimeout(obj, millisecondsTimeout);
		}

		public static bool TryEnter(object obj, TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return TryEnterTimeout(obj, (int)num);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool TryEnterTimeout(object obj, int timeout);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool ObjWait(bool exitContext, int millisecondsTimeout, object obj);

		public static bool Wait(object obj, int millisecondsTimeout, bool exitContext)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			return ObjWait(exitContext, millisecondsTimeout, obj);
		}

		public static bool Wait(object obj, TimeSpan timeout, bool exitContext)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return Wait(obj, (int)num, exitContext);
		}

		public static bool Wait(object obj, int millisecondsTimeout)
		{
			return Wait(obj, millisecondsTimeout, exitContext: false);
		}

		public static bool Wait(object obj, TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return Wait(obj, (int)num, exitContext: false);
		}

		public static bool Wait(object obj)
		{
			return Wait(obj, -1, exitContext: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void ObjPulse(object obj);

		public static void Pulse(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			ObjPulse(obj);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void ObjPulseAll(object obj);

		public static void PulseAll(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			ObjPulseAll(obj);
		}
	}
}
