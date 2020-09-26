using System.Diagnostics;

namespace System.Net.NetworkInformation
{
	internal class IcmpPacket
	{
		private static ushort staticSequenceNumber;

		internal byte type;

		internal byte subCode;

		internal ushort checkSum;

		internal static ushort identifier;

		internal ushort sequenceNumber;

		internal byte[] buffer;

		internal ushort Identifier
		{
			get
			{
				if (identifier == 0)
				{
					identifier = (ushort)Process.GetCurrentProcess().Id;
				}
				return identifier;
			}
		}

		internal IcmpPacket(byte[] buffer)
		{
			type = 8;
			this.buffer = buffer;
			sequenceNumber = staticSequenceNumber++;
			checkSum = (ushort)GetCheckSum();
		}

		private uint GetCheckSum()
		{
			uint num = (uint)(type + Identifier + sequenceNumber);
			int num2;
			for (num2 = 0; num2 < buffer.Length; num2++)
			{
				num += (uint)(buffer[num2] + (buffer[++num2] << 8));
			}
			num = (num >> 16) + (num & 0xFFFF);
			num += num >> 16;
			return ~num;
		}

		internal byte[] GetBytes()
		{
			byte[] array = new byte[buffer.Length + 8];
			byte[] bytes = BitConverter.GetBytes(checkSum);
			byte[] bytes2 = BitConverter.GetBytes(Identifier);
			byte[] bytes3 = BitConverter.GetBytes(sequenceNumber);
			array[0] = type;
			array[1] = subCode;
			Array.Copy(bytes, 0, array, 2, 2);
			Array.Copy(bytes2, 0, array, 4, 2);
			Array.Copy(bytes3, 0, array, 6, 2);
			Array.Copy(buffer, 0, array, 8, buffer.Length);
			return array;
		}
	}
}
