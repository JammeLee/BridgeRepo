namespace System.Net.NetworkInformation
{
	internal struct IpAdapterAddress
	{
		internal uint length;

		internal AdapterAddressFlags flags;

		internal IntPtr next;

		internal IpSocketAddress address;
	}
}
