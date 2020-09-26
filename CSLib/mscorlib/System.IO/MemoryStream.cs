using System.Runtime.InteropServices;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public class MemoryStream : Stream
	{
		private const int MemStreamMaxLength = int.MaxValue;

		private byte[] _buffer;

		private int _origin;

		private int _position;

		private int _length;

		private int _capacity;

		private bool _expandable;

		private bool _writable;

		private bool _exposable;

		private bool _isOpen;

		public override bool CanRead => _isOpen;

		public override bool CanSeek => _isOpen;

		public override bool CanWrite => _writable;

		public virtual int Capacity
		{
			get
			{
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				return _capacity - _origin;
			}
			set
			{
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				if (value == _capacity)
				{
					return;
				}
				if (!_expandable)
				{
					__Error.MemoryStreamNotExpandable();
				}
				if (value < _length)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
				}
				if (value > 0)
				{
					byte[] array = new byte[value];
					if (_length > 0)
					{
						Buffer.InternalBlockCopy(_buffer, 0, array, 0, _length);
					}
					_buffer = array;
				}
				else
				{
					_buffer = null;
				}
				_capacity = value;
			}
		}

		public override long Length
		{
			get
			{
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				return _length - _origin;
			}
		}

		public override long Position
		{
			get
			{
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				return _position - _origin;
			}
			set
			{
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (value > int.MaxValue)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength"));
				}
				_position = _origin + (int)value;
			}
		}

		public MemoryStream()
			: this(0)
		{
		}

		public MemoryStream(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_NegativeCapacity"));
			}
			_buffer = new byte[capacity];
			_capacity = capacity;
			_expandable = true;
			_writable = true;
			_exposable = true;
			_origin = 0;
			_isOpen = true;
		}

		public MemoryStream(byte[] buffer)
			: this(buffer, writable: true)
		{
		}

		public MemoryStream(byte[] buffer, bool writable)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			_buffer = buffer;
			_length = (_capacity = buffer.Length);
			_writable = writable;
			_exposable = false;
			_origin = 0;
			_isOpen = true;
		}

		public MemoryStream(byte[] buffer, int index, int count)
			: this(buffer, index, count, writable: true, publiclyVisible: false)
		{
		}

		public MemoryStream(byte[] buffer, int index, int count, bool writable)
			: this(buffer, index, count, writable, publiclyVisible: false)
		{
		}

		public MemoryStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible)
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
			_buffer = buffer;
			_origin = (_position = index);
			_length = (_capacity = index + count);
			_writable = writable;
			_exposable = publiclyVisible;
			_expandable = false;
			_isOpen = true;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					_isOpen = false;
					_writable = false;
					_expandable = false;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		private bool EnsureCapacity(int value)
		{
			if (value < 0)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_StreamTooLong"));
			}
			if (value > _capacity)
			{
				int num = value;
				if (num < 256)
				{
					num = 256;
				}
				if (num < _capacity * 2)
				{
					num = _capacity * 2;
				}
				Capacity = num;
				return true;
			}
			return false;
		}

		public override void Flush()
		{
		}

		public virtual byte[] GetBuffer()
		{
			if (!_exposable)
			{
				throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_MemStreamBuffer"));
			}
			return _buffer;
		}

		internal byte[] InternalGetBuffer()
		{
			return _buffer;
		}

		internal void InternalGetOriginAndLength(out int origin, out int length)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			origin = _origin;
			length = _length;
		}

		internal int InternalGetPosition()
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			return _position;
		}

		internal int InternalReadInt32()
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			int num = (_position += 4);
			if (num > _length)
			{
				_position = _length;
				__Error.EndOfFile();
			}
			return _buffer[num - 4] | (_buffer[num - 3] << 8) | (_buffer[num - 2] << 16) | (_buffer[num - 1] << 24);
		}

		internal int InternalEmulateRead(int count)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			int num = _length - _position;
			if (num > count)
			{
				num = count;
			}
			if (num < 0)
			{
				num = 0;
			}
			_position += num;
			return num;
		}

		public override int Read([In][Out] byte[] buffer, int offset, int count)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (buffer.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			int num = _length - _position;
			if (num > count)
			{
				num = count;
			}
			if (num <= 0)
			{
				return 0;
			}
			if (num <= 8)
			{
				int num2 = num;
				while (--num2 >= 0)
				{
					buffer[offset + num2] = _buffer[_position + num2];
				}
			}
			else
			{
				Buffer.InternalBlockCopy(_buffer, _position, buffer, offset, num);
			}
			_position += num;
			return num;
		}

		public override int ReadByte()
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (_position >= _length)
			{
				return -1;
			}
			return _buffer[_position++];
		}

		public override long Seek(long offset, SeekOrigin loc)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (offset > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength"));
			}
			switch (loc)
			{
			case SeekOrigin.Begin:
				if (offset < 0)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
				}
				_position = _origin + (int)offset;
				break;
			case SeekOrigin.Current:
				if (offset + _position < _origin)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
				}
				_position += (int)offset;
				break;
			case SeekOrigin.End:
				if (_length + offset < _origin)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
				}
				_position = _length + (int)offset;
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSeekOrigin"));
			}
			return _position;
		}

		public override void SetLength(long value)
		{
			if (!_writable)
			{
				__Error.WriteNotSupported();
			}
			if (value > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength"));
			}
			if (value < 0 || value > int.MaxValue - _origin)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength"));
			}
			int num = _origin + (int)value;
			if (!EnsureCapacity(num) && num > _length)
			{
				Array.Clear(_buffer, _length, num - _length);
			}
			_length = num;
			if (_position > num)
			{
				_position = num;
			}
		}

		public virtual byte[] ToArray()
		{
			byte[] array = new byte[_length - _origin];
			Buffer.InternalBlockCopy(_buffer, _origin, array, 0, _length - _origin);
			return array;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (!_writable)
			{
				__Error.WriteNotSupported();
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (buffer.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			int num = _position + count;
			if (num < 0)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_StreamTooLong"));
			}
			if (num > _length)
			{
				bool flag = _position > _length;
				if (num > _capacity && EnsureCapacity(num))
				{
					flag = false;
				}
				if (flag)
				{
					Array.Clear(_buffer, _length, num - _length);
				}
				_length = num;
			}
			if (count <= 8)
			{
				int num2 = count;
				while (--num2 >= 0)
				{
					_buffer[_position + num2] = buffer[offset + num2];
				}
			}
			else
			{
				Buffer.InternalBlockCopy(buffer, offset, _buffer, _position, count);
			}
			_position = num;
		}

		public override void WriteByte(byte value)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (!_writable)
			{
				__Error.WriteNotSupported();
			}
			if (_position >= _length)
			{
				int num = _position + 1;
				bool flag = _position > _length;
				if (num >= _capacity && EnsureCapacity(num))
				{
					flag = false;
				}
				if (flag)
				{
					Array.Clear(_buffer, _length, _position - _length);
				}
				_length = num;
			}
			_buffer[_position++] = value;
		}

		public virtual void WriteTo(Stream stream)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (stream == null)
			{
				throw new ArgumentNullException("stream", Environment.GetResourceString("ArgumentNull_Stream"));
			}
			stream.Write(_buffer, _origin, _length - _origin);
		}
	}
}
