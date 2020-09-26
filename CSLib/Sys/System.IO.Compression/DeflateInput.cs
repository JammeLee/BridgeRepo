namespace System.IO.Compression
{
	internal class DeflateInput
	{
		private byte[] buffer;

		private int count;

		private int startIndex;

		internal byte[] Buffer
		{
			get
			{
				return buffer;
			}
			set
			{
				buffer = value;
			}
		}

		internal int Count
		{
			get
			{
				return count;
			}
			set
			{
				count = value;
			}
		}

		internal int StartIndex
		{
			get
			{
				return startIndex;
			}
			set
			{
				startIndex = value;
			}
		}

		internal void ConsumeBytes(int n)
		{
			startIndex += n;
			count -= n;
		}
	}
}
