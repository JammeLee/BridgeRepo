using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Net
{
	internal class HttpRequestStream : Stream
	{
		private class HttpRequestStreamAsyncResult : LazyAsyncResult
		{
			internal unsafe NativeOverlapped* m_pOverlapped;

			internal unsafe void* m_pPinnedBuffer;

			internal uint m_dataAlreadyRead;

			private static readonly IOCompletionCallback s_IOCallback = Callback;

			internal HttpRequestStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback)
				: base(asyncObject, userState, callback)
			{
			}

			internal HttpRequestStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback, uint dataAlreadyRead)
				: base(asyncObject, userState, callback)
			{
				m_dataAlreadyRead = dataAlreadyRead;
			}

			internal unsafe HttpRequestStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback, byte[] buffer, int offset, uint size, uint dataAlreadyRead)
				: base(asyncObject, userState, callback)
			{
				m_dataAlreadyRead = dataAlreadyRead;
				m_pOverlapped = new Overlapped
				{
					AsyncResult = this
				}.Pack(s_IOCallback, buffer);
				m_pPinnedBuffer = (void*)Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
			}

			private unsafe static void Callback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
			{
				Overlapped overlapped = Overlapped.Unpack(nativeOverlapped);
				HttpRequestStreamAsyncResult httpRequestStreamAsyncResult = overlapped.AsyncResult as HttpRequestStreamAsyncResult;
				object obj = null;
				try
				{
					if (errorCode != 0 && errorCode != 38)
					{
						httpRequestStreamAsyncResult.ErrorCode = (int)errorCode;
						obj = new HttpListenerException((int)errorCode);
					}
					else
					{
						obj = numBytes;
						if (Logging.On)
						{
							Logging.Dump(Logging.HttpListener, httpRequestStreamAsyncResult, "Callback", (IntPtr)httpRequestStreamAsyncResult.m_pPinnedBuffer, (int)numBytes);
						}
					}
				}
				catch (Exception ex)
				{
					obj = ex;
				}
				httpRequestStreamAsyncResult.InvokeCallback(obj);
			}

			protected unsafe override void Cleanup()
			{
				base.Cleanup();
				if (m_pOverlapped != null)
				{
					Overlapped.Free(m_pOverlapped);
				}
			}
		}

		private const int MaxReadSize = 131072;

		private HttpListenerContext m_HttpContext;

		private uint m_DataChunkOffset;

		private int m_DataChunkIndex;

		private bool m_Closed;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override bool CanRead => true;

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

		internal HttpRequestStream(HttpListenerContext httpContext)
		{
			m_HttpContext = httpContext;
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

		public unsafe override int Read([In][Out] byte[] buffer, int offset, int size)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "Read", "");
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
			if (size == 0 || m_Closed)
			{
				if (Logging.On)
				{
					Logging.Exit(Logging.HttpListener, this, "Read", "dataRead:0");
				}
				return 0;
			}
			uint num = 0u;
			if (m_DataChunkIndex != -1)
			{
				num = UnsafeNclNativeMethods.HttpApi.GetChunks(m_HttpContext.Request.RequestBuffer, m_HttpContext.Request.OriginalBlobAddress, ref m_DataChunkIndex, ref m_DataChunkOffset, buffer, offset, size);
			}
			if (m_DataChunkIndex == -1 && num < size)
			{
				uint num2 = 0u;
				uint num3 = 0u;
				offset += (int)num;
				size -= (int)num;
				if (size > 131072)
				{
					size = 131072;
				}
				fixed (byte* ptr = buffer)
				{
					num2 = UnsafeNclNativeMethods.HttpApi.HttpReceiveRequestEntityBody(m_HttpContext.RequestQueueHandle, m_HttpContext.RequestId, 1u, ptr + offset, (uint)size, &num3, null);
					num += num3;
				}
				if (num2 != 0 && num2 != 38)
				{
					Exception ex = new HttpListenerException((int)num2);
					if (Logging.On)
					{
						Logging.Exception(Logging.HttpListener, this, "Read", ex);
					}
					throw ex;
				}
				UpdateAfterRead(num2, num);
			}
			if (Logging.On)
			{
				Logging.Dump(Logging.HttpListener, this, "Read", buffer, offset, (int)num);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "Read", "dataRead:" + num);
			}
			return (int)num;
		}

		private void UpdateAfterRead(uint statusCode, uint dataRead)
		{
			if (statusCode == 38 || dataRead == 0)
			{
				Close();
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public unsafe override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "BeginRead", "");
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
			if (size == 0 || m_Closed)
			{
				if (Logging.On)
				{
					Logging.Exit(Logging.HttpListener, this, "BeginRead", "");
				}
				HttpRequestStreamAsyncResult httpRequestStreamAsyncResult = new HttpRequestStreamAsyncResult(this, state, callback);
				httpRequestStreamAsyncResult.InvokeCallback(0u);
				return httpRequestStreamAsyncResult;
			}
			HttpRequestStreamAsyncResult httpRequestStreamAsyncResult2 = null;
			uint num = 0u;
			if (m_DataChunkIndex != -1)
			{
				num = UnsafeNclNativeMethods.HttpApi.GetChunks(m_HttpContext.Request.RequestBuffer, m_HttpContext.Request.OriginalBlobAddress, ref m_DataChunkIndex, ref m_DataChunkOffset, buffer, offset, size);
				if (m_DataChunkIndex != -1 && num == size)
				{
					httpRequestStreamAsyncResult2 = new HttpRequestStreamAsyncResult(this, state, callback, buffer, offset, (uint)size, 0u);
					httpRequestStreamAsyncResult2.InvokeCallback(num);
				}
			}
			if (m_DataChunkIndex == -1 && num < size)
			{
				uint num2 = 0u;
				offset += (int)num;
				size -= (int)num;
				if (size > 131072)
				{
					size = 131072;
				}
				httpRequestStreamAsyncResult2 = new HttpRequestStreamAsyncResult(this, state, callback, buffer, offset, (uint)size, num);
				try
				{
					byte[] array;
					if ((array = buffer) != null)
					{
						_ = array.Length;
					}
					m_HttpContext.EnsureBoundHandle();
					num2 = UnsafeNclNativeMethods.HttpApi.HttpReceiveRequestEntityBody(m_HttpContext.RequestQueueHandle, m_HttpContext.RequestId, 1u, httpRequestStreamAsyncResult2.m_pPinnedBuffer, (uint)size, null, httpRequestStreamAsyncResult2.m_pOverlapped);
				}
				catch (Exception e)
				{
					if (Logging.On)
					{
						Logging.Exception(Logging.HttpListener, this, "BeginRead", e);
					}
					httpRequestStreamAsyncResult2.InternalCleanup();
					throw;
				}
				if (num2 != 0 && num2 != 997)
				{
					if (num2 == 38)
					{
						httpRequestStreamAsyncResult2.m_pOverlapped->InternalLow = IntPtr.Zero;
					}
					httpRequestStreamAsyncResult2.InternalCleanup();
					if (num2 != 38)
					{
						Exception ex = new HttpListenerException((int)num2);
						if (Logging.On)
						{
							Logging.Exception(Logging.HttpListener, this, "BeginRead", ex);
						}
						httpRequestStreamAsyncResult2.InternalCleanup();
						throw ex;
					}
					httpRequestStreamAsyncResult2 = new HttpRequestStreamAsyncResult(this, state, callback, num);
					httpRequestStreamAsyncResult2.InvokeCallback(0u);
				}
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "BeginRead", "");
			}
			return httpRequestStreamAsyncResult2;
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "EndRead", "");
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			HttpRequestStreamAsyncResult httpRequestStreamAsyncResult = asyncResult as HttpRequestStreamAsyncResult;
			if (httpRequestStreamAsyncResult == null || httpRequestStreamAsyncResult.AsyncObject != this)
			{
				throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
			}
			if (httpRequestStreamAsyncResult.EndCalled)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndRead"));
			}
			httpRequestStreamAsyncResult.EndCalled = true;
			object obj = httpRequestStreamAsyncResult.InternalWaitForCompletion();
			Exception ex = obj as Exception;
			if (ex != null)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.HttpListener, this, "EndRead", ex);
				}
				throw ex;
			}
			uint num = (uint)obj;
			UpdateAfterRead((uint)httpRequestStreamAsyncResult.ErrorCode, num);
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "EndRead", "");
			}
			return (int)(num + httpRequestStreamAsyncResult.m_dataAlreadyRead);
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			throw new InvalidOperationException(SR.GetString("net_readonlystream"));
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			throw new InvalidOperationException(SR.GetString("net_readonlystream"));
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			throw new InvalidOperationException(SR.GetString("net_readonlystream"));
		}

		protected override void Dispose(bool disposing)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "Dispose", "");
			}
			try
			{
				m_Closed = true;
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
