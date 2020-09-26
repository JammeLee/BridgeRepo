using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[Guid("54F198EC-A63A-45ea-A984-452F68D9B35B")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IProgIdRedirectionEntry
	{
		ProgIdRedirectionEntry AllData
		{
			get;
		}

		string ProgId
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		Guid RedirectedGuid
		{
			get;
		}
	}
}
