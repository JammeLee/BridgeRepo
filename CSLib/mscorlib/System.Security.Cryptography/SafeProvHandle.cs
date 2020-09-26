using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography
{
	internal sealed class SafeProvHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeProvHandle InvalidHandle => new SafeProvHandle(IntPtr.Zero);

		private SafeProvHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern void _FreeCSP(IntPtr pProvCtx);

		protected override bool ReleaseHandle()
		{
			_FreeCSP(handle);
			return true;
		}
	}
}
