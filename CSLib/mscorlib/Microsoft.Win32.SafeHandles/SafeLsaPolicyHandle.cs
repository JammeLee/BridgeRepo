using System;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeLsaPolicyHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeLsaPolicyHandle InvalidHandle => new SafeLsaPolicyHandle(IntPtr.Zero);

		private SafeLsaPolicyHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeLsaPolicyHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.LsaClose(handle) == 0;
		}
	}
}
