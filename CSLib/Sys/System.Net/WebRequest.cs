using System.Collections;
using System.IO;
using System.Net.Cache;
using System.Net.Configuration;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace System.Net
{
	[Serializable]
	public abstract class WebRequest : MarshalByRefObject, ISerializable
	{
		internal class WebProxyWrapperOpaque : IAutoWebProxy, IWebProxy
		{
			protected readonly WebProxy webProxy;

			public ICredentials Credentials
			{
				get
				{
					return webProxy.Credentials;
				}
				set
				{
					webProxy.Credentials = value;
				}
			}

			internal WebProxyWrapperOpaque(WebProxy webProxy)
			{
				this.webProxy = webProxy;
			}

			public Uri GetProxy(Uri destination)
			{
				return webProxy.GetProxy(destination);
			}

			public bool IsBypassed(Uri host)
			{
				return webProxy.IsBypassed(host);
			}

			public ProxyChain GetProxies(Uri destination)
			{
				return ((IAutoWebProxy)webProxy).GetProxies(destination);
			}
		}

		internal class WebProxyWrapper : WebProxyWrapperOpaque
		{
			internal WebProxy WebProxy => webProxy;

			internal WebProxyWrapper(WebProxy webProxy)
				: base(webProxy)
			{
			}
		}

		internal const int DefaultTimeout = 100000;

		private static ArrayList s_PrefixList;

		private static object s_InternalSyncObject;

		private static TimerThread.Queue s_DefaultTimerQueue = TimerThread.CreateQueue(100000);

		private AuthenticationLevel m_AuthenticationLevel;

		private TokenImpersonationLevel m_ImpersonationLevel;

		private RequestCachePolicy m_CachePolicy;

		private RequestCacheProtocol m_CacheProtocol;

		private RequestCacheBinding m_CacheBinding;

		private static IWebProxy s_DefaultWebProxy;

		private static bool s_DefaultWebProxyInitialized;

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		internal static TimerThread.Queue DefaultTimerQueue => s_DefaultTimerQueue;

		internal static ArrayList PrefixList
		{
			get
			{
				if (s_PrefixList == null)
				{
					lock (InternalSyncObject)
					{
						if (s_PrefixList == null)
						{
							s_PrefixList = WebRequestModulesSectionInternal.GetSection().WebRequestModules;
						}
					}
				}
				return s_PrefixList;
			}
			set
			{
				s_PrefixList = value;
			}
		}

		public static RequestCachePolicy DefaultCachePolicy
		{
			get
			{
				return RequestCacheManager.GetBinding(string.Empty).Policy;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				RequestCacheBinding binding = RequestCacheManager.GetBinding(string.Empty);
				RequestCacheManager.SetBinding(string.Empty, new RequestCacheBinding(binding.Cache, binding.Validator, value));
			}
		}

		public virtual RequestCachePolicy CachePolicy
		{
			get
			{
				return m_CachePolicy;
			}
			set
			{
				InternalSetCachePolicy(value);
			}
		}

		public virtual string Method
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual Uri RequestUri
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual string ConnectionGroupName
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual WebHeaderCollection Headers
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual long ContentLength
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual string ContentType
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual ICredentials Credentials
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual bool UseDefaultCredentials
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual IWebProxy Proxy
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual bool PreAuthenticate
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		public virtual int Timeout
		{
			get
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotImplementedException;
			}
		}

		internal RequestCacheProtocol CacheProtocol
		{
			get
			{
				return m_CacheProtocol;
			}
			set
			{
				m_CacheProtocol = value;
			}
		}

		public AuthenticationLevel AuthenticationLevel
		{
			get
			{
				return m_AuthenticationLevel;
			}
			set
			{
				m_AuthenticationLevel = value;
			}
		}

		public TokenImpersonationLevel ImpersonationLevel
		{
			get
			{
				return m_ImpersonationLevel;
			}
			set
			{
				m_ImpersonationLevel = value;
			}
		}

		internal static IWebProxy InternalDefaultWebProxy
		{
			get
			{
				if (!s_DefaultWebProxyInitialized)
				{
					lock (InternalSyncObject)
					{
						if (!s_DefaultWebProxyInitialized)
						{
							DefaultProxySectionInternal section = DefaultProxySectionInternal.GetSection();
							if (section != null)
							{
								s_DefaultWebProxy = section.WebProxy;
							}
							s_DefaultWebProxyInitialized = true;
						}
					}
				}
				return s_DefaultWebProxy;
			}
			set
			{
				if (!s_DefaultWebProxyInitialized)
				{
					lock (InternalSyncObject)
					{
						s_DefaultWebProxy = value;
						s_DefaultWebProxyInitialized = true;
					}
				}
				else
				{
					s_DefaultWebProxy = value;
				}
			}
		}

		public static IWebProxy DefaultWebProxy
		{
			get
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				return InternalDefaultWebProxy;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				InternalDefaultWebProxy = value;
			}
		}

		private static WebRequest Create(Uri requestUri, bool useUriBase)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, "WebRequest", "Create", requestUri.ToString());
			}
			WebRequestPrefixElement webRequestPrefixElement = null;
			bool flag = false;
			string text = (useUriBase ? (requestUri.Scheme + ':') : requestUri.AbsoluteUri);
			int length = text.Length;
			ArrayList prefixList = PrefixList;
			for (int i = 0; i < prefixList.Count; i++)
			{
				webRequestPrefixElement = (WebRequestPrefixElement)prefixList[i];
				if (length >= webRequestPrefixElement.Prefix.Length && string.Compare(webRequestPrefixElement.Prefix, 0, text, 0, webRequestPrefixElement.Prefix.Length, StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = true;
					break;
				}
			}
			WebRequest webRequest = null;
			if (flag)
			{
				webRequest = webRequestPrefixElement.Creator.Create(requestUri);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, "WebRequest", "Create", webRequest);
				}
				return webRequest;
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, "WebRequest", "Create", null);
			}
			throw new NotSupportedException(SR.GetString("net_unknown_prefix"));
		}

		public static WebRequest Create(string requestUriString)
		{
			if (requestUriString == null)
			{
				throw new ArgumentNullException("requestUriString");
			}
			return Create(new Uri(requestUriString), useUriBase: false);
		}

		public static WebRequest Create(Uri requestUri)
		{
			if (requestUri == null)
			{
				throw new ArgumentNullException("requestUri");
			}
			return Create(requestUri, useUriBase: false);
		}

		public static WebRequest CreateDefault(Uri requestUri)
		{
			if (requestUri == null)
			{
				throw new ArgumentNullException("requestUri");
			}
			return Create(requestUri, useUriBase: true);
		}

		public static bool RegisterPrefix(string prefix, IWebRequestCreate creator)
		{
			bool flag = false;
			if (prefix == null)
			{
				throw new ArgumentNullException("prefix");
			}
			if (creator == null)
			{
				throw new ArgumentNullException("creator");
			}
			ExceptionHelper.WebPermissionUnrestricted.Demand();
			lock (InternalSyncObject)
			{
				ArrayList arrayList = (ArrayList)PrefixList.Clone();
				int i;
				for (i = 0; i < arrayList.Count; i++)
				{
					WebRequestPrefixElement webRequestPrefixElement = (WebRequestPrefixElement)arrayList[i];
					if (prefix.Length > webRequestPrefixElement.Prefix.Length)
					{
						break;
					}
					if (prefix.Length == webRequestPrefixElement.Prefix.Length && string.Compare(webRequestPrefixElement.Prefix, prefix, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					arrayList.Insert(i, new WebRequestPrefixElement(prefix, creator));
					PrefixList = arrayList;
				}
			}
			return !flag;
		}

		protected WebRequest()
		{
			m_ImpersonationLevel = TokenImpersonationLevel.Delegation;
			m_AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
		}

		protected WebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			GetObjectData(serializationInfo, streamingContext);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
		}

		private void InternalSetCachePolicy(RequestCachePolicy policy)
		{
			if (m_CacheBinding != null && m_CacheBinding.Cache != null && m_CacheBinding.Validator != null && CacheProtocol == null && policy != null && policy.Level != RequestCacheLevel.BypassCache)
			{
				CacheProtocol = new RequestCacheProtocol(m_CacheBinding.Cache, m_CacheBinding.Validator.CreateValidator());
			}
			m_CachePolicy = policy;
		}

		public virtual Stream GetRequestStream()
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		public virtual WebResponse GetResponse()
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public virtual IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		public virtual WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public virtual IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		public virtual Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		public virtual void Abort()
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		internal virtual ContextAwareResult GetConnectingContext()
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		internal virtual ContextAwareResult GetWritingContext()
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		internal virtual ContextAwareResult GetReadingContext()
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		internal virtual void RequestCallback(object obj)
		{
			throw ExceptionHelper.MethodNotImplementedException;
		}

		public static IWebProxy GetSystemWebProxy()
		{
			ExceptionHelper.WebPermissionUnrestricted.Demand();
			return InternalGetSystemWebProxy();
		}

		internal static IWebProxy InternalGetSystemWebProxy()
		{
			return new WebProxyWrapperOpaque(new WebProxy(enableAutoproxy: true));
		}

		internal void SetupCacheProtocol(Uri uri)
		{
			m_CacheBinding = RequestCacheManager.GetBinding(uri.Scheme);
			InternalSetCachePolicy(m_CacheBinding.Policy);
			if (m_CachePolicy == null)
			{
				InternalSetCachePolicy(DefaultCachePolicy);
			}
		}
	}
}
