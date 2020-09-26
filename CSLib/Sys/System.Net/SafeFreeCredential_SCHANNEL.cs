using System.Security;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeFreeCredential_SCHANNEL : SafeFreeCredentials
	{
		private const string SCHANNEL = "schannel.Dll";

		protected override bool ReleaseHandle()
		{
			return UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.FreeCredentialsHandle(ref _handle) == 0;
		}
	}
}
