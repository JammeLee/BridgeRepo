using System.IO;

namespace System.Net.Cache
{
	internal class RangeStream : Stream, ICloseEx
	{
		private Stream m_ParentStream;

		private long m_Offset;

		private long m_Size;

		private long m_Position;

		public override bool CanRead => m_ParentStream.CanRead;

		public override bool CanSeek => m_ParentStream.CanSeek;

		public override bool CanWrite => m_ParentStream.CanWrite;

		public override long Length
		{
			get
			{
				_ = m_ParentStream.Length;
				return m_Size;
			}
		}

		public override long Position
		{
			get
			{
				return m_ParentStream.Position - m_Offset;
			}
			set
			{
				value += m_Offset;
				if (value > m_Offset + m_Size)
				{
					value = m_Offset + m_Size;
				}
				m_ParentStream.Position = value;
			}
		}

		public override bool CanTimeout => m_ParentStream.CanTimeout;

		public override int ReadTimeout
		{
			get
			{
				return m_ParentStream.ReadTimeout;
			}
			set
			{
				m_ParentStream.ReadTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return m_ParentStream.WriteTimeout;
			}
			set
			{
				m_ParentStream.WriteTimeout = value;
			}
		}

		internal RangeStream(Stream parentStream, long offset, long size)
		{
			m_ParentStream = parentStream;
			m_Offset = offset;
			m_Size = size;
			if (m_ParentStream.CanSeek)
			{
				m_ParentStream.Position = offset;
				m_Position = offset;
				return;
			}
			throw new NotSupportedException(SR.GetString("net_cache_non_seekable_stream_not_supported"));
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
			case SeekOrigin.Begin:
				offset += m_Offset;
				if (offset > m_Offset + m_Size)
				{
					offset = m_Offset + m_Size;
				}
				if (offset < m_Offset)
				{
					offset = m_Offset;
				}
				break;
			case SeekOrigin.End:
				offset -= m_Offset + m_Size;
				if (offset > 0)
				{
					offset = 0L;
				}
				if (offset < -m_Size)
				{
					offset = -m_Size;
				}
				break;
			default:
				if (m_Position + offset > m_Offset + m_Size)
				{
					offset = m_Offset + m_Size - m_Position;
				}
				if (m_Position + offset < m_Offset)
				{
					offset = m_Offset - m_Position;
				}
				break;
			}
			m_Position = m_ParentStream.Seek(offset, origin);
			return m_Position - m_Offset;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException(SR.GetString("net_cache_unsupported_partial_stream"));
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (m_Position + count > m_Offset + m_Size)
			{
				throw new NotSupportedException(SR.GetString("net_cache_unsupported_partial_stream"));
			}
			m_ParentStream.Write(buffer, offset, count);
			m_Position += count;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (m_Position + offset > m_Offset + m_Size)
			{
				throw new NotSupportedException(SR.GetString("net_cache_unsupported_partial_stream"));
			}
			return m_ParentStream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			m_ParentStream.EndWrite(asyncResult);
			m_Position = m_ParentStream.Position;
		}

		public override void Flush()
		{
			m_ParentStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (m_Position >= m_Offset + m_Size)
			{
				return 0;
			}
			if (m_Position + count > m_Offset + m_Size)
			{
				count = (int)(m_Offset + m_Size - m_Position);
			}
			int num = m_ParentStream.Read(buffer, offset, count);
			m_Position += num;
			return num;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (m_Position >= m_Offset + m_Size)
			{
				count = 0;
			}
			else if (m_Position + count > m_Offset + m_Size)
			{
				count = (int)(m_Offset + m_Size - m_Position);
			}
			return m_ParentStream.BeginRead(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			int num = m_ParentStream.EndRead(asyncResult);
			m_Position += num;
			return num;
		}

		protected sealed override void Dispose(bool disposing)
		{
			Dispose(disposing, CloseExState.Normal);
			GC.SuppressFinalize(this);
		}

		void ICloseEx.CloseEx(CloseExState closeState)
		{
			Dispose(disposing: true, closeState);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing, CloseExState closeState)
		{
			ICloseEx closeEx = m_ParentStream as ICloseEx;
			if (closeEx != null)
			{
				closeEx.CloseEx(closeState);
			}
			else
			{
				m_ParentStream.Close();
			}
			base.Dispose(disposing);
		}
	}
}
