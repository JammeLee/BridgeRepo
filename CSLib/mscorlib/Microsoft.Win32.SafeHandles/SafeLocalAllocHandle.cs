using System;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeLocalAllocHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeLocalAllocHandle InvalidHandle => new SafeLocalAllocHandle(IntPtr.Zero);

		private SafeLocalAllocHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeLocalAllocHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.LocalFree(handle) == IntPtr.Zero;
		}
	}
}
