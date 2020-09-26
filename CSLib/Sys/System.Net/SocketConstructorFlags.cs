namespace System.Net
{
	[Flags]
	internal enum SocketConstructorFlags
	{
		WSA_FLAG_OVERLAPPED = 0x1,
		WSA_FLAG_MULTIPOINT_C_ROOT = 0x2,
		WSA_FLAG_MULTIPOINT_C_LEAF = 0x4,
		WSA_FLAG_MULTIPOINT_D_ROOT = 0x8,
		WSA_FLAG_MULTIPOINT_D_LEAF = 0x10
	}
}
