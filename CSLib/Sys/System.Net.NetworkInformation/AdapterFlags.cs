namespace System.Net.NetworkInformation
{
	[Flags]
	internal enum AdapterFlags
	{
		DnsEnabled = 0x1,
		RegisterAdapterSuffix = 0x2,
		DhcpEnabled = 0x4,
		ReceiveOnly = 0x8,
		NoMulticast = 0x10,
		Ipv6OtherStatefulConfig = 0x20
	}
}
