using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("C31FF59E-CD25-47b8-9EF3-CF4433EB97CC")]
	internal interface IAssemblyReferenceDependentAssemblyEntry
	{
		AssemblyReferenceDependentAssemblyEntry AllData
		{
			get;
		}

		string Group
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string Codebase
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		ulong Size
		{
			get;
		}

		object HashValue
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}

		uint HashAlgorithm
		{
			get;
		}

		uint Flags
		{
			get;
		}

		string ResourceFallbackCulture
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string Description
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string SupportUrl
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		ISection HashElements
		{
			get;
		}
	}
}
