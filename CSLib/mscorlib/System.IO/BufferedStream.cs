using System.Globalization;
using System.Runtime.InteropServices;

namespace System.IO
{
	[ComVisible(true)]
	public sealed class BufferedStream : Stream
	{
		private const int _DefaultBufferSize = 4096;

		private Stream _s;

		private byte[] _buffer;

		private int _readPos;

		private int _readLen;

		private int _writePos;

		private int _bufferSize;

		public override bool CanRead
		{
			get
			{
				if (_s != null)
				{
					return _s.CanRead;
				}
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				if (_s != null)
				{
					return _s.CanWrite;
				}
				return false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				if (_s != null)
				{
					return _s.CanSeek;
				}
				return false;
			}
		}

		public override long Length
		{
			get
			{
				if (_s == null)
				{
					__Error.StreamIsClosed();
				}
				if (_writePos > 0)
				{
					FlushWrite();
				}
				return _s.Length;
			}
		}

		public override long Position
		{
			get
			{
				if (_s == null)
				{
					__Error.StreamIsClosed();
				}
				if (!_s.CanSeek)
				{
					__Error.SeekNotSupported();
				}
				return _s.Position + (_readPos - _readLen + _writePos);
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_s == null)
				{
					__Error.StreamIsClosed();
				}
				if (!_s.CanSeek)
				{
					__Error.SeekNotSupported();
				}
				if (_writePos > 0)
				{
					FlushWrite();
				}
				_readPos = 0;
				_readLen = 0;
				_s.Seek(value, SeekOrigin.Begin);
			}
		}

		private BufferedStream()
		{
		}

		public BufferedStream(Stream stream)
			: this(stream, 4096)
		{
		}

		public BufferedStream(Stream stream, int bufferSize)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_MustBePositive"), "bufferSize"));
			}
			_s = stream;
			_bufferSize = bufferSize;
			if (!_s.CanRead && !_s.CanWrite)
			{
				__Error.StreamIsClosed();
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _s != null)
				{
					try
					{
						Flush();
					}
					finally
					{
						_s.Close();
					}
				}
			}
			finally
			{
				_s = null;
				_buffer = null;
				base.Dispose(disposing);
			}
		}

		public override void Flush()
		{
			if (_s == null)
			{
				__Error.StreamIsClosed();
			}
			if (_writePos > 0)
			{
				FlushWrite();
			}
			else if (_readPos < _readLen && _s.CanSeek)
			{
				FlushRead();
			}
			_readPos = 0;
			_readLen = 0;
		}

		private void FlushRead()
		{
			if (_readPos - _readLen != 0)
			{
				_s.Seek(_readPos - _readLen, SeekOrigin.Current);
			}
			_readPos = 0;
			_readLen = 0;
		}

		private void FlushWrite()
		{
			_s.Write(_buffer, 0, _writePos);
			_writePos = 0;
			_s.Flush();
		}

		public override int Read([In][Out] byte[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (_s == null)
			{
				__Error.StreamIsClosed();
			}
			int num = _readLen - _readPos;
			if (num == 0)
			{
				if (!_s.CanRead)
				{
					__Error.ReadNotSupported();
				}
				if (_writePos > 0)
				{
					FlushWrite();
				}
				if (count >= _bufferSize)
				{
					num = _s.Read(array, offset, count);
					_readPos = 0;
					_readLen = 0;
					return num;
				}
				if (_buffer == null)
				{
					_buffer = new byte[_bufferSize];
				}
				num = _s.Read(_buffer, 0, _bufferSize);
				if (num == 0)
				{
					return 0;
				}
				_readPos = 0;
				_readLen = num;
			}
			if (num > count)
			{
				num = count;
			}
			Buffer.InternalBlockCopy(_buffer, _readPos, array, offset, num);
			_readPos += num;
			if (num < count)
			{
				int num2 = _s.Read(array, offset + num, count - num);
				num += num2;
				_readPos = 0;
				_readLen = 0;
			}
			return num;
		}

		public override int ReadByte()
		{
			if (_s == null)
			{
				__Error.StreamIsClosed();
			}
			if (_readLen == 0 && !_s.CanRead)
			{
				__Error.ReadNotSupported();
			}
			if (_readPos == _readLen)
			{
				if (_writePos > 0)
				{
					FlushWrite();
				}
				if (_buffer == null)
				{
					_buffer = new byte[_bufferSize];
				}
				_readLen = _s.Read(_buffer, 0, _bufferSize);
				_readPos = 0;
			}
			if (_readPos == _readLen)
			{
				return -1;
			}
			return _buffer[_readPos++];
		}

		public override void Write(byte[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (_s == null)
			{
				__Error.StreamIsClosed();
			}
			if (_writePos == 0)
			{
				if (!_s.CanWrite)
				{
					__Error.WriteNotSupported();
				}
				if (_readPos < _readLen)
				{
					FlushRead();
				}
				else
				{
					_readPos = 0;
					_readLen = 0;
				}
			}
			if (_writePos > 0)
			{
				int num = _bufferSize - _writePos;
				if (num > 0)
				{
					if (num > count)
					{
						num = count;
					}
					Buffer.InternalBlockCopy(array, offset, _buffer, _writePos, num);
					_writePos += num;
					if (count == num)
					{
						return;
					}
					offset += num;
					count -= num;
				}
				_s.Write(_buffer, 0, _writePos);
				_writePos = 0;
			}
			if (count >= _bufferSize)
			{
				_s.Write(array, offset, count);
			}
			else if (count != 0)
			{
				if (_buffer == null)
				{
					_buffer = new byte[_bufferSize];
				}
				Buffer.InternalBlockCopy(array, offset, _buffer, 0, count);
				_writePos = count;
			}
		}

		public override void WriteByte(byte value)
		{
			if (_s == null)
			{
				__Error.StreamIsClosed();
			}
			if (_writePos == 0)
			{
				if (!_s.CanWrite)
				{
					__Error.WriteNotSupported();
				}
				if (_readPos < _readLen)
				{
					FlushRead();
				}
				else
				{
					_readPos = 0;
					_readLen = 0;
				}
				if (_buffer == null)
				{
					_buffer = new byte[_bufferSize];
				}
			}
			if (_writePos == _bufferSize)
			{
				FlushWrite();
			}
			_buffer[_writePos++] = value;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (_s == null)
			{
				__Error.StreamIsClosed();
			}
			if (!_s.CanSeek)
			{
				__Error.SeekNotSupported();
			}
			if (_writePos > 0)
			{
				FlushWrite();
			}
			else if (origin == SeekOrigin.Current)
			{
				offset -= _readLen - _readPos;
			}
			long num = _s.Position + (_readPos - _readLen);
			long num2 = _s.Seek(offset, origin);
			if (_readLen > 0)
			{
				if (num == num2)
				{
					if (_readPos > 0)
					{
						Buffer.InternalBlockCopy(_buffer, _readPos, _buffer, 0, _readLen - _readPos);
						_readLen -= _readPos;
						_readPos = 0;
					}
					if (_readLen > 0)
					{
						_s.Seek(_readLen, SeekOrigin.Current);
					}
				}
				else if (num - _readPos < num2 && num2 < num + _readLen - _readPos)
				{
					int num3 = (int)(num2 - num);
					Buffer.InternalBlockCopy(_buffer, _readPos + num3, _buffer, 0, _readLen - (_readPos + num3));
					_readLen -= _readPos + num3;
					_readPos = 0;
					if (_readLen > 0)
					{
						_s.Seek(_readLen, SeekOrigin.Current);
					}
				}
				else
				{
					_readPos = 0;
					_readLen = 0;
				}
			}
			return num2;
		}

		public override void SetLength(long value)
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NegFileSize"));
			}
			if (_s == null)
			{
				__Error.StreamIsClosed();
			}
			if (!_s.CanSeek)
			{
				__Error.SeekNotSupported();
			}
			if (!_s.CanWrite)
			{
				__Error.WriteNotSupported();
			}
			if (_writePos > 0)
			{
				FlushWrite();
			}
			else if (_readPos < _readLen)
			{
				FlushRead();
			}
			_readPos = 0;
			_readLen = 0;
			_s.SetLength(value);
		}
	}
}
