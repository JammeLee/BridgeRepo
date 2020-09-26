using System.Runtime.InteropServices;

namespace System.Threading
{
	internal class IUnknownSafeHandle : SafeHandle
	{
		public override bool IsInvalid => handle == IntPtr.Zero;

		public IUnknownSafeHandle()
			: base(IntPtr.Zero, ownsHandle: true)
		{
		}

		protected override bool ReleaseHandle()
		{
			HostExecutionContextManager.ReleaseHostSecurityContext(handle);
			return true;
		}

		internal object Clone()
		{
			IUnknownSafeHandle unknownSafeHandle = new IUnknownSafeHandle();
			if (!IsInvalid)
			{
				HostExecutionContextManager.CloneHostSecurityContext(this, unknownSafeHandle);
			}
			return unknownSafeHandle;
		}
	}
}
