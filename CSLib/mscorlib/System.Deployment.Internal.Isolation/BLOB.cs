using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal struct BLOB : IDisposable
	{
		[MarshalAs(UnmanagedType.U4)]
		public uint Size;

		[MarshalAs(UnmanagedType.SysInt)]
		public IntPtr BlobData;

		public void Dispose()
		{
			if (BlobData != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(BlobData);
				BlobData = IntPtr.Zero;
			}
		}
	}
}
