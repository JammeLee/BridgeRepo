using System.Collections;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation
{
	internal class SystemIPGlobalProperties : IPGlobalProperties
	{
		private FixedInfo fixedInfo;

		private bool fixedInfoInitialized;

		private static string hostName = null;

		private static string domainName = null;

		private static object syncObject = new object();

		internal FixedInfo FixedInfo
		{
			get
			{
				if (!fixedInfoInitialized)
				{
					lock (this)
					{
						if (!fixedInfoInitialized)
						{
							fixedInfo = GetFixedInfo();
							fixedInfoInitialized = true;
						}
					}
				}
				return fixedInfo;
			}
		}

		public override string HostName
		{
			get
			{
				if (hostName == null)
				{
					lock (syncObject)
					{
						if (hostName == null)
						{
							hostName = FixedInfo.HostName;
							domainName = FixedInfo.DomainName;
						}
					}
				}
				return hostName;
			}
		}

		public override string DomainName
		{
			get
			{
				if (domainName == null)
				{
					lock (syncObject)
					{
						if (domainName == null)
						{
							hostName = FixedInfo.HostName;
							domainName = FixedInfo.DomainName;
						}
					}
				}
				return domainName;
			}
		}

		public override NetBiosNodeType NodeType => FixedInfo.NodeType;

		public override string DhcpScopeName => FixedInfo.ScopeId;

		public override bool IsWinsProxy => FixedInfo.EnableProxy;

		internal SystemIPGlobalProperties()
		{
		}

		internal static FixedInfo GetFixedInfo()
		{
			uint pOutBufLen = 0u;
			SafeLocalFree safeLocalFree = null;
			FixedInfo result = default(FixedInfo);
			uint networkParams = UnsafeNetInfoNativeMethods.GetNetworkParams(SafeLocalFree.Zero, ref pOutBufLen);
			while (true)
			{
				switch (networkParams)
				{
				case 111u:
					try
					{
						safeLocalFree = SafeLocalFree.LocalAlloc((int)pOutBufLen);
						networkParams = UnsafeNetInfoNativeMethods.GetNetworkParams(safeLocalFree, ref pOutBufLen);
						if (networkParams == 0)
						{
							result = new FixedInfo((FIXED_INFO)Marshal.PtrToStructure(safeLocalFree.DangerousGetHandle(), typeof(FIXED_INFO)));
						}
					}
					finally
					{
						safeLocalFree?.Close();
					}
					break;
				default:
					throw new NetworkInformationException((int)networkParams);
				case 0u:
					return result;
				}
			}
		}

		public override TcpConnectionInformation[] GetActiveTcpConnections()
		{
			ArrayList arrayList = new ArrayList();
			TcpConnectionInformation[] allTcpConnections = GetAllTcpConnections();
			TcpConnectionInformation[] array = allTcpConnections;
			foreach (TcpConnectionInformation tcpConnectionInformation in array)
			{
				if (tcpConnectionInformation.State != TcpState.Listen)
				{
					arrayList.Add(tcpConnectionInformation);
				}
			}
			allTcpConnections = new TcpConnectionInformation[arrayList.Count];
			for (int j = 0; j < arrayList.Count; j++)
			{
				allTcpConnections[j] = (TcpConnectionInformation)arrayList[j];
			}
			return allTcpConnections;
		}

		public override IPEndPoint[] GetActiveTcpListeners()
		{
			ArrayList arrayList = new ArrayList();
			TcpConnectionInformation[] allTcpConnections = GetAllTcpConnections();
			TcpConnectionInformation[] array = allTcpConnections;
			foreach (TcpConnectionInformation tcpConnectionInformation in array)
			{
				if (tcpConnectionInformation.State == TcpState.Listen)
				{
					arrayList.Add(tcpConnectionInformation.LocalEndPoint);
				}
			}
			IPEndPoint[] array2 = new IPEndPoint[arrayList.Count];
			for (int j = 0; j < arrayList.Count; j++)
			{
				array2[j] = (IPEndPoint)arrayList[j];
			}
			return array2;
		}

		private TcpConnectionInformation[] GetAllTcpConnections()
		{
			uint dwOutBufLen = 0u;
			SafeLocalFree safeLocalFree = null;
			SystemTcpConnectionInformation[] array = null;
			uint tcpTable = UnsafeNetInfoNativeMethods.GetTcpTable(SafeLocalFree.Zero, ref dwOutBufLen, order: true);
			while (true)
			{
				switch (tcpTable)
				{
				case 122u:
					try
					{
						safeLocalFree = SafeLocalFree.LocalAlloc((int)dwOutBufLen);
						tcpTable = UnsafeNetInfoNativeMethods.GetTcpTable(safeLocalFree, ref dwOutBufLen, order: true);
						if (tcpTable != 0)
						{
							break;
						}
						IntPtr intPtr = safeLocalFree.DangerousGetHandle();
						MibTcpTable mibTcpTable = (MibTcpTable)Marshal.PtrToStructure(intPtr, typeof(MibTcpTable));
						if (mibTcpTable.numberOfEntries != 0)
						{
							array = new SystemTcpConnectionInformation[mibTcpTable.numberOfEntries];
							intPtr = (IntPtr)((long)intPtr + Marshal.SizeOf(mibTcpTable.numberOfEntries));
							for (int i = 0; i < mibTcpTable.numberOfEntries; i++)
							{
								MibTcpRow mibTcpRow = (MibTcpRow)Marshal.PtrToStructure(intPtr, typeof(MibTcpRow));
								array[i] = new SystemTcpConnectionInformation(mibTcpRow);
								intPtr = (IntPtr)((long)intPtr + Marshal.SizeOf(mibTcpRow));
							}
						}
					}
					finally
					{
						safeLocalFree?.Close();
					}
					break;
				default:
					throw new NetworkInformationException((int)tcpTable);
				case 0u:
				case 232u:
					if (array == null)
					{
						return new SystemTcpConnectionInformation[0];
					}
					return array;
				}
			}
		}

		public override IPEndPoint[] GetActiveUdpListeners()
		{
			uint dwOutBufLen = 0u;
			SafeLocalFree safeLocalFree = null;
			IPEndPoint[] array = null;
			uint udpTable = UnsafeNetInfoNativeMethods.GetUdpTable(SafeLocalFree.Zero, ref dwOutBufLen, order: true);
			while (true)
			{
				switch (udpTable)
				{
				case 122u:
					try
					{
						safeLocalFree = SafeLocalFree.LocalAlloc((int)dwOutBufLen);
						udpTable = UnsafeNetInfoNativeMethods.GetUdpTable(safeLocalFree, ref dwOutBufLen, order: true);
						if (udpTable != 0)
						{
							break;
						}
						IntPtr intPtr = safeLocalFree.DangerousGetHandle();
						MibUdpTable mibUdpTable = (MibUdpTable)Marshal.PtrToStructure(intPtr, typeof(MibUdpTable));
						if (mibUdpTable.numberOfEntries != 0)
						{
							array = new IPEndPoint[mibUdpTable.numberOfEntries];
							intPtr = (IntPtr)((long)intPtr + Marshal.SizeOf(mibUdpTable.numberOfEntries));
							for (int i = 0; i < mibUdpTable.numberOfEntries; i++)
							{
								MibUdpRow mibUdpRow = (MibUdpRow)Marshal.PtrToStructure(intPtr, typeof(MibUdpRow));
								int port = (mibUdpRow.localPort3 << 24) | (mibUdpRow.localPort4 << 16) | (mibUdpRow.localPort1 << 8) | mibUdpRow.localPort2;
								array[i] = new IPEndPoint(mibUdpRow.localAddr, port);
								intPtr = (IntPtr)((long)intPtr + Marshal.SizeOf(mibUdpRow));
							}
						}
					}
					finally
					{
						safeLocalFree?.Close();
					}
					break;
				default:
					throw new NetworkInformationException((int)udpTable);
				case 0u:
				case 232u:
					if (array == null)
					{
						return new IPEndPoint[0];
					}
					return array;
				}
			}
		}

		public override IPGlobalStatistics GetIPv4GlobalStatistics()
		{
			return new SystemIPGlobalStatistics(AddressFamily.InterNetwork);
		}

		public override IPGlobalStatistics GetIPv6GlobalStatistics()
		{
			return new SystemIPGlobalStatistics(AddressFamily.InterNetworkV6);
		}

		public override TcpStatistics GetTcpIPv4Statistics()
		{
			return new SystemTcpStatistics(AddressFamily.InterNetwork);
		}

		public override TcpStatistics GetTcpIPv6Statistics()
		{
			return new SystemTcpStatistics(AddressFamily.InterNetworkV6);
		}

		public override UdpStatistics GetUdpIPv4Statistics()
		{
			return new SystemUdpStatistics(AddressFamily.InterNetwork);
		}

		public override UdpStatistics GetUdpIPv6Statistics()
		{
			return new SystemUdpStatistics(AddressFamily.InterNetworkV6);
		}

		public override IcmpV4Statistics GetIcmpV4Statistics()
		{
			return new SystemIcmpV4Statistics();
		}

		public override IcmpV6Statistics GetIcmpV6Statistics()
		{
			return new SystemIcmpV6Statistics();
		}
	}
}
