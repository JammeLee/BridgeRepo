using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal SafeRegistryHandle()
			: base(ownsHandle: true)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal SafeRegistryHandle(IntPtr preexistingHandle, bool ownsHandle)
			: base(ownsHandle)
		{
			SetHandle(preexistingHandle);
		}

		[DllImport("advapi32.dll")]
		[SuppressUnmanagedCodeSecurity]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern int RegCloseKey(IntPtr hKey);

		protected override bool ReleaseHandle()
		{
			int num = RegCloseKey(handle);
			return num == 0;
		}
	}
}
