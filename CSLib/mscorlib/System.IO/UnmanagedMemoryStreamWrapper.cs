using System.Runtime.InteropServices;

namespace System.IO
{
	internal sealed class UnmanagedMemoryStreamWrapper : MemoryStream
	{
		private UnmanagedMemoryStream _unmanagedStream;

		public override bool CanRead => _unmanagedStream.CanRead;

		public override bool CanSeek => _unmanagedStream.CanSeek;

		public override bool CanWrite => _unmanagedStream.CanWrite;

		public override int Capacity
		{
			get
			{
				return (int)_unmanagedStream.Capacity;
			}
			set
			{
				throw new IOException(Environment.GetResourceString("IO.IO_FixedCapacity"));
			}
		}

		public override long Length => _unmanagedStream.Length;

		public override long Position
		{
			get
			{
				return _unmanagedStream.Position;
			}
			set
			{
				_unmanagedStream.Position = value;
			}
		}

		internal UnmanagedMemoryStreamWrapper(UnmanagedMemoryStream stream)
		{
			_unmanagedStream = stream;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					_unmanagedStream.Close();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override void Flush()
		{
			_unmanagedStream.Flush();
		}

		public override byte[] GetBuffer()
		{
			throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_MemStreamBuffer"));
		}

		public override int Read([In][Out] byte[] buffer, int offset, int count)
		{
			return _unmanagedStream.Read(buffer, offset, count);
		}

		public override int ReadByte()
		{
			return _unmanagedStream.ReadByte();
		}

		public override long Seek(long offset, SeekOrigin loc)
		{
			return _unmanagedStream.Seek(offset, loc);
		}

		public unsafe override byte[] ToArray()
		{
			if (!_unmanagedStream._isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (!_unmanagedStream.CanRead)
			{
				__Error.ReadNotSupported();
			}
			byte[] array = new byte[_unmanagedStream.Length];
			Buffer.memcpy(_unmanagedStream.Pointer, 0, array, 0, (int)_unmanagedStream.Length);
			return array;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_unmanagedStream.Write(buffer, offset, count);
		}

		public override void WriteByte(byte value)
		{
			_unmanagedStream.WriteByte(value);
		}

		public override void WriteTo(Stream stream)
		{
			if (!_unmanagedStream._isOpen)
			{
				__Error.StreamIsClosed();
			}
			if (!_unmanagedStream.CanRead)
			{
				__Error.ReadNotSupported();
			}
			if (stream == null)
			{
				throw new ArgumentNullException("stream", Environment.GetResourceString("ArgumentNull_Stream"));
			}
			byte[] array = ToArray();
			stream.Write(array, 0, array.Length);
		}
	}
}
