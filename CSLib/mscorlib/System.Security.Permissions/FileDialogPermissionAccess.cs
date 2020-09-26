using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum FileDialogPermissionAccess
	{
		None = 0x0,
		Open = 0x1,
		Save = 0x2,
		OpenSave = 0x3
	}
}
