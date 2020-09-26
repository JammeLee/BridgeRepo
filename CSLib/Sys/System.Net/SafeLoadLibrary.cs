using System.Security;
using Microsoft.Win32.SafeHandles;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeLoadLibrary : SafeHandleZeroOrMinusOneIsInvalid
	{
		private const string KERNEL32 = "kernel32.dll";

		public static readonly SafeLoadLibrary Zero = new SafeLoadLibrary(ownsHandle: false);

		private SafeLoadLibrary()
			: base(ownsHandle: true)
		{
		}

		private SafeLoadLibrary(bool ownsHandle)
			: base(ownsHandle)
		{
		}

		public static SafeLoadLibrary LoadLibraryEx(string library)
		{
			SafeLoadLibrary safeLoadLibrary = (ComNetOS.IsWin9x ? UnsafeNclNativeMethods.SafeNetHandles.LoadLibraryExA(library, null, 0u) : UnsafeNclNativeMethods.SafeNetHandles.LoadLibraryExW(library, null, 0u));
			if (safeLoadLibrary.IsInvalid)
			{
				safeLoadLibrary.SetHandleAsInvalid();
			}
			return safeLoadLibrary;
		}

		protected override bool ReleaseHandle()
		{
			return UnsafeNclNativeMethods.SafeNetHandles.FreeLibrary(handle);
		}
	}
}
