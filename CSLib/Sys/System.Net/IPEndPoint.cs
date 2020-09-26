using System.Globalization;
using System.Net.Sockets;

namespace System.Net
{
	[Serializable]
	public class IPEndPoint : EndPoint
	{
		public const int MinPort = 0;

		public const int MaxPort = 65535;

		internal const int AnyPort = 0;

		private IPAddress m_Address;

		private int m_Port;

		internal static IPEndPoint Any = new IPEndPoint(IPAddress.Any, 0);

		internal static IPEndPoint IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);

		public override AddressFamily AddressFamily => m_Address.AddressFamily;

		public IPAddress Address
		{
			get
			{
				return m_Address;
			}
			set
			{
				m_Address = value;
			}
		}

		public int Port
		{
			get
			{
				return m_Port;
			}
			set
			{
				if (!ValidationHelper.ValidateTcpPort(value))
				{
					throw new ArgumentOutOfRangeException("value");
				}
				m_Port = value;
			}
		}

		public IPEndPoint(long address, int port)
		{
			if (!ValidationHelper.ValidateTcpPort(port))
			{
				throw new ArgumentOutOfRangeException("port");
			}
			m_Port = port;
			m_Address = new IPAddress(address);
		}

		public IPEndPoint(IPAddress address, int port)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (!ValidationHelper.ValidateTcpPort(port))
			{
				throw new ArgumentOutOfRangeException("port");
			}
			m_Port = port;
			m_Address = address;
		}

		public override string ToString()
		{
			return Address.ToString() + ":" + Port.ToString(NumberFormatInfo.InvariantInfo);
		}

		public override SocketAddress Serialize()
		{
			if (m_Address.AddressFamily == AddressFamily.InterNetworkV6)
			{
				SocketAddress socketAddress = new SocketAddress(AddressFamily, 28);
				int port = Port;
				socketAddress[2] = (byte)(port >> 8);
				socketAddress[3] = (byte)port;
				socketAddress[4] = 0;
				socketAddress[5] = 0;
				socketAddress[6] = 0;
				socketAddress[7] = 0;
				long scopeId = Address.ScopeId;
				socketAddress[24] = (byte)scopeId;
				socketAddress[25] = (byte)(scopeId >> 8);
				socketAddress[26] = (byte)(scopeId >> 16);
				socketAddress[27] = (byte)(scopeId >> 24);
				byte[] addressBytes = Address.GetAddressBytes();
				for (int i = 0; i < addressBytes.Length; i++)
				{
					socketAddress[8 + i] = addressBytes[i];
				}
				return socketAddress;
			}
			SocketAddress socketAddress2 = new SocketAddress(m_Address.AddressFamily, 16);
			socketAddress2[2] = (byte)(Port >> 8);
			socketAddress2[3] = (byte)Port;
			socketAddress2[4] = (byte)Address.m_Address;
			socketAddress2[5] = (byte)(Address.m_Address >> 8);
			socketAddress2[6] = (byte)(Address.m_Address >> 16);
			socketAddress2[7] = (byte)(Address.m_Address >> 24);
			return socketAddress2;
		}

		public override EndPoint Create(SocketAddress socketAddress)
		{
			if (socketAddress.Family != AddressFamily)
			{
				throw new ArgumentException(SR.GetString("net_InvalidAddressFamily", socketAddress.Family.ToString(), GetType().FullName, AddressFamily.ToString()), "socketAddress");
			}
			if (socketAddress.Size < 8)
			{
				throw new ArgumentException(SR.GetString("net_InvalidSocketAddressSize", socketAddress.GetType().FullName, GetType().FullName), "socketAddress");
			}
			if (AddressFamily == AddressFamily.InterNetworkV6)
			{
				byte[] array = new byte[16];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = socketAddress[i + 8];
				}
				int port = ((socketAddress[2] << 8) & 0xFF00) | socketAddress[3];
				long scopeid = (socketAddress[27] << 24) + (socketAddress[26] << 16) + (socketAddress[25] << 8) + socketAddress[24];
				return new IPEndPoint(new IPAddress(array, scopeid), port);
			}
			int port2 = ((socketAddress[2] << 8) & 0xFF00) | socketAddress[3];
			long address = ((socketAddress[4] & 0xFF) | ((socketAddress[5] << 8) & 0xFF00) | ((socketAddress[6] << 16) & 0xFF0000) | (socketAddress[7] << 24)) & 0xFFFFFFFFu;
			return new IPEndPoint(address, port2);
		}

		public override bool Equals(object comparand)
		{
			if (!(comparand is IPEndPoint))
			{
				return false;
			}
			if (((IPEndPoint)comparand).m_Address.Equals(m_Address))
			{
				return ((IPEndPoint)comparand).m_Port == m_Port;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return m_Address.GetHashCode() ^ m_Port;
		}

		internal IPEndPoint Snapshot()
		{
			return new IPEndPoint(Address.Snapshot(), Port);
		}
	}
}
