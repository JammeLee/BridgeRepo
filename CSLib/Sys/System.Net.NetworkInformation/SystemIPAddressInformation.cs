using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation
{
	internal class SystemIPAddressInformation : IPAddressInformation
	{
		private IPAddress address;

		internal bool transient;

		internal bool dnsEligible = true;

		public override IPAddress Address => address;

		public override bool IsTransient => transient;

		public override bool IsDnsEligible => dnsEligible;

		internal SystemIPAddressInformation(IPAddress address)
		{
			this.address = address;
			if (address.AddressFamily == AddressFamily.InterNetwork)
			{
				dnsEligible = (address.m_Address & 0xFEA9) <= 0;
			}
		}

		internal SystemIPAddressInformation(IpAdapterUnicastAddress adapterAddress, IPAddress address)
		{
			this.address = address;
			transient = (adapterAddress.flags & AdapterAddressFlags.Transient) > (AdapterAddressFlags)0;
			dnsEligible = (adapterAddress.flags & AdapterAddressFlags.DnsEligible) > (AdapterAddressFlags)0;
		}

		internal SystemIPAddressInformation(IpAdapterAddress adapterAddress, IPAddress address)
		{
			this.address = address;
			transient = (adapterAddress.flags & AdapterAddressFlags.Transient) > (AdapterAddressFlags)0;
			dnsEligible = (adapterAddress.flags & AdapterAddressFlags.DnsEligible) > (AdapterAddressFlags)0;
		}

		internal static IPAddressCollection ToAddressCollection(IntPtr ptr, IPVersion versionSupported)
		{
			IPAddressCollection iPAddressCollection = new IPAddressCollection();
			if (ptr == IntPtr.Zero)
			{
				return iPAddressCollection;
			}
			IpAdapterAddress ipAdapterAddress = (IpAdapterAddress)Marshal.PtrToStructure(ptr, typeof(IpAdapterAddress));
			AddressFamily addressFamily = ((ipAdapterAddress.address.addressLength > 16) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
			SocketAddress socketAddress = new SocketAddress(addressFamily, ipAdapterAddress.address.addressLength);
			Marshal.Copy(ipAdapterAddress.address.address, socketAddress.m_Buffer, 0, ipAdapterAddress.address.addressLength);
			IPEndPoint iPEndPoint = ((addressFamily != AddressFamily.InterNetwork) ? ((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)) : ((IPEndPoint)IPEndPoint.Any.Create(socketAddress)));
			iPAddressCollection.InternalAdd(iPEndPoint.Address);
			while (ipAdapterAddress.next != IntPtr.Zero)
			{
				ipAdapterAddress = (IpAdapterAddress)Marshal.PtrToStructure(ipAdapterAddress.next, typeof(IpAdapterAddress));
				addressFamily = ((ipAdapterAddress.address.addressLength > 16) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
				if ((addressFamily == AddressFamily.InterNetwork && (versionSupported & IPVersion.IPv4) > IPVersion.None) || (addressFamily == AddressFamily.InterNetworkV6 && (versionSupported & IPVersion.IPv6) > IPVersion.None))
				{
					socketAddress = new SocketAddress(addressFamily, ipAdapterAddress.address.addressLength);
					Marshal.Copy(ipAdapterAddress.address.address, socketAddress.m_Buffer, 0, ipAdapterAddress.address.addressLength);
					iPEndPoint = ((addressFamily != AddressFamily.InterNetwork) ? ((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)) : ((IPEndPoint)IPEndPoint.Any.Create(socketAddress)));
					iPAddressCollection.InternalAdd(iPEndPoint.Address);
				}
			}
			return iPAddressCollection;
		}

		internal static IPAddressInformationCollection ToAddressInformationCollection(IntPtr ptr, IPVersion versionSupported)
		{
			IPAddressInformationCollection iPAddressInformationCollection = new IPAddressInformationCollection();
			if (ptr == IntPtr.Zero)
			{
				return iPAddressInformationCollection;
			}
			IpAdapterAddress adapterAddress = (IpAdapterAddress)Marshal.PtrToStructure(ptr, typeof(IpAdapterAddress));
			AddressFamily addressFamily = ((adapterAddress.address.addressLength > 16) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
			SocketAddress socketAddress = new SocketAddress(addressFamily, adapterAddress.address.addressLength);
			Marshal.Copy(adapterAddress.address.address, socketAddress.m_Buffer, 0, adapterAddress.address.addressLength);
			iPAddressInformationCollection.InternalAdd(new SystemIPAddressInformation(address: ((addressFamily != AddressFamily.InterNetwork) ? ((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)) : ((IPEndPoint)IPEndPoint.Any.Create(socketAddress))).Address, adapterAddress: adapterAddress));
			while (adapterAddress.next != IntPtr.Zero)
			{
				adapterAddress = (IpAdapterAddress)Marshal.PtrToStructure(adapterAddress.next, typeof(IpAdapterAddress));
				addressFamily = ((adapterAddress.address.addressLength > 16) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
				if ((addressFamily == AddressFamily.InterNetwork && (versionSupported & IPVersion.IPv4) > IPVersion.None) || (addressFamily == AddressFamily.InterNetworkV6 && (versionSupported & IPVersion.IPv6) > IPVersion.None))
				{
					socketAddress = new SocketAddress(addressFamily, adapterAddress.address.addressLength);
					Marshal.Copy(adapterAddress.address.address, socketAddress.m_Buffer, 0, adapterAddress.address.addressLength);
					iPAddressInformationCollection.InternalAdd(new SystemIPAddressInformation(address: ((addressFamily != AddressFamily.InterNetwork) ? ((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)) : ((IPEndPoint)IPEndPoint.Any.Create(socketAddress))).Address, adapterAddress: adapterAddress));
				}
			}
			return iPAddressInformationCollection;
		}
	}
}
