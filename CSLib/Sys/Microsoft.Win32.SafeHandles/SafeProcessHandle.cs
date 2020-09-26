using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles
{
	[SuppressUnmanagedCodeSecurity]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeProcessHandle InvalidHandle = new SafeProcessHandle(IntPtr.Zero);

		internal SafeProcessHandle()
			: base(ownsHandle: true)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal SafeProcessHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern SafeProcessHandle OpenProcess(int access, bool inherit, int processId);

		internal void InitialSetHandle(IntPtr h)
		{
			handle = h;
		}

		protected override bool ReleaseHandle()
		{
			return SafeNativeMethods.CloseHandle(handle);
		}
	}
}
