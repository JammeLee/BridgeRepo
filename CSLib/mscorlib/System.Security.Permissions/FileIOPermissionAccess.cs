using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum FileIOPermissionAccess
	{
		NoAccess = 0x0,
		Read = 0x1,
		Write = 0x2,
		Append = 0x4,
		PathDiscovery = 0x8,
		AllAccess = 0xF
	}
}
