namespace System.Net
{
	internal class StreamChunkBytes : IReadChunkBytes
	{
		public ConnectStream ChunkStream;

		public int BytesRead;

		public int TotalBytesRead;

		private byte PushByte;

		private bool HavePush;

		public int NextByte
		{
			get
			{
				if (HavePush)
				{
					HavePush = false;
					return PushByte;
				}
				return ChunkStream.ReadSingleByte();
			}
			set
			{
				PushByte = (byte)value;
				HavePush = true;
			}
		}

		public StreamChunkBytes(ConnectStream connectStream)
		{
			ChunkStream = connectStream;
		}
	}
}
