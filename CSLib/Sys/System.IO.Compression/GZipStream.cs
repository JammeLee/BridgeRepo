using System.Security.Permissions;

namespace System.IO.Compression
{
	public class GZipStream : Stream
	{
		private DeflateStream deflateStream;

		public override bool CanRead
		{
			get
			{
				if (deflateStream == null)
				{
					return false;
				}
				return deflateStream.CanRead;
			}
		}

		public override bool CanWrite
		{
			get
			{
				if (deflateStream == null)
				{
					return false;
				}
				return deflateStream.CanWrite;
			}
		}

		public override bool CanSeek
		{
			get
			{
				if (deflateStream == null)
				{
					return false;
				}
				return deflateStream.CanSeek;
			}
		}

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

		public Stream BaseStream
		{
			get
			{
				if (deflateStream != null)
				{
					return deflateStream.BaseStream;
				}
				return null;
			}
		}

		public GZipStream(Stream stream, CompressionMode mode)
			: this(stream, mode, leaveOpen: false)
		{
		}

		public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
		{
			deflateStream = new DeflateStream(stream, mode, leaveOpen, usingGZip: true);
		}

		public override void Flush()
		{
			if (deflateStream == null)
			{
				throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
			}
			deflateStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(SR.GetString("NotSupported"));
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException(SR.GetString("NotSupported"));
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			if (deflateStream == null)
			{
				throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
			}
			return deflateStream.BeginRead(array, offset, count, asyncCallback, asyncState);
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			if (deflateStream == null)
			{
				throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
			}
			return deflateStream.EndRead(asyncResult);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			if (deflateStream == null)
			{
				throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
			}
			return deflateStream.BeginWrite(array, offset, count, asyncCallback, asyncState);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			if (deflateStream == null)
			{
				throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
			}
			deflateStream.EndWrite(asyncResult);
		}

		public override int Read(byte[] array, int offset, int count)
		{
			if (deflateStream == null)
			{
				throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
			}
			return deflateStream.Read(array, offset, count);
		}

		public override void Write(byte[] array, int offset, int count)
		{
			if (deflateStream == null)
			{
				throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
			}
			deflateStream.Write(array, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && deflateStream != null)
				{
					deflateStream.Close();
				}
				deflateStream = null;
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}
}
