using System.Runtime.InteropServices;
using System.Text;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public class StreamReader : TextReader
	{
		private class NullStreamReader : StreamReader
		{
			public override Stream BaseStream => Stream.Null;

			public override Encoding CurrentEncoding => Encoding.Unicode;

			internal NullStreamReader()
				: base(Stream.Null, Encoding.Unicode, detectEncodingFromByteOrderMarks: false, 1)
			{
			}

			protected override void Dispose(bool disposing)
			{
			}

			public override int Peek()
			{
				return -1;
			}

			public override int Read()
			{
				return -1;
			}

			public override int Read(char[] buffer, int index, int count)
			{
				return 0;
			}

			public override string ReadLine()
			{
				return null;
			}

			public override string ReadToEnd()
			{
				return string.Empty;
			}
		}

		internal const int DefaultBufferSize = 1024;

		private const int DefaultFileStreamBufferSize = 4096;

		private const int MinBufferSize = 128;

		public new static readonly StreamReader Null = new NullStreamReader();

		private bool _closable;

		private Stream stream;

		private Encoding encoding;

		private Decoder decoder;

		private byte[] byteBuffer;

		private char[] charBuffer;

		private byte[] _preamble;

		private int charPos;

		private int charLen;

		private int byteLen;

		private int bytePos;

		private int _maxCharsPerBuffer;

		private bool _detectEncoding;

		private bool _checkPreamble;

		private bool _isBlocked;

		public virtual Encoding CurrentEncoding => encoding;

		public virtual Stream BaseStream => stream;

		internal bool Closable => _closable;

		public bool EndOfStream
		{
			get
			{
				if (stream == null)
				{
					__Error.ReaderClosed();
				}
				if (charPos < charLen)
				{
					return false;
				}
				int num = ReadBuffer();
				return num == 0;
			}
		}

		internal StreamReader()
		{
		}

		public StreamReader(Stream stream)
			: this(stream, detectEncodingFromByteOrderMarks: true)
		{
		}

		public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
			: this(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, 1024)
		{
		}

		public StreamReader(Stream stream, Encoding encoding)
			: this(stream, encoding, detectEncodingFromByteOrderMarks: true, 1024)
		{
		}

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this(stream, encoding, detectEncodingFromByteOrderMarks, 1024)
		{
		}

		public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			if (stream == null || encoding == null)
			{
				throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotReadable"));
			}
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
		}

		internal StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool closable)
			: this(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
		{
			_closable = closable;
		}

		public StreamReader(string path)
			: this(path, detectEncodingFromByteOrderMarks: true)
		{
		}

		public StreamReader(string path, bool detectEncodingFromByteOrderMarks)
			: this(path, Encoding.UTF8, detectEncodingFromByteOrderMarks, 1024)
		{
		}

		public StreamReader(string path, Encoding encoding)
			: this(path, encoding, detectEncodingFromByteOrderMarks: true, 1024)
		{
		}

		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
			: this(path, encoding, detectEncodingFromByteOrderMarks, 1024)
		{
		}

		public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			if (path == null || encoding == null)
			{
				throw new ArgumentNullException((path == null) ? "path" : "encoding");
			}
			if (path.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
			}
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
			Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
		}

		private void Init(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		{
			this.stream = stream;
			this.encoding = encoding;
			decoder = encoding.GetDecoder();
			if (bufferSize < 128)
			{
				bufferSize = 128;
			}
			byteBuffer = new byte[bufferSize];
			_maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
			charBuffer = new char[_maxCharsPerBuffer];
			byteLen = 0;
			bytePos = 0;
			_detectEncoding = detectEncodingFromByteOrderMarks;
			_preamble = encoding.GetPreamble();
			_checkPreamble = _preamble.Length > 0;
			_isBlocked = false;
			_closable = true;
		}

		public override void Close()
		{
			Dispose(disposing: true);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (Closable && disposing && stream != null)
				{
					stream.Close();
				}
			}
			finally
			{
				if (Closable && stream != null)
				{
					stream = null;
					encoding = null;
					decoder = null;
					byteBuffer = null;
					charBuffer = null;
					charPos = 0;
					charLen = 0;
					base.Dispose(disposing);
				}
			}
		}

		public void DiscardBufferedData()
		{
			byteLen = 0;
			charLen = 0;
			charPos = 0;
			decoder = encoding.GetDecoder();
			_isBlocked = false;
		}

		public override int Peek()
		{
			if (stream == null)
			{
				__Error.ReaderClosed();
			}
			if (charPos == charLen && (_isBlocked || ReadBuffer() == 0))
			{
				return -1;
			}
			return charBuffer[charPos];
		}

		public override int Read()
		{
			if (stream == null)
			{
				__Error.ReaderClosed();
			}
			if (charPos == charLen && ReadBuffer() == 0)
			{
				return -1;
			}
			int result = charBuffer[charPos];
			charPos++;
			return result;
		}

		public override int Read([In][Out] char[] buffer, int index, int count)
		{
			if (stream == null)
			{
				__Error.ReaderClosed();
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (buffer.Length - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			int num = 0;
			bool readToUserBuffer = false;
			while (count > 0)
			{
				int num2 = charLen - charPos;
				if (num2 == 0)
				{
					num2 = ReadBuffer(buffer, index + num, count, out readToUserBuffer);
				}
				if (num2 == 0)
				{
					break;
				}
				if (num2 > count)
				{
					num2 = count;
				}
				if (!readToUserBuffer)
				{
					Buffer.InternalBlockCopy(charBuffer, charPos * 2, buffer, (index + num) * 2, num2 * 2);
					charPos += num2;
				}
				num += num2;
				count -= num2;
				if (_isBlocked)
				{
					break;
				}
			}
			return num;
		}

		public override string ReadToEnd()
		{
			if (stream == null)
			{
				__Error.ReaderClosed();
			}
			StringBuilder stringBuilder = new StringBuilder(charLen - charPos);
			do
			{
				stringBuilder.Append(charBuffer, charPos, charLen - charPos);
				charPos = charLen;
				ReadBuffer();
			}
			while (charLen > 0);
			return stringBuilder.ToString();
		}

		private void CompressBuffer(int n)
		{
			Buffer.InternalBlockCopy(byteBuffer, n, byteBuffer, 0, byteLen - n);
			byteLen -= n;
		}

		private void DetectEncoding()
		{
			if (byteLen < 2)
			{
				return;
			}
			_detectEncoding = false;
			bool flag = false;
			if (byteBuffer[0] == 254 && byteBuffer[1] == byte.MaxValue)
			{
				encoding = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
				CompressBuffer(2);
				flag = true;
			}
			else if (byteBuffer[0] == byte.MaxValue && byteBuffer[1] == 254)
			{
				if (byteLen >= 4 && byteBuffer[2] == 0 && byteBuffer[3] == 0)
				{
					encoding = new UTF32Encoding(bigEndian: false, byteOrderMark: true);
					CompressBuffer(4);
				}
				else
				{
					encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
					CompressBuffer(2);
				}
				flag = true;
			}
			else if (byteLen >= 3 && byteBuffer[0] == 239 && byteBuffer[1] == 187 && byteBuffer[2] == 191)
			{
				encoding = Encoding.UTF8;
				CompressBuffer(3);
				flag = true;
			}
			else if (byteLen >= 4 && byteBuffer[0] == 0 && byteBuffer[1] == 0 && byteBuffer[2] == 254 && byteBuffer[3] == byte.MaxValue)
			{
				encoding = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
				flag = true;
			}
			else if (byteLen == 2)
			{
				_detectEncoding = true;
			}
			if (flag)
			{
				decoder = encoding.GetDecoder();
				_maxCharsPerBuffer = encoding.GetMaxCharCount(byteBuffer.Length);
				charBuffer = new char[_maxCharsPerBuffer];
			}
		}

		private bool IsPreamble()
		{
			if (!_checkPreamble)
			{
				return _checkPreamble;
			}
			int num = ((byteLen >= _preamble.Length) ? (_preamble.Length - bytePos) : (byteLen - bytePos));
			int num2 = 0;
			while (num2 < num)
			{
				if (byteBuffer[bytePos] != _preamble[bytePos])
				{
					bytePos = 0;
					_checkPreamble = false;
					break;
				}
				num2++;
				bytePos++;
			}
			if (_checkPreamble && bytePos == _preamble.Length)
			{
				CompressBuffer(_preamble.Length);
				bytePos = 0;
				_checkPreamble = false;
				_detectEncoding = false;
			}
			return _checkPreamble;
		}

		private int ReadBuffer()
		{
			charLen = 0;
			charPos = 0;
			if (!_checkPreamble)
			{
				byteLen = 0;
			}
			do
			{
				if (_checkPreamble)
				{
					int num = stream.Read(byteBuffer, bytePos, byteBuffer.Length - bytePos);
					if (num == 0)
					{
						if (byteLen > 0)
						{
							charLen += decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charLen);
						}
						return charLen;
					}
					byteLen += num;
				}
				else
				{
					byteLen = stream.Read(byteBuffer, 0, byteBuffer.Length);
					if (byteLen == 0)
					{
						return charLen;
					}
				}
				_isBlocked = byteLen < byteBuffer.Length;
				if (!IsPreamble())
				{
					if (_detectEncoding && byteLen >= 2)
					{
						DetectEncoding();
					}
					charLen += decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charLen);
				}
			}
			while (charLen == 0);
			return charLen;
		}

		private int ReadBuffer(char[] userBuffer, int userOffset, int desiredChars, out bool readToUserBuffer)
		{
			charLen = 0;
			charPos = 0;
			if (!_checkPreamble)
			{
				byteLen = 0;
			}
			int num = 0;
			readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
			do
			{
				if (_checkPreamble)
				{
					int num2 = stream.Read(byteBuffer, bytePos, byteBuffer.Length - bytePos);
					if (num2 == 0)
					{
						if (byteLen > 0)
						{
							if (readToUserBuffer)
							{
								num += decoder.GetChars(byteBuffer, 0, byteLen, userBuffer, userOffset + num);
								charLen = 0;
							}
							else
							{
								num = decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, num);
								charLen += num;
							}
						}
						return num;
					}
					byteLen += num2;
				}
				else
				{
					byteLen = stream.Read(byteBuffer, 0, byteBuffer.Length);
					if (byteLen == 0)
					{
						return num;
					}
				}
				_isBlocked = byteLen < byteBuffer.Length;
				if (!IsPreamble())
				{
					if (_detectEncoding && byteLen >= 2)
					{
						DetectEncoding();
						readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
					}
					charPos = 0;
					if (readToUserBuffer)
					{
						num += decoder.GetChars(byteBuffer, 0, byteLen, userBuffer, userOffset + num);
						charLen = 0;
					}
					else
					{
						num = decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, num);
						charLen += num;
					}
				}
			}
			while (num == 0);
			_isBlocked &= num < desiredChars;
			return num;
		}

		public override string ReadLine()
		{
			if (stream == null)
			{
				__Error.ReaderClosed();
			}
			if (charPos == charLen && ReadBuffer() == 0)
			{
				return null;
			}
			StringBuilder stringBuilder = null;
			do
			{
				int num = charPos;
				do
				{
					char c = charBuffer[num];
					if (c == '\r' || c == '\n')
					{
						string result;
						if (stringBuilder != null)
						{
							stringBuilder.Append(charBuffer, charPos, num - charPos);
							result = stringBuilder.ToString();
						}
						else
						{
							result = new string(charBuffer, charPos, num - charPos);
						}
						charPos = num + 1;
						if (c == '\r' && (charPos < charLen || ReadBuffer() > 0) && charBuffer[charPos] == '\n')
						{
							charPos++;
						}
						return result;
					}
					num++;
				}
				while (num < charLen);
				num = charLen - charPos;
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(num + 80);
				}
				stringBuilder.Append(charBuffer, charPos, num);
			}
			while (ReadBuffer() > 0);
			return stringBuilder.ToString();
		}
	}
}
