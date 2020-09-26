using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	[ComImport]
	[Guid("285a8860-c84a-11d7-850f-005cd062464f")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ICDF
	{
		object _NewEnum
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}

		uint Count
		{
			get;
		}

		ISection GetRootSection(uint SectionId);

		ISectionEntry GetRootSectionEntry(uint SectionId);

		object GetItem(uint SectionId);
	}
}
