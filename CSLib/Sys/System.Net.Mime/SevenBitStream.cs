using System.IO;

namespace System.Net.Mime
{
	internal class SevenBitStream : DelegatedStream
	{
		internal SevenBitStream(Stream stream)
			: base(stream)
		{
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset >= buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (offset + count > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			CheckBytes(buffer, offset, count);
			return base.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset >= buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (offset + count > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			CheckBytes(buffer, offset, count);
			base.Write(buffer, offset, count);
		}

		private void CheckBytes(byte[] buffer, int offset, int count)
		{
			for (int i = count; i < offset + count; i++)
			{
				if (buffer[i] > 127)
				{
					throw new FormatException(SR.GetString("Mail7BitStreamInvalidCharacter"));
				}
			}
		}
	}
}
