using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace System.Net
{
	public class SocketAddress
	{
		internal const int IPv6AddressSize = 28;

		internal const int IPv4AddressSize = 16;

		private const int WriteableOffset = 2;

		private const int MaxSize = 32;

		internal int m_Size;

		internal byte[] m_Buffer;

		private bool m_changed = true;

		private int m_hash;

		public AddressFamily Family => (AddressFamily)(m_Buffer[0] | (m_Buffer[1] << 8));

		public int Size => m_Size;

		public byte this[int offset]
		{
			get
			{
				if (offset < 0 || offset >= Size)
				{
					throw new IndexOutOfRangeException();
				}
				return m_Buffer[offset];
			}
			set
			{
				if (offset < 0 || offset >= Size)
				{
					throw new IndexOutOfRangeException();
				}
				if (m_Buffer[offset] != value)
				{
					m_changed = true;
				}
				m_Buffer[offset] = value;
			}
		}

		public SocketAddress(AddressFamily family)
			: this(family, 32)
		{
		}

		public SocketAddress(AddressFamily family, int size)
		{
			if (size < 2)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			m_Size = size;
			m_Buffer = new byte[(size / IntPtr.Size + 2) * IntPtr.Size];
			m_Buffer[0] = (byte)family;
			m_Buffer[1] = (byte)((int)family >> 8);
		}

		internal void CopyAddressSizeIntoBuffer()
		{
			m_Buffer[m_Buffer.Length - IntPtr.Size] = (byte)m_Size;
			m_Buffer[m_Buffer.Length - IntPtr.Size + 1] = (byte)(m_Size >> 8);
			m_Buffer[m_Buffer.Length - IntPtr.Size + 2] = (byte)(m_Size >> 16);
			m_Buffer[m_Buffer.Length - IntPtr.Size + 3] = (byte)(m_Size >> 24);
		}

		internal int GetAddressSizeOffset()
		{
			return m_Buffer.Length - IntPtr.Size;
		}

		internal unsafe void SetSize(IntPtr ptr)
		{
			m_Size = *(int*)(void*)ptr;
		}

		public override bool Equals(object comparand)
		{
			SocketAddress socketAddress = comparand as SocketAddress;
			if (socketAddress == null || Size != socketAddress.Size)
			{
				return false;
			}
			for (int i = 0; i < Size; i++)
			{
				if (this[i] != socketAddress[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			if (m_changed)
			{
				m_changed = false;
				m_hash = 0;
				int num = Size & -4;
				int i;
				for (i = 0; i < num; i += 4)
				{
					m_hash ^= m_Buffer[i] | (m_Buffer[i + 1] << 8) | (m_Buffer[i + 2] << 16) | (m_Buffer[i + 3] << 24);
				}
				if (((uint)Size & 3u) != 0)
				{
					int num2 = 0;
					int num3 = 0;
					for (; i < Size; i++)
					{
						num2 |= m_Buffer[i] << num3;
						num3 += 8;
					}
					m_hash ^= num2;
				}
			}
			return m_hash;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 2; i < Size; i++)
			{
				if (i > 2)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(this[i].ToString(NumberFormatInfo.InvariantInfo));
			}
			return Family.ToString() + ":" + Size.ToString(NumberFormatInfo.InvariantInfo) + ":{" + stringBuilder.ToString() + "}";
		}
	}
}
