using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography.X509Certificates
{
	internal sealed class SafeCertContextHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeCertContextHandle InvalidHandle => new SafeCertContextHandle(IntPtr.Zero);

		internal IntPtr pCertContext
		{
			get
			{
				if (handle == IntPtr.Zero)
				{
					return IntPtr.Zero;
				}
				return Marshal.ReadIntPtr(handle);
			}
		}

		private SafeCertContextHandle()
			: base(ownsHandle: true)
		{
		}

		internal SafeCertContextHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern void _FreePCertContext(IntPtr pCert);

		protected override bool ReleaseHandle()
		{
			_FreePCertContext(handle);
			return true;
		}
	}
}
