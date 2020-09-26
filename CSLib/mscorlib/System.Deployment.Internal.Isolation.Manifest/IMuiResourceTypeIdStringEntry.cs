using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[Guid("11df5cad-c183-479b-9a44-3842b71639ce")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMuiResourceTypeIdStringEntry
	{
		MuiResourceTypeIdStringEntry AllData
		{
			get;
		}

		object StringIds
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}

		object IntegerIds
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}
	}
}
