using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	[ComVisible(true)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public sealed class ReaderWriterLock : CriticalFinalizerObject
	{
		private IntPtr _hWriterEvent;

		private IntPtr _hReaderEvent;

		private IntPtr _hObjectHandle;

		private int _dwState;

		private int _dwULockID;

		private int _dwLLockID;

		private int _dwWriterID;

		private int _dwWriterSeqNum;

		private short _wWriterLevel;

		public bool IsReaderLockHeld
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return PrivateGetIsReaderLockHeld();
			}
		}

		public bool IsWriterLockHeld
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return PrivateGetIsWriterLockHeld();
			}
		}

		public int WriterSeqNum => PrivateGetWriterSeqNum();

		public ReaderWriterLock()
		{
			PrivateInitialize();
		}

		~ReaderWriterLock()
		{
			PrivateDestruct();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void AcquireReaderLockInternal(int millisecondsTimeout);

		public void AcquireReaderLock(int millisecondsTimeout)
		{
			AcquireReaderLockInternal(millisecondsTimeout);
		}

		public void AcquireReaderLock(TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			AcquireReaderLockInternal((int)num);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void AcquireWriterLockInternal(int millisecondsTimeout);

		public void AcquireWriterLock(int millisecondsTimeout)
		{
			AcquireWriterLockInternal(millisecondsTimeout);
		}

		public void AcquireWriterLock(TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			AcquireWriterLockInternal((int)num);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private extern void ReleaseReaderLockInternal();

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public void ReleaseReaderLock()
		{
			ReleaseReaderLockInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private extern void ReleaseWriterLockInternal();

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public void ReleaseWriterLock()
		{
			ReleaseWriterLockInternal();
		}

		public LockCookie UpgradeToWriterLock(int millisecondsTimeout)
		{
			LockCookie result = default(LockCookie);
			FCallUpgradeToWriterLock(ref result, millisecondsTimeout);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void FCallUpgradeToWriterLock(ref LockCookie result, int millisecondsTimeout);

		public LockCookie UpgradeToWriterLock(TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return UpgradeToWriterLock((int)num);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void DowngradeFromWriterLockInternal(ref LockCookie lockCookie);

		public void DowngradeFromWriterLock(ref LockCookie lockCookie)
		{
			DowngradeFromWriterLockInternal(ref lockCookie);
		}

		public LockCookie ReleaseLock()
		{
			LockCookie result = default(LockCookie);
			FCallReleaseLock(ref result);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void FCallReleaseLock(ref LockCookie result);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void RestoreLockInternal(ref LockCookie lockCookie);

		public void RestoreLock(ref LockCookie lockCookie)
		{
			RestoreLockInternal(ref lockCookie);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private extern bool PrivateGetIsReaderLockHeld();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private extern bool PrivateGetIsWriterLockHeld();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int PrivateGetWriterSeqNum();

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern bool AnyWritersSince(int seqNum);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void PrivateInitialize();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void PrivateDestruct();
	}
}
