namespace System
{
	internal struct UnSafeCharBuffer
	{
		private unsafe char* m_buffer;

		private int m_totalSize;

		private int m_length;

		public int Length => m_length;

		public unsafe UnSafeCharBuffer(char* buffer, int bufferSize)
		{
			m_buffer = buffer;
			m_totalSize = bufferSize;
			m_length = 0;
		}

		public unsafe void AppendString(string stringToAppend)
		{
			if (!string.IsNullOrEmpty(stringToAppend))
			{
				if (m_totalSize - m_length < stringToAppend.Length)
				{
					throw new IndexOutOfRangeException();
				}
				fixed (char* src = stringToAppend)
				{
					Buffer.memcpyimpl((byte*)src, (byte*)(m_buffer + m_length), stringToAppend.Length * 2);
				}
				m_length += stringToAppend.Length;
			}
		}
	}
}
