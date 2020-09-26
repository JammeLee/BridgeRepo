using System.Collections;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation
{
	internal struct IpAddrString
	{
		internal IntPtr Next;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		internal string IpAddress;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		internal string IpMask;

		internal uint Context;

		internal IPAddressCollection ToIPAddressCollection()
		{
			IpAddrString ipAddrString = this;
			IPAddressCollection iPAddressCollection = new IPAddressCollection();
			if (ipAddrString.IpAddress.Length != 0)
			{
				iPAddressCollection.InternalAdd(IPAddress.Parse(ipAddrString.IpAddress));
			}
			while (ipAddrString.Next != IntPtr.Zero)
			{
				ipAddrString = (IpAddrString)Marshal.PtrToStructure(ipAddrString.Next, typeof(IpAddrString));
				if (ipAddrString.IpAddress.Length != 0)
				{
					iPAddressCollection.InternalAdd(IPAddress.Parse(ipAddrString.IpAddress));
				}
			}
			return iPAddressCollection;
		}

		internal ArrayList ToIPExtendedAddressArrayList()
		{
			IpAddrString ipAddrString = this;
			ArrayList arrayList = new ArrayList();
			if (ipAddrString.IpAddress.Length != 0)
			{
				arrayList.Add(new IPExtendedAddress(IPAddress.Parse(ipAddrString.IpAddress), IPAddress.Parse(ipAddrString.IpMask)));
			}
			while (ipAddrString.Next != IntPtr.Zero)
			{
				ipAddrString = (IpAddrString)Marshal.PtrToStructure(ipAddrString.Next, typeof(IpAddrString));
				if (ipAddrString.IpAddress.Length != 0)
				{
					arrayList.Add(new IPExtendedAddress(IPAddress.Parse(ipAddrString.IpAddress), IPAddress.Parse(ipAddrString.IpMask)));
				}
			}
			return arrayList;
		}

		internal GatewayIPAddressInformationCollection ToIPGatewayAddressCollection()
		{
			IpAddrString ipAddrString = this;
			GatewayIPAddressInformationCollection gatewayIPAddressInformationCollection = new GatewayIPAddressInformationCollection();
			if (ipAddrString.IpAddress.Length != 0)
			{
				gatewayIPAddressInformationCollection.InternalAdd(new SystemGatewayIPAddressInformation(IPAddress.Parse(ipAddrString.IpAddress)));
			}
			while (ipAddrString.Next != IntPtr.Zero)
			{
				ipAddrString = (IpAddrString)Marshal.PtrToStructure(ipAddrString.Next, typeof(IpAddrString));
				if (ipAddrString.IpAddress.Length != 0)
				{
					gatewayIPAddressInformationCollection.InternalAdd(new SystemGatewayIPAddressInformation(IPAddress.Parse(ipAddrString.IpAddress)));
				}
			}
			return gatewayIPAddressInformationCollection;
		}
	}
}
