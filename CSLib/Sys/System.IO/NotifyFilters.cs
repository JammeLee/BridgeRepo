namespace System.IO
{
	[Flags]
	public enum NotifyFilters
	{
		FileName = 0x1,
		DirectoryName = 0x2,
		Attributes = 0x4,
		Size = 0x8,
		LastWrite = 0x10,
		LastAccess = 0x20,
		CreationTime = 0x40,
		Security = 0x100
	}
}
