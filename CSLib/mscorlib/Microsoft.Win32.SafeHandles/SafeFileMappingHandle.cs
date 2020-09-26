using System;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal SafeFileMappingHandle()
			: base(ownsHandle: true)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal SafeFileMappingHandle(IntPtr handle, bool ownsHandle)
			: base(ownsHandle)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.CloseHandle(handle);
		}
	}
}
