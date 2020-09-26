namespace System.Net.NetworkInformation
{
	[Flags]
	internal enum AdapterAddressFlags
	{
		DnsEligible = 0x1,
		Transient = 0x2
	}
}
