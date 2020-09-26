using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("2474ECB4-8EFD-4410-9F31-B3E7C4A07731")]
	internal interface IAssemblyRequestEntry
	{
		AssemblyRequestEntry AllData
		{
			get;
		}

		string Name
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string permissionSetID
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}
	}
}
