using System;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeTokenHandle InvalidHandle => new SafeTokenHandle(IntPtr.Zero);

		private SafeTokenHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeTokenHandle(IntPtr handle)
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
