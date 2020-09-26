using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum RegistryPermissionAccess
	{
		NoAccess = 0x0,
		Read = 0x1,
		Write = 0x2,
		Create = 0x4,
		AllAccess = 0x7
	}
}
