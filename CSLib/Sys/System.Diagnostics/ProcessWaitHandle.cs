using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Diagnostics
{
	internal class ProcessWaitHandle : WaitHandle
	{
		internal ProcessWaitHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle)
		{
			SafeWaitHandle targetHandle = null;
			if (!NativeMethods.DuplicateHandle(new HandleRef(this, NativeMethods.GetCurrentProcess()), (SafeHandle)processHandle, new HandleRef(this, NativeMethods.GetCurrentProcess()), out targetHandle, 0, bInheritHandle: false, 2))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
			base.SafeWaitHandle = targetHandle;
		}
	}
}
