using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum KeyContainerPermissionFlags
	{
		NoFlags = 0x0,
		Create = 0x1,
		Open = 0x2,
		Delete = 0x4,
		Import = 0x10,
		Export = 0x20,
		Sign = 0x100,
		Decrypt = 0x200,
		ViewAcl = 0x1000,
		ChangeAcl = 0x2000,
		AllFlags = 0x3337
	}
}
