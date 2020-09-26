namespace System.Text
{
	[Serializable]
	public sealed class DecoderExceptionFallback : DecoderFallback
	{
		public override int MaxCharCount => 0;

		public override DecoderFallbackBuffer CreateFallbackBuffer()
		{
			return new DecoderExceptionFallbackBuffer();
		}

		public override bool Equals(object value)
		{
			DecoderExceptionFallback decoderExceptionFallback = value as DecoderExceptionFallback;
			if (decoderExceptionFallback != null)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 879;
		}
	}
}
