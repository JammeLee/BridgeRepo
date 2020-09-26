using System.Collections;
using System.Net.Sockets;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Net.NetworkInformation
{
	internal class SystemIPInterfaceProperties : IPInterfaceProperties
	{
		private uint mtu;

		internal uint index;

		internal uint ipv6Index;

		internal IPVersion versionSupported;

		private bool dnsEnabled;

		private bool dynamicDnsEnabled;

		private IPAddressCollection dnsAddresses;

		private UnicastIPAddressInformationCollection unicastAddresses;

		private MulticastIPAddressInformationCollection multicastAddresses;

		private IPAddressInformationCollection anycastAddresses;

		private AdapterFlags adapterFlags;

		private string dnsSuffix;

		private string name;

		private SystemIPv4InterfaceProperties ipv4Properties;

		private SystemIPv6InterfaceProperties ipv6Properties;

		public override bool IsDnsEnabled => dnsEnabled;

		public override bool IsDynamicDnsEnabled => dynamicDnsEnabled;

		public override string DnsSuffix
		{
			get
			{
				if (!ComNetOS.IsWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
				}
				return dnsSuffix;
			}
		}

		public override IPAddressInformationCollection AnycastAddresses => anycastAddresses;

		public override UnicastIPAddressInformationCollection UnicastAddresses => unicastAddresses;

		public override MulticastIPAddressInformationCollection MulticastAddresses => multicastAddresses;

		public override IPAddressCollection DnsAddresses => dnsAddresses;

		public override GatewayIPAddressInformationCollection GatewayAddresses
		{
			get
			{
				if (ipv4Properties != null)
				{
					return ipv4Properties.GetGatewayAddresses();
				}
				return new GatewayIPAddressInformationCollection();
			}
		}

		public override IPAddressCollection DhcpServerAddresses
		{
			get
			{
				if (ipv4Properties != null)
				{
					return ipv4Properties.GetDhcpServerAddresses();
				}
				return new IPAddressCollection();
			}
		}

		public override IPAddressCollection WinsServersAddresses
		{
			get
			{
				if (ipv4Properties != null)
				{
					return ipv4Properties.GetWinsServersAddresses();
				}
				return new IPAddressCollection();
			}
		}

		private SystemIPInterfaceProperties()
		{
		}

		internal SystemIPInterfaceProperties(FixedInfo fixedInfo, IpAdapterAddresses ipAdapterAddresses)
		{
			dnsEnabled = fixedInfo.EnableDns;
			index = ipAdapterAddresses.index;
			name = ipAdapterAddresses.AdapterName;
			ipv6Index = ipAdapterAddresses.ipv6Index;
			if (index != 0)
			{
				versionSupported |= IPVersion.IPv4;
			}
			if (ipv6Index != 0)
			{
				versionSupported |= IPVersion.IPv6;
			}
			mtu = ipAdapterAddresses.mtu;
			adapterFlags = ipAdapterAddresses.flags;
			dnsSuffix = ipAdapterAddresses.dnsSuffix;
			dynamicDnsEnabled = (ipAdapterAddresses.flags & AdapterFlags.DnsEnabled) > (AdapterFlags)0;
			multicastAddresses = SystemMulticastIPAddressInformation.ToAddressInformationCollection(ipAdapterAddresses.FirstMulticastAddress);
			dnsAddresses = SystemIPAddressInformation.ToAddressCollection(ipAdapterAddresses.FirstDnsServerAddress, versionSupported);
			anycastAddresses = SystemIPAddressInformation.ToAddressInformationCollection(ipAdapterAddresses.FirstAnycastAddress, versionSupported);
			unicastAddresses = SystemUnicastIPAddressInformation.ToAddressInformationCollection(ipAdapterAddresses.FirstUnicastAddress);
			if (ipv6Index != 0)
			{
				ipv6Properties = new SystemIPv6InterfaceProperties(ipv6Index, mtu);
			}
		}

		internal SystemIPInterfaceProperties(FixedInfo fixedInfo, IpAdapterInfo ipAdapterInfo)
		{
			dnsEnabled = fixedInfo.EnableDns;
			name = ipAdapterInfo.adapterName;
			index = ipAdapterInfo.index;
			multicastAddresses = new MulticastIPAddressInformationCollection();
			anycastAddresses = new IPAddressInformationCollection();
			if (index != 0)
			{
				versionSupported |= IPVersion.IPv4;
			}
			if (ComNetOS.IsWin2K)
			{
				ReadRegDnsSuffix();
			}
			unicastAddresses = new UnicastIPAddressInformationCollection();
			ArrayList arrayList = ipAdapterInfo.ipAddressList.ToIPExtendedAddressArrayList();
			foreach (IPExtendedAddress item in arrayList)
			{
				unicastAddresses.InternalAdd(new SystemUnicastIPAddressInformation(ipAdapterInfo, item));
			}
			try
			{
				ipv4Properties = new SystemIPv4InterfaceProperties(fixedInfo, ipAdapterInfo);
				if (dnsAddresses == null || dnsAddresses.Count == 0)
				{
					dnsAddresses = ipv4Properties.DnsAddresses;
				}
			}
			catch (NetworkInformationException ex)
			{
				if ((long)ex.ErrorCode != 87)
				{
					throw;
				}
			}
		}

		public override IPv4InterfaceProperties GetIPv4Properties()
		{
			if (index == 0)
			{
				throw new NetworkInformationException(SocketError.ProtocolNotSupported);
			}
			return ipv4Properties;
		}

		public override IPv6InterfaceProperties GetIPv6Properties()
		{
			if (ipv6Index == 0)
			{
				throw new NetworkInformationException(SocketError.ProtocolNotSupported);
			}
			return ipv6Properties;
		}

		internal bool Update(FixedInfo fixedInfo, IpAdapterInfo ipAdapterInfo)
		{
			try
			{
				ArrayList arrayList = ipAdapterInfo.ipAddressList.ToIPExtendedAddressArrayList();
				foreach (IPExtendedAddress item in arrayList)
				{
					foreach (SystemUnicastIPAddressInformation unicastAddress in unicastAddresses)
					{
						if (item.address.Equals(unicastAddress.Address))
						{
							unicastAddress.ipv4Mask = item.mask;
						}
					}
				}
				ipv4Properties = new SystemIPv4InterfaceProperties(fixedInfo, ipAdapterInfo);
				if (dnsAddresses == null || dnsAddresses.Count == 0)
				{
					dnsAddresses = ipv4Properties.DnsAddresses;
				}
			}
			catch (NetworkInformationException ex)
			{
				if ((long)ex.ErrorCode == 87 || (long)ex.ErrorCode == 13 || (long)ex.ErrorCode == 232 || (long)ex.ErrorCode == 1 || (long)ex.ErrorCode == 2)
				{
					return false;
				}
				throw;
			}
			return true;
		}

		[RegistryPermission(SecurityAction.Assert, Read = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces")]
		private void ReadRegDnsSuffix()
		{
			RegistryKey registryKey = null;
			try
			{
				string text = "SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + name;
				registryKey = Registry.LocalMachine.OpenSubKey(text);
				if (registryKey == null)
				{
					return;
				}
				dnsSuffix = (string)registryKey.GetValue("DhcpDomain");
				if (dnsSuffix == null)
				{
					dnsSuffix = (string)registryKey.GetValue("Domain");
					if (dnsSuffix == null)
					{
						dnsSuffix = string.Empty;
					}
				}
			}
			finally
			{
				registryKey?.Close();
			}
		}
	}
}
