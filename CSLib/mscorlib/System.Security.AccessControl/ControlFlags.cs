namespace System.Security.AccessControl
{
	[Flags]
	public enum ControlFlags
	{
		None = 0x0,
		OwnerDefaulted = 0x1,
		GroupDefaulted = 0x2,
		DiscretionaryAclPresent = 0x4,
		DiscretionaryAclDefaulted = 0x8,
		SystemAclPresent = 0x10,
		SystemAclDefaulted = 0x20,
		DiscretionaryAclUntrusted = 0x40,
		ServerSecurity = 0x80,
		DiscretionaryAclAutoInheritRequired = 0x100,
		SystemAclAutoInheritRequired = 0x200,
		DiscretionaryAclAutoInherited = 0x400,
		SystemAclAutoInherited = 0x800,
		DiscretionaryAclProtected = 0x1000,
		SystemAclProtected = 0x2000,
		RMControlValid = 0x4000,
		SelfRelative = 0x8000
	}
}
