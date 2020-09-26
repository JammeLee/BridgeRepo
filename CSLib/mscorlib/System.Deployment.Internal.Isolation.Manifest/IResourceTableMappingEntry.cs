using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("70A4ECEE-B195-4c59-85BF-44B6ACA83F07")]
	internal interface IResourceTableMappingEntry
	{
		ResourceTableMappingEntry AllData
		{
			get;
		}

		string id
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string FinalStringMapped
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}
	}
}
