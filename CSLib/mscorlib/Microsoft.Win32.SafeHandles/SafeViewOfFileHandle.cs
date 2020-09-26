using System;
using System.Security.Permissions;

namespace Microsoft.Win32.SafeHandles
{
	internal sealed class SafeViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal SafeViewOfFileHandle()
			: base(ownsHandle: true)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal SafeViewOfFileHandle(IntPtr handle, bool ownsHandle)
			: base(ownsHandle)
		{
			SetHandle(handle);
		}

		protected override bool ReleaseHandle()
		{
			if (Win32Native.UnmapViewOfFile(handle))
			{
				handle = IntPtr.Zero;
				return true;
			}
			return false;
		}
	}
}
