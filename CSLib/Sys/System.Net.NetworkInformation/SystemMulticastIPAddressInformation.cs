using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation
{
	internal class SystemMulticastIPAddressInformation : MulticastIPAddressInformation
	{
		private IpAdapterAddress adapterAddress;

		private SystemIPAddressInformation innerInfo;

		public override IPAddress Address => innerInfo.Address;

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
				return PrefixOrigin.Other;
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
				return SuffixOrigin.Other;
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
				return DuplicateAddressDetectionState.Invalid;
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
				return 0L;
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
				return 0L;
			}
		}

		public override long DhcpLeaseLifetime => 0L;

		private SystemMulticastIPAddressInformation()
		{
		}

		internal SystemMulticastIPAddressInformation(IpAdapterAddress adapterAddress, IPAddress ipAddress)
		{
			innerInfo = new SystemIPAddressInformation(adapterAddress, ipAddress);
			this.adapterAddress = adapterAddress;
		}

		internal static MulticastIPAddressInformationCollection ToAddressInformationCollection(IntPtr ptr)
		{
			MulticastIPAddressInformationCollection multicastIPAddressInformationCollection = new MulticastIPAddressInformationCollection();
			if (ptr == IntPtr.Zero)
			{
				return multicastIPAddressInformationCollection;
			}
			IpAdapterAddress ipAdapterAddress = (IpAdapterAddress)Marshal.PtrToStructure(ptr, typeof(IpAdapterAddress));
			AddressFamily addressFamily = ((ipAdapterAddress.address.addressLength > 16) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
			SocketAddress socketAddress = new SocketAddress(addressFamily, ipAdapterAddress.address.addressLength);
			Marshal.Copy(ipAdapterAddress.address.address, socketAddress.m_Buffer, 0, ipAdapterAddress.address.addressLength);
			multicastIPAddressInformationCollection.InternalAdd(new SystemMulticastIPAddressInformation(ipAddress: ((addressFamily != AddressFamily.InterNetwork) ? ((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)) : ((IPEndPoint)IPEndPoint.Any.Create(socketAddress))).Address, adapterAddress: ipAdapterAddress));
			while (ipAdapterAddress.next != IntPtr.Zero)
			{
				ipAdapterAddress = (IpAdapterAddress)Marshal.PtrToStructure(ipAdapterAddress.next, typeof(IpAdapterAddress));
				addressFamily = ((ipAdapterAddress.address.addressLength > 16) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
				socketAddress = new SocketAddress(addressFamily, ipAdapterAddress.address.addressLength);
				Marshal.Copy(ipAdapterAddress.address.address, socketAddress.m_Buffer, 0, ipAdapterAddress.address.addressLength);
				multicastIPAddressInformationCollection.InternalAdd(new SystemMulticastIPAddressInformation(ipAddress: ((addressFamily != AddressFamily.InterNetwork) ? ((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)) : ((IPEndPoint)IPEndPoint.Any.Create(socketAddress))).Address, adapterAddress: ipAdapterAddress));
			}
			return multicastIPAddressInformationCollection;
		}
	}
}
