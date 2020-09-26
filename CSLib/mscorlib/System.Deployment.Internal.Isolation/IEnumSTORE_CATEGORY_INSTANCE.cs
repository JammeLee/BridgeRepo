using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("5ba7cb30-8508-4114-8c77-262fcda4fadb")]
	internal interface IEnumSTORE_CATEGORY_INSTANCE
	{
		uint Next([In] uint ulElements, [Out][MarshalAs(UnmanagedType.LPArray)] STORE_CATEGORY_INSTANCE[] rgInstances);

		void Skip([In] uint ulElements);

		void Reset();

		IEnumSTORE_CATEGORY_INSTANCE Clone();
	}
}
