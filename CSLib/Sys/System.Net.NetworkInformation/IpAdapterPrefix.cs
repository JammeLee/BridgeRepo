namespace System.Net.NetworkInformation
{
	internal struct IpAdapterPrefix
	{
		internal uint length;

		internal uint ifIndex;

		internal IntPtr next;

		internal IpSocketAddress address;

		internal uint prefixLength;
	}
}
