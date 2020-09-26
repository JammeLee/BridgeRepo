namespace CSLib.Utility
{
	public class CBitHelper
	{
		public static ushort MergeUInt8(byte high, byte low)
		{
			return (ushort)((high << 8) | low);
		}

		public static uint MergeUInt16(ushort high, ushort low)
		{
			return (uint)((high << 16) | low);
		}

		public static byte GetHighUInt8(ushort data)
		{
			return (byte)((data & 0xFF00) >> 8);
		}

		public static byte GetLowUInt8(ushort data)
		{
			return (byte)(data & 0xFFu);
		}

		public static ushort GetHighUInt16(uint data)
		{
			return (ushort)((data & 0xFFFF0000u) >> 16);
		}

		public static ushort GetLowUInt16(uint data)
		{
			return (ushort)(data & 0xFFFFu);
		}
	}
}
