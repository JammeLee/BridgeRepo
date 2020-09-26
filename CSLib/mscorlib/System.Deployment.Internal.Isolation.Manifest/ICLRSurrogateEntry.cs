using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("1E0422A1-F0D2-44ae-914B-8A2DECCFD22B")]
	internal interface ICLRSurrogateEntry
	{
		CLRSurrogateEntry AllData
		{
			get;
		}

		Guid Clsid
		{
			get;
		}

		string RuntimeVersion
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string ClassName
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}
	}
}
