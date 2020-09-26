using System.Threading;

namespace System.Net
{
	internal class ListenerAsyncResult : LazyAsyncResult
	{
		private static readonly IOCompletionCallback s_IOCallback = WaitCallback;

		private AsyncRequestContext m_RequestContext;

		internal unsafe static IOCompletionCallback IOCallback => s_IOCallback;

		internal ListenerAsyncResult(object asyncObject, object userState, AsyncCallback callback)
			: base(asyncObject, userState, callback)
		{
			m_RequestContext = new AsyncRequestContext(this);
		}

		private unsafe static void WaitCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
		{
			Overlapped overlapped = Overlapped.Unpack(nativeOverlapped);
			ListenerAsyncResult listenerAsyncResult = (ListenerAsyncResult)overlapped.AsyncResult;
			object obj = null;
			try
			{
				if (errorCode != 0 && errorCode != 234)
				{
					listenerAsyncResult.ErrorCode = (int)errorCode;
					obj = new HttpListenerException((int)errorCode);
				}
				else
				{
					HttpListener httpListener = listenerAsyncResult.AsyncObject as HttpListener;
					if (errorCode == 0)
					{
						bool stoleBlob = false;
						try
						{
							obj = httpListener.HandleAuthentication(listenerAsyncResult.m_RequestContext, out stoleBlob);
						}
						finally
						{
							if (stoleBlob)
							{
								listenerAsyncResult.m_RequestContext = ((obj == null) ? new AsyncRequestContext(listenerAsyncResult) : null);
							}
							else
							{
								listenerAsyncResult.m_RequestContext.Reset(0uL, 0u);
							}
						}
					}
					else
					{
						listenerAsyncResult.m_RequestContext.Reset(listenerAsyncResult.m_RequestContext.RequestBlob->RequestId, numBytes);
					}
					if (obj == null)
					{
						uint num = listenerAsyncResult.QueueBeginGetContext();
						if (num != 0 && num != 997)
						{
							obj = new HttpListenerException((int)num);
						}
					}
					if (obj == null)
					{
						return;
					}
				}
			}
			catch (Exception ex)
			{
				if (NclUtilities.IsFatal(ex))
				{
					throw;
				}
				obj = ex;
			}
			listenerAsyncResult.InvokeCallback(obj);
		}

		internal unsafe uint QueueBeginGetContext()
		{
			uint num = 0u;
			while (true)
			{
				(base.AsyncObject as HttpListener).EnsureBoundHandle();
				uint size = 0u;
				num = UnsafeNclNativeMethods.HttpApi.HttpReceiveHttpRequest((base.AsyncObject as HttpListener).RequestQueueHandle, m_RequestContext.RequestBlob->RequestId, 1u, m_RequestContext.RequestBlob, m_RequestContext.Size, &size, m_RequestContext.NativeOverlapped);
				if (num == 87 && m_RequestContext.RequestBlob->RequestId != 0)
				{
					m_RequestContext.RequestBlob->RequestId = 0uL;
					continue;
				}
				if (num != 234)
				{
					break;
				}
				m_RequestContext.Reset(m_RequestContext.RequestBlob->RequestId, size);
			}
			return num;
		}

		protected override void Cleanup()
		{
			if (m_RequestContext != null)
			{
				m_RequestContext.ReleasePins();
				m_RequestContext.Close();
			}
			base.Cleanup();
		}
	}
}
