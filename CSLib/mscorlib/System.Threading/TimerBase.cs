using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	internal sealed class TimerBase : CriticalFinalizerObject, IDisposable
	{
		private IntPtr timerHandle;

		private IntPtr delegateInfo;

		private int timerDeleted;

		private int m_lock;

		~TimerBase()
		{
			bool flag = false;
			do
			{
				if (Interlocked.CompareExchange(ref m_lock, 1, 0) == 0)
				{
					flag = true;
					try
					{
						DeleteTimerNative(null);
					}
					finally
					{
						m_lock = 0;
					}
				}
				Thread.SpinWait(1);
			}
			while (!flag);
		}

		internal void AddTimer(TimerCallback callback, object state, uint dueTime, uint period, ref StackCrawlMark stackMark)
		{
			if (callback != null)
			{
				_TimerCallback timerCallback = new _TimerCallback(callback, state, ref stackMark);
				state = timerCallback;
				AddTimerNative(state, dueTime, period, ref stackMark);
				timerDeleted = 0;
				return;
			}
			throw new ArgumentNullException("TimerCallback");
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal bool ChangeTimer(uint dueTime, uint period)
		{
			bool result = false;
			bool flag = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				do
				{
					if (Interlocked.CompareExchange(ref m_lock, 1, 0) == 0)
					{
						flag = true;
						try
						{
							if (timerDeleted != 0)
							{
								throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
							}
							result = ChangeTimerNative(dueTime, period);
						}
						finally
						{
							m_lock = 0;
						}
					}
					Thread.SpinWait(1);
				}
				while (!flag);
			}
			return result;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal bool Dispose(WaitHandle notifyObject)
		{
			bool result = false;
			bool flag = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				do
				{
					if (Interlocked.CompareExchange(ref m_lock, 1, 0) == 0)
					{
						flag = true;
						try
						{
							result = DeleteTimerNative(notifyObject.SafeWaitHandle);
						}
						finally
						{
							m_lock = 0;
						}
					}
					Thread.SpinWait(1);
				}
				while (!flag);
				GC.SuppressFinalize(this);
			}
			return result;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public void Dispose()
		{
			bool flag = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				do
				{
					if (Interlocked.CompareExchange(ref m_lock, 1, 0) == 0)
					{
						flag = true;
						try
						{
							DeleteTimerNative(null);
						}
						finally
						{
							m_lock = 0;
						}
					}
					Thread.SpinWait(1);
				}
				while (!flag);
				GC.SuppressFinalize(this);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void AddTimerNative(object state, uint dueTime, uint period, ref StackCrawlMark stackMark);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool ChangeTimerNative(uint dueTime, uint period);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool DeleteTimerNative(SafeHandle notifyObject);
	}
}
