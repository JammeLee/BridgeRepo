namespace System.Net
{
	[Flags]
	internal enum NameInfoFlags
	{
		NI_NOFQDN = 0x1,
		NI_NUMERICHOST = 0x2,
		NI_NAMEREQD = 0x4,
		NI_NUMERICSERV = 0x8,
		NI_DGRAM = 0x10
	}
}
