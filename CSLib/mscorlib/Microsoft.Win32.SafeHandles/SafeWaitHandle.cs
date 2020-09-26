using System;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles
{
	[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
	public sealed class SafeWaitHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private SafeWaitHandle()
			: base(ownsHandle: true)
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public SafeWaitHandle(IntPtr existingHandle, bool ownsHandle)
			: base(ownsHandle)
		{
			SetHandle(existingHandle);
		}

		protected override bool ReleaseHandle()
		{
			return Win32Native.CloseHandle(handle);
		}
	}
}
