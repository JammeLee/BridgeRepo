using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[Guid("FD47B733-AFBC-45e4-B7C2-BBEB1D9F766C")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAssemblyReferenceEntry
	{
		AssemblyReferenceEntry AllData
		{
			get;
		}

		IReferenceIdentity ReferenceIdentity
		{
			get;
		}

		uint Flags
		{
			get;
		}

		IAssemblyReferenceDependentAssemblyEntry DependentAssembly
		{
			get;
		}
	}
}
