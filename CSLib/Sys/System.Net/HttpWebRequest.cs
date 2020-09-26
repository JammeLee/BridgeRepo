using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Net.Configuration;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Net
{
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable
	{
		private static class AbortState
		{
			public const int Public = 1;

			public const int Internal = 2;
		}

		[Flags]
		private enum Booleans : uint
		{
			AllowAutoRedirect = 0x1u,
			AllowWriteStreamBuffering = 0x2u,
			ExpectContinue = 0x4u,
			ProxySet = 0x10u,
			UnsafeAuthenticatedConnectionSharing = 0x40u,
			IsVersionHttp10 = 0x80u,
			SendChunked = 0x100u,
			EnableDecompression = 0x200u,
			IsTunnelRequest = 0x400u,
			Default = 0x7u
		}

		internal const HttpStatusCode MaxOkStatus = (HttpStatusCode)299;

		private const HttpStatusCode MaxRedirectionStatus = (HttpStatusCode)399;

		private const int RequestLineConstantSize = 12;

		private const string ContinueHeader = "100-continue";

		internal const string ChunkedHeader = "chunked";

		internal const string GZipHeader = "gzip";

		internal const string DeflateHeader = "deflate";

		private const int DefaultReadWriteTimeout = 300000;

		internal const int DefaultContinueTimeout = 350;

		private bool m_Saw100Continue;

		private bool m_KeepAlive = true;

		private bool m_LockConnection;

		private bool m_NtlmKeepAlive;

		private bool m_PreAuthenticate;

		private DecompressionMethods m_AutomaticDecompression;

		private int m_Aborted;

		private bool m_OnceFailed;

		private bool m_Pipelined = true;

		private bool m_Retry = true;

		private bool m_HeadersCompleted;

		private bool m_IsCurrentAuthenticationStateProxy;

		private bool m_SawInitialResponse;

		private bool m_BodyStarted;

		private bool m_RequestSubmitted;

		private bool m_OriginallyBuffered;

		private bool m_Extra401Retry;

		private static readonly byte[] HttpBytes = new byte[5]
		{
			72,
			84,
			84,
			80,
			47
		};

		private static readonly WaitCallback s_EndWriteHeaders_Part2Callback = EndWriteHeaders_Part2Wrapper;

		private static readonly TimerThread.Callback s_ContinueTimeoutCallback = ContinueTimeoutCallback;

		private static readonly TimerThread.Queue s_ContinueTimerQueue = TimerThread.GetOrCreateQueue(350);

		private static readonly TimerThread.Callback s_TimeoutCallback = TimeoutCallback;

		private static readonly WaitCallback s_AbortWrapper = AbortWrapper;

		private static int s_UniqueGroupId;

		private Booleans _Booleans = Booleans.Default;

		private DateTime _CachedIfModifedSince = DateTime.MinValue;

		private TimerThread.Timer m_ContinueTimer;

		private InterlockedGate m_ContinueGate;

		private object m_PendingReturnResult;

		private LazyAsyncResult _WriteAResult;

		private LazyAsyncResult _ReadAResult;

		private LazyAsyncResult _ConnectionAResult;

		private LazyAsyncResult _ConnectionReaderAResult;

		private TriState _RequestIsAsync;

		private HttpContinueDelegate _ContinueDelegate;

		internal ServicePoint _ServicePoint;

		internal HttpWebResponse _HttpResponse;

		private object _CoreResponse;

		private int _NestedWriteSideCheck;

		private KnownHttpVerb _Verb;

		private KnownHttpVerb _OriginVerb;

		private WebHeaderCollection _HttpRequestHeaders;

		private byte[] _WriteBuffer;

		private HttpWriteMode _HttpWriteMode;

		private Uri _Uri;

		private Uri _OriginUri;

		private string _MediaType;

		private long _ContentLength;

		private IWebProxy _Proxy;

		private ProxyChain _ProxyChain;

		private string _ConnectionGroupName;

		private bool m_InternalConnectionGroup;

		private AuthenticationState _ProxyAuthenticationState;

		private AuthenticationState _ServerAuthenticationState;

		private ICredentials _AuthInfo;

		private HttpAbortDelegate _AbortDelegate;

		private ConnectStream _SubmitWriteStream;

		private ConnectStream _OldSubmitWriteStream;

		private int _MaximumAllowedRedirections;

		private int _AutoRedirects;

		private int _RerequestCount;

		private int _Timeout;

		private TimerThread.Timer _Timer;

		private TimerThread.Queue _TimerQueue;

		private int _RequestContinueCount;

		private int _ReadWriteTimeout;

		private CookieContainer _CookieContainer;

		private int _MaximumResponseHeadersLength;

		private UnlockConnectionDelegate _UnlockDelegate;

		private X509CertificateCollection _ClientCertificates;

		internal TimerThread.Timer RequestTimer => _Timer;

		internal bool Aborted => m_Aborted != 0;

		public bool AllowAutoRedirect
		{
			get
			{
				return (_Booleans & Booleans.AllowAutoRedirect) != 0;
			}
			set
			{
				if (value)
				{
					_Booleans |= Booleans.AllowAutoRedirect;
				}
				else
				{
					_Booleans &= ~Booleans.AllowAutoRedirect;
				}
			}
		}

		public bool AllowWriteStreamBuffering
		{
			get
			{
				return (_Booleans & Booleans.AllowWriteStreamBuffering) != 0;
			}
			set
			{
				if (value)
				{
					_Booleans |= Booleans.AllowWriteStreamBuffering;
				}
				else
				{
					_Booleans &= ~Booleans.AllowWriteStreamBuffering;
				}
			}
		}

		private bool ExpectContinue
		{
			get
			{
				return (_Booleans & Booleans.ExpectContinue) != 0;
			}
			set
			{
				if (value)
				{
					_Booleans |= Booleans.ExpectContinue;
				}
				else
				{
					_Booleans &= ~Booleans.ExpectContinue;
				}
			}
		}

		public bool HaveResponse
		{
			get
			{
				if (_ReadAResult != null)
				{
					return _ReadAResult.InternalPeekCompleted;
				}
				return false;
			}
		}

		internal bool NtlmKeepAlive
		{
			get
			{
				return m_NtlmKeepAlive;
			}
			set
			{
				m_NtlmKeepAlive = value;
			}
		}

		internal bool SawInitialResponse
		{
			get
			{
				return m_SawInitialResponse;
			}
			set
			{
				m_SawInitialResponse = value;
			}
		}

		internal bool BodyStarted => m_BodyStarted;

		public bool KeepAlive
		{
			get
			{
				return m_KeepAlive;
			}
			set
			{
				m_KeepAlive = value;
			}
		}

		internal bool LockConnection
		{
			get
			{
				return m_LockConnection;
			}
			set
			{
				m_LockConnection = value;
			}
		}

		public bool Pipelined
		{
			get
			{
				return m_Pipelined;
			}
			set
			{
				m_Pipelined = value;
			}
		}

		public override bool PreAuthenticate
		{
			get
			{
				return m_PreAuthenticate;
			}
			set
			{
				m_PreAuthenticate = value;
			}
		}

		private bool ProxySet
		{
			get
			{
				return (_Booleans & Booleans.ProxySet) != 0;
			}
			set
			{
				if (value)
				{
					_Booleans |= Booleans.ProxySet;
				}
				else
				{
					_Booleans &= ~Booleans.ProxySet;
				}
			}
		}

		private bool RequestSubmitted => m_RequestSubmitted;

		internal bool Saw100Continue
		{
			get
			{
				return m_Saw100Continue;
			}
			set
			{
				m_Saw100Continue = value;
			}
		}

		public bool UnsafeAuthenticatedConnectionSharing
		{
			get
			{
				return (_Booleans & Booleans.UnsafeAuthenticatedConnectionSharing) != 0;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (value)
				{
					_Booleans |= Booleans.UnsafeAuthenticatedConnectionSharing;
				}
				else
				{
					_Booleans &= ~Booleans.UnsafeAuthenticatedConnectionSharing;
				}
			}
		}

		internal bool UnsafeOrProxyAuthenticatedConnectionSharing
		{
			get
			{
				if (!m_IsCurrentAuthenticationStateProxy)
				{
					return UnsafeAuthenticatedConnectionSharing;
				}
				return true;
			}
		}

		private bool IsVersionHttp10
		{
			get
			{
				return (_Booleans & Booleans.IsVersionHttp10) != 0;
			}
			set
			{
				if (value)
				{
					_Booleans |= Booleans.IsVersionHttp10;
				}
				else
				{
					_Booleans &= ~Booleans.IsVersionHttp10;
				}
			}
		}

		public bool SendChunked
		{
			get
			{
				return (_Booleans & Booleans.SendChunked) != 0;
			}
			set
			{
				if (RequestSubmitted)
				{
					throw new InvalidOperationException(SR.GetString("net_writestarted"));
				}
				if (value)
				{
					_Booleans |= Booleans.SendChunked;
				}
				else
				{
					_Booleans &= ~Booleans.SendChunked;
				}
			}
		}

		public DecompressionMethods AutomaticDecompression
		{
			get
			{
				return m_AutomaticDecompression;
			}
			set
			{
				if (RequestSubmitted)
				{
					throw new InvalidOperationException(SR.GetString("net_writestarted"));
				}
				m_AutomaticDecompression = value;
			}
		}

		internal HttpWriteMode HttpWriteMode
		{
			get
			{
				return _HttpWriteMode;
			}
			set
			{
				_HttpWriteMode = value;
			}
		}

		public new static RequestCachePolicy DefaultCachePolicy
		{
			get
			{
				RequestCachePolicy policy = RequestCacheManager.GetBinding(Uri.UriSchemeHttp).Policy;
				if (policy == null)
				{
					return WebRequest.DefaultCachePolicy;
				}
				return policy;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				RequestCacheBinding binding = RequestCacheManager.GetBinding(Uri.UriSchemeHttp);
				RequestCacheManager.SetBinding(Uri.UriSchemeHttp, new RequestCacheBinding(binding.Cache, binding.Validator, value));
			}
		}

		public static int DefaultMaximumResponseHeadersLength
		{
			get
			{
				return SettingsSectionInternal.Section.MaximumResponseHeadersLength;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_toosmall"));
				}
				SettingsSectionInternal.Section.MaximumResponseHeadersLength = value;
			}
		}

		public static int DefaultMaximumErrorResponseLength
		{
			get
			{
				return SettingsSectionInternal.Section.MaximumErrorResponseLength;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_toosmall"));
				}
				SettingsSectionInternal.Section.MaximumErrorResponseLength = value;
			}
		}

		public int MaximumResponseHeadersLength
		{
			get
			{
				return _MaximumResponseHeadersLength;
			}
			set
			{
				if (RequestSubmitted)
				{
					throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
				}
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_toosmall"));
				}
				_MaximumResponseHeadersLength = value;
			}
		}

		internal HttpAbortDelegate AbortDelegate
		{
			set
			{
				_AbortDelegate = value;
			}
		}

		internal LazyAsyncResult ConnectionAsyncResult => _ConnectionAResult;

		internal LazyAsyncResult ConnectionReaderAsyncResult => _ConnectionReaderAResult;

		private bool UserRetrievedWriteStream
		{
			get
			{
				if (_WriteAResult != null)
				{
					return _WriteAResult.InternalPeekCompleted;
				}
				return false;
			}
		}

		internal bool Async
		{
			get
			{
				return _RequestIsAsync != TriState.False;
			}
			set
			{
				if (_RequestIsAsync == TriState.Unspecified)
				{
					_RequestIsAsync = (value ? TriState.True : TriState.False);
				}
			}
		}

		internal UnlockConnectionDelegate UnlockConnectionDelegate
		{
			get
			{
				return _UnlockDelegate;
			}
			set
			{
				_UnlockDelegate = value;
			}
		}

		private bool UsesProxy => ServicePoint.InternalProxyServicePoint;

		internal HttpStatusCode ResponseStatusCode => _HttpResponse.StatusCode;

		internal bool UsesProxySemantics
		{
			get
			{
				if (ServicePoint.InternalProxyServicePoint)
				{
					if ((object)_Uri.Scheme == Uri.UriSchemeHttps)
					{
						return IsTunnelRequest;
					}
					return true;
				}
				return false;
			}
		}

		internal Uri ChallengedUri => CurrentAuthenticationState.ChallengedUri;

		internal AuthenticationState ProxyAuthenticationState
		{
			get
			{
				if (_ProxyAuthenticationState == null)
				{
					_ProxyAuthenticationState = new AuthenticationState(isProxyAuth: true);
				}
				return _ProxyAuthenticationState;
			}
		}

		internal AuthenticationState ServerAuthenticationState
		{
			get
			{
				if (_ServerAuthenticationState == null)
				{
					_ServerAuthenticationState = new AuthenticationState(isProxyAuth: false);
				}
				return _ServerAuthenticationState;
			}
			set
			{
				_ServerAuthenticationState = value;
			}
		}

		internal AuthenticationState CurrentAuthenticationState
		{
			get
			{
				if (!m_IsCurrentAuthenticationStateProxy)
				{
					return _ServerAuthenticationState;
				}
				return _ProxyAuthenticationState;
			}
			set
			{
				m_IsCurrentAuthenticationStateProxy = _ProxyAuthenticationState == value;
			}
		}

		public X509CertificateCollection ClientCertificates
		{
			get
			{
				if (_ClientCertificates == null)
				{
					_ClientCertificates = new X509CertificateCollection();
				}
				return _ClientCertificates;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				_ClientCertificates = value;
			}
		}

		public CookieContainer CookieContainer
		{
			get
			{
				return _CookieContainer;
			}
			set
			{
				_CookieContainer = value;
			}
		}

		public override Uri RequestUri => _OriginUri;

		public override long ContentLength
		{
			get
			{
				return _ContentLength;
			}
			set
			{
				if (RequestSubmitted)
				{
					throw new InvalidOperationException(SR.GetString("net_writestarted"));
				}
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_clsmall"));
				}
				_ContentLength = value;
			}
		}

		public override int Timeout
		{
			get
			{
				return _Timeout;
			}
			set
			{
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_io_timeout_use_ge_zero"));
				}
				if (_Timeout != value)
				{
					_Timeout = value;
					_TimerQueue = null;
				}
			}
		}

		private TimerThread.Queue TimerQueue
		{
			get
			{
				TimerThread.Queue queue = _TimerQueue;
				if (queue == null)
				{
					queue = (_TimerQueue = TimerThread.GetOrCreateQueue((_Timeout == 0) ? 1 : _Timeout));
				}
				return queue;
			}
		}

		public int ReadWriteTimeout
		{
			get
			{
				return _ReadWriteTimeout;
			}
			set
			{
				if (RequestSubmitted)
				{
					throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
				}
				if (value <= 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_io_timeout_use_gt_zero"));
				}
				_ReadWriteTimeout = value;
			}
		}

		internal bool HeadersCompleted
		{
			get
			{
				return m_HeadersCompleted;
			}
			set
			{
				m_HeadersCompleted = value;
			}
		}

		private bool CanGetRequestStream => !CurrentMethod.ContentBodyNotAllowed;

		internal bool CanGetResponseStream => !CurrentMethod.ExpectNoContentResponse;

		internal bool RequireBody => CurrentMethod.RequireContentBody;

		private bool HasEntityBody
		{
			get
			{
				if (HttpWriteMode != HttpWriteMode.Chunked && HttpWriteMode != HttpWriteMode.Buffer)
				{
					if (HttpWriteMode == HttpWriteMode.ContentLength)
					{
						return ContentLength > 0;
					}
					return false;
				}
				return true;
			}
		}

		public Uri Address => _Uri;

		public HttpContinueDelegate ContinueDelegate
		{
			get
			{
				return _ContinueDelegate;
			}
			set
			{
				_ContinueDelegate = value;
			}
		}

		public ServicePoint ServicePoint => FindServicePoint(forceFind: false);

		public int MaximumAutomaticRedirections
		{
			get
			{
				return _MaximumAllowedRedirections;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException(SR.GetString("net_toosmall"), "value");
				}
				_MaximumAllowedRedirections = value;
			}
		}

		public override string Method
		{
			get
			{
				return _OriginVerb.Name;
			}
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					throw new ArgumentException(SR.GetString("net_badmethod"), "value");
				}
				if (ValidationHelper.IsInvalidHttpString(value))
				{
					throw new ArgumentException(SR.GetString("net_badmethod"), "value");
				}
				_OriginVerb = KnownHttpVerb.Parse(value);
			}
		}

		internal KnownHttpVerb CurrentMethod
		{
			get
			{
				if (_Verb == null)
				{
					return _OriginVerb;
				}
				return _Verb;
			}
			set
			{
				_Verb = value;
			}
		}

		public override ICredentials Credentials
		{
			get
			{
				return _AuthInfo;
			}
			set
			{
				_AuthInfo = value;
			}
		}

		public override bool UseDefaultCredentials
		{
			get
			{
				if (!(Credentials is SystemNetworkCredential))
				{
					return false;
				}
				return true;
			}
			set
			{
				if (RequestSubmitted)
				{
					throw new InvalidOperationException(SR.GetString("net_writestarted"));
				}
				_AuthInfo = (value ? CredentialCache.DefaultCredentials : null);
			}
		}

		internal bool IsTunnelRequest
		{
			get
			{
				return (_Booleans & Booleans.IsTunnelRequest) != 0;
			}
			set
			{
				if (value)
				{
					_Booleans |= Booleans.IsTunnelRequest;
				}
				else
				{
					_Booleans &= ~Booleans.IsTunnelRequest;
				}
			}
		}

		public override string ConnectionGroupName
		{
			get
			{
				return _ConnectionGroupName;
			}
			set
			{
				_ConnectionGroupName = value;
			}
		}

		internal bool InternalConnectionGroup
		{
			set
			{
				m_InternalConnectionGroup = value;
			}
		}

		public override WebHeaderCollection Headers
		{
			get
			{
				return _HttpRequestHeaders;
			}
			set
			{
				if (RequestSubmitted)
				{
					throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
				}
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
				string[] allKeys = value.AllKeys;
				foreach (string name in allKeys)
				{
					webHeaderCollection.Add(name, value[name]);
				}
				_HttpRequestHeaders = webHeaderCollection;
			}
		}

		public override IWebProxy Proxy
		{
			get
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				return _Proxy;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (RequestSubmitted)
				{
					throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
				}
				InternalProxy = value;
			}
		}

		internal IWebProxy InternalProxy
		{
			get
			{
				return _Proxy;
			}
			set
			{
				ProxySet = true;
				_Proxy = value;
				if (_ProxyChain != null)
				{
					_ProxyChain.Dispose();
				}
				_ProxyChain = null;
				FindServicePoint(forceFind: true);
			}
		}

		public Version ProtocolVersion
		{
			get
			{
				if (!IsVersionHttp10)
				{
					return HttpVersion.Version11;
				}
				return HttpVersion.Version10;
			}
			set
			{
				if (value.Equals(HttpVersion.Version11))
				{
					IsVersionHttp10 = false;
					return;
				}
				if (value.Equals(HttpVersion.Version10))
				{
					IsVersionHttp10 = true;
					return;
				}
				throw new ArgumentException(SR.GetString("net_wrongversion"), "value");
			}
		}

		public override string ContentType
		{
			get
			{
				return _HttpRequestHeaders["Content-Type"];
			}
			set
			{
				SetSpecialHeaders("Content-Type", value);
			}
		}

		public string MediaType
		{
			get
			{
				return _MediaType;
			}
			set
			{
				_MediaType = value;
			}
		}

		public string TransferEncoding
		{
			get
			{
				return _HttpRequestHeaders["Transfer-Encoding"];
			}
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					_HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
					return;
				}
				string text = value.ToLower(CultureInfo.InvariantCulture);
				if (text.IndexOf("chunked") != -1)
				{
					throw new ArgumentException(SR.GetString("net_nochunked"), "value");
				}
				if (!SendChunked)
				{
					throw new InvalidOperationException(SR.GetString("net_needchunked"));
				}
				_HttpRequestHeaders.CheckUpdate("Transfer-Encoding", value);
			}
		}

		public string Connection
		{
			get
			{
				return _HttpRequestHeaders["Connection"];
			}
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					_HttpRequestHeaders.Remove("Connection");
					return;
				}
				string text = value.ToLower(CultureInfo.InvariantCulture);
				bool flag = text.IndexOf("keep-alive") != -1;
				bool flag2 = text.IndexOf("close") != -1;
				if (flag || flag2)
				{
					throw new ArgumentException(SR.GetString("net_connarg"), "value");
				}
				_HttpRequestHeaders.CheckUpdate("Connection", value);
			}
		}

		public string Accept
		{
			get
			{
				return _HttpRequestHeaders["Accept"];
			}
			set
			{
				SetSpecialHeaders("Accept", value);
			}
		}

		public string Referer
		{
			get
			{
				return _HttpRequestHeaders["Referer"];
			}
			set
			{
				SetSpecialHeaders("Referer", value);
			}
		}

		public string UserAgent
		{
			get
			{
				return _HttpRequestHeaders["User-Agent"];
			}
			set
			{
				SetSpecialHeaders("User-Agent", value);
			}
		}

		public string Expect
		{
			get
			{
				return _HttpRequestHeaders["Expect"];
			}
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					_HttpRequestHeaders.RemoveInternal("Expect");
					return;
				}
				string text = value.ToLower(CultureInfo.InvariantCulture);
				if (text.IndexOf("100-continue") != -1)
				{
					throw new ArgumentException(SR.GetString("net_no100"), "value");
				}
				_HttpRequestHeaders.CheckUpdate("Expect", value);
			}
		}

		public DateTime IfModifiedSince
		{
			get
			{
				string text = _HttpRequestHeaders["If-Modified-Since"];
				if (text == null)
				{
					return DateTime.Now;
				}
				if (_CachedIfModifedSince != DateTime.MinValue)
				{
					return _CachedIfModifedSince;
				}
				return HttpProtocolUtils.string2date(text);
			}
			set
			{
				SetSpecialHeaders("If-Modified-Since", HttpProtocolUtils.date2string(value));
				_CachedIfModifedSince = value;
			}
		}

		internal byte[] WriteBuffer => _WriteBuffer;

		internal int RequestContinueCount => _RequestContinueCount;

		private bool IdentityRequired
		{
			get
			{
				if (Credentials != null && ComNetOS.IsWinNt)
				{
					if (!(Credentials is SystemNetworkCredential))
					{
						if (!(Credentials is NetworkCredential))
						{
							CredentialCache credentialCache;
							if ((credentialCache = Credentials as CredentialCache) != null)
							{
								return credentialCache.IsDefaultInCache;
							}
							return true;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		private static string UniqueGroupId => Interlocked.Increment(ref s_UniqueGroupId).ToString(NumberFormatInfo.InvariantInfo);

		private bool SetRequestSubmitted()
		{
			bool requestSubmitted = RequestSubmitted;
			m_RequestSubmitted = true;
			return requestSubmitted;
		}

		internal string AuthHeader(HttpResponseHeader header)
		{
			if (_HttpResponse == null)
			{
				return null;
			}
			return _HttpResponse.Headers[header];
		}

		internal long SwitchToContentLength()
		{
			if (HaveResponse)
			{
				return -1L;
			}
			if (HttpWriteMode == HttpWriteMode.Chunked)
			{
				ConnectStream connectStream = _OldSubmitWriteStream;
				if (connectStream == null)
				{
					connectStream = _SubmitWriteStream;
				}
				if (connectStream.Connection != null && connectStream.Connection.IISVersion >= 6)
				{
					return -1L;
				}
			}
			long result = -1L;
			long contentLength = _ContentLength;
			if (HttpWriteMode != HttpWriteMode.None)
			{
				if (HttpWriteMode == HttpWriteMode.Buffer)
				{
					_ContentLength = _SubmitWriteStream.BufferedData.Length;
					m_OriginallyBuffered = true;
					HttpWriteMode = HttpWriteMode.ContentLength;
					return -1L;
				}
				if (NtlmKeepAlive && _OldSubmitWriteStream == null)
				{
					_ContentLength = 0L;
					_SubmitWriteStream.SuppressWrite = true;
					if (!_SubmitWriteStream.BufferOnly)
					{
						result = contentLength;
					}
					if (HttpWriteMode == HttpWriteMode.Chunked)
					{
						HttpWriteMode = HttpWriteMode.ContentLength;
						_SubmitWriteStream.SwitchToContentLength();
						result = -2L;
						_HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
					}
				}
				if (_OldSubmitWriteStream != null)
				{
					if (NtlmKeepAlive)
					{
						_ContentLength = 0L;
					}
					else if (_ContentLength == 0 || HttpWriteMode == HttpWriteMode.Chunked)
					{
						_ContentLength = _OldSubmitWriteStream.BufferedData.Length;
					}
					if (HttpWriteMode == HttpWriteMode.Chunked)
					{
						HttpWriteMode = HttpWriteMode.ContentLength;
						_SubmitWriteStream.SwitchToContentLength();
						_HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
					}
				}
			}
			return result;
		}

		private void PostSwitchToContentLength(long value)
		{
			if (value > -1)
			{
				_ContentLength = value;
			}
			if (value == -2)
			{
				_ContentLength = -1L;
				HttpWriteMode = HttpWriteMode.Chunked;
			}
		}

		private void ClearAuthenticatedConnectionResources()
		{
			if (ProxyAuthenticationState.UniqueGroupId != null || ServerAuthenticationState.UniqueGroupId != null)
			{
				ServicePoint.ReleaseConnectionGroup(GetConnectionGroupLine());
			}
			UnlockConnectionDelegate unlockConnectionDelegate = UnlockConnectionDelegate;
			try
			{
				unlockConnectionDelegate?.Invoke();
				UnlockConnectionDelegate = null;
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
			ProxyAuthenticationState.ClearSession(this);
			ServerAuthenticationState.ClearSession(this);
		}

		private void CheckProtocol(bool onRequestStream)
		{
			if (!CanGetRequestStream)
			{
				if (onRequestStream)
				{
					throw new ProtocolViolationException(SR.GetString("net_nouploadonget"));
				}
				if (HttpWriteMode != 0 && HttpWriteMode != HttpWriteMode.None)
				{
					throw new ProtocolViolationException(SR.GetString("net_nocontentlengthonget"));
				}
				HttpWriteMode = HttpWriteMode.None;
			}
			else if (HttpWriteMode == HttpWriteMode.Unknown)
			{
				if (SendChunked)
				{
					if (ServicePoint.HttpBehaviour == HttpBehaviour.HTTP11 || ServicePoint.HttpBehaviour == HttpBehaviour.Unknown)
					{
						HttpWriteMode = HttpWriteMode.Chunked;
					}
					else
					{
						if (!AllowWriteStreamBuffering)
						{
							throw new ProtocolViolationException(SR.GetString("net_nochunkuploadonhttp10"));
						}
						HttpWriteMode = HttpWriteMode.Buffer;
					}
				}
				else
				{
					HttpWriteMode = ((ContentLength >= 0) ? HttpWriteMode.ContentLength : (onRequestStream ? HttpWriteMode.Buffer : HttpWriteMode.None));
				}
			}
			if (HttpWriteMode != HttpWriteMode.Chunked)
			{
				if (onRequestStream && ContentLength == -1 && !AllowWriteStreamBuffering && KeepAlive)
				{
					throw new ProtocolViolationException(SR.GetString("net_contentlengthmissing"));
				}
				if (!ValidationHelper.IsBlankString(TransferEncoding))
				{
					throw new InvalidOperationException(SR.GetString("net_needchunked"));
				}
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "BeginGetRequestStream", "");
			}
			CheckProtocol(onRequestStream: true);
			ContextAwareResult contextAwareResult = new ContextAwareResult(IdentityRequired, forceCaptureContext: true, this, state, callback);
			lock (contextAwareResult.StartPostingAsyncOp())
			{
				if (_WriteAResult != null && _WriteAResult.InternalPeekCompleted)
				{
					if (_WriteAResult.Result is Exception)
					{
						throw (Exception)_WriteAResult.Result;
					}
					try
					{
						contextAwareResult.InvokeCallback(_WriteAResult.Result);
					}
					catch (Exception exception)
					{
						Abort(exception, 1);
						throw;
					}
				}
				else
				{
					if (!RequestSubmitted && NclUtilities.IsThreadPoolLow())
					{
						Exception ex = new InvalidOperationException(SR.GetString("net_needmorethreads"));
						Abort(ex, 1);
						throw ex;
					}
					lock (this)
					{
						if (_WriteAResult != null)
						{
							throw new InvalidOperationException(SR.GetString("net_repcall"));
						}
						if (SetRequestSubmitted())
						{
							throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
						}
						if (_ReadAResult != null)
						{
							throw (Exception)_ReadAResult.Result;
						}
						_WriteAResult = contextAwareResult;
						Async = true;
					}
					CurrentMethod = _OriginVerb;
					BeginSubmitRequest();
				}
				contextAwareResult.FinishPostingAsyncOp();
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "BeginGetRequestStream", contextAwareResult);
			}
			return contextAwareResult;
		}

		public override Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			TransportContext context;
			return EndGetRequestStream(asyncResult, out context);
		}

		public Stream EndGetRequestStream(IAsyncResult asyncResult, out TransportContext context)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "EndGetRequestStream", "");
			}
			context = null;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
			if (lazyAsyncResult == null || lazyAsyncResult.AsyncObject != this)
			{
				throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
			}
			if (lazyAsyncResult.EndCalled)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndGetRequestStream"));
			}
			ConnectStream connectStream = lazyAsyncResult.InternalWaitForCompletion() as ConnectStream;
			lazyAsyncResult.EndCalled = true;
			if (connectStream == null)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "EndGetRequestStream", lazyAsyncResult.Result as Exception);
				}
				throw (Exception)lazyAsyncResult.Result;
			}
			context = new ConnectStreamContext(connectStream);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "EndGetRequestStream", connectStream);
			}
			return connectStream;
		}

		public override Stream GetRequestStream()
		{
			TransportContext context;
			return GetRequestStream(out context);
		}

		public Stream GetRequestStream(out TransportContext context)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "GetRequestStream", "");
			}
			context = null;
			CheckProtocol(onRequestStream: true);
			if (_WriteAResult == null || !_WriteAResult.InternalPeekCompleted)
			{
				lock (this)
				{
					if (_WriteAResult != null)
					{
						throw new InvalidOperationException(SR.GetString("net_repcall"));
					}
					if (SetRequestSubmitted())
					{
						throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
					}
					if (_ReadAResult != null)
					{
						throw (Exception)_ReadAResult.Result;
					}
					_WriteAResult = new LazyAsyncResult(this, null, null);
					Async = false;
				}
				CurrentMethod = _OriginVerb;
				while (m_Retry && !_WriteAResult.InternalPeekCompleted)
				{
					_OldSubmitWriteStream = null;
					_SubmitWriteStream = null;
					BeginSubmitRequest();
				}
				while (Aborted && !_WriteAResult.InternalPeekCompleted)
				{
					if (!(_CoreResponse is Exception))
					{
						Thread.SpinWait(1);
					}
					else
					{
						CheckWriteSideResponseProcessing();
					}
				}
			}
			ConnectStream connectStream = _WriteAResult.InternalWaitForCompletion() as ConnectStream;
			_WriteAResult.EndCalled = true;
			if (connectStream == null)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "EndGetRequestStream", _WriteAResult.Result as Exception);
				}
				throw (Exception)_WriteAResult.Result;
			}
			context = new ConnectStreamContext(connectStream);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "GetRequestStream", connectStream);
			}
			return connectStream;
		}

		internal void ErrorStatusCodeNotify(Connection connection, bool isKeepAlive, bool fatal)
		{
			ConnectStream submitWriteStream = _SubmitWriteStream;
			if (submitWriteStream != null && submitWriteStream.Connection == connection)
			{
				if (!fatal)
				{
					submitWriteStream.ErrorResponseNotify(isKeepAlive);
				}
				else if (!Aborted)
				{
					submitWriteStream.FatalResponseNotify();
				}
			}
		}

		private HttpProcessingResult DoSubmitRequestProcessing(ref Exception exception)
		{
			HttpProcessingResult httpProcessingResult = HttpProcessingResult.Continue;
			m_Retry = false;
			try
			{
				if (_HttpResponse != null)
				{
					if (_CookieContainer != null)
					{
						CookieModule.OnReceivedHeaders(this);
					}
					ProxyAuthenticationState.Update(this);
					ServerAuthenticationState.Update(this);
				}
				bool flag = false;
				bool flag2 = true;
				if (_HttpResponse == null)
				{
					flag = true;
				}
				else if (CheckResubmitForCache(ref exception) || CheckResubmit(ref exception))
				{
					flag = true;
					flag2 = false;
				}
				ServicePoint servicePoint = null;
				if (flag2)
				{
					WebException ex = exception as WebException;
					if (ex != null && ex.InternalStatus == WebExceptionInternalStatus.ServicePointFatal)
					{
						ProxyChain proxyChain = _ProxyChain;
						if (proxyChain != null)
						{
							servicePoint = ServicePointManager.FindServicePoint(proxyChain);
						}
						flag = servicePoint != null;
					}
				}
				if (flag)
				{
					if (base.CacheProtocol != null && _HttpResponse != null)
					{
						base.CacheProtocol.Reset();
					}
					ClearRequestForResubmit();
					WebException ex2 = exception as WebException;
					if (ex2 != null && (ex2.Status == WebExceptionStatus.PipelineFailure || ex2.Status == WebExceptionStatus.KeepAliveFailure))
					{
						m_Extra401Retry = true;
					}
					if (servicePoint == null)
					{
						servicePoint = FindServicePoint(forceFind: true);
					}
					else
					{
						_ServicePoint = servicePoint;
					}
					if (Async)
					{
						SubmitRequest(servicePoint);
					}
					else
					{
						m_Retry = true;
					}
					httpProcessingResult = HttpProcessingResult.WriteWait;
				}
			}
			finally
			{
				if (httpProcessingResult == HttpProcessingResult.Continue)
				{
					ClearAuthenticatedConnectionResources();
				}
			}
			return httpProcessingResult;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "BeginGetResponse", "");
			}
			CheckProtocol(onRequestStream: false);
			ConnectStream connectStream = ((_OldSubmitWriteStream != null) ? _OldSubmitWriteStream : _SubmitWriteStream);
			if (connectStream != null && !connectStream.IsClosed && connectStream.BytesLeftToWrite == 0)
			{
				connectStream.Close();
			}
			ContextAwareResult contextAwareResult = new ContextAwareResult(IdentityRequired, forceCaptureContext: true, this, state, callback);
			if (!RequestSubmitted && NclUtilities.IsThreadPoolLow())
			{
				Exception ex = new InvalidOperationException(SR.GetString("net_needmorethreads"));
				Abort(ex, 1);
				throw ex;
			}
			lock (contextAwareResult.StartPostingAsyncOp())
			{
				bool flag = false;
				bool flag2;
				lock (this)
				{
					flag2 = SetRequestSubmitted();
					if (HaveResponse)
					{
						flag = true;
					}
					else
					{
						if (_ReadAResult != null)
						{
							throw new InvalidOperationException(SR.GetString("net_repcall"));
						}
						_ReadAResult = contextAwareResult;
						Async = true;
					}
				}
				CheckDeferredCallDone(connectStream);
				if (flag)
				{
					if (Logging.On)
					{
						Logging.Exit(Logging.Web, this, "BeginGetResponse", _ReadAResult.Result);
					}
					Exception ex2 = _ReadAResult.Result as Exception;
					if (ex2 != null)
					{
						throw ex2;
					}
					try
					{
						contextAwareResult.InvokeCallback(_ReadAResult.Result);
					}
					catch (Exception exception)
					{
						Abort(exception, 1);
						throw;
					}
				}
				else
				{
					if (!flag2)
					{
						CurrentMethod = _OriginVerb;
					}
					if (_RerequestCount > 0 || !flag2)
					{
						while (m_Retry)
						{
							BeginSubmitRequest();
						}
					}
				}
				contextAwareResult.FinishPostingAsyncOp();
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "BeginGetResponse", contextAwareResult);
			}
			return contextAwareResult;
		}

		public override WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "EndGetResponse", "");
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
			if (lazyAsyncResult == null || lazyAsyncResult.AsyncObject != this)
			{
				throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
			}
			if (lazyAsyncResult.EndCalled)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndGetResponse"));
			}
			HttpWebResponse httpWebResponse = lazyAsyncResult.InternalWaitForCompletion() as HttpWebResponse;
			lazyAsyncResult.EndCalled = true;
			if (httpWebResponse == null)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "EndGetResponse", lazyAsyncResult.Result as Exception);
				}
				throw (Exception)lazyAsyncResult.Result;
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "EndGetResponse", httpWebResponse);
			}
			return httpWebResponse;
		}

		private void CheckDeferredCallDone(ConnectStream stream)
		{
			object obj = Interlocked.Exchange(ref m_PendingReturnResult, DBNull.Value);
			if (obj == NclConstants.Sentinel)
			{
				EndSubmitRequest();
			}
			else if (obj != null && obj != DBNull.Value)
			{
				stream.ProcessWriteCallDone(obj as ConnectionReturnResult);
			}
		}

		public override WebResponse GetResponse()
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "GetResponse", "");
			}
			CheckProtocol(onRequestStream: false);
			ConnectStream connectStream = ((_OldSubmitWriteStream != null) ? _OldSubmitWriteStream : _SubmitWriteStream);
			if (connectStream != null && !connectStream.IsClosed && connectStream.BytesLeftToWrite == 0)
			{
				connectStream.Close();
			}
			bool flag = false;
			HttpWebResponse httpWebResponse = null;
			bool flag2;
			lock (this)
			{
				flag2 = SetRequestSubmitted();
				if (HaveResponse)
				{
					flag = true;
					httpWebResponse = _ReadAResult.Result as HttpWebResponse;
				}
				else
				{
					if (_ReadAResult != null)
					{
						throw new InvalidOperationException(SR.GetString("net_repcall"));
					}
					Async = false;
					if (Async)
					{
						ContextAwareResult contextAwareResult = new ContextAwareResult(IdentityRequired, forceCaptureContext: true, this, null, null);
						contextAwareResult.StartPostingAsyncOp(lockCapture: false);
						contextAwareResult.FinishPostingAsyncOp();
						_ReadAResult = contextAwareResult;
					}
					else
					{
						_ReadAResult = new LazyAsyncResult(this, null, null);
					}
				}
			}
			CheckDeferredCallDone(connectStream);
			if (!flag)
			{
				if (_Timer == null)
				{
					_Timer = TimerQueue.CreateTimer(s_TimeoutCallback, this);
				}
				if (!flag2)
				{
					CurrentMethod = _OriginVerb;
				}
				while (m_Retry)
				{
					BeginSubmitRequest();
				}
				while (!Async && Aborted && !_ReadAResult.InternalPeekCompleted)
				{
					if (!(_CoreResponse is Exception))
					{
						Thread.SpinWait(1);
					}
					else
					{
						CheckWriteSideResponseProcessing();
					}
				}
				httpWebResponse = _ReadAResult.InternalWaitForCompletion() as HttpWebResponse;
				_ReadAResult.EndCalled = true;
			}
			if (httpWebResponse == null)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "EndGetResponse", _ReadAResult.Result as Exception);
				}
				throw (Exception)_ReadAResult.Result;
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "GetResponse", httpWebResponse);
			}
			return httpWebResponse;
		}

		internal void WriteCallDone(ConnectStream stream, ConnectionReturnResult returnResult)
		{
			if (!object.ReferenceEquals(stream, (_OldSubmitWriteStream != null) ? _OldSubmitWriteStream : _SubmitWriteStream))
			{
				stream.ProcessWriteCallDone(returnResult);
				return;
			}
			if (!UserRetrievedWriteStream)
			{
				stream.ProcessWriteCallDone(returnResult);
				return;
			}
			object value = ((returnResult == null) ? ((object)Missing.Value) : ((object)returnResult));
			object obj = Interlocked.CompareExchange(ref m_PendingReturnResult, value, null);
			if (obj == DBNull.Value)
			{
				stream.ProcessWriteCallDone(returnResult);
			}
		}

		internal void NeedEndSubmitRequest()
		{
			object obj = Interlocked.CompareExchange(ref m_PendingReturnResult, NclConstants.Sentinel, null);
			if (obj == DBNull.Value)
			{
				EndSubmitRequest();
			}
		}

		internal void CallContinueDelegateCallback(object state)
		{
			CoreResponseData coreResponseData = (CoreResponseData)state;
			ContinueDelegate((int)coreResponseData.m_StatusCode, coreResponseData.m_ResponseHeaders);
		}

		private void SetSpecialHeaders(string HeaderName, string value)
		{
			value = WebHeaderCollection.CheckBadChars(value, isHeaderValue: true);
			_HttpRequestHeaders.RemoveInternal(HeaderName);
			if (value.Length != 0)
			{
				_HttpRequestHeaders.AddInternal(HeaderName, value);
			}
		}

		public override void Abort()
		{
			Abort(null, 1);
		}

		private void Abort(Exception exception, int abortState)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "Abort", (exception == null) ? "" : exception.Message);
			}
			if (Interlocked.CompareExchange(ref m_Aborted, abortState, 0) == 0)
			{
				m_OnceFailed = true;
				CancelTimer();
				WebException ex = exception as WebException;
				if (exception == null)
				{
					ex = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
				}
				else if (ex == null)
				{
					ex = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), exception, WebExceptionStatus.RequestCanceled, _HttpResponse);
				}
				try
				{
					Thread.MemoryBarrier();
					HttpAbortDelegate abortDelegate = _AbortDelegate;
					if (abortDelegate == null || abortDelegate(this, ex))
					{
						LazyAsyncResult lazyAsyncResult = (Async ? null : ConnectionAsyncResult);
						LazyAsyncResult lazyAsyncResult2 = (Async ? null : ConnectionReaderAsyncResult);
						SetResponse(ex);
						lazyAsyncResult?.InvokeCallback(ex);
						lazyAsyncResult2?.InvokeCallback(ex);
					}
					ClearAuthenticatedConnectionResources();
				}
				catch (Exception exception2)
				{
					if (NclUtilities.IsFatal(exception2))
					{
						throw;
					}
				}
				catch
				{
				}
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "Abort", "");
			}
		}

		private void CancelTimer()
		{
			_Timer?.Cancel();
		}

		private static void TimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
		{
			ThreadPool.UnsafeQueueUserWorkItem(s_AbortWrapper, context);
		}

		private static void AbortWrapper(object context)
		{
			((HttpWebRequest)context).Abort(new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout), 1);
		}

		private ServicePoint FindServicePoint(bool forceFind)
		{
			ServicePoint servicePoint = _ServicePoint;
			if (servicePoint == null || forceFind)
			{
				lock (this)
				{
					if (_ServicePoint == null || forceFind)
					{
						if (!ProxySet)
						{
							_Proxy = WebRequest.InternalDefaultWebProxy;
						}
						if (_ProxyChain != null)
						{
							_ProxyChain.Dispose();
						}
						_ServicePoint = ServicePointManager.FindServicePoint(_Uri, _Proxy, out _ProxyChain, ref _AbortDelegate, ref m_Aborted);
						if (Logging.On)
						{
							Logging.Associate(Logging.Web, this, _ServicePoint);
						}
					}
				}
				servicePoint = _ServicePoint;
			}
			return servicePoint;
		}

		private void InvokeGetRequestStreamCallback()
		{
			LazyAsyncResult writeAResult = _WriteAResult;
			if (writeAResult == null)
			{
				return;
			}
			try
			{
				writeAResult.InvokeCallback(_SubmitWriteStream);
			}
			catch (Exception exception)
			{
				if (NclUtilities.IsFatal(exception))
				{
					throw;
				}
				Abort(exception, 1);
				throw;
			}
		}

		internal void SetRequestSubmitDone(ConnectStream submitStream)
		{
			if (!Async)
			{
				ConnectionAsyncResult.InvokeCallback();
			}
			if (AllowWriteStreamBuffering)
			{
				submitStream.EnableWriteBuffering();
			}
			if (submitStream.CanTimeout)
			{
				submitStream.ReadTimeout = ReadWriteTimeout;
				submitStream.WriteTimeout = ReadWriteTimeout;
			}
			if (Logging.On)
			{
				Logging.Associate(Logging.Web, this, submitStream);
			}
			TransportContext transportContext = new ConnectStreamContext(submitStream);
			ServerAuthenticationState.TransportContext = transportContext;
			ProxyAuthenticationState.TransportContext = transportContext;
			_SubmitWriteStream = submitStream;
			if (Async && _CoreResponse != null && _CoreResponse != DBNull.Value)
			{
				submitStream.CallDone();
			}
			else
			{
				EndSubmitRequest();
			}
		}

		internal void WriteHeadersCallback(WebExceptionStatus errorStatus, ConnectStream stream, bool async)
		{
			if (errorStatus == WebExceptionStatus.Success)
			{
				if (!EndWriteHeaders(async))
				{
					errorStatus = WebExceptionStatus.Pending;
				}
				else if (stream.BytesLeftToWrite == 0)
				{
					stream.CallDone();
				}
			}
		}

		internal void SetRequestContinue()
		{
			SetRequestContinue(null);
		}

		internal void SetRequestContinue(CoreResponseData continueResponse)
		{
			_RequestContinueCount++;
			if (HttpWriteMode == HttpWriteMode.None || !m_ContinueGate.Complete())
			{
				return;
			}
			TimerThread.Timer continueTimer = m_ContinueTimer;
			m_ContinueTimer = null;
			if (continueTimer != null && !continueTimer.Cancel())
			{
				return;
			}
			if (continueResponse != null && ContinueDelegate != null)
			{
				ExecutionContext executionContext = (Async ? GetWritingContext().ContextCopy : null);
				if (executionContext == null)
				{
					ContinueDelegate((int)continueResponse.m_StatusCode, continueResponse.m_ResponseHeaders);
				}
				else
				{
					ExecutionContext.Run(executionContext, CallContinueDelegateCallback, continueResponse);
				}
			}
			EndWriteHeaders_Part2();
		}

		internal void OpenWriteSideResponseWindow()
		{
			_CoreResponse = DBNull.Value;
			_NestedWriteSideCheck = 0;
		}

		internal void CheckWriteSideResponseProcessing()
		{
			object obj = (Async ? Interlocked.CompareExchange(ref _CoreResponse, null, DBNull.Value) : _CoreResponse);
			if (obj == DBNull.Value)
			{
				return;
			}
			if (obj == null)
			{
				throw new InternalException();
			}
			if (Async || ++_NestedWriteSideCheck == 1)
			{
				Exception ex = obj as Exception;
				if (ex != null)
				{
					SetResponse(ex);
				}
				else
				{
					SetResponse(obj as CoreResponseData);
				}
			}
		}

		internal void SetAndOrProcessResponse(object responseOrException)
		{
			if (responseOrException == null)
			{
				throw new InternalException();
			}
			CoreResponseData coreResponseData = responseOrException as CoreResponseData;
			WebException ex = responseOrException as WebException;
			object obj = Interlocked.CompareExchange(ref _CoreResponse, responseOrException, DBNull.Value);
			if (obj != null)
			{
				if (obj.GetType() == typeof(CoreResponseData))
				{
					if (coreResponseData != null)
					{
						throw new InternalException();
					}
					if (ex != null && ex.InternalStatus != WebExceptionInternalStatus.ServicePointFatal && ex.InternalStatus != 0)
					{
						return;
					}
				}
				else if (obj.GetType() != typeof(DBNull))
				{
					if (coreResponseData == null)
					{
						throw new InternalException();
					}
					ICloseEx closeEx = coreResponseData.m_ConnectStream as ICloseEx;
					if (closeEx != null)
					{
						closeEx.CloseEx(CloseExState.Silent);
					}
					else
					{
						coreResponseData.m_ConnectStream.Close();
					}
					return;
				}
			}
			if (obj == DBNull.Value)
			{
				if (!Async)
				{
					LazyAsyncResult connectionAsyncResult = ConnectionAsyncResult;
					LazyAsyncResult connectionReaderAsyncResult = ConnectionReaderAsyncResult;
					connectionAsyncResult.InvokeCallback(responseOrException);
					connectionReaderAsyncResult.InvokeCallback(responseOrException);
				}
				return;
			}
			if (obj != null)
			{
				Exception ex2 = responseOrException as Exception;
				if (ex2 != null)
				{
					SetResponse(ex2);
					return;
				}
				throw new InternalException();
			}
			obj = Interlocked.CompareExchange(ref _CoreResponse, responseOrException, null);
			if (obj != null && coreResponseData != null)
			{
				ICloseEx closeEx2 = coreResponseData.m_ConnectStream as ICloseEx;
				if (closeEx2 != null)
				{
					closeEx2.CloseEx(CloseExState.Silent);
				}
				else
				{
					coreResponseData.m_ConnectStream.Close();
				}
				return;
			}
			if (!Async)
			{
				throw new InternalException();
			}
			if (coreResponseData != null)
			{
				SetResponse(coreResponseData);
			}
			else
			{
				SetResponse(responseOrException as Exception);
			}
		}

		private void SetResponse(CoreResponseData coreResponseData)
		{
			try
			{
				if (!Async)
				{
					LazyAsyncResult connectionAsyncResult = ConnectionAsyncResult;
					LazyAsyncResult connectionReaderAsyncResult = ConnectionReaderAsyncResult;
					connectionAsyncResult.InvokeCallback(coreResponseData);
					connectionReaderAsyncResult.InvokeCallback(coreResponseData);
				}
				if (coreResponseData != null)
				{
					if (coreResponseData.m_ConnectStream.CanTimeout)
					{
						coreResponseData.m_ConnectStream.WriteTimeout = ReadWriteTimeout;
						coreResponseData.m_ConnectStream.ReadTimeout = ReadWriteTimeout;
					}
					_HttpResponse = new HttpWebResponse(_Uri, CurrentMethod, coreResponseData, _MediaType, UsesProxySemantics, AutomaticDecompression);
					if (Logging.On)
					{
						Logging.Associate(Logging.Web, this, coreResponseData.m_ConnectStream);
					}
					if (Logging.On)
					{
						Logging.Associate(Logging.Web, this, _HttpResponse);
					}
					ProcessResponse();
				}
				else
				{
					Abort(null, 1);
				}
			}
			catch (Exception exception)
			{
				Abort(exception, 2);
			}
		}

		private void ProcessResponse()
		{
			HttpProcessingResult httpProcessingResult = HttpProcessingResult.Continue;
			Exception exception = null;
			if (DoSubmitRequestProcessing(ref exception) != 0)
			{
				return;
			}
			CancelTimer();
			object result = ((exception != null) ? ((ISerializable)exception) : ((ISerializable)_HttpResponse));
			if (_ReadAResult == null)
			{
				lock (this)
				{
					if (_ReadAResult == null)
					{
						_ReadAResult = new LazyAsyncResult(null, null, null, result);
					}
				}
			}
			try
			{
				FinishRequest(_HttpResponse, exception);
				_ReadAResult.InvokeCallback(result);
				try
				{
					SetRequestContinue();
				}
				catch
				{
				}
			}
			catch (Exception exception2)
			{
				Abort(exception2, 1);
				throw;
			}
			finally
			{
				if (exception == null && _ReadAResult.Result != _HttpResponse)
				{
					WebException ex = _ReadAResult.Result as WebException;
					if (ex != null && ex.Response != null)
					{
						_HttpResponse.Abort();
					}
				}
			}
		}

		private void SetResponse(Exception E)
		{
			HttpProcessingResult httpProcessingResult = HttpProcessingResult.Continue;
			WebException ex = (HaveResponse ? (_ReadAResult.Result as WebException) : null);
			WebException ex2 = E as WebException;
			if (ex != null && (ex.InternalStatus == WebExceptionInternalStatus.RequestFatal || ex.InternalStatus == WebExceptionInternalStatus.ServicePointFatal) && (ex2 == null || ex2.InternalStatus != 0))
			{
				E = ex;
			}
			else
			{
				ex = ex2;
			}
			if (E != null && Logging.On)
			{
				Logging.Exception(Logging.Web, this, "", ex);
			}
			try
			{
				if (ex != null && (ex.InternalStatus == WebExceptionInternalStatus.Isolated || ex.InternalStatus == WebExceptionInternalStatus.ServicePointFatal || (ex.InternalStatus == WebExceptionInternalStatus.Recoverable && !m_OnceFailed)))
				{
					if (ex.InternalStatus == WebExceptionInternalStatus.Recoverable)
					{
						m_OnceFailed = true;
					}
					Pipelined = false;
					if (_SubmitWriteStream != null && _OldSubmitWriteStream == null && _SubmitWriteStream.BufferOnly)
					{
						_OldSubmitWriteStream = _SubmitWriteStream;
					}
					httpProcessingResult = DoSubmitRequestProcessing(ref E);
				}
			}
			catch (Exception ex3)
			{
				if (NclUtilities.IsFatal(ex3))
				{
					throw;
				}
				httpProcessingResult = HttpProcessingResult.Continue;
				E = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), ex3, WebExceptionStatus.RequestCanceled, _HttpResponse);
			}
			finally
			{
				if (httpProcessingResult == HttpProcessingResult.Continue)
				{
					CancelTimer();
					if (!(E is WebException) && !(E is SecurityException))
					{
						E = ((_HttpResponse != null) ? new WebException(SR.GetString("net_servererror", NetRes.GetWebStatusCodeString(ResponseStatusCode, _HttpResponse.StatusDescription)), E, WebExceptionStatus.ProtocolError, _HttpResponse) : new WebException(E.Message, E));
					}
					LazyAsyncResult lazyAsyncResult = null;
					HttpWebResponse httpResponse = _HttpResponse;
					LazyAsyncResult writeAResult;
					lock (this)
					{
						writeAResult = _WriteAResult;
						if (_ReadAResult == null)
						{
							_ReadAResult = new LazyAsyncResult(null, null, null, E);
						}
						else
						{
							lazyAsyncResult = _ReadAResult;
						}
					}
					try
					{
						FinishRequest(httpResponse, E);
						try
						{
							writeAResult?.InvokeCallback(E);
						}
						finally
						{
							lazyAsyncResult?.InvokeCallback(E);
						}
					}
					finally
					{
						(_ReadAResult.Result as HttpWebResponse)?.Abort();
						if (base.CacheProtocol != null)
						{
							base.CacheProtocol.Abort();
						}
					}
				}
			}
		}

		internal override ContextAwareResult GetConnectingContext()
		{
			if (!Async)
			{
				return null;
			}
			ContextAwareResult contextAwareResult = ((HttpWriteMode == HttpWriteMode.None || _OldSubmitWriteStream != null || _WriteAResult == null) ? _ReadAResult : _WriteAResult) as ContextAwareResult;
			if (contextAwareResult == null)
			{
				throw new InternalException();
			}
			return contextAwareResult;
		}

		internal override ContextAwareResult GetWritingContext()
		{
			if (!Async)
			{
				return null;
			}
			ContextAwareResult contextAwareResult = ((HttpWriteMode == HttpWriteMode.None || HttpWriteMode == HttpWriteMode.Buffer || m_PendingReturnResult == DBNull.Value || m_OriginallyBuffered || _WriteAResult == null) ? _ReadAResult : _WriteAResult) as ContextAwareResult;
			if (contextAwareResult == null)
			{
				throw new InternalException();
			}
			return contextAwareResult;
		}

		internal override ContextAwareResult GetReadingContext()
		{
			if (!Async)
			{
				return null;
			}
			ContextAwareResult contextAwareResult = _ReadAResult as ContextAwareResult;
			if (contextAwareResult == null)
			{
				throw new InternalException();
			}
			return contextAwareResult;
		}

		private void BeginSubmitRequest()
		{
			SubmitRequest(FindServicePoint(forceFind: false));
		}

		private void SubmitRequest(ServicePoint servicePoint)
		{
			if (!Async)
			{
				_ConnectionAResult = new LazyAsyncResult(this, null, null);
				_ConnectionReaderAResult = new LazyAsyncResult(this, null, null);
				OpenWriteSideResponseWindow();
			}
			if (_Timer == null && !Async)
			{
				_Timer = TimerQueue.CreateTimer(s_TimeoutCallback, this);
			}
			try
			{
				if (_SubmitWriteStream != null && _SubmitWriteStream.IsPostStream)
				{
					if (_OldSubmitWriteStream == null && !_SubmitWriteStream.ErrorInStream)
					{
						_OldSubmitWriteStream = _SubmitWriteStream;
					}
					_WriteBuffer = null;
				}
				m_Retry = false;
				if (PreAuthenticate)
				{
					if (UsesProxySemantics && _Proxy != null && _Proxy.Credentials != null)
					{
						ProxyAuthenticationState.PreAuthIfNeeded(this, _Proxy.Credentials);
					}
					if (Credentials != null)
					{
						ServerAuthenticationState.PreAuthIfNeeded(this, Credentials);
					}
				}
				if (WriteBuffer == null)
				{
					UpdateHeaders();
				}
				if (!CheckCacheRetrieveBeforeSubmit())
				{
					servicePoint.SubmitRequest(this, GetConnectionGroupLine());
				}
			}
			finally
			{
				if (!Async)
				{
					CheckWriteSideResponseProcessing();
				}
			}
		}

		private bool CheckCacheRetrieveBeforeSubmit()
		{
			if (base.CacheProtocol == null)
			{
				return false;
			}
			try
			{
				Uri uri = _Uri;
				if (uri.Fragment.Length != 0)
				{
					uri = new Uri(uri.GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.SafeUnescaped));
				}
				base.CacheProtocol.GetRetrieveStatus(uri, this);
				if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
				{
					throw base.CacheProtocol.ProtocolException;
				}
				if (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse)
				{
					return false;
				}
				if (HttpWriteMode != HttpWriteMode.None)
				{
					throw new NotSupportedException(SR.GetString("net_cache_not_supported_body"));
				}
				HttpRequestCacheValidator httpRequestCacheValidator = (HttpRequestCacheValidator)base.CacheProtocol.Validator;
				CoreResponseData coreResponseData = new CoreResponseData();
				coreResponseData.m_IsVersionHttp11 = httpRequestCacheValidator.CacheHttpVersion.Equals(HttpVersion.Version11);
				coreResponseData.m_StatusCode = httpRequestCacheValidator.CacheStatusCode;
				coreResponseData.m_StatusDescription = httpRequestCacheValidator.CacheStatusDescription;
				coreResponseData.m_ResponseHeaders = httpRequestCacheValidator.CacheHeaders;
				coreResponseData.m_ContentLength = base.CacheProtocol.ResponseStreamLength;
				coreResponseData.m_ConnectStream = base.CacheProtocol.ResponseStream;
				_HttpResponse = new HttpWebResponse(_Uri, CurrentMethod, coreResponseData, _MediaType, UsesProxySemantics, AutomaticDecompression);
				_HttpResponse.InternalSetFromCache = true;
				_HttpResponse.InternalSetIsCacheFresh = httpRequestCacheValidator.CacheFreshnessStatus != CacheFreshnessStatus.Stale;
				ProcessResponse();
				return true;
			}
			catch (Exception exception)
			{
				Abort(exception, 1);
				throw;
			}
		}

		private bool CheckCacheRetrieveOnResponse()
		{
			if (base.CacheProtocol == null)
			{
				return true;
			}
			if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
			{
				throw base.CacheProtocol.ProtocolException;
			}
			Stream responseStream = _HttpResponse.ResponseStream;
			base.CacheProtocol.GetRevalidateStatus(_HttpResponse, _HttpResponse.ResponseStream);
			if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer)
			{
				return false;
			}
			if (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse && base.CacheProtocol.ProtocolStatus != CacheValidationStatus.CombineCachedAndServerResponse)
			{
				return true;
			}
			if (HttpWriteMode != HttpWriteMode.None)
			{
				throw new NotSupportedException(SR.GetString("net_cache_not_supported_body"));
			}
			CoreResponseData coreResponseData = new CoreResponseData();
			HttpRequestCacheValidator httpRequestCacheValidator = (HttpRequestCacheValidator)base.CacheProtocol.Validator;
			coreResponseData.m_IsVersionHttp11 = httpRequestCacheValidator.CacheHttpVersion.Equals(HttpVersion.Version11);
			coreResponseData.m_StatusCode = httpRequestCacheValidator.CacheStatusCode;
			coreResponseData.m_StatusDescription = httpRequestCacheValidator.CacheStatusDescription;
			coreResponseData.m_ResponseHeaders = ((base.CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse) ? new WebHeaderCollection(httpRequestCacheValidator.CacheHeaders) : httpRequestCacheValidator.CacheHeaders);
			coreResponseData.m_ContentLength = base.CacheProtocol.ResponseStreamLength;
			coreResponseData.m_ConnectStream = base.CacheProtocol.ResponseStream;
			_HttpResponse = new HttpWebResponse(_Uri, CurrentMethod, coreResponseData, _MediaType, UsesProxySemantics, AutomaticDecompression);
			if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.ReturnCachedResponse)
			{
				_HttpResponse.InternalSetFromCache = true;
				_HttpResponse.InternalSetIsCacheFresh = base.CacheProtocol.IsCacheFresh;
				if (responseStream != null)
				{
					try
					{
						responseStream.Close();
					}
					catch
					{
					}
				}
			}
			return true;
		}

		private void CheckCacheUpdateOnResponse()
		{
			if (base.CacheProtocol != null)
			{
				if (base.CacheProtocol.GetUpdateStatus(_HttpResponse, _HttpResponse.ResponseStream) == CacheValidationStatus.UpdateResponseInformation)
				{
					_HttpResponse.ResponseStream = base.CacheProtocol.ResponseStream;
				}
				else if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
				{
					throw base.CacheProtocol.ProtocolException;
				}
			}
		}

		private void EndSubmitRequest()
		{
			try
			{
				if (HttpWriteMode == HttpWriteMode.Buffer)
				{
					InvokeGetRequestStreamCallback();
					return;
				}
				if (WriteBuffer == null)
				{
					long value = SwitchToContentLength();
					SerializeHeaders();
					PostSwitchToContentLength(value);
				}
				_SubmitWriteStream.WriteHeaders(Async);
			}
			catch
			{
				_SubmitWriteStream?.CallDone();
				throw;
			}
			finally
			{
				if (!Async)
				{
					CheckWriteSideResponseProcessing();
				}
			}
		}

		internal bool EndWriteHeaders(bool async)
		{
			try
			{
				if ((ContentLength > 0 || HttpWriteMode == HttpWriteMode.Chunked) && ExpectContinue && _ServicePoint.Understands100Continue && (async ? m_ContinueGate.StartTrigger(exclusive: true) : m_ContinueGate.Trigger(exclusive: true)))
				{
					if (async)
					{
						try
						{
							m_ContinueTimer = s_ContinueTimerQueue.CreateTimer(s_ContinueTimeoutCallback, this);
						}
						finally
						{
							m_ContinueGate.FinishTrigger();
						}
						return false;
					}
					_SubmitWriteStream.PollAndRead(UserRetrievedWriteStream);
					return true;
				}
				EndWriteHeaders_Part2();
			}
			catch
			{
				_SubmitWriteStream?.CallDone();
				throw;
			}
			return true;
		}

		private static void ContinueTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)context;
			if (httpWebRequest.HttpWriteMode != HttpWriteMode.None)
			{
				if (httpWebRequest.CompleteContinueGate())
				{
					httpWebRequest.m_ContinueTimer = null;
				}
				ThreadPool.UnsafeQueueUserWorkItem(s_EndWriteHeaders_Part2Callback, httpWebRequest);
			}
		}

		private bool CompleteContinueGate()
		{
			return m_ContinueGate.Complete();
		}

		private static void EndWriteHeaders_Part2Wrapper(object state)
		{
			((HttpWebRequest)state).EndWriteHeaders_Part2();
		}

		internal void EndWriteHeaders_Part2()
		{
			try
			{
				ConnectStream submitWriteStream = _SubmitWriteStream;
				if (HttpWriteMode != HttpWriteMode.None)
				{
					m_BodyStarted = true;
					if (AllowWriteStreamBuffering)
					{
						if (submitWriteStream.BufferOnly)
						{
							_OldSubmitWriteStream = submitWriteStream;
						}
						if (_OldSubmitWriteStream != null)
						{
							submitWriteStream.ResubmitWrite(_OldSubmitWriteStream, NtlmKeepAlive && ContentLength == 0);
							submitWriteStream.CloseInternal(internalCall: true);
						}
					}
				}
				else
				{
					if (submitWriteStream != null)
					{
						submitWriteStream.CloseInternal(internalCall: true);
						submitWriteStream = null;
					}
					_OldSubmitWriteStream = null;
				}
				InvokeGetRequestStreamCallback();
			}
			catch
			{
				_SubmitWriteStream?.CallDone();
				throw;
			}
		}

		private int GenerateConnectRequestLine(int headersSize)
		{
			int num = 0;
			HostHeaderString hostHeaderString = new HostHeaderString(HostAndPort(addDefaultPort: true));
			int num2 = CurrentMethod.Name.Length + hostHeaderString.ByteCount + 12 + headersSize;
			_WriteBuffer = new byte[num2];
			num = Encoding.ASCII.GetBytes(CurrentMethod.Name, 0, CurrentMethod.Name.Length, WriteBuffer, 0);
			WriteBuffer[num++] = 32;
			hostHeaderString.Copy(WriteBuffer, num);
			num += hostHeaderString.ByteCount;
			WriteBuffer[num++] = 32;
			return num;
		}

		internal string HostAndPort(bool addDefaultPort)
		{
			Uri uri = ((!IsTunnelRequest) ? _Uri : _OriginUri);
			string text = ((uri.HostNameType != UriHostNameType.IPv6) ? uri.DnsSafeHost : ("[" + TrimScopeID(uri.DnsSafeHost) + "]"));
			if (addDefaultPort || !uri.IsDefaultPort)
			{
				return text + ":" + uri.Port;
			}
			return text;
		}

		private string TrimScopeID(string s)
		{
			int num = s.LastIndexOf('%');
			if (num > 0)
			{
				return s.Substring(0, num);
			}
			return s;
		}

		private int GenerateProxyRequestLine(int headersSize)
		{
			if ((object)_Uri.Scheme == Uri.UriSchemeFtp)
			{
				return GenerateFtpProxyRequestLine(headersSize);
			}
			int num = 0;
			string components = _Uri.GetComponents(UriComponents.Scheme | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
			HostHeaderString hostHeaderString = new HostHeaderString(HostAndPort(addDefaultPort: false));
			string components2 = _Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
			int num2 = CurrentMethod.Name.Length + components.Length + hostHeaderString.ByteCount + components2.Length + 12 + headersSize;
			_WriteBuffer = new byte[num2];
			num = Encoding.ASCII.GetBytes(CurrentMethod.Name, 0, CurrentMethod.Name.Length, WriteBuffer, 0);
			WriteBuffer[num++] = 32;
			num += Encoding.ASCII.GetBytes(components, 0, components.Length, WriteBuffer, num);
			hostHeaderString.Copy(WriteBuffer, num);
			num += hostHeaderString.ByteCount;
			num += Encoding.ASCII.GetBytes(components2, 0, components2.Length, WriteBuffer, num);
			WriteBuffer[num++] = 32;
			return num;
		}

		private int GenerateFtpProxyRequestLine(int headersSize)
		{
			int num = 0;
			string components = _Uri.GetComponents(UriComponents.Scheme | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
			string text = _Uri.GetComponents(UriComponents.UserInfo | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
			HostHeaderString hostHeaderString = new HostHeaderString(HostAndPort(addDefaultPort: false));
			string components2 = _Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
			if (text == "")
			{
				string text2 = null;
				string text3 = null;
				NetworkCredential credential = Credentials.GetCredential(_Uri, "basic");
				if (credential != null && credential != FtpWebRequest.DefaultNetworkCredential)
				{
					text2 = credential.InternalGetDomainUserName();
					text3 = credential.InternalGetPassword();
					text3 = ((text3 == null) ? string.Empty : text3);
				}
				if (text2 != null)
				{
					text2 = text2.Replace(":", "%3A");
					text3 = text3.Replace(":", "%3A");
					text2 = text2.Replace("\\", "%5C");
					text3 = text3.Replace("\\", "%5C");
					text2 = text2.Replace("/", "%2F");
					text3 = text3.Replace("/", "%2F");
					text2 = text2.Replace("?", "%3F");
					text3 = text3.Replace("?", "%3F");
					text2 = text2.Replace("#", "%23");
					text3 = text3.Replace("#", "%23");
					text2 = text2.Replace("%", "%25");
					text3 = text3.Replace("%", "%25");
					text2 = text2.Replace("@", "%40");
					text3 = text3.Replace("@", "%40");
					text = text2 + ":" + text3 + "@";
				}
			}
			int num2 = CurrentMethod.Name.Length + components.Length + text.Length + hostHeaderString.ByteCount + components2.Length + 12 + headersSize;
			_WriteBuffer = new byte[num2];
			num = Encoding.ASCII.GetBytes(CurrentMethod.Name, 0, CurrentMethod.Name.Length, WriteBuffer, 0);
			WriteBuffer[num++] = 32;
			num += Encoding.ASCII.GetBytes(components, 0, components.Length, WriteBuffer, num);
			num += Encoding.ASCII.GetBytes(text, 0, text.Length, WriteBuffer, num);
			hostHeaderString.Copy(WriteBuffer, num);
			num += hostHeaderString.ByteCount;
			num += Encoding.ASCII.GetBytes(components2, 0, components2.Length, WriteBuffer, num);
			WriteBuffer[num++] = 32;
			return num;
		}

		private int GenerateRequestLine(int headersSize)
		{
			int num = 0;
			string pathAndQuery = _Uri.PathAndQuery;
			int num2 = CurrentMethod.Name.Length + pathAndQuery.Length + 12 + headersSize;
			_WriteBuffer = new byte[num2];
			num = Encoding.ASCII.GetBytes(CurrentMethod.Name, 0, CurrentMethod.Name.Length, WriteBuffer, 0);
			WriteBuffer[num++] = 32;
			num += Encoding.ASCII.GetBytes(pathAndQuery, 0, pathAndQuery.Length, WriteBuffer, num);
			WriteBuffer[num++] = 32;
			return num;
		}

		internal void UpdateHeaders()
		{
			HostHeaderString hostHeaderString = new HostHeaderString(HostAndPort(addDefaultPort: false));
			string @string = WebHeaderCollection.HeaderEncoding.GetString(hostHeaderString.Bytes, 0, hostHeaderString.ByteCount);
			_HttpRequestHeaders.ChangeInternal("Host", @string);
			if (_CookieContainer != null)
			{
				CookieModule.OnSendingHeaders(this);
			}
		}

		internal void SerializeHeaders()
		{
			if (HttpWriteMode != HttpWriteMode.None)
			{
				if (HttpWriteMode == HttpWriteMode.Chunked)
				{
					_HttpRequestHeaders.AddInternal("Transfer-Encoding", "chunked");
				}
				else if (ContentLength >= 0)
				{
					_HttpRequestHeaders.ChangeInternal("Content-Length", _ContentLength.ToString(NumberFormatInfo.InvariantInfo));
				}
				ExpectContinue = ExpectContinue && !IsVersionHttp10 && ServicePoint.Expect100Continue;
				if ((ContentLength > 0 || HttpWriteMode == HttpWriteMode.Chunked) && ExpectContinue)
				{
					_HttpRequestHeaders.AddInternal("Expect", "100-continue");
				}
			}
			if ((AutomaticDecompression & DecompressionMethods.GZip) != 0)
			{
				if ((AutomaticDecompression & DecompressionMethods.Deflate) != 0)
				{
					_HttpRequestHeaders.AddInternal("Accept-Encoding", "gzip, deflate");
				}
				else
				{
					_HttpRequestHeaders.AddInternal("Accept-Encoding", "gzip");
				}
			}
			else if ((AutomaticDecompression & DecompressionMethods.Deflate) != 0)
			{
				_HttpRequestHeaders.AddInternal("Accept-Encoding", "deflate");
			}
			string name = "Connection";
			if (UsesProxySemantics || IsTunnelRequest)
			{
				_HttpRequestHeaders.RemoveInternal("Connection");
				name = "Proxy-Connection";
				if (!ValidationHelper.IsBlankString(Connection))
				{
					_HttpRequestHeaders.AddInternal("Proxy-Connection", _HttpRequestHeaders["Connection"]);
				}
			}
			else
			{
				_HttpRequestHeaders.RemoveInternal("Proxy-Connection");
			}
			if (KeepAlive || NtlmKeepAlive)
			{
				if (IsVersionHttp10 || (int)ServicePoint.HttpBehaviour <= 1)
				{
					_HttpRequestHeaders.AddInternal((UsesProxySemantics || IsTunnelRequest) ? "Proxy-Connection" : "Connection", "Keep-Alive");
				}
			}
			else if (!IsVersionHttp10)
			{
				_HttpRequestHeaders.AddInternal(name, "Close");
			}
			string text = _HttpRequestHeaders.ToString();
			int byteCount = WebHeaderCollection.HeaderEncoding.GetByteCount(text);
			int num = (CurrentMethod.ConnectRequest ? GenerateConnectRequestLine(byteCount) : ((!UsesProxySemantics) ? GenerateRequestLine(byteCount) : GenerateProxyRequestLine(byteCount)));
			Buffer.BlockCopy(HttpBytes, 0, WriteBuffer, num, HttpBytes.Length);
			num += HttpBytes.Length;
			WriteBuffer[num++] = 49;
			WriteBuffer[num++] = 46;
			WriteBuffer[num++] = (byte)(IsVersionHttp10 ? 48 : 49);
			WriteBuffer[num++] = 13;
			WriteBuffer[num++] = 10;
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, this, "Request: " + Encoding.ASCII.GetString(WriteBuffer, 0, num));
			}
			WebHeaderCollection.HeaderEncoding.GetBytes(text, 0, text.Length, WriteBuffer, num);
		}

		internal HttpWebRequest(Uri uri, ServicePoint servicePoint)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "HttpWebRequest", uri);
			}
			new WebPermission(NetworkAccess.Connect, uri).Demand();
			_HttpRequestHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
			_Proxy = WebRequest.InternalDefaultWebProxy;
			_HttpWriteMode = HttpWriteMode.Unknown;
			_MaximumAllowedRedirections = 50;
			_Timeout = 100000;
			_TimerQueue = WebRequest.DefaultTimerQueue;
			_ReadWriteTimeout = 300000;
			_MaximumResponseHeadersLength = DefaultMaximumResponseHeadersLength;
			_ContentLength = -1L;
			_OriginVerb = KnownHttpVerb.Get;
			_OriginUri = uri;
			_Uri = _OriginUri;
			_ServicePoint = servicePoint;
			_RequestIsAsync = TriState.Unspecified;
			SetupCacheProtocol(_OriginUri);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "HttpWebRequest", null);
			}
		}

		internal HttpWebRequest(Uri proxyUri, Uri requestUri, HttpWebRequest orginalRequest)
			: this(proxyUri, null)
		{
			_OriginVerb = KnownHttpVerb.Parse("CONNECT");
			Pipelined = false;
			_OriginUri = requestUri;
			IsTunnelRequest = true;
			_ConnectionGroupName = ServicePointManager.SpecialConnectGroupName + "(" + UniqueGroupId + ")";
			m_InternalConnectionGroup = true;
			ServerAuthenticationState = new AuthenticationState(isProxyAuth: true);
			base.CacheProtocol = null;
		}

		[Obsolete("Serialization is obsoleted for this type.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected HttpWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
			ExceptionHelper.WebPermissionUnrestricted.Demand();
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "HttpWebRequest", serializationInfo);
			}
			_HttpRequestHeaders = (WebHeaderCollection)serializationInfo.GetValue("_HttpRequestHeaders", typeof(WebHeaderCollection));
			_Proxy = (IWebProxy)serializationInfo.GetValue("_Proxy", typeof(IWebProxy));
			KeepAlive = serializationInfo.GetBoolean("_KeepAlive");
			Pipelined = serializationInfo.GetBoolean("_Pipelined");
			AllowAutoRedirect = serializationInfo.GetBoolean("_AllowAutoRedirect");
			AllowWriteStreamBuffering = serializationInfo.GetBoolean("_AllowWriteStreamBuffering");
			HttpWriteMode = (HttpWriteMode)serializationInfo.GetInt32("_HttpWriteMode");
			_MaximumAllowedRedirections = serializationInfo.GetInt32("_MaximumAllowedRedirections");
			_AutoRedirects = serializationInfo.GetInt32("_AutoRedirects");
			_Timeout = serializationInfo.GetInt32("_Timeout");
			try
			{
				_ReadWriteTimeout = serializationInfo.GetInt32("_ReadWriteTimeout");
			}
			catch
			{
				_ReadWriteTimeout = 300000;
			}
			try
			{
				_MaximumResponseHeadersLength = serializationInfo.GetInt32("_MaximumResponseHeadersLength");
			}
			catch
			{
				_MaximumResponseHeadersLength = DefaultMaximumResponseHeadersLength;
			}
			_ContentLength = serializationInfo.GetInt64("_ContentLength");
			_MediaType = serializationInfo.GetString("_MediaType");
			_OriginVerb = KnownHttpVerb.Parse(serializationInfo.GetString("_OriginVerb"));
			_ConnectionGroupName = serializationInfo.GetString("_ConnectionGroupName");
			ProtocolVersion = (Version)serializationInfo.GetValue("_Version", typeof(Version));
			_OriginUri = (Uri)serializationInfo.GetValue("_OriginUri", typeof(Uri));
			SetupCacheProtocol(_OriginUri);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "HttpWebRequest", null);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			GetObjectData(serializationInfo, streamingContext);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			serializationInfo.AddValue("_HttpRequestHeaders", _HttpRequestHeaders, typeof(WebHeaderCollection));
			serializationInfo.AddValue("_Proxy", _Proxy, typeof(IWebProxy));
			serializationInfo.AddValue("_KeepAlive", KeepAlive);
			serializationInfo.AddValue("_Pipelined", Pipelined);
			serializationInfo.AddValue("_AllowAutoRedirect", AllowAutoRedirect);
			serializationInfo.AddValue("_AllowWriteStreamBuffering", AllowWriteStreamBuffering);
			serializationInfo.AddValue("_HttpWriteMode", HttpWriteMode);
			serializationInfo.AddValue("_MaximumAllowedRedirections", _MaximumAllowedRedirections);
			serializationInfo.AddValue("_AutoRedirects", _AutoRedirects);
			serializationInfo.AddValue("_Timeout", _Timeout);
			serializationInfo.AddValue("_ReadWriteTimeout", _ReadWriteTimeout);
			serializationInfo.AddValue("_MaximumResponseHeadersLength", _MaximumResponseHeadersLength);
			serializationInfo.AddValue("_ContentLength", ContentLength);
			serializationInfo.AddValue("_MediaType", _MediaType);
			serializationInfo.AddValue("_OriginVerb", _OriginVerb);
			serializationInfo.AddValue("_ConnectionGroupName", _ConnectionGroupName);
			serializationInfo.AddValue("_Version", ProtocolVersion, typeof(Version));
			serializationInfo.AddValue("_OriginUri", _OriginUri, typeof(Uri));
			base.GetObjectData(serializationInfo, streamingContext);
		}

		internal static StringBuilder GenerateConnectionGroup(string connectionGroupName, bool unsafeConnectionGroup, bool isInternalGroup)
		{
			StringBuilder stringBuilder = new StringBuilder(connectionGroupName);
			stringBuilder.Append(unsafeConnectionGroup ? "U>" : "S>");
			if (isInternalGroup)
			{
				stringBuilder.Append("I>");
			}
			return stringBuilder;
		}

		internal string GetConnectionGroupLine()
		{
			StringBuilder stringBuilder = GenerateConnectionGroup(_ConnectionGroupName, UnsafeAuthenticatedConnectionSharing, m_InternalConnectionGroup);
			if (_Uri.Scheme == Uri.UriSchemeHttps || IsTunnelRequest)
			{
				if (UsesProxy)
				{
					stringBuilder.Append(HostAndPort(addDefaultPort: true));
					stringBuilder.Append("$");
				}
				if (_ClientCertificates != null && ClientCertificates.Count > 0)
				{
					stringBuilder.Append(ClientCertificates.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));
				}
			}
			if (ProxyAuthenticationState.UniqueGroupId != null)
			{
				stringBuilder.Append(ProxyAuthenticationState.UniqueGroupId);
			}
			else if (ServerAuthenticationState.UniqueGroupId != null)
			{
				stringBuilder.Append(ServerAuthenticationState.UniqueGroupId);
			}
			return stringBuilder.ToString();
		}

		private bool CheckResubmitForAuth()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (UsesProxySemantics && _Proxy != null && _Proxy.Credentials != null)
			{
				try
				{
					flag |= ProxyAuthenticationState.AttemptAuthenticate(this, _Proxy.Credentials);
				}
				catch (Win32Exception)
				{
					if (!m_Extra401Retry)
					{
						throw;
					}
					flag3 = true;
				}
				flag2 = true;
			}
			if (Credentials != null && !flag3)
			{
				try
				{
					flag |= ServerAuthenticationState.AttemptAuthenticate(this, Credentials);
				}
				catch (Win32Exception)
				{
					if (!m_Extra401Retry)
					{
						throw;
					}
					flag = false;
				}
				flag2 = true;
			}
			if (!flag && flag2 && m_Extra401Retry)
			{
				ClearAuthenticatedConnectionResources();
				m_Extra401Retry = false;
				flag = true;
			}
			return flag;
		}

		private bool CheckResubmitForCache(ref Exception e)
		{
			if (!CheckCacheRetrieveOnResponse())
			{
				if (AllowAutoRedirect)
				{
					if (Logging.On)
					{
						Logging.PrintWarning(Logging.Web, this, "", SR.GetString("net_log_cache_validation_failed_resubmit"));
					}
					return true;
				}
				if (Logging.On)
				{
					Logging.PrintError(Logging.Web, this, "", SR.GetString("net_log_cache_refused_server_response"));
				}
				e = new InvalidOperationException(SR.GetString("net_cache_not_accept_response"));
				return false;
			}
			CheckCacheUpdateOnResponse();
			return false;
		}

		private bool CheckResubmit(ref Exception e)
		{
			bool flag = false;
			if (ResponseStatusCode == HttpStatusCode.Unauthorized || ResponseStatusCode == HttpStatusCode.ProxyAuthenticationRequired)
			{
				try
				{
					if (!(flag = CheckResubmitForAuth()))
					{
						e = new WebException(SR.GetString("net_servererror", NetRes.GetWebStatusCodeString(ResponseStatusCode, _HttpResponse.StatusDescription)), null, WebExceptionStatus.ProtocolError, _HttpResponse);
						return false;
					}
				}
				catch (Win32Exception innerException)
				{
					throw new WebException(SR.GetString("net_servererror", NetRes.GetWebStatusCodeString(ResponseStatusCode, _HttpResponse.StatusDescription)), innerException, WebExceptionStatus.ProtocolError, _HttpResponse);
				}
			}
			else
			{
				if (ServerAuthenticationState != null && ServerAuthenticationState.Authorization != null)
				{
					HttpWebResponse httpResponse = _HttpResponse;
					if (httpResponse != null)
					{
						httpResponse.InternalSetIsMutuallyAuthenticated = ServerAuthenticationState.Authorization.MutuallyAuthenticated;
						if (base.AuthenticationLevel == AuthenticationLevel.MutualAuthRequired && !httpResponse.IsMutuallyAuthenticated)
						{
							throw new WebException(SR.GetString("net_webstatus_RequestCanceled"), new ProtocolViolationException(SR.GetString("net_mutualauthfailed")), WebExceptionStatus.RequestCanceled, httpResponse);
						}
					}
				}
				if (ResponseStatusCode == HttpStatusCode.BadRequest && SendChunked && ServicePoint.InternalProxyServicePoint)
				{
					ClearAuthenticatedConnectionResources();
					return true;
				}
				if (!AllowAutoRedirect || (ResponseStatusCode != HttpStatusCode.MultipleChoices && ResponseStatusCode != HttpStatusCode.MovedPermanently && ResponseStatusCode != HttpStatusCode.Found && ResponseStatusCode != HttpStatusCode.SeeOther && ResponseStatusCode != HttpStatusCode.TemporaryRedirect))
				{
					if (ResponseStatusCode > (HttpStatusCode)399)
					{
						e = new WebException(SR.GetString("net_servererror", NetRes.GetWebStatusCodeString(ResponseStatusCode, _HttpResponse.StatusDescription)), null, WebExceptionStatus.ProtocolError, _HttpResponse);
						return false;
					}
					if (AllowAutoRedirect && ResponseStatusCode > (HttpStatusCode)299)
					{
						e = new WebException(SR.GetString("net_servererror", NetRes.GetWebStatusCodeString(ResponseStatusCode, _HttpResponse.StatusDescription)), null, WebExceptionStatus.ProtocolError, _HttpResponse);
						return false;
					}
					return false;
				}
				_AutoRedirects++;
				if (_AutoRedirects > _MaximumAllowedRedirections)
				{
					e = new WebException(SR.GetString("net_tooManyRedirections"), null, WebExceptionStatus.ProtocolError, _HttpResponse);
					return false;
				}
				string location = _HttpResponse.Headers.Location;
				if (location == null)
				{
					e = new WebException(SR.GetString("net_servererror", NetRes.GetWebStatusCodeString(ResponseStatusCode, _HttpResponse.StatusDescription)), null, WebExceptionStatus.ProtocolError, _HttpResponse);
					return false;
				}
				Uri uri;
				try
				{
					uri = new Uri(_Uri, location);
				}
				catch (UriFormatException innerException2)
				{
					e = new WebException(SR.GetString("net_resubmitprotofailed"), innerException2, WebExceptionStatus.ProtocolError, _HttpResponse);
					return false;
				}
				if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
				{
					e = new WebException(SR.GetString("net_resubmitprotofailed"), null, WebExceptionStatus.ProtocolError, _HttpResponse);
					return false;
				}
				try
				{
					ExecutionContext executionContext = (Async ? GetReadingContext().ContextCopy : null);
					CodeAccessPermission codeAccessPermission = new WebPermission(NetworkAccess.Connect, uri);
					if (executionContext == null)
					{
						codeAccessPermission.Demand();
					}
					else
					{
						ExecutionContext.Run(executionContext, NclUtilities.ContextRelativeDemandCallback, codeAccessPermission);
					}
				}
				catch (SecurityException innerException3)
				{
					e = new SecurityException(SR.GetString("net_redirect_perm"), new WebException(SR.GetString("net_resubmitcanceled"), innerException3, WebExceptionStatus.ProtocolError, _HttpResponse));
					return false;
				}
				_Uri = uri;
				bool flag2 = false;
				if (ResponseStatusCode > (HttpStatusCode)299 && Logging.On)
				{
					Logging.PrintWarning(Logging.Web, this, "", SR.GetString("net_log_server_response_error_code", ((int)ResponseStatusCode).ToString(NumberFormatInfo.InvariantInfo)));
				}
				switch (ResponseStatusCode)
				{
				case HttpStatusCode.MovedPermanently:
				case HttpStatusCode.Found:
					if (CurrentMethod.Equals(KnownHttpVerb.Post))
					{
						flag2 = true;
					}
					break;
				default:
					flag2 = true;
					break;
				case HttpStatusCode.TemporaryRedirect:
					break;
				}
				if (flag2)
				{
					CurrentMethod = KnownHttpVerb.Get;
					ExpectContinue = false;
					HttpWriteMode = HttpWriteMode.None;
				}
				ICredentials credentials = Credentials as CredentialCache;
				if (credentials == null)
				{
					credentials = Credentials as SystemNetworkCredential;
				}
				if (credentials == null)
				{
					Credentials = null;
				}
				ProxyAuthenticationState.ClearAuthReq(this);
				ServerAuthenticationState.ClearAuthReq(this);
				if (_OriginUri.Scheme == Uri.UriSchemeHttps)
				{
					_HttpRequestHeaders.RemoveInternal("Referer");
				}
			}
			if (HttpWriteMode != HttpWriteMode.None && !AllowWriteStreamBuffering && (HttpWriteMode != HttpWriteMode.ContentLength || ContentLength != 0))
			{
				e = new WebException(SR.GetString("net_need_writebuffering"), null, WebExceptionStatus.ProtocolError, _HttpResponse);
				return false;
			}
			if (!flag)
			{
				ClearAuthenticatedConnectionResources();
			}
			if (Logging.On)
			{
				Logging.PrintWarning(Logging.Web, this, "", SR.GetString("net_log_resubmitting_request"));
			}
			return true;
		}

		private void ClearRequestForResubmit()
		{
			_HttpRequestHeaders.RemoveInternal("Host");
			_HttpRequestHeaders.RemoveInternal("Connection");
			_HttpRequestHeaders.RemoveInternal("Proxy-Connection");
			_HttpRequestHeaders.RemoveInternal("Content-Length");
			_HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
			_HttpRequestHeaders.RemoveInternal("Expect");
			if (_HttpResponse != null && _HttpResponse.ResponseStream != null)
			{
				if (!_HttpResponse.KeepAlive)
				{
					(_HttpResponse.ResponseStream as ConnectStream)?.ErrorResponseNotify(isKeepAlive: false);
				}
				ICloseEx closeEx = _HttpResponse.ResponseStream as ICloseEx;
				if (closeEx != null)
				{
					closeEx.CloseEx(CloseExState.Silent);
				}
				else
				{
					_HttpResponse.ResponseStream.Close();
				}
			}
			_AbortDelegate = null;
			if (_SubmitWriteStream != null)
			{
				if (((_HttpResponse != null && _HttpResponse.KeepAlive) || _SubmitWriteStream.IgnoreSocketErrors) && HasEntityBody)
				{
					SetRequestContinue();
					if (!Async && UserRetrievedWriteStream)
					{
						_SubmitWriteStream.CallDone();
					}
				}
				if ((Async || UserRetrievedWriteStream) && _OldSubmitWriteStream != null && _OldSubmitWriteStream != _SubmitWriteStream)
				{
					_SubmitWriteStream.CloseInternal(internalCall: true);
				}
			}
			m_ContinueGate.Reset();
			_RerequestCount++;
			m_BodyStarted = false;
			HeadersCompleted = false;
			_WriteBuffer = null;
			m_Extra401Retry = false;
			_HttpResponse = null;
			if (!Aborted && Async)
			{
				_CoreResponse = null;
			}
		}

		private void FinishRequest(HttpWebResponse response, Exception errorException)
		{
			if (!_ReadAResult.InternalPeekCompleted && m_Aborted != 1 && response != null && errorException != null)
			{
				response.ResponseStream = MakeMemoryStream(response.ResponseStream);
			}
			if (errorException != null && _SubmitWriteStream != null && !_SubmitWriteStream.IsClosed)
			{
				_SubmitWriteStream.ErrorResponseNotify(_SubmitWriteStream.Connection.KeepAlive);
			}
			if (errorException == null && _HttpResponse != null && (_HttpWriteMode == HttpWriteMode.Chunked || _ContentLength > 0) && ExpectContinue && !Saw100Continue && _ServicePoint.Understands100Continue && !IsTunnelRequest && ResponseStatusCode <= (HttpStatusCode)299)
			{
				_ServicePoint.Understands100Continue = false;
			}
		}

		private Stream MakeMemoryStream(Stream stream)
		{
			if (stream == null || stream is SyncMemoryStream)
			{
				return stream;
			}
			SyncMemoryStream syncMemoryStream = new SyncMemoryStream(0);
			try
			{
				if (stream.CanRead)
				{
					byte[] array = new byte[1024];
					int num = 0;
					int num2 = ((DefaultMaximumErrorResponseLength == -1) ? array.Length : (DefaultMaximumErrorResponseLength * 1024));
					while ((num = stream.Read(array, 0, Math.Min(array.Length, num2))) > 0)
					{
						syncMemoryStream.Write(array, 0, num);
						if (DefaultMaximumErrorResponseLength != -1)
						{
							num2 -= num;
						}
					}
				}
				syncMemoryStream.Position = 0L;
				return syncMemoryStream;
			}
			catch
			{
				return syncMemoryStream;
			}
			finally
			{
				try
				{
					ICloseEx closeEx = stream as ICloseEx;
					if (closeEx != null)
					{
						closeEx.CloseEx(CloseExState.Silent);
					}
					else
					{
						stream.Close();
					}
				}
				catch
				{
				}
			}
		}

		public void AddRange(int from, int to)
		{
			AddRange("bytes", from, to);
		}

		public void AddRange(int range)
		{
			AddRange("bytes", range);
		}

		public void AddRange(string rangeSpecifier, int from, int to)
		{
			if (rangeSpecifier == null)
			{
				throw new ArgumentNullException("rangeSpecifier");
			}
			if (from < 0 || to < 0)
			{
				throw new ArgumentOutOfRangeException(SR.GetString("net_rangetoosmall"));
			}
			if (from > to)
			{
				throw new ArgumentOutOfRangeException(SR.GetString("net_fromto"));
			}
			if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
			{
				throw new ArgumentException(SR.GetString("net_nottoken"), "rangeSpecifier");
			}
			if (!AddRange(rangeSpecifier, from.ToString(NumberFormatInfo.InvariantInfo), to.ToString(NumberFormatInfo.InvariantInfo)))
			{
				throw new InvalidOperationException(SR.GetString("net_rangetype"));
			}
		}

		public void AddRange(string rangeSpecifier, int range)
		{
			if (rangeSpecifier == null)
			{
				throw new ArgumentNullException("rangeSpecifier");
			}
			if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
			{
				throw new ArgumentException(SR.GetString("net_nottoken"), "rangeSpecifier");
			}
			if (!AddRange(rangeSpecifier, range.ToString(NumberFormatInfo.InvariantInfo), (range >= 0) ? "" : null))
			{
				throw new InvalidOperationException(SR.GetString("net_rangetype"));
			}
		}

		private bool AddRange(string rangeSpecifier, string from, string to)
		{
			string text = _HttpRequestHeaders["Range"];
			if (text == null || text.Length == 0)
			{
				text = rangeSpecifier + "=";
			}
			else
			{
				if (string.Compare(text.Substring(0, text.IndexOf('=')), rangeSpecifier, StringComparison.OrdinalIgnoreCase) != 0)
				{
					return false;
				}
				text = string.Empty;
			}
			text += from.ToString();
			if (to != null)
			{
				text = text + "-" + to;
			}
			_HttpRequestHeaders.SetAddVerified("Range", text);
			return true;
		}
	}
}
