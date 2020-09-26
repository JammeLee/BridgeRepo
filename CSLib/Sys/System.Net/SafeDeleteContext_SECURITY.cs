using System.Security;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeDeleteContext_SECURITY : SafeDeleteContext
	{
		private const string SECURITY = "security.Dll";

		internal SafeDeleteContext_SECURITY()
		{
		}

		protected override bool ReleaseHandle()
		{
			if (_EffectiveCredential != null)
			{
				_EffectiveCredential.DangerousRelease();
			}
			return UnsafeNclNativeMethods.SafeNetHandles_SECURITY.DeleteSecurityContext(ref _handle) == 0;
		}
	}
}
