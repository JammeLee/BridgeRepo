using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum EnvironmentPermissionAccess
	{
		NoAccess = 0x0,
		Read = 0x1,
		Write = 0x2,
		AllAccess = 0x3
	}
}
