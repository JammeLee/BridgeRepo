using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[Guid("1583EFE9-832F-4d08-B041-CAC5ACEDB948")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEntryPointEntry
	{
		EntryPointEntry AllData
		{
			get;
		}

		string Name
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string CommandLine_File
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string CommandLine_Parameters
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		IReferenceIdentity Identity
		{
			get;
		}

		uint Flags
		{
			get;
		}
	}
}
