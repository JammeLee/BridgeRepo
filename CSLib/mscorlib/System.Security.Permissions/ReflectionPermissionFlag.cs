using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum ReflectionPermissionFlag
	{
		NoFlags = 0x0,
		[Obsolete("This API has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
		TypeInformation = 0x1,
		MemberAccess = 0x2,
		ReflectionEmit = 0x4,
		[ComVisible(false)]
		RestrictedMemberAccess = 0x8,
		AllFlags = 0x7
	}
}
