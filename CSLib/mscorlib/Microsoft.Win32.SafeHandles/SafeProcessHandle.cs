using System;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeProcessHandle InvalidHandle => new SafeProcessHandle(IntPtr.Zero);

		private SafeProcessHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeProcessHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.CloseHandle(handle);
		}
	}
}
