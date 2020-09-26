using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles
{
	[SuppressUnmanagedCodeSecurity]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	internal sealed class SafeEventHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal SafeEventHandle()
			: base(ownsHandle: true)
		{
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern SafeEventHandle CreateEvent(HandleRef lpEventAttributes, bool bManualReset, bool bInitialState, string name);

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern bool CloseHandle(IntPtr handle);

		protected override bool ReleaseHandle()
		{
			return CloseHandle(handle);
		}
	}
}
