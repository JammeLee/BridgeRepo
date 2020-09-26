using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Net
{
	internal class HttpResponseStream : Stream
	{
		private HttpListenerContext m_HttpContext;

		private long m_LeftToWrite = long.MinValue;

		private bool m_Closed;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override bool CanRead => false;

		public override long Length
		{
			get
			{
				throw new NotSupportedException(SR.GetString("net_noseek"));
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException(SR.GetString("net_noseek"));
			}
			set
			{
				throw new NotSupportedException(SR.GetString("net_noseek"));
			}
		}

		internal HttpResponseStream(HttpListenerContext httpContext)
		{
			m_HttpContext = httpContext;
		}

		internal UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS ComputeLeftToWrite()
		{
			UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS result = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE;
			if (!m_HttpContext.Response.ComputedHeaders)
			{
				result = m_HttpContext.Response.ComputeHeaders();
			}
			if (m_LeftToWrite == long.MinValue)
			{
				UnsafeNclNativeMethods.HttpApi.HTTP_VERB knownMethod = m_HttpContext.GetKnownMethod();
				m_LeftToWrite = ((knownMethod != UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbHEAD) ? m_HttpContext.Response.ContentLength64 : 0);
				if (m_LeftToWrite == 0)
				{
					Close();
				}
				else if (knownMethod == UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbOPTIONS && m_LeftToWrite > 0)
				{
					throw new ProtocolViolationException(SR.GetString("net_nouploadonget"));
				}
			}
			return result;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override int Read([In][Out] byte[] buffer, int offset, int size)
		{
			throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
		}

		public unsafe override void Write(byte[] buffer, int offset, int size)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "Write", "");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (size < 0 || size > buffer.Length - offset)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS hTTP_FLAGS = ComputeLeftToWrite();
			if (size == 0 || m_Closed)
			{
				if (Logging.On)
				{
					Logging.Exit(Logging.HttpListener, this, "Write", "");
				}
				return;
			}
			if (m_LeftToWrite > 0 && size > m_LeftToWrite)
			{
				throw new ProtocolViolationException(SR.GetString("net_entitytoobig"));
			}
			uint num = (uint)size;
			SafeLocalFree safeLocalFree = null;
			IntPtr zero = IntPtr.Zero;
			bool sentHeaders = m_HttpContext.Response.SentHeaders;
			uint num2;
			try
			{
				try
				{
					fixed (byte* ptr = buffer)
					{
						byte* ptr2 = ptr;
						if (m_HttpContext.Response.BoundaryType == BoundaryType.Chunked)
						{
							string text = size.ToString("x", CultureInfo.InvariantCulture);
							num += (uint)(text.Length + 4);
							safeLocalFree = SafeLocalFree.LocalAlloc((int)num);
							zero = safeLocalFree.DangerousGetHandle();
							for (int i = 0; i < text.Length; i++)
							{
								Marshal.WriteByte(zero, i, (byte)text[i]);
							}
							Marshal.WriteInt16(zero, text.Length, 2573);
							Marshal.Copy(buffer, offset, IntPtrHelper.Add(zero, text.Length + 2), size);
							Marshal.WriteInt16(zero, (int)(num - 2), 2573);
							ptr2 = (byte*)(void*)zero;
							offset = 0;
						}
						UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK hTTP_DATA_CHUNK = default(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK);
						hTTP_DATA_CHUNK.DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
						hTTP_DATA_CHUNK.pBuffer = ptr2 + offset;
						hTTP_DATA_CHUNK.BufferLength = num;
						hTTP_FLAGS |= ((m_LeftToWrite != size) ? UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_SEND_RESPONSE_FLAG_MORE_DATA : UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE);
						if (!sentHeaders)
						{
							num2 = m_HttpContext.Response.SendHeaders(&hTTP_DATA_CHUNK, null, hTTP_FLAGS);
						}
						else
						{
							num2 = UnsafeNclNativeMethods.HttpApi.HttpSendResponseEntityBody(m_HttpContext.RequestQueueHandle, m_HttpContext.RequestId, (uint)hTTP_FLAGS, 1, &hTTP_DATA_CHUNK, null, SafeLocalFree.Zero, 0u, null, null);
							if (m_HttpContext.Listener.IgnoreWriteExceptions)
							{
								num2 = 0u;
							}
						}
					}
				}
				finally
				{
				}
			}
			finally
			{
				safeLocalFree?.Close();
			}
			if (num2 != 0 && num2 != 38)
			{
				Exception ex = new HttpListenerException((int)num2);
				if (Logging.On)
				{
					Logging.Exception(Logging.HttpListener, this, "Write", ex);
				}
				m_HttpContext.Abort();
				throw ex;
			}
			UpdateAfterWrite(num);
			if (Logging.On)
			{
				Logging.Dump(Logging.HttpListener, this, "Write", buffer, offset, (int)num);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "Write", "");
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public unsafe override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (size < 0 || size > buffer.Length - offset)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS hTTP_FLAGS = ComputeLeftToWrite();
			if (size == 0 || m_Closed)
			{
				if (Logging.On)
				{
					Logging.Exit(Logging.HttpListener, this, "BeginWrite", "");
				}
				HttpResponseStreamAsyncResult httpResponseStreamAsyncResult = new HttpResponseStreamAsyncResult(this, state, callback);
				httpResponseStreamAsyncResult.InvokeCallback(0u);
				return httpResponseStreamAsyncResult;
			}
			if (m_LeftToWrite > 0 && size > m_LeftToWrite)
			{
				throw new ProtocolViolationException(SR.GetString("net_entitytoobig"));
			}
			hTTP_FLAGS |= ((m_LeftToWrite != size) ? UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_SEND_RESPONSE_FLAG_MORE_DATA : UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE);
			bool sentHeaders = m_HttpContext.Response.SentHeaders;
			HttpResponseStreamAsyncResult httpResponseStreamAsyncResult2 = new HttpResponseStreamAsyncResult(this, state, callback, buffer, offset, size, m_HttpContext.Response.BoundaryType == BoundaryType.Chunked, sentHeaders);
			UpdateAfterWrite((uint)((m_HttpContext.Response.BoundaryType != BoundaryType.Chunked) ? size : 0));
			uint num;
			try
			{
				if (!sentHeaders)
				{
					num = m_HttpContext.Response.SendHeaders(null, httpResponseStreamAsyncResult2, hTTP_FLAGS);
				}
				else
				{
					m_HttpContext.EnsureBoundHandle();
					num = UnsafeNclNativeMethods.HttpApi.HttpSendResponseEntityBody(m_HttpContext.RequestQueueHandle, m_HttpContext.RequestId, (uint)hTTP_FLAGS, httpResponseStreamAsyncResult2.dataChunkCount, httpResponseStreamAsyncResult2.pDataChunks, null, SafeLocalFree.Zero, 0u, httpResponseStreamAsyncResult2.m_pOverlapped, null);
				}
			}
			catch (Exception e)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.HttpListener, this, "BeginWrite", e);
				}
				httpResponseStreamAsyncResult2.InternalCleanup();
				m_HttpContext.Abort();
				throw;
			}
			if (num != 0 && num != 997)
			{
				httpResponseStreamAsyncResult2.InternalCleanup();
				if (!m_HttpContext.Listener.IgnoreWriteExceptions || !sentHeaders)
				{
					Exception ex = new HttpListenerException((int)num);
					if (Logging.On)
					{
						Logging.Exception(Logging.HttpListener, this, "BeginWrite", ex);
					}
					m_HttpContext.Abort();
					throw ex;
				}
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "BeginWrite", "");
			}
			return httpResponseStreamAsyncResult2;
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "EndWrite", "");
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			HttpResponseStreamAsyncResult httpResponseStreamAsyncResult = asyncResult as HttpResponseStreamAsyncResult;
			if (httpResponseStreamAsyncResult == null || httpResponseStreamAsyncResult.AsyncObject != this)
			{
				throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
			}
			if (httpResponseStreamAsyncResult.EndCalled)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndWrite"));
			}
			httpResponseStreamAsyncResult.EndCalled = true;
			object obj = httpResponseStreamAsyncResult.InternalWaitForCompletion();
			Exception ex = obj as Exception;
			if (ex != null)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.HttpListener, this, "EndWrite", ex);
				}
				m_HttpContext.Abort();
				throw ex;
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "EndWrite", "");
			}
		}

		private void UpdateAfterWrite(uint dataWritten)
		{
			if (m_LeftToWrite > 0)
			{
				m_LeftToWrite -= dataWritten;
			}
			if (m_LeftToWrite == 0)
			{
				m_Closed = true;
			}
		}

		protected unsafe override void Dispose(bool disposing)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "Close", "");
			}
			try
			{
				if (disposing)
				{
					if (m_Closed)
					{
						if (Logging.On)
						{
							Logging.Exit(Logging.HttpListener, this, "Close", "");
						}
						return;
					}
					m_Closed = true;
					UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS hTTP_FLAGS = ComputeLeftToWrite();
					if (m_LeftToWrite > 0)
					{
						throw new InvalidOperationException(SR.GetString("net_io_notenoughbyteswritten"));
					}
					bool sentHeaders = m_HttpContext.Response.SentHeaders;
					if (sentHeaders && m_LeftToWrite == 0)
					{
						if (Logging.On)
						{
							Logging.Exit(Logging.HttpListener, this, "Close", "");
						}
						return;
					}
					uint num = 0u;
					if ((m_HttpContext.Response.BoundaryType == BoundaryType.Chunked || m_HttpContext.Response.BoundaryType == BoundaryType.None) && string.Compare(m_HttpContext.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (m_HttpContext.Response.BoundaryType == BoundaryType.None)
						{
							hTTP_FLAGS |= UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY;
						}
						try
						{
							fixed (void* pBuffer = NclConstants.ChunkTerminator)
							{
								UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* ptr = null;
								if (m_HttpContext.Response.BoundaryType == BoundaryType.Chunked)
								{
									UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK hTTP_DATA_CHUNK = default(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK);
									hTTP_DATA_CHUNK.DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
									hTTP_DATA_CHUNK.pBuffer = (byte*)pBuffer;
									hTTP_DATA_CHUNK.BufferLength = (uint)NclConstants.ChunkTerminator.Length;
									ptr = &hTTP_DATA_CHUNK;
								}
								if (!sentHeaders)
								{
									num = m_HttpContext.Response.SendHeaders(ptr, null, hTTP_FLAGS);
								}
								else
								{
									num = UnsafeNclNativeMethods.HttpApi.HttpSendResponseEntityBody(m_HttpContext.RequestQueueHandle, m_HttpContext.RequestId, (uint)hTTP_FLAGS, (ushort)((ptr != null) ? 1 : 0), ptr, null, SafeLocalFree.Zero, 0u, null, null);
									if (m_HttpContext.Listener.IgnoreWriteExceptions)
									{
										num = 0u;
									}
								}
							}
						}
						finally
						{
						}
					}
					else if (!sentHeaders)
					{
						num = m_HttpContext.Response.SendHeaders(null, null, hTTP_FLAGS);
					}
					if (num != 0 && num != 38)
					{
						Exception ex = new HttpListenerException((int)num);
						if (Logging.On)
						{
							Logging.Exception(Logging.HttpListener, this, "Close", ex);
						}
						m_HttpContext.Abort();
						throw ex;
					}
					m_LeftToWrite = 0L;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "Dispose", "");
			}
		}
	}
}
