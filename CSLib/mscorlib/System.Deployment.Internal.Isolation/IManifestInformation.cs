using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	[ComImport]
	[Guid("81c85208-fe61-4c15-b5bb-ff5ea66baad9")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IManifestInformation
	{
		void get_FullPath([MarshalAs(UnmanagedType.LPWStr)] out string FullPath);
	}
}
