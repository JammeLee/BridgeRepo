using System;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeLsaMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeLsaMemoryHandle InvalidHandle => new SafeLsaMemoryHandle(IntPtr.Zero);

		private SafeLsaMemoryHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeLsaMemoryHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.LsaFreeMemory(handle) == 0;
		}
	}
}
