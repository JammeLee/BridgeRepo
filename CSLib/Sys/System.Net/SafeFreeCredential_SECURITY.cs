using System.Security;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeFreeCredential_SECURITY : SafeFreeCredentials
	{
		private const string SECURITY = "security.Dll";

		protected override bool ReleaseHandle()
		{
			return UnsafeNclNativeMethods.SafeNetHandles_SECURITY.FreeCredentialsHandle(ref _handle) == 0;
		}
	}
}
