namespace System.Net
{
	internal struct BufferChunkBytes : IReadChunkBytes
	{
		public byte[] Buffer;

		public int Offset;

		public int Count;

		public int NextByte
		{
			get
			{
				if (Count != 0)
				{
					Count--;
					return Buffer[Offset++];
				}
				return -1;
			}
			set
			{
				Count++;
				Offset--;
				Buffer[Offset] = (byte)value;
			}
		}
	}
}
