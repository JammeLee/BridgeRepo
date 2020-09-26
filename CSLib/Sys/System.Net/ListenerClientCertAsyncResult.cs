using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net
{
	internal class ListenerClientCertAsyncResult : LazyAsyncResult
	{
		private unsafe NativeOverlapped* m_pOverlapped;

		private byte[] m_BackingBuffer;

		private unsafe UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* m_MemoryBlob;

		private uint m_Size;

		private static readonly IOCompletionCallback s_IOCallback = WaitCallback;

		internal unsafe NativeOverlapped* NativeOverlapped => m_pOverlapped;

		internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* RequestBlob => m_MemoryBlob;

		internal ListenerClientCertAsyncResult(object asyncObject, object userState, AsyncCallback callback, uint size)
			: base(asyncObject, userState, callback)
		{
			Reset(size);
		}

		internal unsafe void Reset(uint size)
		{
			if (size != m_Size)
			{
				if (m_Size != 0)
				{
					Overlapped.Free(m_pOverlapped);
				}
				m_Size = size;
				if (size == 0)
				{
					m_pOverlapped = null;
					m_MemoryBlob = null;
					m_BackingBuffer = null;
				}
				else
				{
					m_BackingBuffer = new byte[checked((int)size)];
					Overlapped overlapped = new Overlapped();
					overlapped.AsyncResult = this;
					m_pOverlapped = overlapped.Pack(s_IOCallback, m_BackingBuffer);
					m_MemoryBlob = (UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(m_BackingBuffer, 0);
				}
			}
		}

		private unsafe static void WaitCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
		{
			Overlapped overlapped = Overlapped.Unpack(nativeOverlapped);
			ListenerClientCertAsyncResult listenerClientCertAsyncResult = (ListenerClientCertAsyncResult)overlapped.AsyncResult;
			HttpListenerRequest httpListenerRequest = (HttpListenerRequest)listenerClientCertAsyncResult.AsyncObject;
			object result = null;
			try
			{
				if (errorCode == 234)
				{
					UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* requestBlob = listenerClientCertAsyncResult.RequestBlob;
					listenerClientCertAsyncResult.Reset(numBytes + requestBlob->CertEncodedSize);
					uint num = 0u;
					errorCode = UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate(httpListenerRequest.HttpListenerContext.RequestQueueHandle, httpListenerRequest.m_ConnectionId, 0u, listenerClientCertAsyncResult.m_MemoryBlob, listenerClientCertAsyncResult.m_Size, &num, listenerClientCertAsyncResult.m_pOverlapped);
					if (errorCode == 997 || errorCode == 0)
					{
						return;
					}
				}
				if (errorCode != 0)
				{
					listenerClientCertAsyncResult.ErrorCode = (int)errorCode;
					result = new HttpListenerException((int)errorCode);
				}
				else
				{
					UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* memoryBlob = listenerClientCertAsyncResult.m_MemoryBlob;
					if (memoryBlob != null)
					{
						if (memoryBlob->pCertEncoded != null)
						{
							try
							{
								byte[] array = new byte[memoryBlob->CertEncodedSize];
								Marshal.Copy((IntPtr)memoryBlob->pCertEncoded, array, 0, array.Length);
								X509Certificate2 x509Certificate2 = (httpListenerRequest.ClientCertificate = new X509Certificate2(array));
								result = x509Certificate2;
							}
							catch (CryptographicException ex)
							{
								result = ex;
							}
							catch (SecurityException ex2)
							{
								result = ex2;
							}
						}
						httpListenerRequest.SetClientCertificateError((int)memoryBlob->CertFlags);
					}
				}
			}
			catch (Exception ex3)
			{
				if (NclUtilities.IsFatal(ex3))
				{
					throw;
				}
				result = ex3;
			}
			finally
			{
				if (errorCode != 997)
				{
					httpListenerRequest.ClientCertState = ListenerClientCertState.Completed;
				}
			}
			listenerClientCertAsyncResult.InvokeCallback(result);
		}

		protected unsafe override void Cleanup()
		{
			if (m_pOverlapped != null)
			{
				m_MemoryBlob = null;
				Overlapped.Free(m_pOverlapped);
				m_pOverlapped = null;
			}
			GC.SuppressFinalize(this);
			base.Cleanup();
		}

		unsafe ~ListenerClientCertAsyncResult()
		{
			if (m_pOverlapped != null && !NclUtilities.HasShutdownStarted)
			{
				Overlapped.Free(m_pOverlapped);
				m_pOverlapped = null;
			}
		}
	}
}
