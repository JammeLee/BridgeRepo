using System.Security;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeFreeContextBuffer_SECUR32 : SafeFreeContextBuffer
	{
		private const string SECUR32 = "secur32.dll";

		internal SafeFreeContextBuffer_SECUR32()
		{
		}

		protected override bool ReleaseHandle()
		{
			return UnsafeNclNativeMethods.SafeNetHandles_SECUR32.FreeContextBuffer(handle) == 0;
		}
	}
}
