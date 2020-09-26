using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography.X509Certificates
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

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern void _FreeCertStoreContext(IntPtr hCertStore);

		protected override bool ReleaseHandle()
		{
			_FreeCertStoreContext(handle);
			return true;
		}
	}
}
