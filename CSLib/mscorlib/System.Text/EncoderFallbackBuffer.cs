namespace System.Text
{
	public abstract class EncoderFallbackBuffer
	{
		private const int iMaxRecursion = 250;

		internal unsafe char* charStart = null;

		internal unsafe char* charEnd;

		internal EncoderNLS encoder;

		internal bool setEncoder;

		internal bool bUsedEncoder;

		internal bool bFallingBack;

		internal int iRecursionCount;

		public abstract int Remaining
		{
			get;
		}

		public abstract bool Fallback(char charUnknown, int index);

		public abstract bool Fallback(char charUnknownHigh, char charUnknownLow, int index);

		public abstract char GetNextChar();

		public abstract bool MovePrevious();

		public virtual void Reset()
		{
			while (GetNextChar() != 0)
			{
			}
		}

		internal unsafe void InternalReset()
		{
			charStart = null;
			bFallingBack = false;
			iRecursionCount = 0;
			Reset();
		}

		internal unsafe void InternalInitialize(char* charStart, char* charEnd, EncoderNLS encoder, bool setEncoder)
		{
			this.charStart = charStart;
			this.charEnd = charEnd;
			this.encoder = encoder;
			this.setEncoder = setEncoder;
			bUsedEncoder = false;
			bFallingBack = false;
			iRecursionCount = 0;
		}

		internal char InternalGetNextChar()
		{
			char nextChar = GetNextChar();
			bFallingBack = nextChar != '\0';
			if (nextChar == '\0')
			{
				iRecursionCount = 0;
			}
			return nextChar;
		}

		internal unsafe virtual bool InternalFallback(char ch, ref char* chars)
		{
			int index = (int)(chars - charStart) - 1;
			if (char.IsHighSurrogate(ch))
			{
				if (chars >= charEnd)
				{
					if (encoder != null && !encoder.MustFlush)
					{
						if (setEncoder)
						{
							bUsedEncoder = true;
							encoder.charLeftOver = ch;
						}
						bFallingBack = false;
						return false;
					}
				}
				else
				{
					char c = *chars;
					if (char.IsLowSurrogate(c))
					{
						if (bFallingBack && iRecursionCount++ > 250)
						{
							ThrowLastCharRecursive(char.ConvertToUtf32(ch, c));
						}
						chars++;
						bFallingBack = Fallback(ch, c, index);
						return bFallingBack;
					}
				}
			}
			if (bFallingBack && iRecursionCount++ > 250)
			{
				ThrowLastCharRecursive(ch);
			}
			bFallingBack = Fallback(ch, index);
			return bFallingBack;
		}

		internal void ThrowLastCharRecursive(int charRecursive)
		{
			throw new ArgumentException(Environment.GetResourceString("Argument_RecursiveFallback", charRecursive), "chars");
		}
	}
}
