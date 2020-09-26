using System;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles
{
	[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
	public sealed class SafeFileHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private SafeFileHandle()
			: base(ownsHandle: true)
		{
		}

		public SafeFileHandle(IntPtr preexistingHandle, bool ownsHandle)
			: base(ownsHandle)
		{
			SetHandle(preexistingHandle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.CloseHandle(handle);
		}
	}
}
