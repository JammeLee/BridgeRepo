using System.Security;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeFreeContextBufferChannelBinding_SECUR32 : SafeFreeContextBufferChannelBinding
	{
		protected override bool ReleaseHandle()
		{
			return UnsafeNclNativeMethods.SafeNetHandles_SECUR32.FreeContextBuffer(handle) == 0;
		}
	}
}
