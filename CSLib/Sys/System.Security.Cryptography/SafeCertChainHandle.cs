using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography
{
	internal sealed class SafeCertChainHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeCertChainHandle InvalidHandle => new SafeCertChainHandle(IntPtr.Zero);

		private SafeCertChainHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeCertChainHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		[DllImport("crypt32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SuppressUnmanagedCodeSecurity]
		private static extern void CertFreeCertificateChain(IntPtr handle);

		protected override bool ReleaseHandle()
		{
			CertFreeCertificateChain(handle);
			return true;
		}
	}
}
