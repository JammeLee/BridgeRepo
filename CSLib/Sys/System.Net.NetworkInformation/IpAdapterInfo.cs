using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation
{
	internal struct IpAdapterInfo
	{
		internal const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;

		internal const int MAX_ADAPTER_NAME_LENGTH = 256;

		internal const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

		internal IntPtr Next;

		internal uint comboIndex;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		internal string adapterName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 132)]
		internal string description;

		internal uint addressLength;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		internal byte[] address;

		internal uint index;

		internal OldInterfaceType type;

		internal bool dhcpEnabled;

		internal IntPtr currentIpAddress;

		internal IpAddrString ipAddressList;

		internal IpAddrString gatewayList;

		internal IpAddrString dhcpServer;

		[MarshalAs(UnmanagedType.Bool)]
		internal bool haveWins;

		internal IpAddrString primaryWinsServer;

		internal IpAddrString secondaryWinsServer;

		internal uint leaseObtained;

		internal uint leaseExpires;
	}
}
