namespace System.Security.AccessControl
{
	[Flags]
	public enum AceFlags : byte
	{
		None = 0x0,
		ObjectInherit = 0x1,
		ContainerInherit = 0x2,
		NoPropagateInherit = 0x4,
		InheritOnly = 0x8,
		Inherited = 0x10,
		SuccessfulAccess = 0x40,
		FailedAccess = 0x80,
		InheritanceFlags = 0xF,
		AuditFlags = 0xC0
	}
}
