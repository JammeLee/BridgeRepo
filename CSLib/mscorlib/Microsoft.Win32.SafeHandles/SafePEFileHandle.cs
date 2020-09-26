using System;
using System.Security.Policy;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafePEFileHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafePEFileHandle InvalidHandle => new SafePEFileHandle(IntPtr.Zero);

		private SafePEFileHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			Hash._ReleasePEFile(handle);
			return true;
		}
	}
}
