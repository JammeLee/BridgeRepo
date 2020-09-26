using System.Collections;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Net.NetworkInformation
{
	internal class SystemNetworkInterface : NetworkInterface
	{
		private string name;

		private string id;

		private string description;

		private byte[] physicalAddress;

		private uint addressLength;

		private NetworkInterfaceType type;

		private OperationalStatus operStatus;

		private long speed;

		internal uint index;

		internal uint ipv6Index;

		private AdapterFlags adapterFlags;

		private SystemIPInterfaceProperties interfaceProperties;

		internal static int InternalLoopbackInterfaceIndex
		{
			get
			{
				int result;
				int bestInterface = (int)UnsafeNetInfoNativeMethods.GetBestInterface(16777343, out result);
				if (bestInterface != 0)
				{
					throw new NetworkInformationException(bestInterface);
				}
				return result;
			}
		}

		public override string Id => id;

		public override string Name => name;

		public override string Description => description;

		public override NetworkInterfaceType NetworkInterfaceType => type;

		public override OperationalStatus OperationalStatus => operStatus;

		public override long Speed
		{
			get
			{
				if (speed == 0)
				{
					SystemIPv4InterfaceStatistics systemIPv4InterfaceStatistics = new SystemIPv4InterfaceStatistics(index);
					speed = systemIPv4InterfaceStatistics.Speed;
				}
				return speed;
			}
		}

		public override bool IsReceiveOnly
		{
			get
			{
				if (!ComNetOS.IsPostWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
				}
				return (adapterFlags & AdapterFlags.ReceiveOnly) > (AdapterFlags)0;
			}
		}

		public override bool SupportsMulticast
		{
			get
			{
				if (!ComNetOS.IsPostWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
				}
				return (adapterFlags & AdapterFlags.NoMulticast) == 0;
			}
		}

		private SystemNetworkInterface()
		{
		}

		internal static NetworkInterface[] GetNetworkInterfaces()
		{
			return GetNetworkInterfaces(AddressFamily.Unspecified);
		}

		internal static bool InternalGetIsNetworkAvailable()
		{
			if (ComNetOS.IsWinNt)
			{
				NetworkInterface[] networkInterfaces = GetNetworkInterfaces();
				NetworkInterface[] array = networkInterfaces;
				foreach (NetworkInterface networkInterface in array)
				{
					if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
					{
						return true;
					}
				}
				return false;
			}
			uint flags = 0u;
			return UnsafeWinINetNativeMethods.InternetGetConnectedState(ref flags, 0u);
		}

		private static NetworkInterface[] GetNetworkInterfaces(AddressFamily family)
		{
			IpHelperErrors.CheckFamilyUnspecified(family);
			if (ComNetOS.IsPostWin2K)
			{
				return PostWin2KGetNetworkInterfaces(family);
			}
			FixedInfo fixedInfo = SystemIPGlobalProperties.GetFixedInfo();
			if (family != 0 && family != AddressFamily.InterNetwork)
			{
				throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
			}
			SafeLocalFree safeLocalFree = null;
			uint pOutBufLen = 0u;
			ArrayList arrayList = new ArrayList();
			uint adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(SafeLocalFree.Zero, ref pOutBufLen);
			while (true)
			{
				switch (adaptersInfo)
				{
				case 111u:
					try
					{
						safeLocalFree = SafeLocalFree.LocalAlloc((int)pOutBufLen);
						adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(safeLocalFree, ref pOutBufLen);
						if (adaptersInfo == 0)
						{
							IpAdapterInfo ipAdapterInfo = (IpAdapterInfo)Marshal.PtrToStructure(safeLocalFree.DangerousGetHandle(), typeof(IpAdapterInfo));
							arrayList.Add(new SystemNetworkInterface(fixedInfo, ipAdapterInfo));
							while (ipAdapterInfo.Next != IntPtr.Zero)
							{
								ipAdapterInfo = (IpAdapterInfo)Marshal.PtrToStructure(ipAdapterInfo.Next, typeof(IpAdapterInfo));
								arrayList.Add(new SystemNetworkInterface(fixedInfo, ipAdapterInfo));
							}
						}
					}
					finally
					{
						safeLocalFree?.Close();
					}
					break;
				case 232u:
					return new SystemNetworkInterface[0];
				default:
					throw new NetworkInformationException((int)adaptersInfo);
				case 0u:
				{
					SystemNetworkInterface[] array = new SystemNetworkInterface[arrayList.Count];
					for (int i = 0; i < arrayList.Count; i++)
					{
						array[i] = (SystemNetworkInterface)arrayList[i];
					}
					return array;
				}
				}
			}
		}

		private static SystemNetworkInterface[] GetAdaptersAddresses(AddressFamily family, FixedInfo fixedInfo)
		{
			uint outBufLen = 0u;
			SafeLocalFree safeLocalFree = null;
			ArrayList arrayList = new ArrayList();
			SystemNetworkInterface[] array = null;
			uint adaptersAddresses = UnsafeNetInfoNativeMethods.GetAdaptersAddresses(family, 0u, IntPtr.Zero, SafeLocalFree.Zero, ref outBufLen);
			while (true)
			{
				switch (adaptersAddresses)
				{
				case 111u:
					try
					{
						safeLocalFree = SafeLocalFree.LocalAlloc((int)outBufLen);
						adaptersAddresses = UnsafeNetInfoNativeMethods.GetAdaptersAddresses(family, 0u, IntPtr.Zero, safeLocalFree, ref outBufLen);
						if (adaptersAddresses == 0)
						{
							IpAdapterAddresses ipAdapterAddresses = (IpAdapterAddresses)Marshal.PtrToStructure(safeLocalFree.DangerousGetHandle(), typeof(IpAdapterAddresses));
							arrayList.Add(new SystemNetworkInterface(fixedInfo, ipAdapterAddresses));
							while (ipAdapterAddresses.next != IntPtr.Zero)
							{
								ipAdapterAddresses = (IpAdapterAddresses)Marshal.PtrToStructure(ipAdapterAddresses.next, typeof(IpAdapterAddresses));
								arrayList.Add(new SystemNetworkInterface(fixedInfo, ipAdapterAddresses));
							}
						}
					}
					finally
					{
						safeLocalFree?.Close();
						safeLocalFree = null;
					}
					break;
				case 87u:
				case 232u:
					return new SystemNetworkInterface[0];
				default:
					throw new NetworkInformationException((int)adaptersAddresses);
				case 0u:
				{
					array = new SystemNetworkInterface[arrayList.Count];
					for (int i = 0; i < arrayList.Count; i++)
					{
						array[i] = (SystemNetworkInterface)arrayList[i];
					}
					return array;
				}
				}
			}
		}

		private static SystemNetworkInterface[] PostWin2KGetNetworkInterfaces(AddressFamily family)
		{
			FixedInfo fixedInfo = SystemIPGlobalProperties.GetFixedInfo();
			SystemNetworkInterface[] array = null;
			while (true)
			{
				try
				{
					array = GetAdaptersAddresses(family, fixedInfo);
				}
				catch (NetworkInformationException ex)
				{
					if ((long)ex.ErrorCode != 1)
					{
						throw;
					}
					continue;
				}
				break;
			}
			if (!Socket.SupportsIPv4)
			{
				return array;
			}
			uint pOutBufLen = 0u;
			uint num = 0u;
			SafeLocalFree safeLocalFree = null;
			if (family == AddressFamily.Unspecified || family == AddressFamily.InterNetwork)
			{
				num = UnsafeNetInfoNativeMethods.GetAdaptersInfo(SafeLocalFree.Zero, ref pOutBufLen);
				int num2 = 0;
				while (num == 111)
				{
					try
					{
						safeLocalFree = SafeLocalFree.LocalAlloc((int)pOutBufLen);
						num = UnsafeNetInfoNativeMethods.GetAdaptersInfo(safeLocalFree, ref pOutBufLen);
						if (num != 0)
						{
							continue;
						}
						IntPtr intPtr = safeLocalFree.DangerousGetHandle();
						bool flag = false;
						while (intPtr != IntPtr.Zero)
						{
							IpAdapterInfo ipAdapterInfo = (IpAdapterInfo)Marshal.PtrToStructure(intPtr, typeof(IpAdapterInfo));
							for (int i = 0; i < array.Length; i++)
							{
								if (array[i] != null && ipAdapterInfo.index == array[i].index)
								{
									if (!array[i].interfaceProperties.Update(fixedInfo, ipAdapterInfo))
									{
										array[i] = null;
										num2++;
									}
									break;
								}
							}
							intPtr = ipAdapterInfo.Next;
						}
					}
					finally
					{
						safeLocalFree?.Close();
					}
				}
				if (num2 != 0)
				{
					SystemNetworkInterface[] array2 = new SystemNetworkInterface[array.Length - num2];
					int num3 = 0;
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != null)
						{
							array2[num3++] = array[j];
						}
					}
					array = array2;
				}
			}
			if (num != 0 && num != 232)
			{
				throw new NetworkInformationException((int)num);
			}
			return array;
		}

		internal SystemNetworkInterface(FixedInfo fixedInfo, IpAdapterAddresses ipAdapterAddresses)
		{
			id = ipAdapterAddresses.AdapterName;
			name = ipAdapterAddresses.friendlyName;
			description = ipAdapterAddresses.description;
			index = ipAdapterAddresses.index;
			physicalAddress = ipAdapterAddresses.address;
			addressLength = ipAdapterAddresses.addressLength;
			type = ipAdapterAddresses.type;
			operStatus = ipAdapterAddresses.operStatus;
			ipv6Index = ipAdapterAddresses.ipv6Index;
			adapterFlags = ipAdapterAddresses.flags;
			interfaceProperties = new SystemIPInterfaceProperties(fixedInfo, ipAdapterAddresses);
		}

		internal SystemNetworkInterface(FixedInfo fixedInfo, IpAdapterInfo ipAdapterInfo)
		{
			id = ipAdapterInfo.adapterName;
			name = string.Empty;
			description = ipAdapterInfo.description;
			index = ipAdapterInfo.index;
			physicalAddress = ipAdapterInfo.address;
			addressLength = ipAdapterInfo.addressLength;
			if (ComNetOS.IsWin2K && !ComNetOS.IsPostWin2K)
			{
				name = ReadAdapterName(id);
			}
			if (name.Length == 0)
			{
				name = description;
			}
			SystemIPv4InterfaceStatistics systemIPv4InterfaceStatistics = new SystemIPv4InterfaceStatistics(index);
			operStatus = systemIPv4InterfaceStatistics.OperationalStatus;
			switch (ipAdapterInfo.type)
			{
			case OldInterfaceType.Ethernet:
				type = NetworkInterfaceType.Ethernet;
				break;
			case OldInterfaceType.Fddi:
				type = NetworkInterfaceType.Fddi;
				break;
			case OldInterfaceType.Loopback:
				type = NetworkInterfaceType.Loopback;
				break;
			case OldInterfaceType.Ppp:
				type = NetworkInterfaceType.Ppp;
				break;
			case OldInterfaceType.Slip:
				type = NetworkInterfaceType.Slip;
				break;
			case OldInterfaceType.TokenRing:
				type = NetworkInterfaceType.TokenRing;
				break;
			default:
				type = NetworkInterfaceType.Unknown;
				break;
			}
			interfaceProperties = new SystemIPInterfaceProperties(fixedInfo, ipAdapterInfo);
		}

		public override PhysicalAddress GetPhysicalAddress()
		{
			byte[] array = new byte[addressLength];
			Array.Copy(physicalAddress, array, addressLength);
			return new PhysicalAddress(array);
		}

		public override IPInterfaceProperties GetIPProperties()
		{
			return interfaceProperties;
		}

		public override IPv4InterfaceStatistics GetIPv4Statistics()
		{
			return new SystemIPv4InterfaceStatistics(index);
		}

		public override bool Supports(NetworkInterfaceComponent networkInterfaceComponent)
		{
			if (networkInterfaceComponent == NetworkInterfaceComponent.IPv6 && ipv6Index != 0)
			{
				return true;
			}
			if (networkInterfaceComponent == NetworkInterfaceComponent.IPv4 && index != 0)
			{
				return true;
			}
			return false;
		}

		[RegistryPermission(SecurityAction.Assert, Read = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}")]
		private string ReadAdapterName(string id)
		{
			RegistryKey registryKey = null;
			string empty = string.Empty;
			try
			{
				string text = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + id + "\\Connection";
				registryKey = Registry.LocalMachine.OpenSubKey(text);
				if (registryKey != null)
				{
					empty = (string)registryKey.GetValue("Name");
					if (empty == null)
					{
						return string.Empty;
					}
					return empty;
				}
				return empty;
			}
			finally
			{
				registryKey?.Close();
			}
		}
	}
}
