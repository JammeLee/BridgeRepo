using System.IO;
using System.Threading;

namespace System.Net.Cache
{
	internal class CombinedReadStream : Stream, ICloseEx
	{
		private class InnerAsyncResult : LazyAsyncResult
		{
			public byte[] Buffer;

			public int Offset;

			public int Count;

			public InnerAsyncResult(object userState, AsyncCallback userCallback, byte[] buffer, int offset, int count)
				: base(null, userState, userCallback)
			{
				Buffer = buffer;
				Offset = offset;
				Count = count;
			}
		}

		private Stream m_HeadStream;

		private Stream m_TailStream;

		private bool m_HeadEOF;

		private long m_HeadLength;

		private int m_ReadNesting;

		private AsyncCallback m_ReadCallback;

		public override bool CanRead
		{
			get
			{
				if (!m_HeadEOF)
				{
					return m_HeadStream.CanRead;
				}
				return m_TailStream.CanRead;
			}
		}

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => m_TailStream.Length + (m_HeadEOF ? m_HeadLength : m_HeadStream.Length);

		public override long Position
		{
			get
			{
				return m_TailStream.Position + (m_HeadEOF ? m_HeadLength : m_HeadStream.Position);
			}
			set
			{
				throw new NotSupportedException(SR.GetString("net_noseek"));
			}
		}

		public override bool CanTimeout
		{
			get
			{
				if (m_TailStream.CanTimeout)
				{
					return m_HeadStream.CanTimeout;
				}
				return false;
			}
		}

		public override int ReadTimeout
		{
			get
			{
				if (!m_HeadEOF)
				{
					return m_HeadStream.ReadTimeout;
				}
				return m_TailStream.ReadTimeout;
			}
			set
			{
				int num3 = (m_TailStream.ReadTimeout = (m_HeadStream.ReadTimeout = value));
			}
		}

		public override int WriteTimeout
		{
			get
			{
				if (!m_HeadEOF)
				{
					return m_HeadStream.WriteTimeout;
				}
				return m_TailStream.WriteTimeout;
			}
			set
			{
				int num3 = (m_TailStream.WriteTimeout = (m_HeadStream.WriteTimeout = value));
			}
		}

		internal CombinedReadStream(Stream headStream, Stream tailStream)
		{
			m_HeadStream = headStream;
			m_TailStream = tailStream;
			m_HeadEOF = headStream == Stream.Null;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				if (Interlocked.Increment(ref m_ReadNesting) != 1)
				{
					throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", "Read", "read"));
				}
				if (m_HeadEOF)
				{
					return m_TailStream.Read(buffer, offset, count);
				}
				int num = m_HeadStream.Read(buffer, offset, count);
				m_HeadLength += num;
				if (num == 0 && count != 0)
				{
					m_HeadEOF = true;
					m_HeadStream.Close();
					num = m_TailStream.Read(buffer, offset, count);
				}
				return num;
			}
			finally
			{
				Interlocked.Decrement(ref m_ReadNesting);
			}
		}

		private void ReadCallback(IAsyncResult transportResult)
		{
			if (transportResult.CompletedSynchronously)
			{
				return;
			}
			InnerAsyncResult innerAsyncResult = transportResult.AsyncState as InnerAsyncResult;
			try
			{
				int num;
				if (!m_HeadEOF)
				{
					num = m_HeadStream.EndRead(transportResult);
					m_HeadLength += num;
				}
				else
				{
					num = m_TailStream.EndRead(transportResult);
				}
				if (m_HeadEOF || num != 0 || innerAsyncResult.Count == 0)
				{
					goto IL_00a9;
				}
				m_HeadEOF = true;
				m_HeadStream.Close();
				IAsyncResult asyncResult = m_TailStream.BeginRead(innerAsyncResult.Buffer, innerAsyncResult.Offset, innerAsyncResult.Count, m_ReadCallback, innerAsyncResult);
				if (!asyncResult.CompletedSynchronously)
				{
					return;
				}
				num = m_TailStream.EndRead(asyncResult);
				goto IL_00a9;
				IL_00a9:
				innerAsyncResult.Buffer = null;
				innerAsyncResult.InvokeCallback(num);
			}
			catch (Exception result)
			{
				if (innerAsyncResult.InternalPeekCompleted)
				{
					throw;
				}
				innerAsyncResult.InvokeCallback(result);
			}
			catch
			{
				if (innerAsyncResult.InternalPeekCompleted)
				{
					throw;
				}
				innerAsyncResult.InvokeCallback(new Exception(SR.GetString("net_nonClsCompliantException")));
			}
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			try
			{
				if (Interlocked.Increment(ref m_ReadNesting) != 1)
				{
					throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", "BeginRead", "read"));
				}
				if (m_ReadCallback == null)
				{
					m_ReadCallback = ReadCallback;
				}
				if (m_HeadEOF)
				{
					return m_TailStream.BeginRead(buffer, offset, count, callback, state);
				}
				InnerAsyncResult innerAsyncResult = new InnerAsyncResult(state, callback, buffer, offset, count);
				IAsyncResult asyncResult = m_HeadStream.BeginRead(buffer, offset, count, m_ReadCallback, innerAsyncResult);
				if (!asyncResult.CompletedSynchronously)
				{
					return innerAsyncResult;
				}
				int num = m_HeadStream.EndRead(asyncResult);
				m_HeadLength += num;
				if (num == 0 && innerAsyncResult.Count != 0)
				{
					m_HeadEOF = true;
					m_HeadStream.Close();
					return m_TailStream.BeginRead(buffer, offset, count, callback, state);
				}
				innerAsyncResult.Buffer = null;
				innerAsyncResult.InvokeCallback(count);
				return innerAsyncResult;
			}
			catch
			{
				Interlocked.Decrement(ref m_ReadNesting);
				throw;
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			if (Interlocked.Decrement(ref m_ReadNesting) != 0)
			{
				Interlocked.Increment(ref m_ReadNesting);
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndRead"));
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			InnerAsyncResult innerAsyncResult = asyncResult as InnerAsyncResult;
			if (innerAsyncResult == null)
			{
				if (!m_HeadEOF)
				{
					return m_HeadStream.EndRead(asyncResult);
				}
				return m_TailStream.EndRead(asyncResult);
			}
			innerAsyncResult.InternalWaitForCompletion();
			if (innerAsyncResult.Result is Exception)
			{
				throw (Exception)innerAsyncResult.Result;
			}
			return (int)innerAsyncResult.Result;
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
			try
			{
				if (!m_HeadEOF)
				{
					ICloseEx closeEx = m_HeadStream as ICloseEx;
					if (closeEx != null)
					{
						closeEx.CloseEx(closeState);
					}
					else
					{
						m_HeadStream.Close();
					}
				}
			}
			finally
			{
				ICloseEx closeEx2 = m_TailStream as ICloseEx;
				if (closeEx2 != null)
				{
					closeEx2.CloseEx(closeState);
				}
				else
				{
					m_TailStream.Close();
				}
			}
			if (!disposing)
			{
				m_HeadStream = null;
				m_TailStream = null;
			}
		}
	}
}
