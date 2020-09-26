using System.Security;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeDeleteContext_SECUR32 : SafeDeleteContext
	{
		private const string SECUR32 = "secur32.Dll";

		internal SafeDeleteContext_SECUR32()
		{
		}

		protected override bool ReleaseHandle()
		{
			if (_EffectiveCredential != null)
			{
				_EffectiveCredential.DangerousRelease();
			}
			return UnsafeNclNativeMethods.SafeNetHandles_SECUR32.DeleteSecurityContext(ref _handle) == 0;
		}
	}
}
