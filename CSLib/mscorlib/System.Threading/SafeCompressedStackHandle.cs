using System.Runtime.InteropServices;

namespace System.Threading
{
	internal class SafeCompressedStackHandle : SafeHandle
	{
		public override bool IsInvalid => handle == IntPtr.Zero;

		public SafeCompressedStackHandle()
			: base(IntPtr.Zero, ownsHandle: true)
		{
		}

		protected override bool ReleaseHandle()
		{
			CompressedStack.DestroyDelayedCompressedStack(handle);
			handle = IntPtr.Zero;
			return true;
		}
	}
}
