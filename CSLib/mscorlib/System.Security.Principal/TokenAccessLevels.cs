using System.Runtime.InteropServices;

namespace System.Security.Principal
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum TokenAccessLevels
	{
		AssignPrimary = 0x1,
		Duplicate = 0x2,
		Impersonate = 0x4,
		Query = 0x8,
		QuerySource = 0x10,
		AdjustPrivileges = 0x20,
		AdjustGroups = 0x40,
		AdjustDefault = 0x80,
		AdjustSessionId = 0x100,
		Read = 0x20008,
		Write = 0x200E0,
		AllAccess = 0xF01FF,
		MaximumAllowed = 0x2000000
	}
}
