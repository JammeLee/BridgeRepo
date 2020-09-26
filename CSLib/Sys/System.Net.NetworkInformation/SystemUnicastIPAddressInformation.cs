using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation
{
	internal class SystemUnicastIPAddressInformation : UnicastIPAddressInformation
	{
		private IpAdapterUnicastAddress adapterAddress;

		private long dhcpLeaseLifetime;

		private SystemIPAddressInformation innerInfo;

		internal IPAddress ipv4Mask;

		public override IPAddress Address => innerInfo.Address;

		public override IPAddress IPv4Mask
		{
			get
			{
				if (Address.AddressFamily != AddressFamily.InterNetwork)
				{
					return new IPAddress(0);
				}
				return ipv4Mask;
			}
		}

		public override bool IsTransient => innerInfo.IsTransient;

		public override bool IsDnsEligible => innerInfo.IsDnsEligible;

		public override PrefixOrigin PrefixOrigin
		{
			get
			{
				if (!ComNetOS.IsPostWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
				}
				return adapterAddress.prefixOrigin;
			}
		}

		public override SuffixOrigin SuffixOrigin
		{
			get
			{
				if (!ComNetOS.IsPostWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
				}
				return adapterAddress.suffixOrigin;
			}
		}

		public override DuplicateAddressDetectionState DuplicateAddressDetectionState
		{
			get
			{
				if (!ComNetOS.IsPostWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
				}
				return adapterAddress.dadState;
			}
		}

		public override long AddressValidLifetime
		{
			get
			{
				if (!ComNetOS.IsPostWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
				}
				return adapterAddress.validLifetime;
			}
		}

		public override long AddressPreferredLifetime
		{
			get
			{
				if (!ComNetOS.IsPostWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
				}
				return adapterAddress.preferredLifetime;
			}
		}

		public override long DhcpLeaseLifetime => dhcpLeaseLifetime;

		private SystemUnicastIPAddressInformation()
		{
		}

		internal SystemUnicastIPAddressInformation(IpAdapterInfo ipAdapterInfo, IPExtendedAddress address)
		{
			innerInfo = new SystemIPAddressInformation(address.address);
			DateTime d = new DateTime(1970, 1, 1).AddSeconds(ipAdapterInfo.leaseExpires);
			dhcpLeaseLifetime = (long)(d - DateTime.UtcNow).TotalSeconds;
			ipv4Mask = address.mask;
		}

		internal SystemUnicastIPAddressInformation(IpAdapterUnicastAddress adapterAddress, IPAddress ipAddress)
		{
			innerInfo = new SystemIPAddressInformation(adapterAddress, ipAddress);
			this.adapterAddress = adapterAddress;
			dhcpLeaseLifetime = adapterAddress.leaseLifetime;
		}

		internal static UnicastIPAddressInformationCollection ToAddressInformationCollection(IntPtr ptr)
		{
			UnicastIPAddressInformationCollection unicastIPAddressInformationCollection = new UnicastIPAddressInformationCollection();
			if (ptr == IntPtr.Zero)
			{
				return unicastIPAddressInformationCollection;
			}
			IpAdapterUnicastAddress ipAdapterUnicastAddress = (IpAdapterUnicastAddress)Marshal.PtrToStructure(ptr, typeof(IpAdapterUnicastAddress));
			AddressFamily addressFamily = ((ipAdapterUnicastAddress.address.addressLength > 16) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
			SocketAddress socketAddress = new SocketAddress(addressFamily, ipAdapterUnicastAddress.address.addressLength);
			Marshal.Copy(ipAdapterUnicastAddress.address.address, socketAddress.m_Buffer, 0, ipAdapterUnicastAddress.address.addressLength);
			unicastIPAddressInformationCollection.InternalAdd(new SystemUnicastIPAddressInformation(ipAddress: ((addressFamily != AddressFamily.InterNetwork) ? ((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)) : ((IPEndPoint)IPEndPoint.Any.Create(socketAddress))).Address, adapterAddress: ipAdapterUnicastAddress));
			while (ipAdapterUnicastAddress.next != IntPtr.Zero)
			{
				ipAdapterUnicastAddress = (IpAdapterUnicastAddress)Marshal.PtrToStructure(ipAdapterUnicastAddress.next, typeof(IpAdapterUnicastAddress));
				addressFamily = ((ipAdapterUnicastAddress.address.addressLength > 16) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
				socketAddress = new SocketAddress(addressFamily, ipAdapterUnicastAddress.address.addressLength);
				Marshal.Copy(ipAdapterUnicastAddress.address.address, socketAddress.m_Buffer, 0, ipAdapterUnicastAddress.address.addressLength);
				unicastIPAddressInformationCollection.InternalAdd(new SystemUnicastIPAddressInformation(ipAddress: ((addressFamily != AddressFamily.InterNetwork) ? ((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)) : ((IPEndPoint)IPEndPoint.Any.Create(socketAddress))).Address, adapterAddress: ipAdapterUnicastAddress));
			}
			return unicastIPAddressInformationCollection;
		}
	}
}
