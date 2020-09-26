using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public class StreamWriter : TextWriter
	{
		private const int DefaultBufferSize = 1024;

		private const int DefaultFileStreamBufferSize = 4096;

		private const int MinBufferSize = 128;

		public new static readonly StreamWriter Null = new StreamWriter(Stream.Null, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), 128, closeable: false);

		internal Stream stream;

		private Encoding encoding;

		private Encoder encoder;

		internal byte[] byteBuffer;

		internal char[] charBuffer;

		internal int charPos;

		internal int charLen;

		internal bool autoFlush;

		private bool haveWrittenPreamble;

		private bool closable;

		[NonSerialized]
		private MdaHelper mdaHelper;

		private static Encoding _UTF8NoBOM;

		internal static Encoding UTF8NoBOM
		{
			get
			{
				if (_UTF8NoBOM == null)
				{
					UTF8Encoding uTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
					Thread.MemoryBarrier();
					_UTF8NoBOM = uTF8NoBOM;
				}
				return _UTF8NoBOM;
			}
		}

		public virtual bool AutoFlush
		{
			get
			{
				return autoFlush;
			}
			set
			{
				autoFlush = value;
				if (value)
				{
					Flush(flushStream: true, flushEncoder: false);
				}
			}
		}

		public virtual Stream BaseStream => stream;

		internal bool Closable => closable;

		internal bool HaveWrittenPreamble
		{
			set
			{
				haveWrittenPreamble = value;
			}
		}

		public override Encoding Encoding => encoding;

		internal StreamWriter()
			: base(null)
		{
		}

		public StreamWriter(Stream stream)
			: this(stream, UTF8NoBOM, 1024)
		{
		}

		public StreamWriter(Stream stream, Encoding encoding)
			: this(stream, encoding, 1024)
		{
		}

		public StreamWriter(Stream stream, Encoding encoding, int bufferSize)
			: base(null)
		{
			if (stream == null || encoding == null)
			{
				throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
			}
			if (!stream.CanWrite)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotWritable"));
			}
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			Init(stream, encoding, bufferSize);
		}

		internal StreamWriter(Stream stream, Encoding encoding, int bufferSize, bool closeable)
			: this(stream, encoding, bufferSize)
		{
			closable = closeable;
		}

		public StreamWriter(string path)
			: this(path, append: false, UTF8NoBOM, 1024)
		{
		}

		public StreamWriter(string path, bool append)
			: this(path, append, UTF8NoBOM, 1024)
		{
		}

		public StreamWriter(string path, bool append, Encoding encoding)
			: this(path, append, encoding, 1024)
		{
		}

		public StreamWriter(string path, bool append, Encoding encoding, int bufferSize)
			: base(null)
		{
			if (path == null || encoding == null)
			{
				throw new ArgumentNullException((path == null) ? "path" : "encoding");
			}
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			Stream stream = CreateFile(path, append);
			Init(stream, encoding, bufferSize);
		}

		private void Init(Stream stream, Encoding encoding, int bufferSize)
		{
			this.stream = stream;
			this.encoding = encoding;
			encoder = encoding.GetEncoder();
			if (bufferSize < 128)
			{
				bufferSize = 128;
			}
			charBuffer = new char[bufferSize];
			byteBuffer = new byte[encoding.GetMaxByteCount(bufferSize)];
			charLen = bufferSize;
			if (stream.CanSeek && stream.Position > 0)
			{
				haveWrittenPreamble = true;
			}
			closable = true;
			if (Mda.StreamWriterBufferMDAEnabled)
			{
				string stackTrace = Environment.GetStackTrace(null, needFileInfo: false);
				mdaHelper = new MdaHelper(this, stackTrace);
			}
		}

		private static Stream CreateFile(string path, bool append)
		{
			FileMode mode = (append ? FileMode.Append : FileMode.Create);
			return new FileStream(path, mode, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan);
		}

		public override void Close()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (stream != null && (disposing || (!Closable && stream is __ConsoleStream)))
				{
					Flush(flushStream: true, flushEncoder: true);
					if (mdaHelper != null)
					{
						GC.SuppressFinalize(mdaHelper);
					}
				}
			}
			finally
			{
				if (Closable && stream != null)
				{
					try
					{
						if (disposing)
						{
							stream.Close();
						}
					}
					finally
					{
						stream = null;
						byteBuffer = null;
						charBuffer = null;
						encoding = null;
						encoder = null;
						charLen = 0;
						base.Dispose(disposing);
					}
				}
			}
		}

		public override void Flush()
		{
			Flush(flushStream: true, flushEncoder: true);
		}

		private void Flush(bool flushStream, bool flushEncoder)
		{
			if (stream == null)
			{
				__Error.WriterClosed();
			}
			if (charPos == 0 && !flushStream && !flushEncoder)
			{
				return;
			}
			if (!haveWrittenPreamble)
			{
				haveWrittenPreamble = true;
				byte[] preamble = encoding.GetPreamble();
				if (preamble.Length > 0)
				{
					stream.Write(preamble, 0, preamble.Length);
				}
			}
			int bytes = encoder.GetBytes(charBuffer, 0, charPos, byteBuffer, 0, flushEncoder);
			charPos = 0;
			if (bytes > 0)
			{
				stream.Write(byteBuffer, 0, bytes);
			}
			if (flushStream)
			{
				stream.Flush();
			}
		}

		public override void Write(char value)
		{
			if (charPos == charLen)
			{
				Flush(flushStream: false, flushEncoder: false);
			}
			charBuffer[charPos] = value;
			charPos++;
			if (autoFlush)
			{
				Flush(flushStream: true, flushEncoder: false);
			}
		}

		public override void Write(char[] buffer)
		{
			if (buffer == null)
			{
				return;
			}
			int num = 0;
			int num2 = buffer.Length;
			while (num2 > 0)
			{
				if (charPos == charLen)
				{
					Flush(flushStream: false, flushEncoder: false);
				}
				int num3 = charLen - charPos;
				if (num3 > num2)
				{
					num3 = num2;
				}
				Buffer.InternalBlockCopy(buffer, num * 2, charBuffer, charPos * 2, num3 * 2);
				charPos += num3;
				num += num3;
				num2 -= num3;
			}
			if (autoFlush)
			{
				Flush(flushStream: true, flushEncoder: false);
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (buffer.Length - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			while (count > 0)
			{
				if (charPos == charLen)
				{
					Flush(flushStream: false, flushEncoder: false);
				}
				int num = charLen - charPos;
				if (num > count)
				{
					num = count;
				}
				Buffer.InternalBlockCopy(buffer, index * 2, charBuffer, charPos * 2, num * 2);
				charPos += num;
				index += num;
				count -= num;
			}
			if (autoFlush)
			{
				Flush(flushStream: true, flushEncoder: false);
			}
		}

		public override void Write(string value)
		{
			if (value == null)
			{
				return;
			}
			int num = value.Length;
			int num2 = 0;
			while (num > 0)
			{
				if (charPos == charLen)
				{
					Flush(flushStream: false, flushEncoder: false);
				}
				int num3 = charLen - charPos;
				if (num3 > num)
				{
					num3 = num;
				}
				value.CopyTo(num2, charBuffer, charPos, num3);
				charPos += num3;
				num2 += num3;
				num -= num3;
			}
			if (autoFlush)
			{
				Flush(flushStream: true, flushEncoder: false);
			}
		}
	}
}
