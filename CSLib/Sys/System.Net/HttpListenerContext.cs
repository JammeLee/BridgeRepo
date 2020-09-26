using System.Security.Permissions;
using System.Security.Principal;

namespace System.Net
{
	public sealed class HttpListenerContext
	{
		internal const string NTLM = "NTLM";

		private HttpListener m_Listener;

		private HttpListenerRequest m_Request;

		private HttpListenerResponse m_Response;

		private IPrincipal m_User;

		private string m_MutualAuthentication;

		private bool m_PromoteCookiesToRfc2965;

		public HttpListenerRequest Request => m_Request;

		public HttpListenerResponse Response
		{
			get
			{
				if (Logging.On)
				{
					Logging.Enter(Logging.HttpListener, this, "Response", "");
				}
				if (m_Response == null)
				{
					m_Response = new HttpListenerResponse(this);
				}
				if (Logging.On)
				{
					Logging.Exit(Logging.HttpListener, this, "Response", "");
				}
				return m_Response;
			}
		}

		public IPrincipal User
		{
			get
			{
				if (!(m_User is WindowsPrincipal))
				{
					return m_User;
				}
				new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Demand();
				return m_User;
			}
		}

		internal bool PromoteCookiesToRfc2965 => m_PromoteCookiesToRfc2965;

		internal string MutualAuthentication => m_MutualAuthentication;

		internal HttpListener Listener => m_Listener;

		internal SafeCloseHandle RequestQueueHandle => m_Listener.RequestQueueHandle;

		internal ulong RequestId => Request.RequestId;

		internal unsafe HttpListenerContext(HttpListener httpListener, RequestContextBase memoryBlob)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "httpListener#" + ValidationHelper.HashString(httpListener) + " requestBlob=" + ValidationHelper.HashString((IntPtr)memoryBlob.RequestBlob));
			}
			m_Listener = httpListener;
			m_Request = new HttpListenerRequest(this, memoryBlob);
		}

		internal void SetIdentity(IPrincipal principal, string mutualAuthentication)
		{
			m_MutualAuthentication = mutualAuthentication;
			m_User = principal;
		}

		internal void EnsureBoundHandle()
		{
			m_Listener.EnsureBoundHandle();
		}

		internal void Close()
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "Close()", "");
			}
			try
			{
				if (m_Response != null)
				{
					m_Response.Close();
				}
			}
			finally
			{
				try
				{
					m_Request.Close();
				}
				finally
				{
					IDisposable disposable = ((m_User == null) ? null : (m_User.Identity as IDisposable));
					if (disposable != null && m_User.Identity.AuthenticationType != "NTLM" && !m_Listener.UnsafeConnectionNtlmAuthentication)
					{
						disposable.Dispose();
					}
				}
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "Close", "");
			}
		}

		internal void Abort()
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "Abort", "");
			}
			CancelRequest(RequestQueueHandle, m_Request.RequestId);
			try
			{
				m_Request.Close();
			}
			finally
			{
				((m_User == null) ? null : (m_User.Identity as IDisposable))?.Dispose();
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "Abort", "");
			}
		}

		internal UnsafeNclNativeMethods.HttpApi.HTTP_VERB GetKnownMethod()
		{
			return UnsafeNclNativeMethods.HttpApi.GetKnownVerb(Request.RequestBuffer, Request.OriginalBlobAddress);
		}

		internal unsafe static void CancelRequest(SafeCloseHandle requestQueueHandle, ulong requestId)
		{
			UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK hTTP_DATA_CHUNK = default(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK);
			hTTP_DATA_CHUNK.DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
			hTTP_DATA_CHUNK.pBuffer = (byte*)(&hTTP_DATA_CHUNK);
			UnsafeNclNativeMethods.HttpApi.HttpSendResponseEntityBody(requestQueueHandle, requestId, 1u, 1, &hTTP_DATA_CHUNK, null, SafeLocalFree.Zero, 0u, null, null);
		}
	}
}
