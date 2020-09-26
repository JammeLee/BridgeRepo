namespace System.Security.Cryptography.X509Certificates
{
	[Flags]
	public enum OpenFlags
	{
		ReadOnly = 0x0,
		ReadWrite = 0x1,
		MaxAllowed = 0x2,
		OpenExistingOnly = 0x4,
		IncludeArchived = 0x8
	}
}
