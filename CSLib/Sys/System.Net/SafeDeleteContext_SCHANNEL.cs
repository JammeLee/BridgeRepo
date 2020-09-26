using System.Security;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeDeleteContext_SCHANNEL : SafeDeleteContext
	{
		private const string SCHANNEL = "schannel.Dll";

		internal SafeDeleteContext_SCHANNEL()
		{
		}

		protected override bool ReleaseHandle()
		{
			if (_EffectiveCredential != null)
			{
				_EffectiveCredential.DangerousRelease();
			}
			return UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.DeleteSecurityContext(ref _handle) == 0;
		}
	}
}
