using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.IO
{
	[CLSCompliant(false)]
	public class UnmanagedMemoryStream : Stream
	{
		private const long UnmanagedMemStreamMaxLength = long.MaxValue;

		private unsafe byte* _mem;

		private long _length;

		private long _capacity;

		private long _position;

		private FileAccess _access;

		internal bool _isOpen;

		public override bool CanRead
		{
			get
			{
				if (_isOpen)
				{
					return (_access & FileAccess.Read) != 0;
				}
				return false;
			}
		}

		public override bool CanSeek => _isOpen;

		public override bool CanWrite
		{
			get
			{
				if (_isOpen)
				{
					return (_access & FileAccess.Write) != 0;
				}
				return false;
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
				return _length;
			}
		}

		public long Capacity
		{
			get
			{
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				return _capacity;
			}
		}

		public unsafe override long Position
		{
			get
			{
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				return _position;
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
				if (value > int.MaxValue || _mem + value < _mem)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength"));
				}
				_position = value;
			}
		}

		public unsafe byte* PositionPointer
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				long position = _position;
				if (position > _capacity)
				{
					throw new IndexOutOfRangeException(Environment.GetResourceString("IndexOutOfRange_UMSPosition"));
				}
				byte* result = _mem + position;
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				return result;
			}
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				if (!_isOpen)
				{
					__Error.StreamIsClosed();
				}
				if (new IntPtr(value - _mem).ToInt64() > long.MaxValue)
				{
					throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_UnmanagedMemStreamLength"));
				}
				if (value < _mem)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
				}
				_position = value - _mem;
			}
		}

		internal unsafe byte* Pointer => _mem;

		protected unsafe UnmanagedMemoryStream()
		{
			_mem = null;
			_isOpen = false;
		}

		public unsafe UnmanagedMemoryStream(byte* pointer, long length)
		{
			Initialize(pointer, length, length, FileAccess.Read, skipSecurityCheck: false);
		}

		public unsafe UnmanagedMemoryStream(byte* pointer, long length, long capacity, FileAccess access)
		{
			Initialize(pointer, length, capacity, access, skipSecurityCheck: false);
		}

		internal unsafe UnmanagedMemoryStream(byte* pointer, long length, long capacity, FileAccess access, bool skipSecurityCheck)
		{
			Initialize(pointer, length, capacity, access, skipSecurityCheck);
		}

		protected unsafe void Initialize(byte* pointer, long length, long capacity, FileAccess access)
		{
			Initialize(pointer, length, capacity, access, skipSecurityCheck: false);
		}

		internal unsafe void Initialize(byte* pointer, long length, long capacity, FileAccess access, bool skipSecurityCheck)
		{
			if (pointer == null)
			{
				throw new ArgumentNullException("pointer");
			}
			if (length < 0 || capacity < 0)
			{
				throw new ArgumentOutOfRangeException((length < 0) ? "length" : "capacity", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (length > capacity)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_LengthGreaterThanCapacity"));
			}
			if ((nuint)((long)pointer + capacity) < (nuint)pointer)
			{
				throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_UnmanagedMemStreamWrapAround"));
			}
			if (access < FileAccess.Read || access > FileAccess.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("access", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
			}
			if (_isOpen)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CalledTwice"));
			}
			if (!skipSecurityCheck)
			{
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			}
			_mem = pointer;
			_length = length;
			_capacity = capacity;
			_access = access;
			_isOpen = true;
		}

		protected override void Dispose(bool disposing)
		{
			_isOpen = false;
			base.Dispose(disposing);
		}

		public override void Flush()
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
		}

		public unsafe override int Read([In][Out] byte[] buffer, int offset, int count)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if ((_access & FileAccess.Read) == 0)
			{
				__Error.ReadNotSupported();
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
			long position = _position;
			long num = _length - position;
			if (num > count)
			{
				num = count;
			}
			if (num <= 0)
			{
				return 0;
			}
			int num2 = (int)num;
			if (num2 < 0)
			{
				num2 = 0;
			}
			Buffer.memcpy(_mem + position, 0, buffer, offset, num2);
			_position = position + num;
			return num2;
		}

		public unsafe override int ReadByte()
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if ((_access & FileAccess.Read) == 0)
			{
				__Error.ReadNotSupported();
			}
			long position = _position;
			if (position >= _length)
			{
				return -1;
			}
			_position = position + 1;
			return _mem[position];
		}

		public override long Seek(long offset, SeekOrigin loc)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (offset > long.MaxValue)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_UnmanagedMemStreamLength"));
			}
			switch (loc)
			{
			case SeekOrigin.Begin:
				if (offset < 0)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
				}
				_position = offset;
				break;
			case SeekOrigin.Current:
				if (offset + _position < 0)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
				}
				_position += offset;
				break;
			case SeekOrigin.End:
				if (_length + offset < 0)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
				}
				_position = _length + offset;
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSeekOrigin"));
			}
			return _position;
		}

		public unsafe override void SetLength(long value)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if ((_access & FileAccess.Write) == 0)
			{
				__Error.WriteNotSupported();
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (value > _capacity)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_FixedCapacity"));
			}
			long length = _length;
			if (value > length)
			{
				Buffer.ZeroMemory(_mem + length, value - length);
			}
			_length = value;
			if (_position > value)
			{
				_position = value;
			}
		}

		public unsafe override void Write(byte[] buffer, int offset, int count)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if ((_access & FileAccess.Write) == 0)
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
			long position = _position;
			long length = _length;
			long num = position + count;
			if (num < 0)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_StreamTooLong"));
			}
			if (num > length)
			{
				if (num > _capacity)
				{
					throw new NotSupportedException(Environment.GetResourceString("IO.IO_FixedCapacity"));
				}
				_length = num;
			}
			if (position > length)
			{
				Buffer.ZeroMemory(_mem + length, position - length);
			}
			Buffer.memcpy(buffer, offset, _mem + position, 0, count);
			_position = num;
		}

		public unsafe override void WriteByte(byte value)
		{
			if (!_isOpen)
			{
				__Error.StreamIsClosed();
			}
			if ((_access & FileAccess.Write) == 0)
			{
				__Error.WriteNotSupported();
			}
			long position = _position;
			long length = _length;
			long num = position + 1;
			if (position >= length)
			{
				if (num < 0)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_StreamTooLong"));
				}
				if (num > _capacity)
				{
					throw new NotSupportedException(Environment.GetResourceString("IO.IO_FixedCapacity"));
				}
				_length = num;
				if (position > length)
				{
					Buffer.ZeroMemory(_mem + length, position - length);
				}
			}
			_mem[position] = value;
			_position = num;
		}
	}
}
