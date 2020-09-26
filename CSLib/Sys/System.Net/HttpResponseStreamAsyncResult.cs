using System.Runtime.InteropServices;
using System.Threading;

namespace System.Net
{
	internal class HttpResponseStreamAsyncResult : LazyAsyncResult
	{
		internal unsafe NativeOverlapped* m_pOverlapped;

		private UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK[] m_DataChunks;

		internal bool m_SentHeaders;

		private static readonly IOCompletionCallback s_IOCallback = Callback;

		internal ushort dataChunkCount => (ushort)m_DataChunks.Length;

		internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pDataChunks => (UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(m_DataChunks, 0);

		internal HttpResponseStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback)
			: base(asyncObject, userState, callback)
		{
		}

		internal unsafe HttpResponseStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback, byte[] buffer, int offset, int size, bool chunked, bool sentHeaders)
			: base(asyncObject, userState, callback)
		{
			m_SentHeaders = sentHeaders;
			Overlapped overlapped = new Overlapped
			{
				AsyncResult = this
			};
			m_DataChunks = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK[(!chunked) ? 1 : 3];
			object[] array = new object[1 + m_DataChunks.Length];
			array[m_DataChunks.Length] = m_DataChunks;
			int offset2 = 0;
			byte[] array2 = null;
			if (chunked)
			{
				array2 = ConnectStream.GetChunkHeader(size, out offset2);
				m_DataChunks[0] = default(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK);
				m_DataChunks[0].DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
				m_DataChunks[0].BufferLength = (uint)(array2.Length - offset2);
				array[0] = array2;
				m_DataChunks[1] = default(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK);
				m_DataChunks[1].DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
				m_DataChunks[1].BufferLength = (uint)size;
				array[1] = buffer;
				m_DataChunks[2] = default(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK);
				m_DataChunks[2].DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
				m_DataChunks[2].BufferLength = (uint)NclConstants.CRLF.Length;
				array[2] = NclConstants.CRLF;
			}
			else
			{
				m_DataChunks[0] = default(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK);
				m_DataChunks[0].DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
				m_DataChunks[0].BufferLength = (uint)size;
				array[0] = buffer;
			}
			m_pOverlapped = overlapped.Pack(s_IOCallback, array);
			if (chunked)
			{
				m_DataChunks[0].pBuffer = (byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(array2, offset2);
				m_DataChunks[1].pBuffer = (byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
				m_DataChunks[2].pBuffer = (byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(NclConstants.CRLF, 0);
			}
			else
			{
				m_DataChunks[0].pBuffer = (byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
			}
		}

		private unsafe static void Callback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
		{
			Overlapped overlapped = Overlapped.Unpack(nativeOverlapped);
			HttpResponseStreamAsyncResult httpResponseStreamAsyncResult = overlapped.AsyncResult as HttpResponseStreamAsyncResult;
			object obj = null;
			try
			{
				if (errorCode != 0 && errorCode != 38)
				{
					httpResponseStreamAsyncResult.ErrorCode = (int)errorCode;
					obj = new HttpListenerException((int)errorCode);
				}
				else
				{
					obj = ((httpResponseStreamAsyncResult.m_DataChunks.Length == 1) ? httpResponseStreamAsyncResult.m_DataChunks[0].BufferLength : 0u);
					if (Logging.On)
					{
						for (int i = 0; i < httpResponseStreamAsyncResult.m_DataChunks.Length; i++)
						{
							Logging.Dump(Logging.HttpListener, httpResponseStreamAsyncResult, "Callback", (IntPtr)httpResponseStreamAsyncResult.m_DataChunks[0].pBuffer, (int)httpResponseStreamAsyncResult.m_DataChunks[0].BufferLength);
						}
					}
				}
			}
			catch (Exception ex)
			{
				obj = ex;
			}
			catch
			{
				obj = new Exception(SR.GetString("net_nonClsCompliantException"));
			}
			httpResponseStreamAsyncResult.InvokeCallback(obj);
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
}
