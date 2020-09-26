using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeCloseIcmpHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private bool IsPostWin2K;

		private SafeCloseIcmpHandle()
			: base(ownsHandle: true)
		{
			IsPostWin2K = ComNetOS.IsPostWin2K;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override bool ReleaseHandle()
		{
			if (IsPostWin2K)
			{
				return UnsafeNetInfoNativeMethods.IcmpCloseHandle(handle);
			}
			return UnsafeIcmpNativeMethods.IcmpCloseHandle(handle);
		}
	}
}
