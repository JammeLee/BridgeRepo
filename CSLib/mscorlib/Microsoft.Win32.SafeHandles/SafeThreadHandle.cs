using System;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private SafeThreadHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeThreadHandle(IntPtr handle)
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
