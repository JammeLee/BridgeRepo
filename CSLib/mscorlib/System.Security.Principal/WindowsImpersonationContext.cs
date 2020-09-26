using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Principal
{
	[ComVisible(true)]
	public class WindowsImpersonationContext : IDisposable
	{
		private SafeTokenHandle m_safeTokenHandle = SafeTokenHandle.InvalidHandle;

		private WindowsIdentity m_wi;

		private FrameSecurityDescriptor m_fsd;

		private WindowsImpersonationContext()
		{
		}

		internal WindowsImpersonationContext(SafeTokenHandle safeTokenHandle, WindowsIdentity wi, bool isImpersonating, FrameSecurityDescriptor fsd)
		{
			if (!WindowsIdentity.RunningOnWin2K)
			{
				return;
			}
			if (safeTokenHandle.IsInvalid)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));
			}
			if (isImpersonating)
			{
				if (!Win32Native.DuplicateHandle(Win32Native.GetCurrentProcess(), safeTokenHandle, Win32Native.GetCurrentProcess(), ref m_safeTokenHandle, 0u, bInheritHandle: true, 2u))
				{
					throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
				}
				m_wi = wi;
			}
			m_fsd = fsd;
		}

		public void Undo()
		{
			if (!WindowsIdentity.RunningOnWin2K)
			{
				return;
			}
			int num = 0;
			if (m_safeTokenHandle.IsInvalid)
			{
				num = Win32.RevertToSelf();
				if (num < 0)
				{
					throw new SecurityException(Win32Native.GetMessage(num));
				}
			}
			else
			{
				num = Win32.RevertToSelf();
				if (num < 0)
				{
					throw new SecurityException(Win32Native.GetMessage(num));
				}
				num = Win32.ImpersonateLoggedOnUser(m_safeTokenHandle);
				if (num < 0)
				{
					throw new SecurityException(Win32Native.GetMessage(num));
				}
			}
			WindowsIdentity.UpdateThreadWI(m_wi);
			if (m_fsd != null)
			{
				m_fsd.SetTokenHandles(null, null);
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal bool UndoNoThrow()
		{
			bool flag = false;
			try
			{
				if (!WindowsIdentity.RunningOnWin2K)
				{
					return true;
				}
				int num = 0;
				if (m_safeTokenHandle.IsInvalid)
				{
					num = Win32.RevertToSelf();
				}
				else
				{
					num = Win32.RevertToSelf();
					if (num >= 0)
					{
						num = Win32.ImpersonateLoggedOnUser(m_safeTokenHandle);
					}
				}
				flag = num >= 0;
				if (m_fsd != null)
				{
					m_fsd.SetTokenHandles(null, null);
					return flag;
				}
				return flag;
			}
			catch
			{
				return false;
			}
		}

		[ComVisible(false)]
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && m_safeTokenHandle != null && !m_safeTokenHandle.IsClosed)
			{
				Undo();
				m_safeTokenHandle.Dispose();
			}
		}

		[ComVisible(false)]
		public void Dispose()
		{
			Dispose(disposing: true);
		}
	}
}
