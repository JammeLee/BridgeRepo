using System;
using System.Text;
using CSLib.Utility;

namespace PTBuf
{
	public class CStream
	{
		protected byte[] m_Buffer;

		protected int m_Size;

		protected int m_ReadPos;

		protected int m_WritePos;

		private static Encoding m_ᜀ = Encoding.UTF8;

		public int Size => m_Size;

		public int ReadPos
		{
			get
			{
				return m_ReadPos;
			}
			set
			{
				m_ReadPos = value;
			}
		}

		public int WritePos
		{
			get
			{
				return m_WritePos;
			}
			set
			{
				m_WritePos = value;
			}
		}

		public int RemainReadSize => WritePos - ReadPos;

		public int RemainWriteSize => m_Size - m_WritePos;

		public byte[] Buffer => m_Buffer;

		public CStream()
		{
			m_Buffer = new byte[m_Size = 1];
		}

		public CStream(int capacity)
		{
			m_Size = capacity;
			m_Buffer = new byte[capacity];
		}

		public CStream(byte[] data)
		{
			m_Buffer = data;
			if (m_Buffer != null)
			{
				m_Size = m_Buffer.Length;
			}
		}

		public bool isCanRead(int size)
		{
			//Discarded unreachable code: IL_0013
			if (m_ReadPos + size > m_Size)
			{
				if (true)
				{
				}
				return false;
			}
			return true;
		}

		public void ensureCapacity(int length)
		{
			//Discarded unreachable code: IL_0043
			int num = 2;
			while (true)
			{
				int num2;
				switch (num)
				{
				default:
					if (length > RemainWriteSize)
					{
						num = 0;
						continue;
					}
					return;
				case 0:
					if (true)
					{
					}
					num = 5;
					continue;
				case 6:
					num2 = length + m_Size;
					break;
				case 3:
					num = 1;
					continue;
				case 1:
					num2 = m_Size * 2;
					break;
				case 5:
					num = ((length + m_Size <= m_Size * 2) ? 3 : 6);
					continue;
				case 4:
					return;
				}
				int num3 = num2;
				Array.Resize(ref m_Buffer, num3);
				m_Size = num3;
				num = 4;
			}
		}

		public void Reset()
		{
			m_ReadPos = 0;
			m_WritePos = 0;
		}

		public static int WriteEncodedIntLenght(int value)
		{
			return ᜀ(value);
		}

		private static int ᜀ(int A_0)
		{
			//Discarded unreachable code: IL_0064
			int num = 1;
			uint num3 = default(uint);
			int num2 = default(int);
			while (true)
			{
				switch (num)
				{
				default:
					if (A_0 < 0)
					{
						num = 0;
						break;
					}
					num3 = (uint)A_0;
					num2 = 0;
					num = 4;
					break;
				case 0:
					return 10;
				case 4:
				case 5:
					num = 3;
					break;
				case 3:
					if (true)
					{
					}
					if (num3 >= 128)
					{
						num3 >>= 7;
						num2++;
						num = 5;
					}
					else
					{
						num = 2;
					}
					break;
				case 2:
					return num2 + 1;
				}
			}
		}

		public bool ReadBool()
		{
			//Discarded unreachable code: IL_0021
			if (m_Buffer[m_ReadPos++] != 0)
			{
				return true;
			}
			if (true)
			{
			}
			return false;
		}

		public byte ReadByte()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			return m_Buffer[m_ReadPos++];
		}

		public byte[] ReadBytes(int count)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			byte[] array = new byte[count];
			Array.Copy(m_Buffer, m_ReadPos, array, 0, count);
			m_ReadPos += count;
			return array;
		}

		public void ReadBytes(byte[] bytes, int offset, int count)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			Array.Copy(m_Buffer, m_ReadPos, bytes, offset, count);
			m_ReadPos += count;
		}

		public uint ReadUInt32()
		{
			return (uint)ReadInt32();
		}

		public int ReadInt32()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			int result = ReadInt32(m_Buffer, m_ReadPos);
			m_ReadPos += 4;
			return result;
		}

		public int ReadsFixed32()
		{
			return (int)ReadFixed32();
		}

		public uint ReadFixed32()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			uint result = ReadFixed32(m_Buffer, m_ReadPos);
			m_ReadPos += 4;
			return result;
		}

		public ulong ReadFixed64()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ulong result = ReadFixed64(m_Buffer, m_ReadPos);
			m_ReadPos += 8;
			return result;
		}

		public long ReadsFixed64()
		{
			return (long)ReadFixed64();
		}

		public short ReadInt16()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			short result = ReadInt16(m_Buffer, m_ReadPos);
			m_ReadPos += 2;
			return result;
		}

		public static int ReadInt32(byte[] data, int offset)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			return (data[offset + 3] & 0xFF) + ((data[offset + 2] & 0xFF) << 8) + ((data[offset + 1] & 0xFF) << 16) + ((data[offset] & 0xFF) << 24);
		}

		public static uint ReadFixed32(byte[] data, int offset)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			return (uint)((data[offset] & 0xFF) + ((data[offset + 1] & 0xFF) << 8) + ((data[offset + 2] & 0xFF) << 16) + ((data[offset + 3] & 0xFF) << 24));
		}

		public static ulong ReadFixed64(byte[] data, int offset)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			return data[offset++] | ((ulong)data[offset++] << 8) | ((ulong)data[offset++] << 16) | ((ulong)data[offset++] << 24) | ((ulong)data[offset++] << 32) | ((ulong)data[offset++] << 40) | ((ulong)data[offset++] << 48) | ((ulong)data[offset++] << 56);
		}

		public static short ReadInt16(byte[] data, int offset)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			return (short)(((data[offset + 1] & 0xFF) << 8) + (data[offset] & 0xFF));
		}

		public long ReadInt64()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			while (true)
			{
				long num = (m_Buffer[m_ReadPos + 7] & 0xFF) + ((m_Buffer[m_ReadPos + 6] & 0xFF) << 8) + ((m_Buffer[m_ReadPos + 5] & 0xFF) << 16) + ((m_Buffer[m_ReadPos + 4] & 0xFF) << 24) + (m_Buffer[m_ReadPos + 3] & 0xFF) + ((m_Buffer[m_ReadPos + 2] & 0xFF) << 8) + ((m_Buffer[m_ReadPos + 1] & 0xFF) << 16) + ((m_Buffer[m_ReadPos] & 0xFF) << 24);
				m_ReadPos += 8;
				int num2 = 2;
				while (true)
				{
					switch (num2)
					{
					case 2:
						if (num < 0)
						{
							num2 = 0;
							continue;
						}
						goto case 1;
					case 0:
						num++;
						num2 = 1;
						continue;
					case 1:
						return num;
					}
					break;
				}
			}
		}

		public float ReadSingle()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			float result = BitConverter.ToSingle(m_Buffer, m_ReadPos);
			m_ReadPos += 4;
			return result;
		}

		public double ReadDouble()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			double result = BitConverter.ToDouble(m_Buffer, m_ReadPos);
			m_ReadPos += 8;
			return result;
		}

		public ulong ReadUInt64()
		{
			return (ulong)ReadInt64();
		}

		public int Read7BitEncodedInt()
		{
			return (int)Read7BitEncodedULong();
		}

		public ulong Read7BitEncodedULong()
		{
			//Discarded unreachable code: IL_0099
			int a_ = 12;
			byte b = default(byte);
			while (true)
			{
				ulong num = 0uL;
				int num2 = 0;
				int num3 = 4;
				while (true)
				{
					switch (num3)
					{
					case 0:
						if ((b & 0x80) == 0)
						{
							num3 = 3;
							continue;
						}
						goto case 4;
					case 2:
						throw new FormatException(CSimpleThreadPool.b("\u0e47╉㹋⍍ㅏ♑\u0b53ᑕ㥗㹙歛ᱝय़ᙡⵣ\u0865ᱧ奩幫", a_));
					case 4:
						num3 = 1;
						continue;
					case 1:
						if (num2 != 77)
						{
							b = ReadByte();
							num |= ((ulong)b & 0x7FuL) << num2;
							num2 += 7;
							num3 = 0;
						}
						else
						{
							num3 = 2;
						}
						continue;
					case 3:
						if (true)
						{
						}
						return num;
					}
					break;
				}
			}
		}

		public string ReadString()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			int num = Read7BitEncodedInt();
			if (num == 0)
			{
				return "";
			}
			string @string = CStream.m_ᜀ.GetString(Buffer, ReadPos, num);
			ReadPos += num;
			return @string;
		}

		public void Write(byte value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(1);
			m_Buffer[m_WritePos++] = value;
		}

		public void Write(bool value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(1);
			m_Buffer[m_WritePos++] = (byte)(value ? 1u : 0u);
		}

		public void Write(byte[] buffer)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(buffer.Length);
			Array.Copy(buffer, 0, m_Buffer, m_WritePos, buffer.Length);
			m_WritePos += buffer.Length;
		}

		public void Write(short value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(2);
			m_Buffer[m_WritePos + 1] = (byte)(value >> 8);
			m_Buffer[m_WritePos] = (byte)value;
			m_WritePos += 2;
		}

		public void Write(ushort value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(2);
			m_Buffer[m_WritePos + 1] = (byte)(value >> 8);
			m_Buffer[m_WritePos] = (byte)value;
			m_WritePos += 2;
		}

		public void WriteInt32(int value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(4);
			m_Buffer[m_WritePos] = (byte)value;
			m_Buffer[m_WritePos + 1] = (byte)(value >> 8);
			m_Buffer[m_WritePos + 2] = (byte)(value >> 16);
			m_Buffer[m_WritePos + 3] = (byte)(value >> 24);
			m_WritePos += 4;
		}

		public void Write(int value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(4);
			m_Buffer[m_WritePos + 3] = (byte)value;
			m_Buffer[m_WritePos + 2] = (byte)(value >> 8);
			m_Buffer[m_WritePos + 1] = (byte)(value >> 16);
			m_Buffer[m_WritePos] = (byte)(value >> 24);
			m_WritePos += 4;
		}

		public void WriteFixed32(uint value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(4);
			m_Buffer[m_WritePos] = (byte)value;
			m_Buffer[m_WritePos + 1] = (byte)(value >> 8);
			m_Buffer[m_WritePos + 2] = (byte)(value >> 16);
			m_Buffer[m_WritePos + 3] = (byte)(value >> 24);
			m_WritePos += 4;
		}

		public void WritesFixed32(int value)
		{
			WriteFixed32((uint)value);
		}

		public void WritesFixed64(long value)
		{
			WriteFixed64((ulong)value);
		}

		public void Write(long value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(8);
			m_Buffer[m_WritePos + 7] = (byte)value;
			m_Buffer[m_WritePos + 6] = (byte)(value >> 8);
			m_Buffer[m_WritePos + 5] = (byte)(value >> 16);
			m_Buffer[m_WritePos + 4] = (byte)(value >> 24);
			m_Buffer[m_WritePos + 3] = (byte)(value >> 32);
			m_Buffer[m_WritePos + 2] = (byte)(value >> 40);
			m_Buffer[m_WritePos + 1] = (byte)(value >> 48);
			m_Buffer[m_WritePos] = (byte)(value >> 56);
			m_WritePos += 8;
		}

		public void WriteFixed64(ulong value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(8);
			m_Buffer[m_WritePos] = (byte)value;
			m_Buffer[m_WritePos + 1] = (byte)(value >> 8);
			m_Buffer[m_WritePos + 2] = (byte)(value >> 16);
			m_Buffer[m_WritePos + 3] = (byte)(value >> 24);
			m_Buffer[m_WritePos + 4] = (byte)(value >> 32);
			m_Buffer[m_WritePos + 5] = (byte)(value >> 40);
			m_Buffer[m_WritePos + 6] = (byte)(value >> 48);
			m_Buffer[m_WritePos + 7] = (byte)(value >> 56);
			m_WritePos += 8;
		}

		public void Write(byte[] data, int offset, int count)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ensureCapacity(count);
			Array.Copy(data, offset, m_Buffer, m_WritePos, count);
			m_WritePos += count;
		}

		public void Move(int offset)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			Array.Copy(m_Buffer, offset, m_Buffer, 0, WritePos - offset);
			WritePos -= offset;
			ReadPos = 0;
		}

		public void Write(float value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			byte[] bytes = BitConverter.GetBytes(value);
			ensureCapacity(4);
			Array.Copy(bytes, 0, m_Buffer, m_WritePos, 4);
			m_WritePos += 4;
		}

		public void Write(double value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			byte[] bytes = BitConverter.GetBytes(value);
			ensureCapacity(8);
			Array.Copy(bytes, 0, m_Buffer, m_WritePos, 8);
			m_WritePos += 8;
		}

		public void WriteEncodedInt(int value)
		{
			WriteUInt64Variant(value);
		}

		public void WriteUInt64Variant(long value)
		{
			//Discarded unreachable code: IL_002f
			while (true)
			{
				ulong num = (ulong)value;
				int num2 = 0;
				while (true)
				{
					switch (num2)
					{
					case 0:
					case 1:
						num2 = 3;
						continue;
					case 3:
						if (true)
						{
						}
						if (num < 128)
						{
							num2 = 2;
							continue;
						}
						Write((byte)(num | 0x80));
						num >>= 7;
						num2 = 1;
						continue;
					case 2:
						Write((byte)num);
						return;
					}
					break;
				}
			}
		}

		public void Write(string value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			int length = value.Length;
			if (length == 0)
			{
				WriteEncodedInt(value.Length);
				return;
			}
			int byteCount = CStream.m_ᜀ.GetByteCount(value);
			WriteEncodedInt(byteCount);
			ensureCapacity(byteCount);
			CStream.m_ᜀ.GetBytes(value, 0, length, Buffer, WritePos);
			WritePos += byteCount;
		}
	}
}
