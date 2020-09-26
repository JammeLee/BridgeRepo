namespace System.Net.NetworkInformation
{
	internal struct IPExtendedAddress
	{
		internal IPAddress mask;

		internal IPAddress address;

		internal IPExtendedAddress(IPAddress address, IPAddress mask)
		{
			this.address = address;
			this.mask = mask;
		}
	}
}
