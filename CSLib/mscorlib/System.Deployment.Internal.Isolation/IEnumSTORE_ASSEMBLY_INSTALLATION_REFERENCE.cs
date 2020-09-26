using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("d8b1aacb-5142-4abb-bcc1-e9dc9052a89e")]
	internal interface IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE
	{
		uint Next([In] uint celt, [Out][MarshalAs(UnmanagedType.LPArray)] StoreApplicationReference[] rgelt);

		void Skip([In] uint celt);

		void Reset();

		IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE Clone();
	}
}
