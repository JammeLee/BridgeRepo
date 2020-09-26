using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace System.Net
{
	public sealed class HttpListenerRequest
	{
		private enum SslStatus : byte
		{
			Insecure,
			NoClientCert,
			ClientCert
		}

		private static class Helpers
		{
			private class UrlDecoder
			{
				private int _bufferSize;

				private int _numChars;

				private char[] _charBuffer;

				private int _numBytes;

				private byte[] _byteBuffer;

				private Encoding _encoding;

				private void FlushBytes()
				{
					if (_numBytes > 0)
					{
						_numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
						_numBytes = 0;
					}
				}

				internal UrlDecoder(int bufferSize, Encoding encoding)
				{
					_bufferSize = bufferSize;
					_encoding = encoding;
					_charBuffer = new char[bufferSize];
				}

				internal void AddChar(char ch)
				{
					if (_numBytes > 0)
					{
						FlushBytes();
					}
					_charBuffer[_numChars++] = ch;
				}

				internal void AddByte(byte b)
				{
					if (_byteBuffer == null)
					{
						_byteBuffer = new byte[_bufferSize];
					}
					_byteBuffer[_numBytes++] = b;
				}

				internal string GetString()
				{
					if (_numBytes > 0)
					{
						FlushBytes();
					}
					if (_numChars > 0)
					{
						return new string(_charBuffer, 0, _numChars);
					}
					return string.Empty;
				}
			}

			internal static string GetAttributeFromHeader(string headerValue, string attrName)
			{
				if (headerValue == null)
				{
					return null;
				}
				int length = headerValue.Length;
				int length2 = attrName.Length;
				int i;
				for (i = 1; i < length; i += length2)
				{
					i = CultureInfo.InvariantCulture.CompareInfo.IndexOf(headerValue, attrName, i, CompareOptions.IgnoreCase);
					if (i < 0 || i + length2 >= length)
					{
						break;
					}
					char c = headerValue[i - 1];
					char c2 = headerValue[i + length2];
					if ((c == ';' || c == ',' || char.IsWhiteSpace(c)) && (c2 == '=' || char.IsWhiteSpace(c2)))
					{
						break;
					}
				}
				if (i < 0 || i >= length)
				{
					return null;
				}
				for (i += length2; i < length && char.IsWhiteSpace(headerValue[i]); i++)
				{
				}
				if (i >= length || headerValue[i] != '=')
				{
					return null;
				}
				for (i++; i < length && char.IsWhiteSpace(headerValue[i]); i++)
				{
				}
				if (i >= length)
				{
					return null;
				}
				string text = null;
				int num;
				if (i < length && headerValue[i] == '"')
				{
					if (i == length - 1)
					{
						return null;
					}
					num = headerValue.IndexOf('"', i + 1);
					if (num < 0 || num == i + 1)
					{
						return null;
					}
					return headerValue.Substring(i + 1, num - i - 1).Trim();
				}
				for (num = i; num < length && headerValue[num] != ' ' && headerValue[num] != ','; num++)
				{
				}
				if (num == i)
				{
					return null;
				}
				return headerValue.Substring(i, num - i).Trim();
			}

			internal static string[] ParseMultivalueHeader(string s)
			{
				int num = s?.Length ?? 0;
				if (num == 0)
				{
					return null;
				}
				ArrayList arrayList = new ArrayList();
				int num2 = 0;
				while (num2 < num)
				{
					int num3 = s.IndexOf(',', num2);
					if (num3 < 0)
					{
						num3 = num;
					}
					arrayList.Add(s.Substring(num2, num3 - num2));
					num2 = num3 + 1;
					if (num2 < num && s[num2] == ' ')
					{
						num2++;
					}
				}
				int count = arrayList.Count;
				if (count == 0)
				{
					return null;
				}
				string[] array = new string[count];
				arrayList.CopyTo(0, array, 0, count);
				return array;
			}

			private static string UrlDecodeStringFromStringInternal(string s, Encoding e)
			{
				int length = s.Length;
				UrlDecoder urlDecoder = new UrlDecoder(length, e);
				for (int i = 0; i < length; i++)
				{
					char c = s[i];
					switch (c)
					{
					case '+':
						c = ' ';
						break;
					case '%':
						if (i >= length - 2)
						{
							break;
						}
						if (s[i + 1] == 'u' && i < length - 5)
						{
							int num = HexToInt(s[i + 2]);
							int num2 = HexToInt(s[i + 3]);
							int num3 = HexToInt(s[i + 4]);
							int num4 = HexToInt(s[i + 5]);
							if (num >= 0 && num2 >= 0 && num3 >= 0 && num4 >= 0)
							{
								c = (char)((num << 12) | (num2 << 8) | (num3 << 4) | num4);
								i += 5;
								urlDecoder.AddChar(c);
								continue;
							}
						}
						else
						{
							int num5 = HexToInt(s[i + 1]);
							int num6 = HexToInt(s[i + 2]);
							if (num5 >= 0 && num6 >= 0)
							{
								byte b = (byte)((num5 << 4) | num6);
								i += 2;
								urlDecoder.AddByte(b);
								continue;
							}
						}
						break;
					}
					if ((c & 0xFF80) == 0)
					{
						urlDecoder.AddByte((byte)c);
					}
					else
					{
						urlDecoder.AddChar(c);
					}
				}
				return urlDecoder.GetString();
			}

			private static int HexToInt(char h)
			{
				if (h < '0' || h > '9')
				{
					if (h < 'a' || h > 'f')
					{
						if (h < 'A' || h > 'F')
						{
							return -1;
						}
						return h - 65 + 10;
					}
					return h - 97 + 10;
				}
				return h - 48;
			}

			internal static void FillFromString(NameValueCollection nvc, string s, bool urlencoded, Encoding encoding)
			{
				int num = s?.Length ?? 0;
				for (int i = ((s.Length > 0 && s[0] == '?') ? 1 : 0); i < num; i++)
				{
					int num2 = i;
					int num3 = -1;
					for (; i < num; i++)
					{
						switch (s[i])
						{
						case '=':
							if (num3 < 0)
							{
								num3 = i;
							}
							continue;
						default:
							continue;
						case '&':
							break;
						}
						break;
					}
					string text = null;
					string text2 = null;
					if (num3 >= 0)
					{
						text = s.Substring(num2, num3 - num2);
						text2 = s.Substring(num3 + 1, i - num3 - 1);
					}
					else
					{
						text2 = s.Substring(num2, i - num2);
					}
					if (urlencoded)
					{
						nvc.Add((text == null) ? null : UrlDecodeStringFromStringInternal(text, encoding), UrlDecodeStringFromStringInternal(text2, encoding));
					}
					else
					{
						nvc.Add(text, text2);
					}
					if (i == num - 1 && s[i] == '&')
					{
						nvc.Add(null, "");
					}
				}
			}
		}

		internal const uint CertBoblSize = 1500u;

		private Uri m_RequestUri;

		private ulong m_RequestId;

		internal ulong m_ConnectionId;

		private SslStatus m_SslStatus;

		private string m_RawUrl;

		private string m_CookedUrl;

		private long m_ContentLength;

		private Stream m_RequestStream;

		private string m_HttpMethod;

		private TriState m_KeepAlive;

		private Version m_Version;

		private WebHeaderCollection m_WebHeaders;

		private IPEndPoint m_LocalEndPoint;

		private IPEndPoint m_RemoteEndPoint;

		private BoundaryType m_BoundaryType;

		private ListenerClientCertState m_ClientCertState;

		private X509Certificate2 m_ClientCertificate;

		private int m_ClientCertificateError;

		private RequestContextBase m_MemoryBlob;

		private CookieCollection m_Cookies;

		private HttpListenerContext m_HttpContext;

		private bool m_IsDisposed;

		private string m_ServiceName;

		internal HttpListenerContext HttpListenerContext => m_HttpContext;

		internal byte[] RequestBuffer
		{
			get
			{
				CheckDisposed();
				return m_MemoryBlob.RequestBuffer;
			}
		}

		internal IntPtr OriginalBlobAddress
		{
			get
			{
				CheckDisposed();
				return m_MemoryBlob.OriginalBlobAddress;
			}
		}

		public unsafe Guid RequestTraceIdentifier
		{
			get
			{
				Guid result = default(Guid);
				*(ulong*)(8 + (byte*)(&result)) = RequestId;
				return result;
			}
		}

		internal ulong RequestId => m_RequestId;

		public string[] AcceptTypes => Helpers.ParseMultivalueHeader(GetKnownHeader(HttpRequestHeader.Accept));

		public Encoding ContentEncoding
		{
			get
			{
				if (UserAgent != null && CultureInfo.InvariantCulture.CompareInfo.IsPrefix(UserAgent, "UP"))
				{
					string text = Headers["x-up-devcap-post-charset"];
					if (text != null && text.Length > 0)
					{
						try
						{
							return Encoding.GetEncoding(text);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				if (HasEntityBody && ContentType != null)
				{
					string attributeFromHeader = Helpers.GetAttributeFromHeader(ContentType, "charset");
					if (attributeFromHeader != null)
					{
						try
						{
							return Encoding.GetEncoding(attributeFromHeader);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				return Encoding.Default;
			}
		}

		public long ContentLength64
		{
			get
			{
				if (m_BoundaryType == BoundaryType.None)
				{
					if (GetKnownHeader(HttpRequestHeader.TransferEncoding) == "chunked")
					{
						m_BoundaryType = BoundaryType.Chunked;
						m_ContentLength = -1L;
					}
					else
					{
						m_ContentLength = 0L;
						m_BoundaryType = BoundaryType.ContentLength;
						string knownHeader = GetKnownHeader(HttpRequestHeader.ContentLength);
						if (knownHeader != null && !long.TryParse(knownHeader, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out m_ContentLength))
						{
							m_ContentLength = 0L;
							m_BoundaryType = BoundaryType.Invalid;
						}
					}
				}
				return m_ContentLength;
			}
		}

		public string ContentType => GetKnownHeader(HttpRequestHeader.ContentType);

		public NameValueCollection Headers
		{
			get
			{
				if (m_WebHeaders == null)
				{
					m_WebHeaders = UnsafeNclNativeMethods.HttpApi.GetHeaders(RequestBuffer, OriginalBlobAddress);
				}
				return m_WebHeaders;
			}
		}

		public string HttpMethod
		{
			get
			{
				if (m_HttpMethod == null)
				{
					m_HttpMethod = UnsafeNclNativeMethods.HttpApi.GetVerb(RequestBuffer, OriginalBlobAddress);
				}
				return m_HttpMethod;
			}
		}

		public Stream InputStream
		{
			get
			{
				if (Logging.On)
				{
					Logging.Enter(Logging.HttpListener, this, "InputStream_get", "");
				}
				if (m_RequestStream == null)
				{
					m_RequestStream = (HasEntityBody ? new HttpRequestStream(HttpListenerContext) : Stream.Null);
				}
				if (Logging.On)
				{
					Logging.Exit(Logging.HttpListener, this, "InputStream_get", "");
				}
				return m_RequestStream;
			}
		}

		public bool IsAuthenticated
		{
			get
			{
				IPrincipal user = HttpListenerContext.User;
				if (user != null && user.Identity != null)
				{
					return user.Identity.IsAuthenticated;
				}
				return false;
			}
		}

		public bool IsLocal => LocalEndPoint.Address == RemoteEndPoint.Address;

		internal bool InternalIsLocal => LocalEndPoint.Address.Equals(RemoteEndPoint.Address);

		public bool IsSecureConnection => m_SslStatus != SslStatus.Insecure;

		public NameValueCollection QueryString
		{
			get
			{
				NameValueCollection nameValueCollection = new NameValueCollection();
				Helpers.FillFromString(nameValueCollection, Url.Query, urlencoded: true, ContentEncoding);
				return nameValueCollection;
			}
		}

		public string RawUrl => m_RawUrl;

		public string ServiceName
		{
			get
			{
				return m_ServiceName;
			}
			internal set
			{
				m_ServiceName = value;
			}
		}

		public Uri Url => RequestUri;

		public Uri UrlReferrer
		{
			get
			{
				string knownHeader = GetKnownHeader(HttpRequestHeader.Referer);
				if (knownHeader == null)
				{
					return null;
				}
				if (!Uri.TryCreate(knownHeader, UriKind.RelativeOrAbsolute, out var result))
				{
					return null;
				}
				return result;
			}
		}

		public string UserAgent => GetKnownHeader(HttpRequestHeader.UserAgent);

		public string UserHostAddress => LocalEndPoint.ToString();

		public string UserHostName => GetKnownHeader(HttpRequestHeader.Host);

		public string[] UserLanguages => Helpers.ParseMultivalueHeader(GetKnownHeader(HttpRequestHeader.AcceptLanguage));

		public int ClientCertificateError
		{
			get
			{
				if (m_ClientCertState == ListenerClientCertState.NotInitialized)
				{
					throw new InvalidOperationException(SR.GetString("net_listener_mustcall", "GetClientCertificate()/BeginGetClientCertificate()"));
				}
				if (m_ClientCertState == ListenerClientCertState.InProgress)
				{
					throw new InvalidOperationException(SR.GetString("net_listener_mustcompletecall", "GetClientCertificate()/BeginGetClientCertificate()"));
				}
				return m_ClientCertificateError;
			}
		}

		internal X509Certificate2 ClientCertificate
		{
			set
			{
				m_ClientCertificate = value;
			}
		}

		internal ListenerClientCertState ClientCertState
		{
			set
			{
				m_ClientCertState = value;
			}
		}

		public TransportContext TransportContext => new HttpListenerRequestContext(this);

		public CookieCollection Cookies
		{
			get
			{
				if (m_Cookies == null)
				{
					string knownHeader = GetKnownHeader(HttpRequestHeader.Cookie);
					if (knownHeader != null && knownHeader.Length > 0)
					{
						m_Cookies = ParseCookies(RequestUri, knownHeader);
					}
					if (m_Cookies == null)
					{
						m_Cookies = new CookieCollection();
					}
					if (HttpListenerContext.PromoteCookiesToRfc2965)
					{
						for (int i = 0; i < m_Cookies.Count; i++)
						{
							if (m_Cookies[i].Variant == CookieVariant.Rfc2109)
							{
								m_Cookies[i].Variant = CookieVariant.Rfc2965;
							}
						}
					}
				}
				return m_Cookies;
			}
		}

		public Version ProtocolVersion => m_Version;

		public bool HasEntityBody
		{
			get
			{
				if ((ContentLength64 <= 0 || m_BoundaryType != 0) && m_BoundaryType != BoundaryType.Chunked)
				{
					return m_BoundaryType == BoundaryType.Multipart;
				}
				return true;
			}
		}

		public bool KeepAlive
		{
			get
			{
				if (m_KeepAlive == TriState.Unspecified)
				{
					string text = Headers["Proxy-Connection"];
					if (string.IsNullOrEmpty(text))
					{
						text = GetKnownHeader(HttpRequestHeader.Connection);
					}
					if (string.IsNullOrEmpty(text))
					{
						if (ProtocolVersion >= HttpVersion.Version11)
						{
							m_KeepAlive = TriState.True;
						}
						else
						{
							text = GetKnownHeader(HttpRequestHeader.KeepAlive);
							m_KeepAlive = ((!string.IsNullOrEmpty(text)) ? TriState.True : TriState.False);
						}
					}
					else
					{
						m_KeepAlive = ((text.ToLower(CultureInfo.InvariantCulture).IndexOf("close") < 0 || text.ToLower(CultureInfo.InvariantCulture).IndexOf("keep-alive") >= 0) ? TriState.True : TriState.False);
					}
				}
				return m_KeepAlive == TriState.True;
			}
		}

		public IPEndPoint RemoteEndPoint
		{
			get
			{
				if (m_RemoteEndPoint == null)
				{
					m_RemoteEndPoint = UnsafeNclNativeMethods.HttpApi.GetRemoteEndPoint(RequestBuffer, OriginalBlobAddress);
				}
				return m_RemoteEndPoint;
			}
		}

		public IPEndPoint LocalEndPoint
		{
			get
			{
				if (m_LocalEndPoint == null)
				{
					m_LocalEndPoint = UnsafeNclNativeMethods.HttpApi.GetLocalEndPoint(RequestBuffer, OriginalBlobAddress);
				}
				return m_LocalEndPoint;
			}
		}

		private string RequestScheme
		{
			get
			{
				if (!IsSecureConnection)
				{
					return "http://";
				}
				return "https://";
			}
		}

		private Uri RequestUri
		{
			get
			{
				if (m_RequestUri == null)
				{
					bool flag = false;
					if (!string.IsNullOrEmpty(m_CookedUrl))
					{
						flag = Uri.TryCreate(m_CookedUrl, UriKind.Absolute, out m_RequestUri);
					}
					if (!flag && RawUrl != null && RawUrl.Length > 0 && RawUrl[0] != '/')
					{
						flag = Uri.TryCreate(RawUrl, UriKind.Absolute, out m_RequestUri);
					}
					if (!flag)
					{
						string str = RequestScheme + UserHostName;
						str = ((RawUrl == null || RawUrl.Length <= 0) ? (str + "/") : ((RawUrl[0] == '/') ? (str + RawUrl) : (str + "/" + RawUrl)));
						flag = Uri.TryCreate(str, UriKind.Absolute, out m_RequestUri);
					}
				}
				return m_RequestUri;
			}
		}

		internal unsafe HttpListenerRequest(HttpListenerContext httpContext, RequestContextBase memoryBlob)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "httpContext#" + ValidationHelper.HashString(httpContext) + " memoryBlob# " + ValidationHelper.HashString((IntPtr)memoryBlob.RequestBlob));
			}
			if (Logging.On)
			{
				Logging.Associate(Logging.HttpListener, this, httpContext);
			}
			m_HttpContext = httpContext;
			m_MemoryBlob = memoryBlob;
			m_BoundaryType = BoundaryType.None;
			m_RequestId = memoryBlob.RequestBlob->RequestId;
			m_ConnectionId = memoryBlob.RequestBlob->ConnectionId;
			m_SslStatus = ((memoryBlob.RequestBlob->pSslInfo != null) ? ((memoryBlob.RequestBlob->pSslInfo->SslClientCertNegotiated == 0) ? SslStatus.NoClientCert : SslStatus.ClientCert) : SslStatus.Insecure);
			if (memoryBlob.RequestBlob->pRawUrl != null && memoryBlob.RequestBlob->RawUrlLength > 0)
			{
				m_RawUrl = Marshal.PtrToStringAnsi((IntPtr)memoryBlob.RequestBlob->pRawUrl, memoryBlob.RequestBlob->RawUrlLength);
			}
			if (memoryBlob.RequestBlob->CookedUrl.pFullUrl != null && memoryBlob.RequestBlob->CookedUrl.FullUrlLength > 0)
			{
				m_CookedUrl = Marshal.PtrToStringUni((IntPtr)memoryBlob.RequestBlob->CookedUrl.pFullUrl, (int)memoryBlob.RequestBlob->CookedUrl.FullUrlLength / 2);
			}
			m_Version = new Version(memoryBlob.RequestBlob->Version.MajorVersion, memoryBlob.RequestBlob->Version.MinorVersion);
			m_ClientCertState = ListenerClientCertState.NotInitialized;
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "httpContext#" + ValidationHelper.HashString(httpContext) + " RequestUri:" + ValidationHelper.ToString(RequestUri) + " Content-Length:" + ValidationHelper.ToString(ContentLength64) + " HTTP Method:" + ValidationHelper.ToString(HttpMethod));
			}
			if (Logging.On)
			{
				StringBuilder stringBuilder = new StringBuilder("HttpListenerRequest Headers:\n");
				for (int i = 0; i < Headers.Count; i++)
				{
					stringBuilder.Append("\t");
					stringBuilder.Append(Headers.GetKey(i));
					stringBuilder.Append(" : ");
					stringBuilder.Append(Headers.Get(i));
					stringBuilder.Append("\n");
				}
				Logging.PrintInfo(Logging.HttpListener, this, ".ctor", stringBuilder.ToString());
			}
		}

		internal void DetachBlob(RequestContextBase memoryBlob)
		{
			if (memoryBlob != null && memoryBlob == m_MemoryBlob)
			{
				m_MemoryBlob = null;
			}
		}

		internal void ReleasePins()
		{
			m_MemoryBlob.ReleasePins();
		}

		internal void SetClientCertificateError(int clientCertificateError)
		{
			m_ClientCertificateError = clientCertificateError;
		}

		public X509Certificate2 GetClientCertificate()
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "GetClientCertificate", "");
			}
			try
			{
				ProcessClientCertificate();
			}
			finally
			{
				if (Logging.On)
				{
					Logging.Exit(Logging.HttpListener, this, "GetClientCertificate", ValidationHelper.ToString(m_ClientCertificate));
				}
			}
			return m_ClientCertificate;
		}

		public IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.HttpListener, this, "BeginGetClientCertificate", "");
			}
			return AsyncProcessClientCertificate(requestCallback, state);
		}

		public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "EndGetClientCertificate", "");
			}
			X509Certificate2 x509Certificate = null;
			try
			{
				if (asyncResult == null)
				{
					throw new ArgumentNullException("asyncResult");
				}
				ListenerClientCertAsyncResult listenerClientCertAsyncResult = asyncResult as ListenerClientCertAsyncResult;
				if (listenerClientCertAsyncResult == null || listenerClientCertAsyncResult.AsyncObject != this)
				{
					throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
				}
				if (listenerClientCertAsyncResult.EndCalled)
				{
					throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndGetClientCertificate"));
				}
				listenerClientCertAsyncResult.EndCalled = true;
				x509Certificate = listenerClientCertAsyncResult.InternalWaitForCompletion() as X509Certificate2;
			}
			finally
			{
				if (Logging.On)
				{
					Logging.Exit(Logging.HttpListener, this, "EndGetClientCertificate", ValidationHelper.HashString(x509Certificate));
				}
			}
			return x509Certificate;
		}

		private CookieCollection ParseCookies(Uri uri, string setCookieHeader)
		{
			CookieCollection cookieCollection = new CookieCollection();
			CookieParser cookieParser = new CookieParser(setCookieHeader);
			while (true)
			{
				Cookie server = cookieParser.GetServer();
				if (server == null)
				{
					break;
				}
				if (server.Name.Length != 0)
				{
					cookieCollection.InternalAdd(server, isStrict: true);
				}
			}
			return cookieCollection;
		}

		internal void Close()
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.HttpListener, this, "Close", "");
			}
			RequestContextBase memoryBlob = m_MemoryBlob;
			if (memoryBlob != null)
			{
				memoryBlob.Close();
				m_MemoryBlob = null;
			}
			m_IsDisposed = true;
			if (Logging.On)
			{
				Logging.Exit(Logging.HttpListener, this, "Close", "");
			}
		}

		private unsafe ListenerClientCertAsyncResult AsyncProcessClientCertificate(AsyncCallback requestCallback, object state)
		{
			if (m_ClientCertState == ListenerClientCertState.InProgress)
			{
				throw new InvalidOperationException(SR.GetString("net_listener_callinprogress", "GetClientCertificate()/BeginGetClientCertificate()"));
			}
			m_ClientCertState = ListenerClientCertState.InProgress;
			HttpListenerContext.EnsureBoundHandle();
			ListenerClientCertAsyncResult listenerClientCertAsyncResult = null;
			if (m_SslStatus != 0)
			{
				uint num = 1500u;
				listenerClientCertAsyncResult = new ListenerClientCertAsyncResult(this, state, requestCallback, num);
				try
				{
					while (true)
					{
						uint num2 = 0u;
						uint num3 = UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate(HttpListenerContext.RequestQueueHandle, m_ConnectionId, 0u, listenerClientCertAsyncResult.RequestBlob, num, &num2, listenerClientCertAsyncResult.NativeOverlapped);
						switch (num3)
						{
						case 234u:
							break;
						default:
							throw new HttpListenerException((int)num3);
						case 0u:
							return listenerClientCertAsyncResult;
						case 997u:
							return listenerClientCertAsyncResult;
						}
						UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* requestBlob = listenerClientCertAsyncResult.RequestBlob;
						num = num2 + requestBlob->CertEncodedSize;
						listenerClientCertAsyncResult.Reset(num);
					}
				}
				catch
				{
					listenerClientCertAsyncResult?.InternalCleanup();
					throw;
				}
			}
			listenerClientCertAsyncResult = new ListenerClientCertAsyncResult(this, state, requestCallback, 0u);
			listenerClientCertAsyncResult.InvokeCallback();
			return listenerClientCertAsyncResult;
		}

		private unsafe void ProcessClientCertificate()
		{
			if (m_ClientCertState == ListenerClientCertState.InProgress)
			{
				throw new InvalidOperationException(SR.GetString("net_listener_callinprogress", "GetClientCertificate()/BeginGetClientCertificate()"));
			}
			m_ClientCertState = ListenerClientCertState.InProgress;
			if (m_SslStatus != 0)
			{
				uint num = 1500u;
				while (true)
				{
					byte[] array = new byte[checked((int)num)];
					try
					{
						fixed (byte* ptr = array)
						{
							UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* ptr2 = (UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO*)ptr;
							uint num2 = 0u;
							switch (UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate(HttpListenerContext.RequestQueueHandle, m_ConnectionId, 0u, ptr2, num, &num2, null))
							{
							case 234u:
								num = num2 + ptr2->CertEncodedSize;
								goto end_IL_0066;
							case 0u:
								if (ptr2 == null)
								{
									break;
								}
								if (ptr2->pCertEncoded != null)
								{
									try
									{
										byte[] array2 = new byte[ptr2->CertEncodedSize];
										Marshal.Copy((IntPtr)ptr2->pCertEncoded, array2, 0, array2.Length);
										m_ClientCertificate = new X509Certificate2(array2);
									}
									catch (CryptographicException)
									{
									}
									catch (SecurityException)
									{
									}
								}
								m_ClientCertificateError = (int)ptr2->CertFlags;
								break;
							}
							goto IL_0100;
							end_IL_0066:;
						}
					}
					finally
					{
					}
				}
			}
			goto IL_0100;
			IL_0100:
			m_ClientCertState = ListenerClientCertState.Completed;
		}

		private string GetKnownHeader(HttpRequestHeader header)
		{
			return UnsafeNclNativeMethods.HttpApi.GetKnownHeader(RequestBuffer, OriginalBlobAddress, (int)header);
		}

		internal ChannelBinding GetChannelBinding()
		{
			return HttpListenerContext.Listener.GetChannelBindingFromTls(m_ConnectionId);
		}

		internal void CheckDisposed()
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}
	}
}
