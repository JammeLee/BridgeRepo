using System.IO;
using System.Threading;

namespace System.Net.Cache
{
	internal class ForwardingReadStream : Stream, ICloseEx
	{
		private class InnerAsyncResult : LazyAsyncResult
		{
			public byte[] Buffer;

			public int Offset;

			public int Count;

			public bool IsWriteCompletion;

			public InnerAsyncResult(object userState, AsyncCallback userCallback, byte[] buffer, int offset, int count)
				: base(null, userState, userCallback)
			{
				Buffer = buffer;
				Offset = offset;
				Count = count;
			}
		}

		private Stream m_OriginalStream;

		private Stream m_ShadowStream;

		private int m_ReadNesting;

		private bool m_ShadowStreamIsDead;

		private AsyncCallback m_ReadCallback;

		private long m_BytesToSkip;

		private bool m_ThrowOnWriteError;

		private bool m_SeenReadEOF;

		private int _Disposed;

		public override bool CanRead => m_OriginalStream.CanRead;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => m_OriginalStream.Length - m_BytesToSkip;

		public override long Position
		{
			get
			{
				return m_OriginalStream.Position - m_BytesToSkip;
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
				if (m_OriginalStream.CanTimeout)
				{
					return m_ShadowStream.CanTimeout;
				}
				return false;
			}
		}

		public override int ReadTimeout
		{
			get
			{
				return m_OriginalStream.ReadTimeout;
			}
			set
			{
				int num3 = (m_OriginalStream.ReadTimeout = (m_ShadowStream.ReadTimeout = value));
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return m_ShadowStream.WriteTimeout;
			}
			set
			{
				int num3 = (m_OriginalStream.WriteTimeout = (m_ShadowStream.WriteTimeout = value));
			}
		}

		internal ForwardingReadStream(Stream originalStream, Stream shadowStream, long bytesToSkip, bool throwOnWriteError)
		{
			if (!shadowStream.CanWrite)
			{
				throw new ArgumentException(SR.GetString("net_cache_shadowstream_not_writable"), "shadowStream");
			}
			m_OriginalStream = originalStream;
			m_ShadowStream = shadowStream;
			m_BytesToSkip = bytesToSkip;
			m_ThrowOnWriteError = throwOnWriteError;
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
			bool flag = false;
			int num = -1;
			if (Interlocked.Increment(ref m_ReadNesting) != 1)
			{
				throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", "Read", "read"));
			}
			try
			{
				if (m_BytesToSkip != 0)
				{
					byte[] array = new byte[4096];
					while (m_BytesToSkip != 0)
					{
						int num2 = m_OriginalStream.Read(array, 0, (int)((m_BytesToSkip < array.Length) ? m_BytesToSkip : array.Length));
						if (num2 == 0)
						{
							m_SeenReadEOF = true;
						}
						m_BytesToSkip -= num2;
						if (!m_ShadowStreamIsDead)
						{
							m_ShadowStream.Write(array, 0, num2);
						}
					}
				}
				num = m_OriginalStream.Read(buffer, offset, count);
				if (num == 0)
				{
					m_SeenReadEOF = true;
				}
				if (m_ShadowStreamIsDead)
				{
					return num;
				}
				flag = true;
				m_ShadowStream.Write(buffer, offset, num);
				return num;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!m_ShadowStreamIsDead)
				{
					m_ShadowStreamIsDead = true;
					try
					{
						if (m_ShadowStream is ICloseEx)
						{
							((ICloseEx)m_ShadowStream).CloseEx(CloseExState.Abort | CloseExState.Silent);
						}
						else
						{
							m_ShadowStream.Close();
						}
					}
					catch (Exception)
					{
						if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
						{
							throw;
						}
					}
					catch
					{
					}
				}
				if (!flag || m_ThrowOnWriteError)
				{
					throw;
				}
				return num;
			}
			catch
			{
				if (!m_ShadowStreamIsDead)
				{
					m_ShadowStreamIsDead = true;
					try
					{
						if (m_ShadowStream is ICloseEx)
						{
							((ICloseEx)m_ShadowStream).CloseEx(CloseExState.Abort | CloseExState.Silent);
						}
						else
						{
							m_ShadowStream.Close();
						}
					}
					catch (Exception exception)
					{
						if (NclUtilities.IsFatal(exception))
						{
							throw;
						}
					}
					catch
					{
					}
				}
				if (!flag || m_ThrowOnWriteError)
				{
					throw;
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
			if (!transportResult.CompletedSynchronously)
			{
				_ = transportResult.AsyncState;
				ReadComplete(transportResult);
			}
		}

		private void ReadComplete(IAsyncResult transportResult)
		{
			while (true)
			{
				InnerAsyncResult innerAsyncResult = transportResult.AsyncState as InnerAsyncResult;
				try
				{
					if (!innerAsyncResult.IsWriteCompletion)
					{
						innerAsyncResult.Count = m_OriginalStream.EndRead(transportResult);
						if (innerAsyncResult.Count == 0)
						{
							m_SeenReadEOF = true;
						}
						if (!m_ShadowStreamIsDead)
						{
							innerAsyncResult.IsWriteCompletion = true;
							transportResult = m_ShadowStream.BeginWrite(innerAsyncResult.Buffer, innerAsyncResult.Offset, innerAsyncResult.Count, m_ReadCallback, innerAsyncResult);
							if (transportResult.CompletedSynchronously)
							{
								continue;
							}
							return;
						}
					}
					else
					{
						m_ShadowStream.EndWrite(transportResult);
						innerAsyncResult.IsWriteCompletion = false;
					}
				}
				catch (Exception result)
				{
					if (innerAsyncResult.InternalPeekCompleted)
					{
						throw;
					}
					try
					{
						m_ShadowStreamIsDead = true;
						if (m_ShadowStream is ICloseEx)
						{
							((ICloseEx)m_ShadowStream).CloseEx(CloseExState.Abort | CloseExState.Silent);
						}
						else
						{
							m_ShadowStream.Close();
						}
					}
					catch (Exception)
					{
					}
					catch
					{
					}
					if (!innerAsyncResult.IsWriteCompletion || m_ThrowOnWriteError)
					{
						if (transportResult.CompletedSynchronously)
						{
							throw;
						}
						innerAsyncResult.InvokeCallback(result);
						return;
					}
				}
				catch
				{
					if (innerAsyncResult.InternalPeekCompleted)
					{
						throw;
					}
					try
					{
						m_ShadowStreamIsDead = true;
						if (m_ShadowStream is ICloseEx)
						{
							((ICloseEx)m_ShadowStream).CloseEx(CloseExState.Abort | CloseExState.Silent);
						}
						else
						{
							m_ShadowStream.Close();
						}
					}
					catch (Exception)
					{
					}
					catch
					{
					}
					if (!innerAsyncResult.IsWriteCompletion || m_ThrowOnWriteError)
					{
						if (transportResult.CompletedSynchronously)
						{
							throw;
						}
						innerAsyncResult.InvokeCallback(new Exception(SR.GetString("net_nonClsCompliantException")));
						return;
					}
				}
				try
				{
					if (m_BytesToSkip != 0)
					{
						m_BytesToSkip -= innerAsyncResult.Count;
						innerAsyncResult.Count = (int)((m_BytesToSkip < innerAsyncResult.Buffer.Length) ? m_BytesToSkip : innerAsyncResult.Buffer.Length);
						if (m_BytesToSkip == 0)
						{
							transportResult = innerAsyncResult;
							innerAsyncResult = innerAsyncResult.AsyncState as InnerAsyncResult;
						}
						transportResult = m_OriginalStream.BeginRead(innerAsyncResult.Buffer, innerAsyncResult.Offset, innerAsyncResult.Count, m_ReadCallback, innerAsyncResult);
						if (transportResult.CompletedSynchronously)
						{
							continue;
						}
						return;
					}
					innerAsyncResult.InvokeCallback(innerAsyncResult.Count);
					return;
				}
				catch (Exception result2)
				{
					if (innerAsyncResult.InternalPeekCompleted)
					{
						throw;
					}
					try
					{
						m_ShadowStreamIsDead = true;
						if (m_ShadowStream is ICloseEx)
						{
							((ICloseEx)m_ShadowStream).CloseEx(CloseExState.Abort | CloseExState.Silent);
						}
						else
						{
							m_ShadowStream.Close();
						}
					}
					catch (Exception)
					{
					}
					catch
					{
					}
					if (transportResult.CompletedSynchronously)
					{
						throw;
					}
					innerAsyncResult.InvokeCallback(result2);
					return;
				}
				catch
				{
					if (innerAsyncResult.InternalPeekCompleted)
					{
						throw;
					}
					try
					{
						m_ShadowStreamIsDead = true;
						if (m_ShadowStream is ICloseEx)
						{
							((ICloseEx)m_ShadowStream).CloseEx(CloseExState.Abort | CloseExState.Silent);
						}
						else
						{
							m_ShadowStream.Close();
						}
					}
					catch (Exception)
					{
					}
					catch
					{
					}
					if (transportResult.CompletedSynchronously)
					{
						throw;
					}
					innerAsyncResult.InvokeCallback(new Exception(SR.GetString("net_nonClsCompliantException")));
					return;
				}
			}
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (Interlocked.Increment(ref m_ReadNesting) != 1)
			{
				throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", "BeginRead", "read"));
			}
			try
			{
				if (m_ReadCallback == null)
				{
					m_ReadCallback = ReadCallback;
				}
				if (m_ShadowStreamIsDead && m_BytesToSkip == 0)
				{
					return m_OriginalStream.BeginRead(buffer, offset, count, callback, state);
				}
				InnerAsyncResult innerAsyncResult = new InnerAsyncResult(state, callback, buffer, offset, count);
				if (m_BytesToSkip != 0)
				{
					InnerAsyncResult userState = innerAsyncResult;
					innerAsyncResult = new InnerAsyncResult(userState, null, new byte[4096], 0, (int)((m_BytesToSkip < buffer.Length) ? m_BytesToSkip : buffer.Length));
				}
				IAsyncResult asyncResult = m_OriginalStream.BeginRead(innerAsyncResult.Buffer, innerAsyncResult.Offset, innerAsyncResult.Count, m_ReadCallback, innerAsyncResult);
				if (asyncResult.CompletedSynchronously)
				{
					ReadComplete(asyncResult);
				}
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
			if (innerAsyncResult == null && m_OriginalStream.EndRead(asyncResult) == 0)
			{
				m_SeenReadEOF = true;
			}
			bool flag = false;
			try
			{
				innerAsyncResult.InternalWaitForCompletion();
				if (innerAsyncResult.Result is Exception)
				{
					throw (Exception)innerAsyncResult.Result;
				}
				flag = true;
			}
			finally
			{
				if (!flag && !m_ShadowStreamIsDead)
				{
					m_ShadowStreamIsDead = true;
					if (m_ShadowStream is ICloseEx)
					{
						((ICloseEx)m_ShadowStream).CloseEx(CloseExState.Abort | CloseExState.Silent);
					}
					else
					{
						m_ShadowStream.Close();
					}
				}
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
			if (Interlocked.Increment(ref _Disposed) != 1)
			{
				return;
			}
			if (closeState == CloseExState.Silent)
			{
				try
				{
					int num;
					for (int i = 0; i < ConnectStream.s_DrainingBuffer.Length; i += num)
					{
						if ((num = Read(ConnectStream.s_DrainingBuffer, 0, ConnectStream.s_DrainingBuffer.Length)) <= 0)
						{
							break;
						}
					}
				}
				catch (Exception ex)
				{
					if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
					{
						throw;
					}
				}
				catch
				{
				}
			}
			Dispose(disposing: true, closeState);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing, CloseExState closeState)
		{
			try
			{
				ICloseEx closeEx = m_OriginalStream as ICloseEx;
				if (closeEx != null)
				{
					closeEx.CloseEx(closeState);
				}
				else
				{
					m_OriginalStream.Close();
				}
			}
			finally
			{
				if (!m_SeenReadEOF)
				{
					closeState |= CloseExState.Abort;
				}
				if (m_ShadowStream is ICloseEx)
				{
					((ICloseEx)m_ShadowStream).CloseEx(closeState);
				}
				else
				{
					m_ShadowStream.Close();
				}
			}
			if (!disposing)
			{
				m_OriginalStream = null;
				m_ShadowStream = null;
			}
			base.Dispose(disposing);
		}
	}
}
