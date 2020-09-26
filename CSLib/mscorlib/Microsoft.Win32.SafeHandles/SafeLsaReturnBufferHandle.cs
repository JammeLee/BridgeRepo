using System;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeLsaReturnBufferHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeLsaReturnBufferHandle InvalidHandle => new SafeLsaReturnBufferHandle(IntPtr.Zero);

		private SafeLsaReturnBufferHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeLsaReturnBufferHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.LsaFreeReturnBuffer(handle) >= 0;
		}
	}
}
