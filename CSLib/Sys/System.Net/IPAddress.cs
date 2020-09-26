using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace System.Net
{
	[Serializable]
	public class IPAddress
	{
		internal const long LoopbackMask = 127L;

		internal const string InaddrNoneString = "255.255.255.255";

		internal const string InaddrNoneStringHex = "0xff.0xff.0xff.0xff";

		internal const string InaddrNoneStringOct = "0377.0377.0377.0377";

		internal const int IPv4AddressBytes = 4;

		internal const int IPv6AddressBytes = 16;

		internal const int NumberOfLabels = 8;

		public static readonly IPAddress Any = new IPAddress(0);

		public static readonly IPAddress Loopback = new IPAddress(16777343);

		public static readonly IPAddress Broadcast = new IPAddress(4294967295L);

		public static readonly IPAddress None = Broadcast;

		internal long m_Address;

		[NonSerialized]
		internal string m_ToString;

		public static readonly IPAddress IPv6Any;

		public static readonly IPAddress IPv6Loopback;

		public static readonly IPAddress IPv6None;

		private AddressFamily m_Family = AddressFamily.InterNetwork;

		private ushort[] m_Numbers = new ushort[8];

		private long m_ScopeId;

		private int m_HashCode;

		[Obsolete("This property has been deprecated. It is address family dependent. Please use IPAddress.Equals method to perform comparisons. http://go.microsoft.com/fwlink/?linkid=14202")]
		public long Address
		{
			get
			{
				if (m_Family == AddressFamily.InterNetworkV6)
				{
					throw new SocketException(SocketError.OperationNotSupported);
				}
				return m_Address;
			}
			set
			{
				if (m_Family == AddressFamily.InterNetworkV6)
				{
					throw new SocketException(SocketError.OperationNotSupported);
				}
				if (m_Address != value)
				{
					m_ToString = null;
					m_Address = value;
				}
			}
		}

		public AddressFamily AddressFamily => m_Family;

		public long ScopeId
		{
			get
			{
				if (m_Family == AddressFamily.InterNetwork)
				{
					throw new SocketException(SocketError.OperationNotSupported);
				}
				return m_ScopeId;
			}
			set
			{
				if (m_Family == AddressFamily.InterNetwork)
				{
					throw new SocketException(SocketError.OperationNotSupported);
				}
				if (value < 0 || value > uint.MaxValue)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (m_ScopeId != value)
				{
					m_Address = value;
					m_ScopeId = value;
				}
			}
		}

		internal bool IsBroadcast
		{
			get
			{
				if (m_Family == AddressFamily.InterNetworkV6)
				{
					return false;
				}
				return m_Address == Broadcast.m_Address;
			}
		}

		public bool IsIPv6Multicast
		{
			get
			{
				if (m_Family == AddressFamily.InterNetworkV6)
				{
					return (m_Numbers[0] & 0xFF00) == 65280;
				}
				return false;
			}
		}

		public bool IsIPv6LinkLocal
		{
			get
			{
				if (m_Family == AddressFamily.InterNetworkV6)
				{
					return (m_Numbers[0] & 0xFFC0) == 65152;
				}
				return false;
			}
		}

		public bool IsIPv6SiteLocal
		{
			get
			{
				if (m_Family == AddressFamily.InterNetworkV6)
				{
					return (m_Numbers[0] & 0xFFC0) == 65216;
				}
				return false;
			}
		}

		public IPAddress(long newAddress)
		{
			if (newAddress < 0 || newAddress > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("newAddress");
			}
			m_Address = newAddress;
		}

		public IPAddress(byte[] address, long scopeid)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (address.Length != 16)
			{
				throw new ArgumentException(SR.GetString("dns_bad_ip_address"), "address");
			}
			m_Family = AddressFamily.InterNetworkV6;
			for (int i = 0; i < 8; i++)
			{
				m_Numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
			}
			if (scopeid < 0 || scopeid > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("scopeid");
			}
			m_ScopeId = scopeid;
		}

		private IPAddress(ushort[] address, uint scopeid)
		{
			m_Family = AddressFamily.InterNetworkV6;
			m_Numbers = address;
			m_ScopeId = scopeid;
		}

		public IPAddress(byte[] address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (address.Length != 4 && address.Length != 16)
			{
				throw new ArgumentException(SR.GetString("dns_bad_ip_address"), "address");
			}
			if (address.Length == 4)
			{
				m_Family = AddressFamily.InterNetwork;
				m_Address = ((address[3] << 24) | (address[2] << 16) | (address[1] << 8) | address[0]) & 0xFFFFFFFFu;
				return;
			}
			m_Family = AddressFamily.InterNetworkV6;
			for (int i = 0; i < 8; i++)
			{
				m_Numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
			}
		}

		internal IPAddress(int newAddress)
		{
			m_Address = newAddress & 0xFFFFFFFFu;
		}

		public static bool TryParse(string ipString, out IPAddress address)
		{
			address = InternalParse(ipString, tryParse: true);
			return address != null;
		}

		public static IPAddress Parse(string ipString)
		{
			return InternalParse(ipString, tryParse: false);
		}

		private unsafe static IPAddress InternalParse(string ipString, bool tryParse)
		{
			if (ipString == null)
			{
				throw new ArgumentNullException("ipString");
			}
			if (ipString.IndexOf(':') != -1)
			{
				SocketException ex = null;
				long num = 0L;
				if (Socket.OSSupportsIPv6)
				{
					byte[] array = new byte[16];
					SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetworkV6, 28);
					if (UnsafeNclNativeMethods.OSSOCK.WSAStringToAddress(ipString, AddressFamily.InterNetworkV6, IntPtr.Zero, socketAddress.m_Buffer, ref socketAddress.m_Size) == SocketError.Success)
					{
						for (int i = 0; i < 16; i++)
						{
							array[i] = socketAddress[i + 8];
						}
						num = (socketAddress[27] << 24) + (socketAddress[26] << 16) + (socketAddress[25] << 8) + socketAddress[24];
						return new IPAddress(array, num);
					}
					if (tryParse)
					{
						return null;
					}
					ex = new SocketException();
				}
				else
				{
					int start = 0;
					if (ipString[0] != '[')
					{
						ipString += ']';
					}
					else
					{
						start = 1;
					}
					int end = ipString.Length;
					fixed (char* name = ipString)
					{
						if (IPv6AddressHelper.IsValid(name, start, ref end) || end != ipString.Length)
						{
							ushort[] array2 = new ushort[8];
							string scopeId = null;
							fixed (ushort* numbers = array2)
							{
								IPv6AddressHelper.Parse(ipString, numbers, 0, ref scopeId);
							}
							if (scopeId == null || scopeId.Length == 0)
							{
								return new IPAddress(array2, 0u);
							}
							scopeId = scopeId.Substring(1);
							if (uint.TryParse(scopeId, NumberStyles.None, null, out var result))
							{
								return new IPAddress(array2, result);
							}
						}
					}
					if (tryParse)
					{
						return null;
					}
					ex = new SocketException(SocketError.InvalidArgument);
				}
				throw new FormatException(SR.GetString("dns_bad_ip_address"), ex);
			}
			int num2 = -1;
			if (ipString.Length > 0 && ipString[0] >= '0' && ipString[0] <= '9' && ((ipString[ipString.Length - 1] >= '0' && ipString[ipString.Length - 1] <= '9') || (ipString[ipString.Length - 1] >= 'a' && ipString[ipString.Length - 1] <= 'f') || (ipString[ipString.Length - 1] >= 'A' && ipString[ipString.Length - 1] <= 'F')))
			{
				Socket.InitializeSockets();
				num2 = UnsafeNclNativeMethods.OSSOCK.inet_addr(ipString);
			}
			if (num2 == -1 && string.Compare(ipString, "255.255.255.255", StringComparison.Ordinal) != 0 && string.Compare(ipString, "0xff.0xff.0xff.0xff", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(ipString, "0377.0377.0377.0377", StringComparison.Ordinal) != 0)
			{
				if (tryParse)
				{
					return null;
				}
				throw new FormatException(SR.GetString("dns_bad_ip_address"));
			}
			return new IPAddress(num2);
		}

		public byte[] GetAddressBytes()
		{
			byte[] array;
			if (m_Family != AddressFamily.InterNetworkV6)
			{
				array = new byte[4]
				{
					(byte)m_Address,
					(byte)(m_Address >> 8),
					(byte)(m_Address >> 16),
					(byte)(m_Address >> 24)
				};
			}
			else
			{
				array = new byte[16];
				int num = 0;
				for (int i = 0; i < 8; i++)
				{
					array[num++] = (byte)((uint)(m_Numbers[i] >> 8) & 0xFFu);
					array[num++] = (byte)(m_Numbers[i] & 0xFFu);
				}
			}
			return array;
		}

		public unsafe override string ToString()
		{
			if (m_ToString == null)
			{
				if (m_Family == AddressFamily.InterNetworkV6)
				{
					int addressStringLength = 256;
					StringBuilder stringBuilder = new StringBuilder(addressStringLength);
					if (Socket.OSSupportsIPv6)
					{
						SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetworkV6, 28);
						int num = 8;
						for (int i = 0; i < 8; i++)
						{
							socketAddress[num++] = (byte)(m_Numbers[i] >> 8);
							socketAddress[num++] = (byte)m_Numbers[i];
						}
						if (m_ScopeId > 0)
						{
							socketAddress[24] = (byte)m_ScopeId;
							socketAddress[25] = (byte)(m_ScopeId >> 8);
							socketAddress[26] = (byte)(m_ScopeId >> 16);
							socketAddress[27] = (byte)(m_ScopeId >> 24);
						}
						if (UnsafeNclNativeMethods.OSSOCK.WSAAddressToString(socketAddress.m_Buffer, socketAddress.m_Size, IntPtr.Zero, stringBuilder, ref addressStringLength) != 0)
						{
							throw new SocketException();
						}
					}
					else
					{
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", m_Numbers[0])).Append(':');
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", m_Numbers[1])).Append(':');
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", m_Numbers[2])).Append(':');
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", m_Numbers[3])).Append(':');
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", m_Numbers[4])).Append(':');
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", m_Numbers[5])).Append(':');
						stringBuilder.Append((m_Numbers[6] >> 8) & 0xFF).Append('.');
						stringBuilder.Append(m_Numbers[6] & 0xFF).Append('.');
						stringBuilder.Append((m_Numbers[7] >> 8) & 0xFF).Append('.');
						stringBuilder.Append(m_Numbers[7] & 0xFF);
						if (m_ScopeId != 0)
						{
							stringBuilder.Append('%').Append((uint)m_ScopeId);
						}
					}
					m_ToString = stringBuilder.ToString();
				}
				else
				{
					int num2 = 15;
					char* ptr = (char*)stackalloc byte[2 * 15];
					int num3 = (int)((m_Address >> 24) & 0xFF);
					do
					{
						ptr[--num2] = (char)(48 + num3 % 10);
						num3 /= 10;
					}
					while (num3 > 0);
					ptr[--num2] = '.';
					num3 = (int)((m_Address >> 16) & 0xFF);
					do
					{
						ptr[--num2] = (char)(48 + num3 % 10);
						num3 /= 10;
					}
					while (num3 > 0);
					ptr[--num2] = '.';
					num3 = (int)((m_Address >> 8) & 0xFF);
					do
					{
						ptr[--num2] = (char)(48 + num3 % 10);
						num3 /= 10;
					}
					while (num3 > 0);
					ptr[--num2] = '.';
					num3 = (int)(m_Address & 0xFF);
					do
					{
						ptr[--num2] = (char)(48 + num3 % 10);
						num3 /= 10;
					}
					while (num3 > 0);
					m_ToString = new string(ptr, num2, 15 - num2);
				}
			}
			return m_ToString;
		}

		public static long HostToNetworkOrder(long host)
		{
			return ((HostToNetworkOrder((int)host) & 0xFFFFFFFFu) << 32) | (HostToNetworkOrder((int)(host >> 32)) & 0xFFFFFFFFu);
		}

		public static int HostToNetworkOrder(int host)
		{
			return ((HostToNetworkOrder((short)host) & 0xFFFF) << 16) | (HostToNetworkOrder((short)(host >> 16)) & 0xFFFF);
		}

		public static short HostToNetworkOrder(short host)
		{
			return (short)(((host & 0xFF) << 8) | ((host >> 8) & 0xFF));
		}

		public static long NetworkToHostOrder(long network)
		{
			return HostToNetworkOrder(network);
		}

		public static int NetworkToHostOrder(int network)
		{
			return HostToNetworkOrder(network);
		}

		public static short NetworkToHostOrder(short network)
		{
			return HostToNetworkOrder(network);
		}

		public static bool IsLoopback(IPAddress address)
		{
			if (address.m_Family == AddressFamily.InterNetworkV6)
			{
				return address.Equals(IPv6Loopback);
			}
			return (address.m_Address & 0x7F) == (Loopback.m_Address & 0x7F);
		}

		internal bool Equals(object comparand, bool compareScopeId)
		{
			if (!(comparand is IPAddress))
			{
				return false;
			}
			if (m_Family != ((IPAddress)comparand).m_Family)
			{
				return false;
			}
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				for (int i = 0; i < 8; i++)
				{
					if (((IPAddress)comparand).m_Numbers[i] != m_Numbers[i])
					{
						return false;
					}
				}
				if (((IPAddress)comparand).m_ScopeId == m_ScopeId)
				{
					return true;
				}
				if (!compareScopeId)
				{
					return true;
				}
				return false;
			}
			return ((IPAddress)comparand).m_Address == m_Address;
		}

		public override bool Equals(object comparand)
		{
			return Equals(comparand, compareScopeId: true);
		}

		public override int GetHashCode()
		{
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				if (m_HashCode == 0)
				{
					m_HashCode = Uri.CalculateCaseInsensitiveHashCode(ToString());
				}
				return m_HashCode;
			}
			return (int)m_Address;
		}

		internal IPAddress Snapshot()
		{
			return m_Family switch
			{
				AddressFamily.InterNetwork => new IPAddress(m_Address), 
				AddressFamily.InterNetworkV6 => new IPAddress(m_Numbers, (uint)m_ScopeId), 
				_ => throw new InternalException(), 
			};
		}

		static IPAddress()
		{
			byte[] address = new byte[16];
			IPv6Any = new IPAddress(address, 0L);
			IPv6Loopback = new IPAddress(new byte[16]
			{
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1
			}, 0L);
			byte[] address2 = new byte[16];
			IPv6None = new IPAddress(address2, 0L);
		}
	}
}
