using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("285a8861-c84a-11d7-850f-005cd062464f")]
	internal interface ISectionEntry
	{
		object GetField(uint fieldId);

		string GetFieldName(uint fieldId);
	}
}
