using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public abstract class Stream : MarshalByRefObject, IDisposable
	{
		private delegate int ReadDelegate([In][Out] byte[] bytes, int index, int offset);

		private delegate void WriteDelegate(byte[] bytes, int index, int offset);

		[Serializable]
		private sealed class NullStream : Stream
		{
			public override bool CanRead => true;

			public override bool CanWrite => true;

			public override bool CanSeek => true;

			public override long Length => 0L;

			public override long Position
			{
				get
				{
					return 0L;
				}
				set
				{
				}
			}

			internal NullStream()
			{
			}

			public override void Flush()
			{
			}

			public override int Read([In][Out] byte[] buffer, int offset, int count)
			{
				return 0;
			}

			public override int ReadByte()
			{
				return -1;
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
			}

			public override void WriteByte(byte value)
			{
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				return 0L;
			}

			public override void SetLength(long length)
			{
			}
		}

		[Serializable]
		internal sealed class SyncStream : Stream, IDisposable
		{
			private Stream _stream;

			public override bool CanRead => _stream.CanRead;

			public override bool CanWrite => _stream.CanWrite;

			public override bool CanSeek => _stream.CanSeek;

			[ComVisible(false)]
			public override bool CanTimeout => _stream.CanTimeout;

			public override long Length
			{
				get
				{
					lock (_stream)
					{
						return _stream.Length;
					}
				}
			}

			public override long Position
			{
				get
				{
					lock (_stream)
					{
						return _stream.Position;
					}
				}
				set
				{
					lock (_stream)
					{
						_stream.Position = value;
					}
				}
			}

			[ComVisible(false)]
			public override int ReadTimeout
			{
				get
				{
					return _stream.ReadTimeout;
				}
				set
				{
					_stream.ReadTimeout = value;
				}
			}

			[ComVisible(false)]
			public override int WriteTimeout
			{
				get
				{
					return _stream.WriteTimeout;
				}
				set
				{
					_stream.WriteTimeout = value;
				}
			}

			internal SyncStream(Stream stream)
			{
				if (stream == null)
				{
					throw new ArgumentNullException("stream");
				}
				_stream = stream;
			}

			public override void Close()
			{
				lock (_stream)
				{
					try
					{
						_stream.Close();
					}
					finally
					{
						base.Dispose(disposing: true);
					}
				}
			}

			protected override void Dispose(bool disposing)
			{
				lock (_stream)
				{
					try
					{
						if (disposing)
						{
							((IDisposable)_stream).Dispose();
						}
					}
					finally
					{
						base.Dispose(disposing);
					}
				}
			}

			public override void Flush()
			{
				lock (_stream)
				{
					_stream.Flush();
				}
			}

			public override int Read([In][Out] byte[] bytes, int offset, int count)
			{
				lock (_stream)
				{
					return _stream.Read(bytes, offset, count);
				}
			}

			public override int ReadByte()
			{
				lock (_stream)
				{
					return _stream.ReadByte();
				}
			}

			[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
			public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				lock (_stream)
				{
					return _stream.BeginRead(buffer, offset, count, callback, state);
				}
			}

			public override int EndRead(IAsyncResult asyncResult)
			{
				lock (_stream)
				{
					return _stream.EndRead(asyncResult);
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				lock (_stream)
				{
					return _stream.Seek(offset, origin);
				}
			}

			public override void SetLength(long length)
			{
				lock (_stream)
				{
					_stream.SetLength(length);
				}
			}

			public override void Write(byte[] bytes, int offset, int count)
			{
				lock (_stream)
				{
					_stream.Write(bytes, offset, count);
				}
			}

			public override void WriteByte(byte b)
			{
				lock (_stream)
				{
					_stream.WriteByte(b);
				}
			}

			[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
			public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				lock (_stream)
				{
					return _stream.BeginWrite(buffer, offset, count, callback, state);
				}
			}

			public override void EndWrite(IAsyncResult asyncResult)
			{
				lock (_stream)
				{
					_stream.EndWrite(asyncResult);
				}
			}
		}

		public static readonly Stream Null = new NullStream();

		[NonSerialized]
		private ReadDelegate _readDelegate;

		[NonSerialized]
		private WriteDelegate _writeDelegate;

		[NonSerialized]
		private AutoResetEvent _asyncActiveEvent;

		[NonSerialized]
		private int _asyncActiveCount = 1;

		public abstract bool CanRead
		{
			get;
		}

		public abstract bool CanSeek
		{
			get;
		}

		[ComVisible(false)]
		public virtual bool CanTimeout => false;

		public abstract bool CanWrite
		{
			get;
		}

		public abstract long Length
		{
			get;
		}

		public abstract long Position
		{
			get;
			set;
		}

		[ComVisible(false)]
		public virtual int ReadTimeout
		{
			get
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TimeoutsNotSupported"));
			}
			set
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TimeoutsNotSupported"));
			}
		}

		[ComVisible(false)]
		public virtual int WriteTimeout
		{
			get
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TimeoutsNotSupported"));
			}
			set
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TimeoutsNotSupported"));
			}
		}

		public virtual void Close()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void Dispose()
		{
			Close();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _asyncActiveEvent != null)
			{
				_CloseAsyncActiveEvent(Interlocked.Decrement(ref _asyncActiveCount));
			}
		}

		private void _CloseAsyncActiveEvent(int asyncActiveCount)
		{
			if (_asyncActiveEvent != null && asyncActiveCount == 0)
			{
				_asyncActiveEvent.Close();
				_asyncActiveEvent = null;
			}
		}

		public abstract void Flush();

		[Obsolete("CreateWaitHandle will be removed eventually.  Please use \"new ManualResetEvent(false)\" instead.")]
		protected virtual WaitHandle CreateWaitHandle()
		{
			return new ManualResetEvent(initialState: false);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public virtual IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (!CanRead)
			{
				__Error.ReadNotSupported();
			}
			Interlocked.Increment(ref _asyncActiveCount);
			ReadDelegate readDelegate = Read;
			if (_asyncActiveEvent == null)
			{
				lock (this)
				{
					if (_asyncActiveEvent == null)
					{
						_asyncActiveEvent = new AutoResetEvent(initialState: true);
					}
				}
			}
			_asyncActiveEvent.WaitOne();
			_readDelegate = readDelegate;
			return readDelegate.BeginInvoke(buffer, offset, count, callback, state);
		}

		public virtual int EndRead(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			if (_readDelegate == null)
			{
				throw new ArgumentException(Environment.GetResourceString("InvalidOperation_WrongAsyncResultOrEndReadCalledMultiple"));
			}
			int num = -1;
			try
			{
				return _readDelegate.EndInvoke(asyncResult);
			}
			finally
			{
				_readDelegate = null;
				_asyncActiveEvent.Set();
				_CloseAsyncActiveEvent(Interlocked.Decrement(ref _asyncActiveCount));
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public virtual IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (!CanWrite)
			{
				__Error.WriteNotSupported();
			}
			Interlocked.Increment(ref _asyncActiveCount);
			WriteDelegate writeDelegate = Write;
			if (_asyncActiveEvent == null)
			{
				lock (this)
				{
					if (_asyncActiveEvent == null)
					{
						_asyncActiveEvent = new AutoResetEvent(initialState: true);
					}
				}
			}
			_asyncActiveEvent.WaitOne();
			_writeDelegate = writeDelegate;
			return writeDelegate.BeginInvoke(buffer, offset, count, callback, state);
		}

		public virtual void EndWrite(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			if (_writeDelegate == null)
			{
				throw new ArgumentException(Environment.GetResourceString("InvalidOperation_WrongAsyncResultOrEndWriteCalledMultiple"));
			}
			try
			{
				_writeDelegate.EndInvoke(asyncResult);
			}
			finally
			{
				_writeDelegate = null;
				_asyncActiveEvent.Set();
				_CloseAsyncActiveEvent(Interlocked.Decrement(ref _asyncActiveCount));
			}
		}

		public abstract long Seek(long offset, SeekOrigin origin);

		public abstract void SetLength(long value);

		public abstract int Read([In][Out] byte[] buffer, int offset, int count);

		public virtual int ReadByte()
		{
			byte[] array = new byte[1];
			if (Read(array, 0, 1) == 0)
			{
				return -1;
			}
			return array[0];
		}

		public abstract void Write(byte[] buffer, int offset, int count);

		public virtual void WriteByte(byte value)
		{
			Write(new byte[1]
			{
				value
			}, 0, 1);
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public static Stream Synchronized(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (stream is SyncStream)
			{
				return stream;
			}
			return new SyncStream(stream);
		}
	}
}
