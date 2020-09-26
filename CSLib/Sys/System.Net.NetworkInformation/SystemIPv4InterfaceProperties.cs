using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation
{
	internal class SystemIPv4InterfaceProperties : IPv4InterfaceProperties
	{
		private bool haveWins;

		private bool dhcpEnabled;

		private bool routingEnabled;

		private bool autoConfigEnabled;

		private bool autoConfigActive;

		private uint index;

		private uint mtu;

		private GatewayIPAddressInformationCollection gatewayAddresses;

		private IPAddressCollection dhcpAddresses;

		private IPAddressCollection winsServerAddresses;

		internal IPAddressCollection dnsAddresses;

		internal IPAddressCollection DnsAddresses => dnsAddresses;

		public override bool UsesWins => haveWins;

		public override bool IsDhcpEnabled => dhcpEnabled;

		public override bool IsForwardingEnabled => routingEnabled;

		public override bool IsAutomaticPrivateAddressingEnabled => autoConfigEnabled;

		public override bool IsAutomaticPrivateAddressingActive => autoConfigActive;

		public override int Mtu => (int)mtu;

		public override int Index => (int)index;

		internal SystemIPv4InterfaceProperties(FixedInfo fixedInfo, IpAdapterInfo ipAdapterInfo)
		{
			index = ipAdapterInfo.index;
			routingEnabled = fixedInfo.EnableRouting;
			dhcpEnabled = ipAdapterInfo.dhcpEnabled;
			haveWins = ipAdapterInfo.haveWins;
			gatewayAddresses = ipAdapterInfo.gatewayList.ToIPGatewayAddressCollection();
			dhcpAddresses = ipAdapterInfo.dhcpServer.ToIPAddressCollection();
			IPAddressCollection iPAddressCollection = ipAdapterInfo.primaryWinsServer.ToIPAddressCollection();
			IPAddressCollection iPAddressCollection2 = ipAdapterInfo.secondaryWinsServer.ToIPAddressCollection();
			winsServerAddresses = new IPAddressCollection();
			foreach (IPAddress item in iPAddressCollection)
			{
				winsServerAddresses.InternalAdd(item);
			}
			foreach (IPAddress item2 in iPAddressCollection2)
			{
				winsServerAddresses.InternalAdd(item2);
			}
			SystemIPv4InterfaceStatistics systemIPv4InterfaceStatistics = new SystemIPv4InterfaceStatistics(index);
			mtu = (uint)systemIPv4InterfaceStatistics.Mtu;
			if (ComNetOS.IsWin2K)
			{
				GetPerAdapterInfo(ipAdapterInfo.index);
			}
			else
			{
				dnsAddresses = fixedInfo.DnsAddresses;
			}
		}

		internal GatewayIPAddressInformationCollection GetGatewayAddresses()
		{
			return gatewayAddresses;
		}

		internal IPAddressCollection GetDhcpServerAddresses()
		{
			return dhcpAddresses;
		}

		internal IPAddressCollection GetWinsServersAddresses()
		{
			return winsServerAddresses;
		}

		private void GetPerAdapterInfo(uint index)
		{
			if (index == 0)
			{
				return;
			}
			uint pOutBufLen = 0u;
			SafeLocalFree safeLocalFree = null;
			uint perAdapterInfo = UnsafeNetInfoNativeMethods.GetPerAdapterInfo(index, SafeLocalFree.Zero, ref pOutBufLen);
			while (perAdapterInfo == 111)
			{
				try
				{
					safeLocalFree = SafeLocalFree.LocalAlloc((int)pOutBufLen);
					perAdapterInfo = UnsafeNetInfoNativeMethods.GetPerAdapterInfo(index, safeLocalFree, ref pOutBufLen);
					if (perAdapterInfo == 0)
					{
						IpPerAdapterInfo ipPerAdapterInfo = (IpPerAdapterInfo)Marshal.PtrToStructure(safeLocalFree.DangerousGetHandle(), typeof(IpPerAdapterInfo));
						autoConfigEnabled = ipPerAdapterInfo.autoconfigEnabled;
						autoConfigActive = ipPerAdapterInfo.autoconfigActive;
						dnsAddresses = ipPerAdapterInfo.dnsServerList.ToIPAddressCollection();
					}
				}
				finally
				{
					if (dnsAddresses == null)
					{
						dnsAddresses = new IPAddressCollection();
					}
					safeLocalFree?.Close();
				}
			}
			if (dnsAddresses == null)
			{
				dnsAddresses = new IPAddressCollection();
			}
			if (perAdapterInfo != 0)
			{
				throw new NetworkInformationException((int)perAdapterInfo);
			}
		}
	}
}
