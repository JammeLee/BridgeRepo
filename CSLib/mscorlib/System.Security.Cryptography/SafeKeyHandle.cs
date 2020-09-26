using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography
{
	internal sealed class SafeKeyHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeKeyHandle InvalidHandle => new SafeKeyHandle(IntPtr.Zero);

		private SafeKeyHandle(IntPtr handle)
			: base(ownsHandle: true)
		{
			SetHandle(handle);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern void _FreeHKey(IntPtr pKeyCtx);

		protected override bool ReleaseHandle()
		{
			_FreeHKey(handle);
			return true;
		}
	}
}
