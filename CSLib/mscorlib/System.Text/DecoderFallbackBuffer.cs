using System.Globalization;

namespace System.Text
{
	public abstract class DecoderFallbackBuffer
	{
		internal unsafe byte* byteStart = null;

		internal unsafe char* charEnd = null;

		public abstract int Remaining
		{
			get;
		}

		public abstract bool Fallback(byte[] bytesUnknown, int index);

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
			byteStart = null;
			Reset();
		}

		internal unsafe void InternalInitialize(byte* byteStart, char* charEnd)
		{
			this.byteStart = byteStart;
			this.charEnd = charEnd;
		}

		internal unsafe virtual bool InternalFallback(byte[] bytes, byte* pBytes, ref char* chars)
		{
			if (Fallback(bytes, (int)(pBytes - byteStart - bytes.Length)))
			{
				char* ptr = chars;
				bool flag = false;
				char nextChar;
				while ((nextChar = GetNextChar()) != 0)
				{
					if (char.IsSurrogate(nextChar))
					{
						if (char.IsHighSurrogate(nextChar))
						{
							if (flag)
							{
								throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
							}
							flag = true;
						}
						else
						{
							if (!flag)
							{
								throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
							}
							flag = false;
						}
					}
					if (ptr >= charEnd)
					{
						return false;
					}
					char* num = ptr;
					ptr = num + 1;
					*num = nextChar;
				}
				if (flag)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
				}
				chars = ptr;
			}
			return true;
		}

		internal unsafe virtual int InternalFallback(byte[] bytes, byte* pBytes)
		{
			if (Fallback(bytes, (int)(pBytes - byteStart - bytes.Length)))
			{
				int num = 0;
				bool flag = false;
				char nextChar;
				while ((nextChar = GetNextChar()) != 0)
				{
					if (char.IsSurrogate(nextChar))
					{
						if (char.IsHighSurrogate(nextChar))
						{
							if (flag)
							{
								throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
							}
							flag = true;
						}
						else
						{
							if (!flag)
							{
								throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
							}
							flag = false;
						}
					}
					num++;
				}
				if (flag)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
				}
				return num;
			}
			return 0;
		}

		internal void ThrowLastBytesRecursive(byte[] bytesUnknown)
		{
			StringBuilder stringBuilder = new StringBuilder(bytesUnknown.Length * 3);
			int i;
			for (i = 0; i < bytesUnknown.Length && i < 20; i++)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "\\x{0:X2}", bytesUnknown[i]));
			}
			if (i == 20)
			{
				stringBuilder.Append(" ...");
			}
			throw new ArgumentException(Environment.GetResourceString("Argument_RecursiveFallbackBytes", stringBuilder.ToString()), "bytesUnknown");
		}
	}
}
