using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography
{
	internal sealed class SafeCertStoreHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeCertStoreHandle InvalidHandle => new SafeCertStoreHandle(IntPtr.Zero);

		private SafeCertStoreHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeCertStoreHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		[DllImport("crypt32.dll", SetLastError = true)]
		[SuppressUnmanagedCodeSecurity]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern bool CertCloseStore(IntPtr hCertStore, uint dwFlags);

		protected override bool ReleaseHandle()
		{
			return CertCloseStore(handle, 0u);
		}
	}
}
