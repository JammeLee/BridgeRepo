namespace System.Net.Sockets
{
	public class SendPacketsElement
	{
		internal string m_FilePath;

		internal byte[] m_Buffer;

		internal int m_Offset;

		internal int m_Count;

		internal UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags m_Flags;

		public string FilePath => m_FilePath;

		public byte[] Buffer => m_Buffer;

		public int Count => m_Count;

		public int Offset => m_Offset;

		public bool EndOfPacket => (m_Flags & UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.EndOfPacket) != 0;

		private SendPacketsElement()
		{
		}

		public SendPacketsElement(string filepath)
			: this(filepath, null, 0, 0, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.File)
		{
		}

		public SendPacketsElement(string filepath, int offset, int count)
			: this(filepath, null, offset, count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.File)
		{
		}

		public SendPacketsElement(string filepath, int offset, int count, bool endOfPacket)
			: this(filepath, null, offset, count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.File | UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.EndOfPacket)
		{
		}

		public SendPacketsElement(byte[] buffer)
			: this(null, buffer, 0, buffer.Length, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.Memory)
		{
		}

		public SendPacketsElement(byte[] buffer, int offset, int count)
			: this(null, buffer, offset, count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.Memory)
		{
		}

		public SendPacketsElement(byte[] buffer, int offset, int count, bool endOfPacket)
			: this(null, buffer, offset, count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.Memory | UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.EndOfPacket)
		{
		}

		private SendPacketsElement(string filepath, byte[] buffer, int offset, int count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags flags)
		{
			m_FilePath = filepath;
			m_Buffer = buffer;
			m_Offset = offset;
			m_Count = count;
			m_Flags = flags;
		}
	}
}
