namespace System.Net.NetworkInformation
{
	[Flags]
	internal enum StartIPOptions
	{
		Both = 0x3,
		None = 0x0,
		StartIPv4 = 0x1,
		StartIPv6 = 0x2
	}
}
