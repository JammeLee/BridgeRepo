using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	[ComImport]
	[Guid("285a8871-c84a-11d7-850f-005cd062464f")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ISectionWithStringKey
	{
		bool IsCaseInsensitive
		{
			get;
		}

		void Lookup([MarshalAs(UnmanagedType.LPWStr)] string wzStringKey, [MarshalAs(UnmanagedType.Interface)] out object ppUnknown);
	}
}
