using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[Guid("CF168CF4-4E8F-4d92-9D2A-60E5CA21CF85")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IDependentOSMetadataEntry
	{
		DependentOSMetadataEntry AllData
		{
			get;
		}

		string SupportUrl
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string Description
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		ushort MajorVersion
		{
			get;
		}

		ushort MinorVersion
		{
			get;
		}

		ushort BuildNumber
		{
			get;
		}

		byte ServicePackMajor
		{
			get;
		}

		byte ServicePackMinor
		{
			get;
		}
	}
}
