using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[StructLayout(LayoutKind.Sequential)]
	internal class MuiResourceTypeIdStringEntry : IDisposable
	{
		[MarshalAs(UnmanagedType.SysInt)]
		public IntPtr StringIds;

		public uint StringIdsSize;

		[MarshalAs(UnmanagedType.SysInt)]
		public IntPtr IntegerIds;

		public uint IntegerIdsSize;

		~MuiResourceTypeIdStringEntry()
		{
			Dispose(fDisposing: false);
		}

		void IDisposable.Dispose()
		{
			Dispose(fDisposing: true);
		}

		public void Dispose(bool fDisposing)
		{
			if (StringIds != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(StringIds);
				StringIds = IntPtr.Zero;
			}
			if (IntegerIds != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(IntegerIds);
				IntegerIds = IntPtr.Zero;
			}
			if (fDisposing)
			{
				GC.SuppressFinalize(this);
			}
		}
	}
}
