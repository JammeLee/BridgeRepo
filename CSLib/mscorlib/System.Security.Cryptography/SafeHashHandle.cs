using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography
{
	internal sealed class SafeHashHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeHashHandle InvalidHandle => new SafeHashHandle(IntPtr.Zero);

		private SafeHashHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern void _FreeHash(IntPtr pHashCtx);

		protected override bool ReleaseHandle()
		{
			_FreeHash(handle);
			return true;
		}
	}
}
