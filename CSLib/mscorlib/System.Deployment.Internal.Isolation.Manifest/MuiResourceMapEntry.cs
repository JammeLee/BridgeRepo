using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[StructLayout(LayoutKind.Sequential)]
	internal class MuiResourceMapEntry : IDisposable
	{
		[MarshalAs(UnmanagedType.SysInt)]
		public IntPtr ResourceTypeIdInt;

		public uint ResourceTypeIdIntSize;

		[MarshalAs(UnmanagedType.SysInt)]
		public IntPtr ResourceTypeIdString;

		public uint ResourceTypeIdStringSize;

		~MuiResourceMapEntry()
		{
			Dispose(fDisposing: false);
		}

		void IDisposable.Dispose()
		{
			Dispose(fDisposing: true);
		}

		public void Dispose(bool fDisposing)
		{
			if (ResourceTypeIdInt != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(ResourceTypeIdInt);
				ResourceTypeIdInt = IntPtr.Zero;
			}
			if (ResourceTypeIdString != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(ResourceTypeIdString);
				ResourceTypeIdString = IntPtr.Zero;
			}
			if (fDisposing)
			{
				GC.SuppressFinalize(this);
			}
		}
	}
}
