using System;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeLsaLogonProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeLsaLogonProcessHandle InvalidHandle => new SafeLsaLogonProcessHandle(IntPtr.Zero);

		private SafeLsaLogonProcessHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeLsaLogonProcessHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.LsaDeregisterLogonProcess(handle) >= 0;
		}
	}
}
