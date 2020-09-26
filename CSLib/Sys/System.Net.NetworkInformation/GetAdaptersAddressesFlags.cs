namespace System.Net.NetworkInformation
{
	[Flags]
	internal enum GetAdaptersAddressesFlags
	{
		SkipUnicast = 0x1,
		SkipAnycast = 0x2,
		SkipMulticast = 0x4,
		SkipDnsServer = 0x8,
		IncludePrefix = 0x10,
		SkipFriendlyName = 0x20
	}
}
