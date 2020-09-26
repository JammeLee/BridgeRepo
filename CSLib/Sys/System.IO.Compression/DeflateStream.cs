using System.Security.Permissions;
using System.Threading;

namespace System.IO.Compression
{
	public class DeflateStream : Stream
	{
		internal delegate void AsyncWriteDelegate(byte[] array, int offset, int count, bool isAsync);

		private const int bufferSize = 4096;

		private Stream _stream;

		private CompressionMode _mode;

		private bool _leaveOpen;

		private Inflater inflater;

		private Deflater deflater;

		private byte[] buffer;

		private int asyncOperations;

		private readonly AsyncCallback m_CallBack;

		private readonly AsyncWriteDelegate m_AsyncWriterDelegate;

		public override bool CanRead
		{
			get
			{
				if (_stream == null)
				{
					return false;
				}
				if (_mode == CompressionMode.Decompress)
				{
					return _stream.CanRead;
				}
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				if (_stream == null)
				{
					return false;
				}
				if (_mode == CompressionMode.Compress)
				{
					return _stream.CanWrite;
				}
				return false;
			}
		}

		public override bool CanSeek => false;

		public override long Length
		{
			get
			{
				throw new NotSupportedException(SR.GetString("NotSupported"));
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException(SR.GetString("NotSupported"));
			}
			set
			{
				throw new NotSupportedException(SR.GetString("NotSupported"));
			}
		}

		public Stream BaseStream => _stream;

		public DeflateStream(Stream stream, CompressionMode mode)
			: this(stream, mode, leaveOpen: false, usingGZip: false)
		{
		}

		public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen)
			: this(stream, mode, leaveOpen, usingGZip: false)
		{
		}

		internal DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen, bool usingGZip)
		{
			_stream = stream;
			_mode = mode;
			_leaveOpen = leaveOpen;
			if (_stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			switch (_mode)
			{
			case CompressionMode.Decompress:
				if (!_stream.CanRead)
				{
					throw new ArgumentException(SR.GetString("NotReadableStream"), "stream");
				}
				inflater = new Inflater(usingGZip);
				m_CallBack = ReadCallback;
				break;
			case CompressionMode.Compress:
				if (!_stream.CanWrite)
				{
					throw new ArgumentException(SR.GetString("NotWriteableStream"), "stream");
				}
				deflater = new Deflater(usingGZip);
				m_AsyncWriterDelegate = InternalWrite;
				m_CallBack = WriteCallback;
				break;
			default:
				throw new ArgumentException(SR.GetString("ArgumentOutOfRange_Enum"), "mode");
			}
			buffer = new byte[4096];
		}

		public override void Flush()
		{
			if (_stream == null)
			{
				throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(SR.GetString("NotSupported"));
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException(SR.GetString("NotSupported"));
		}

		public override int Read(byte[] array, int offset, int count)
		{
			EnsureDecompressionMode();
			ValidateParameters(array, offset, count);
			int num = offset;
			int num2 = count;
			while (true)
			{
				int num3 = inflater.Inflate(array, num, num2);
				num += num3;
				num2 -= num3;
				if (num2 == 0 || inflater.Finished())
				{
					break;
				}
				int num4 = _stream.Read(buffer, 0, buffer.Length);
				if (num4 == 0)
				{
					break;
				}
				inflater.SetInput(buffer, 0, num4);
			}
			return count - num2;
		}

		private void ValidateParameters(byte[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (array.Length - offset < count)
			{
				throw new ArgumentException(SR.GetString("InvalidArgumentOffsetCount"));
			}
			if (_stream == null)
			{
				throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
			}
		}

		private void EnsureDecompressionMode()
		{
			if (_mode != 0)
			{
				throw new InvalidOperationException(SR.GetString("CannotReadFromDeflateStream"));
			}
		}

		private void EnsureCompressionMode()
		{
			if (_mode != CompressionMode.Compress)
			{
				throw new InvalidOperationException(SR.GetString("CannotWriteToDeflateStream"));
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			EnsureDecompressionMode();
			if (asyncOperations != 0)
			{
				throw new InvalidOperationException(SR.GetString("InvalidBeginCall"));
			}
			Interlocked.Increment(ref asyncOperations);
			try
			{
				ValidateParameters(array, offset, count);
				DeflateStreamAsyncResult deflateStreamAsyncResult = new DeflateStreamAsyncResult(this, asyncState, asyncCallback, array, offset, count);
				deflateStreamAsyncResult.isWrite = false;
				int num = inflater.Inflate(array, offset, count);
				if (num != 0)
				{
					deflateStreamAsyncResult.InvokeCallback(completedSynchronously: true, num);
					return deflateStreamAsyncResult;
				}
				if (inflater.Finished())
				{
					deflateStreamAsyncResult.InvokeCallback(completedSynchronously: true, 0);
					return deflateStreamAsyncResult;
				}
				_stream.BeginRead(buffer, 0, buffer.Length, m_CallBack, deflateStreamAsyncResult);
				deflateStreamAsyncResult.m_CompletedSynchronously &= deflateStreamAsyncResult.IsCompleted;
				return deflateStreamAsyncResult;
			}
			catch
			{
				Interlocked.Decrement(ref asyncOperations);
				throw;
			}
		}

		private void ReadCallback(IAsyncResult baseStreamResult)
		{
			DeflateStreamAsyncResult deflateStreamAsyncResult = (DeflateStreamAsyncResult)baseStreamResult.AsyncState;
			deflateStreamAsyncResult.m_CompletedSynchronously &= baseStreamResult.CompletedSynchronously;
			int num = 0;
			try
			{
				num = _stream.EndRead(baseStreamResult);
			}
			catch (Exception result)
			{
				deflateStreamAsyncResult.InvokeCallback(result);
				return;
			}
			if (num <= 0)
			{
				deflateStreamAsyncResult.InvokeCallback(0);
				return;
			}
			inflater.SetInput(buffer, 0, num);
			num = inflater.Inflate(deflateStreamAsyncResult.buffer, deflateStreamAsyncResult.offset, deflateStreamAsyncResult.count);
			if (num == 0 && !inflater.Finished())
			{
				_stream.BeginRead(buffer, 0, buffer.Length, m_CallBack, deflateStreamAsyncResult);
			}
			else
			{
				deflateStreamAsyncResult.InvokeCallback(num);
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			EnsureDecompressionMode();
			if (asyncOperations != 1)
			{
				throw new InvalidOperationException(SR.GetString("InvalidEndCall"));
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			if (_stream == null)
			{
				throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
			}
			DeflateStreamAsyncResult deflateStreamAsyncResult = asyncResult as DeflateStreamAsyncResult;
			if (deflateStreamAsyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			try
			{
				if (!deflateStreamAsyncResult.IsCompleted)
				{
					deflateStreamAsyncResult.AsyncWaitHandle.WaitOne();
				}
			}
			finally
			{
				Interlocked.Decrement(ref asyncOperations);
				deflateStreamAsyncResult.Close();
			}
			if (deflateStreamAsyncResult.Result is Exception)
			{
				throw (Exception)deflateStreamAsyncResult.Result;
			}
			return (int)deflateStreamAsyncResult.Result;
		}

		public override void Write(byte[] array, int offset, int count)
		{
			EnsureCompressionMode();
			ValidateParameters(array, offset, count);
			InternalWrite(array, offset, count, isAsync: false);
		}

		internal void InternalWrite(byte[] array, int offset, int count, bool isAsync)
		{
			while (!deflater.NeedsInput())
			{
				int deflateOutput = deflater.GetDeflateOutput(buffer);
				if (deflateOutput != 0)
				{
					if (isAsync)
					{
						IAsyncResult asyncResult = _stream.BeginWrite(buffer, 0, deflateOutput, null, null);
						_stream.EndWrite(asyncResult);
					}
					else
					{
						_stream.Write(buffer, 0, deflateOutput);
					}
				}
			}
			deflater.SetInput(array, offset, count);
			while (!deflater.NeedsInput())
			{
				int deflateOutput = deflater.GetDeflateOutput(buffer);
				if (deflateOutput != 0)
				{
					if (isAsync)
					{
						IAsyncResult asyncResult2 = _stream.BeginWrite(buffer, 0, deflateOutput, null, null);
						_stream.EndWrite(asyncResult2);
					}
					else
					{
						_stream.Write(buffer, 0, deflateOutput);
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!disposing || _stream == null)
				{
					return;
				}
				Flush();
				if (_mode != CompressionMode.Compress || _stream == null)
				{
					return;
				}
				int deflateOutput;
				while (!deflater.NeedsInput())
				{
					deflateOutput = deflater.GetDeflateOutput(buffer);
					if (deflateOutput != 0)
					{
						_stream.Write(buffer, 0, deflateOutput);
					}
				}
				deflateOutput = deflater.Finish(buffer);
				if (deflateOutput > 0)
				{
					_stream.Write(buffer, 0, deflateOutput);
				}
			}
			finally
			{
				try
				{
					if (disposing && !_leaveOpen && _stream != null)
					{
						_stream.Close();
					}
				}
				finally
				{
					_stream = null;
					base.Dispose(disposing);
				}
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			EnsureCompressionMode();
			if (asyncOperations != 0)
			{
				throw new InvalidOperationException(SR.GetString("InvalidBeginCall"));
			}
			Interlocked.Increment(ref asyncOperations);
			try
			{
				ValidateParameters(array, offset, count);
				DeflateStreamAsyncResult deflateStreamAsyncResult = new DeflateStreamAsyncResult(this, asyncState, asyncCallback, array, offset, count);
				deflateStreamAsyncResult.isWrite = true;
				m_AsyncWriterDelegate.BeginInvoke(array, offset, count, isAsync: true, m_CallBack, deflateStreamAsyncResult);
				deflateStreamAsyncResult.m_CompletedSynchronously &= deflateStreamAsyncResult.IsCompleted;
				return deflateStreamAsyncResult;
			}
			catch
			{
				Interlocked.Decrement(ref asyncOperations);
				throw;
			}
		}

		private void WriteCallback(IAsyncResult asyncResult)
		{
			DeflateStreamAsyncResult deflateStreamAsyncResult = (DeflateStreamAsyncResult)asyncResult.AsyncState;
			deflateStreamAsyncResult.m_CompletedSynchronously &= asyncResult.CompletedSynchronously;
			try
			{
				m_AsyncWriterDelegate.EndInvoke(asyncResult);
			}
			catch (Exception result)
			{
				deflateStreamAsyncResult.InvokeCallback(result);
				return;
			}
			deflateStreamAsyncResult.InvokeCallback(null);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			EnsureCompressionMode();
			if (asyncOperations != 1)
			{
				throw new InvalidOperationException(SR.GetString("InvalidEndCall"));
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			if (_stream == null)
			{
				throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
			}
			DeflateStreamAsyncResult deflateStreamAsyncResult = asyncResult as DeflateStreamAsyncResult;
			if (deflateStreamAsyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			try
			{
				if (!deflateStreamAsyncResult.IsCompleted)
				{
					deflateStreamAsyncResult.AsyncWaitHandle.WaitOne();
				}
			}
			finally
			{
				Interlocked.Decrement(ref asyncOperations);
				deflateStreamAsyncResult.Close();
			}
			if (deflateStreamAsyncResult.Result is Exception)
			{
				throw (Exception)deflateStreamAsyncResult.Result;
			}
		}
	}
}
