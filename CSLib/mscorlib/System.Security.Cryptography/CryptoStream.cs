using System.IO;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class CryptoStream : Stream, IDisposable
	{
		private Stream _stream;

		private ICryptoTransform _Transform;

		private byte[] _InputBuffer;

		private int _InputBufferIndex;

		private int _InputBlockSize;

		private byte[] _OutputBuffer;

		private int _OutputBufferIndex;

		private int _OutputBlockSize;

		private CryptoStreamMode _transformMode;

		private bool _canRead;

		private bool _canWrite;

		private bool _finalBlockTransformed;

		public override bool CanRead => _canRead;

		public override bool CanSeek => false;

		public override bool CanWrite => _canWrite;

		public override long Length
		{
			get
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
			}
			set
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
			}
		}

		public CryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
		{
			_stream = stream;
			_transformMode = mode;
			_Transform = transform;
			switch (_transformMode)
			{
			case CryptoStreamMode.Read:
				if (!_stream.CanRead)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotReadable"), "stream");
				}
				_canRead = true;
				break;
			case CryptoStreamMode.Write:
				if (!_stream.CanWrite)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotWritable"), "stream");
				}
				_canWrite = true;
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			InitializeBuffer();
		}

		public void FlushFinalBlock()
		{
			if (_finalBlockTransformed)
			{
				throw new NotSupportedException(Environment.GetResourceString("Cryptography_CryptoStream_FlushFinalBlockTwice"));
			}
			byte[] array = _Transform.TransformFinalBlock(_InputBuffer, 0, _InputBufferIndex);
			_finalBlockTransformed = true;
			if (_canWrite && _OutputBufferIndex > 0)
			{
				_stream.Write(_OutputBuffer, 0, _OutputBufferIndex);
				_OutputBufferIndex = 0;
			}
			if (_canWrite)
			{
				_stream.Write(array, 0, array.Length);
			}
			if (_stream is CryptoStream)
			{
				((CryptoStream)_stream).FlushFinalBlock();
			}
			else
			{
				_stream.Flush();
			}
			if (_InputBuffer != null)
			{
				Array.Clear(_InputBuffer, 0, _InputBuffer.Length);
			}
			if (_OutputBuffer != null)
			{
				Array.Clear(_OutputBuffer, 0, _OutputBuffer.Length);
			}
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
		}

		public override int Read([In][Out] byte[] buffer, int offset, int count)
		{
			if (!_canRead)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnreadableStream"));
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
			int num = count;
			int num2 = offset;
			if (_OutputBufferIndex != 0)
			{
				if (_OutputBufferIndex > count)
				{
					Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, offset, count);
					Buffer.InternalBlockCopy(_OutputBuffer, count, _OutputBuffer, 0, _OutputBufferIndex - count);
					_OutputBufferIndex -= count;
					return count;
				}
				Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, offset, _OutputBufferIndex);
				num -= _OutputBufferIndex;
				num2 += _OutputBufferIndex;
				_OutputBufferIndex = 0;
			}
			if (_finalBlockTransformed)
			{
				return count - num;
			}
			int num3 = 0;
			if (num > _OutputBlockSize && _Transform.CanTransformMultipleBlocks)
			{
				int num4 = num / _OutputBlockSize;
				int num5 = num4 * _InputBlockSize;
				byte[] array = new byte[num5];
				Buffer.InternalBlockCopy(_InputBuffer, 0, array, 0, _InputBufferIndex);
				num3 = _InputBufferIndex;
				num3 += _stream.Read(array, _InputBufferIndex, num5 - _InputBufferIndex);
				_InputBufferIndex = 0;
				if (num3 <= _InputBlockSize)
				{
					_InputBuffer = array;
					_InputBufferIndex = num3;
				}
				else
				{
					int num6 = num3 / _InputBlockSize * _InputBlockSize;
					int num7 = num3 - num6;
					if (num7 != 0)
					{
						_InputBufferIndex = num7;
						Buffer.InternalBlockCopy(array, num6, _InputBuffer, 0, num7);
					}
					byte[] array2 = new byte[num6 / _InputBlockSize * _OutputBlockSize];
					int num8 = _Transform.TransformBlock(array, 0, num6, array2, 0);
					Buffer.InternalBlockCopy(array2, 0, buffer, num2, num8);
					Array.Clear(array, 0, array.Length);
					Array.Clear(array2, 0, array2.Length);
					num -= num8;
					num2 += num8;
				}
			}
			while (num > 0)
			{
				while (_InputBufferIndex < _InputBlockSize)
				{
					num3 = _stream.Read(_InputBuffer, _InputBufferIndex, _InputBlockSize - _InputBufferIndex);
					if (num3 != 0)
					{
						_InputBufferIndex += num3;
						continue;
					}
					_OutputBufferIndex = (_OutputBuffer = _Transform.TransformFinalBlock(_InputBuffer, 0, _InputBufferIndex)).Length;
					_finalBlockTransformed = true;
					if (num < _OutputBufferIndex)
					{
						Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, num2, num);
						_OutputBufferIndex -= num;
						Buffer.InternalBlockCopy(_OutputBuffer, num, _OutputBuffer, 0, _OutputBufferIndex);
						return count;
					}
					Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, num2, _OutputBufferIndex);
					num -= _OutputBufferIndex;
					_OutputBufferIndex = 0;
					return count - num;
				}
				int num8 = _Transform.TransformBlock(_InputBuffer, 0, _InputBlockSize, _OutputBuffer, 0);
				_InputBufferIndex = 0;
				if (num >= num8)
				{
					Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, num2, num8);
					num2 += num8;
					num -= num8;
					continue;
				}
				Buffer.InternalBlockCopy(_OutputBuffer, 0, buffer, num2, num);
				_OutputBufferIndex = num8 - num;
				Buffer.InternalBlockCopy(_OutputBuffer, num, _OutputBuffer, 0, _OutputBufferIndex);
				return count;
			}
			return count;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!_canWrite)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnwritableStream"));
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
			int num = count;
			int num2 = offset;
			if (_InputBufferIndex > 0)
			{
				if (count < _InputBlockSize - _InputBufferIndex)
				{
					Buffer.InternalBlockCopy(buffer, offset, _InputBuffer, _InputBufferIndex, count);
					_InputBufferIndex += count;
					return;
				}
				Buffer.InternalBlockCopy(buffer, offset, _InputBuffer, _InputBufferIndex, _InputBlockSize - _InputBufferIndex);
				num2 += _InputBlockSize - _InputBufferIndex;
				num -= _InputBlockSize - _InputBufferIndex;
				_InputBufferIndex = _InputBlockSize;
			}
			if (_OutputBufferIndex > 0)
			{
				_stream.Write(_OutputBuffer, 0, _OutputBufferIndex);
				_OutputBufferIndex = 0;
			}
			if (_InputBufferIndex == _InputBlockSize)
			{
				int count2 = _Transform.TransformBlock(_InputBuffer, 0, _InputBlockSize, _OutputBuffer, 0);
				_stream.Write(_OutputBuffer, 0, count2);
				_InputBufferIndex = 0;
			}
			while (num > 0)
			{
				if (num >= _InputBlockSize)
				{
					if (_Transform.CanTransformMultipleBlocks)
					{
						int num3 = num / _InputBlockSize;
						int num4 = num3 * _InputBlockSize;
						byte[] array = new byte[num3 * _OutputBlockSize];
						int count2 = _Transform.TransformBlock(buffer, num2, num4, array, 0);
						_stream.Write(array, 0, count2);
						num2 += num4;
						num -= num4;
					}
					else
					{
						int count2 = _Transform.TransformBlock(buffer, num2, _InputBlockSize, _OutputBuffer, 0);
						_stream.Write(_OutputBuffer, 0, count2);
						num2 += _InputBlockSize;
						num -= _InputBlockSize;
					}
					continue;
				}
				Buffer.InternalBlockCopy(buffer, num2, _InputBuffer, 0, num);
				_InputBufferIndex += num;
				break;
			}
		}

		public void Clear()
		{
			Close();
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (!_finalBlockTransformed)
					{
						FlushFinalBlock();
					}
					_stream.Close();
					if (_InputBuffer != null)
					{
						Array.Clear(_InputBuffer, 0, _InputBuffer.Length);
					}
					if (_OutputBuffer != null)
					{
						Array.Clear(_OutputBuffer, 0, _OutputBuffer.Length);
					}
					_InputBuffer = null;
					_OutputBuffer = null;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		private void InitializeBuffer()
		{
			if (_Transform != null)
			{
				_InputBlockSize = _Transform.InputBlockSize;
				_InputBuffer = new byte[_InputBlockSize];
				_OutputBlockSize = _Transform.OutputBlockSize;
				_OutputBuffer = new byte[_OutputBlockSize];
			}
		}
	}
}
