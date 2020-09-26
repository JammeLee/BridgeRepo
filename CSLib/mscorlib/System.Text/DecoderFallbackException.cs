using System.Runtime.Serialization;

namespace System.Text
{
	[Serializable]
	public sealed class DecoderFallbackException : ArgumentException
	{
		private byte[] bytesUnknown;

		private int index;

		public byte[] BytesUnknown => bytesUnknown;

		public int Index => index;

		public DecoderFallbackException()
			: base(Environment.GetResourceString("Arg_ArgumentException"))
		{
			SetErrorCode(-2147024809);
		}

		public DecoderFallbackException(string message)
			: base(message)
		{
			SetErrorCode(-2147024809);
		}

		public DecoderFallbackException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2147024809);
		}

		internal DecoderFallbackException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public DecoderFallbackException(string message, byte[] bytesUnknown, int index)
			: base(message)
		{
			this.bytesUnknown = bytesUnknown;
			this.index = index;
		}
	}
}
