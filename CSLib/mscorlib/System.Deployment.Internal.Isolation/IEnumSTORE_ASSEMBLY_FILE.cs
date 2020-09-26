using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("a5c6aaa3-03e4-478d-b9f5-2e45908d5e4f")]
	internal interface IEnumSTORE_ASSEMBLY_FILE
	{
		uint Next([In] uint celt, [Out][MarshalAs(UnmanagedType.LPArray)] STORE_ASSEMBLY_FILE[] rgelt);

		void Skip([In] uint celt);

		void Reset();

		IEnumSTORE_ASSEMBLY_FILE Clone();
	}
}
