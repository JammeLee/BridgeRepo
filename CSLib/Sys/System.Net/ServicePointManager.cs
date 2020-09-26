using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net.Configuration;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;

namespace System.Net
{
	public class ServicePointManager
	{
		private delegate T ConfigurationLoaderDelegate<T>(T initialValue);

		public const int DefaultNonPersistentConnectionLimit = 4;

		public const int DefaultPersistentConnectionLimit = 2;

		private const int DefaultAspPersistentConnectionLimit = 10;

		private const string RegistryGlobalStrongCryptoName = "SchUseStrongCrypto";

		private const string RegistryGlobalSendAuxRecordName = "SchSendAuxRecord";

		private const string RegistryLocalSendAuxRecordName = "System.Net.ServicePointManager.SchSendAuxRecord";

		private const string RegistryGlobalSystemDefaultTlsVersionsName = "SystemDefaultTlsVersions";

		private const string RegistryLocalSystemDefaultTlsVersionsName = "System.Net.ServicePointManager.SystemDefaultTlsVersions";

		private const string RegistryLocalSecureProtocolName = "System.Net.ServicePointManager.SecurityProtocol";

		private const string RegistryGlobalRequireCertificateEKUs = "RequireCertificateEKUs";

		private const string RegistryLocalRequireCertificateEKUs = "System.Net.ServicePointManager.RequireCertificateEKUs";

		private const string RegistryGlobalUseHttpPipeliningAndBufferPooling = "UseHttpPipeliningAndBufferPooling";

		private const string RegistryLocalUseHttpPipeliningAndBufferPooling = "System.Net.ServicePointManager.UseHttpPipeliningAndBufferPooling";

		private const string RegistryGlobalUseStrictRfcInterimResponseHandling = "UseStrictRfcInterimResponseHandling";

		private const string RegistryLocalUseStrictRfcInterimResponseHandling = "System.Net.ServicePointManager.UseStrictRfcInterimResponseHandling";

		private const string RegistryGlobalAllowDangerousUnicodeDecompositions = "AllowDangerousUnicodeDecompositions";

		private const string RegistryLocalAllowDangerousUnicodeDecompositions = "System.Uri.AllowDangerousUnicodeDecompositions";

		private const string RegistryGlobalUseStrictIPv6AddressParsing = "UseStrictIPv6AddressParsing";

		private const string RegistryLocalUseStrictIPv6AddressParsing = "System.Uri.UseStrictIPv6AddressParsing";

		private const string RegistryGlobalAllowAllUriEncodingExpansion = "AllowAllUriEncodingExpansion";

		private const string RegistryLocalAllowAllUriEncodingExpansion = "System.Uri.AllowAllUriEncodingExpansion";

		internal static readonly string SpecialConnectGroupName = "/.NET/NetClasses/HttpWebRequest/CONNECT__Group$$/";

		internal static readonly TimerThread.Callback s_IdleServicePointTimeoutDelegate = IdleServicePointTimeoutCallback;

		private static Hashtable s_ServicePointTable = new Hashtable(10);

		private static TimerThread.Queue s_ServicePointIdlingQueue = TimerThread.GetOrCreateQueue(100000);

		private static int s_MaxServicePoints = 0;

		private static CertPolicyValidationCallback s_CertPolicyValidationCallback = new CertPolicyValidationCallback();

		private static ServerCertValidationCallback s_ServerCertValidationCallback = null;

		private static bool s_disableStrongCrypto;

		private static bool s_disableSendAuxRecord;

		private static bool s_disableSystemDefaultTlsVersions;

		private static SslProtocols s_defaultSslProtocols;

		private static bool s_dontCheckCertificateEKUs;

		private static bool s_useHttpPipeliningAndBufferPooling;

		private static bool s_useStrictRfcInterimResponseHandling;

		private static bool s_allowDangerousUnicodeDecompositions;

		private static bool s_useStrictIPv6AddressParsing;

		private static bool s_allowAllUriEncodingExpansion;

		private static Hashtable s_ConfigTable = null;

		private static int s_ConnectionLimit = PersistentConnectionLimit;

		private static SecurityProtocolType s_SecurityProtocolType;

		internal static bool s_UseTcpKeepAlive = false;

		internal static int s_TcpKeepAliveTime;

		internal static int s_TcpKeepAliveInterval;

		private static bool s_UserChangedLimit;

		private static object configurationLoadedLock = new object();

		private static volatile bool configurationLoaded = false;

		private static int InternalConnectionLimit
		{
			get
			{
				if (s_ConfigTable == null)
				{
					s_ConfigTable = ConfigTable;
				}
				return s_ConnectionLimit;
			}
			set
			{
				if (s_ConfigTable == null)
				{
					s_ConfigTable = ConfigTable;
				}
				s_UserChangedLimit = true;
				s_ConnectionLimit = value;
			}
		}

		private static int PersistentConnectionLimit
		{
			get
			{
				if (ComNetOS.IsAspNetServer)
				{
					return 10;
				}
				return 2;
			}
		}

		private static Hashtable ConfigTable
		{
			get
			{
				if (s_ConfigTable == null)
				{
					lock (s_ServicePointTable)
					{
						if (s_ConfigTable == null)
						{
							Hashtable hashtable = ConnectionManagementSectionInternal.GetSection().ConnectionManagement;
							if (hashtable == null)
							{
								hashtable = new Hashtable();
							}
							if (hashtable.ContainsKey("*"))
							{
								int num = (int)hashtable["*"];
								if (num < 1)
								{
									num = PersistentConnectionLimit;
								}
								s_ConnectionLimit = num;
							}
							s_ConfigTable = hashtable;
						}
					}
				}
				return s_ConfigTable;
			}
		}

		internal static TimerThread.Callback IdleServicePointTimeoutDelegate => s_IdleServicePointTimeoutDelegate;

		public static SecurityProtocolType SecurityProtocol
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_SecurityProtocolType;
			}
			set
			{
				EnsureConfigurationLoaded();
				ValidateSecurityProtocol(value);
				s_SecurityProtocolType = value;
			}
		}

		internal static bool DisableStrongCrypto
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_disableStrongCrypto;
			}
		}

		internal static bool DisableSystemDefaultTlsVersions
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_disableSystemDefaultTlsVersions;
			}
		}

		internal static bool DisableSendAuxRecord
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_disableSendAuxRecord;
			}
		}

		internal static bool DisableCertificateEKUs
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_dontCheckCertificateEKUs;
			}
		}

		internal static SslProtocols DefaultSslProtocols
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_defaultSslProtocols;
			}
		}

		internal static bool UseHttpPipeliningAndBufferPooling
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_useHttpPipeliningAndBufferPooling;
			}
		}

		internal static bool UseStrictRfcInterimResponseHandling
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_useStrictRfcInterimResponseHandling;
			}
		}

		internal static bool AllowDangerousUnicodeDecompositions
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_allowDangerousUnicodeDecompositions;
			}
		}

		internal static bool UseStrictIPv6AddressParsing
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_useStrictIPv6AddressParsing;
			}
		}

		internal static bool AllowAllUriEncodingExpansion
		{
			get
			{
				EnsureConfigurationLoaded();
				return s_allowAllUriEncodingExpansion;
			}
		}

		public static int MaxServicePoints
		{
			get
			{
				return s_MaxServicePoints;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (!ValidationHelper.ValidateRange(value, 0, int.MaxValue))
				{
					throw new ArgumentOutOfRangeException("value");
				}
				s_MaxServicePoints = value;
			}
		}

		public static int DefaultConnectionLimit
		{
			get
			{
				return InternalConnectionLimit;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (value > 0)
				{
					InternalConnectionLimit = value;
					return;
				}
				throw new ArgumentOutOfRangeException(SR.GetString("net_toosmall"));
			}
		}

		public static int MaxServicePointIdleTime
		{
			get
			{
				return s_ServicePointIdlingQueue.Duration;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (!ValidationHelper.ValidateRange(value, -1, int.MaxValue))
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (s_ServicePointIdlingQueue.Duration != value)
				{
					s_ServicePointIdlingQueue = TimerThread.GetOrCreateQueue(value);
				}
			}
		}

		public static bool UseNagleAlgorithm
		{
			get
			{
				return SettingsSectionInternal.Section.UseNagleAlgorithm;
			}
			set
			{
				SettingsSectionInternal.Section.UseNagleAlgorithm = value;
			}
		}

		public static bool Expect100Continue
		{
			get
			{
				return SettingsSectionInternal.Section.Expect100Continue;
			}
			set
			{
				SettingsSectionInternal.Section.Expect100Continue = value;
			}
		}

		public static bool EnableDnsRoundRobin
		{
			get
			{
				return SettingsSectionInternal.Section.EnableDnsRoundRobin;
			}
			set
			{
				SettingsSectionInternal.Section.EnableDnsRoundRobin = value;
			}
		}

		public static int DnsRefreshTimeout
		{
			get
			{
				return SettingsSectionInternal.Section.DnsRefreshTimeout;
			}
			set
			{
				if (value < -1)
				{
					SettingsSectionInternal.Section.DnsRefreshTimeout = -1;
				}
				else
				{
					SettingsSectionInternal.Section.DnsRefreshTimeout = value;
				}
			}
		}

		[Obsolete("CertificatePolicy is obsoleted for this type, please use ServerCertificateValidationCallback instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static ICertificatePolicy CertificatePolicy
		{
			get
			{
				return GetLegacyCertificatePolicy();
			}
			set
			{
				ExceptionHelper.UnmanagedPermission.Demand();
				s_CertPolicyValidationCallback = new CertPolicyValidationCallback(value);
			}
		}

		internal static CertPolicyValidationCallback CertPolicyValidationCallback => s_CertPolicyValidationCallback;

		public static RemoteCertificateValidationCallback ServerCertificateValidationCallback
		{
			get
			{
				if (s_ServerCertValidationCallback == null)
				{
					return null;
				}
				return s_ServerCertValidationCallback.ValidationCallback;
			}
			set
			{
				ExceptionHelper.InfrastructurePermission.Demand();
				s_ServerCertValidationCallback = new ServerCertValidationCallback(value);
			}
		}

		internal static ServerCertValidationCallback ServerCertValidationCallback => s_ServerCertValidationCallback;

		public static bool CheckCertificateRevocationList
		{
			get
			{
				return SettingsSectionInternal.Section.CheckCertificateRevocationList;
			}
			set
			{
				ExceptionHelper.UnmanagedPermission.Demand();
				SettingsSectionInternal.Section.CheckCertificateRevocationList = value;
			}
		}

		internal static bool CheckCertificateName => SettingsSectionInternal.Section.CheckCertificateName;

		[Conditional("DEBUG")]
		internal static void Debug(int requestHash)
		{
			try
			{
				foreach (WeakReference item in s_ServicePointTable)
				{
					if (item != null && item.IsAlive)
					{
						ServicePoint servicePoint = (ServicePoint)item.Target;
					}
					else
					{
						ServicePoint servicePoint = null;
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

		private static void IdleServicePointTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
		{
			ServicePoint servicePoint = (ServicePoint)context;
			lock (s_ServicePointTable)
			{
				s_ServicePointTable.Remove(servicePoint.LookupString);
			}
			servicePoint.ReleaseAllConnectionGroups();
		}

		private ServicePointManager()
		{
		}

		private static void ValidateSecurityProtocol(SecurityProtocolType value)
		{
			SecurityProtocolType securityProtocolType = (SecurityProtocolType)4080;
			if ((value & ~securityProtocolType) != 0)
			{
				throw new NotSupportedException(SR.GetString("net_securityprotocolnotsupported"));
			}
		}

		internal static ICertificatePolicy GetLegacyCertificatePolicy()
		{
			if (s_CertPolicyValidationCallback == null)
			{
				return null;
			}
			return s_CertPolicyValidationCallback.CertificatePolicy;
		}

		internal static string MakeQueryString(Uri address)
		{
			if (address.IsDefaultPort)
			{
				return address.Scheme + "://" + address.DnsSafeHost;
			}
			return address.Scheme + "://" + address.DnsSafeHost + ":" + address.Port;
		}

		internal static string MakeQueryString(Uri address1, bool isProxy)
		{
			if (isProxy)
			{
				return MakeQueryString(address1) + "://proxy";
			}
			return MakeQueryString(address1);
		}

		public static ServicePoint FindServicePoint(Uri address)
		{
			return FindServicePoint(address, null);
		}

		public static ServicePoint FindServicePoint(string uriString, IWebProxy proxy)
		{
			Uri address = new Uri(uriString);
			return FindServicePoint(address, proxy);
		}

		public static ServicePoint FindServicePoint(Uri address, IWebProxy proxy)
		{
			HttpAbortDelegate abortDelegate = null;
			int abortState = 0;
			ProxyChain chain;
			return FindServicePoint(address, proxy, out chain, ref abortDelegate, ref abortState);
		}

		internal static ServicePoint FindServicePoint(Uri address, IWebProxy proxy, out ProxyChain chain, ref HttpAbortDelegate abortDelegate, ref int abortState)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			bool isProxyServicePoint = false;
			chain = null;
			Uri uri = null;
			if (proxy != null && !address.IsLoopback)
			{
				IAutoWebProxy autoWebProxy = proxy as IAutoWebProxy;
				if (autoWebProxy != null)
				{
					chain = autoWebProxy.GetProxies(address);
					abortDelegate = chain.HttpAbortDelegate;
					try
					{
						Thread.MemoryBarrier();
						if (abortState != 0)
						{
							Exception ex = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
							throw ex;
						}
						chain.Enumerator.MoveNext();
						uri = chain.Enumerator.Current;
					}
					finally
					{
						abortDelegate = null;
					}
				}
				else if (!proxy.IsBypassed(address))
				{
					uri = proxy.GetProxy(address);
				}
				if (uri != null)
				{
					address = uri;
					isProxyServicePoint = true;
				}
			}
			return FindServicePointHelper(address, isProxyServicePoint);
		}

		internal static ServicePoint FindServicePoint(ProxyChain chain)
		{
			if (!chain.Enumerator.MoveNext())
			{
				return null;
			}
			Uri current = chain.Enumerator.Current;
			return FindServicePointHelper((current == null) ? chain.Destination : current, current != null);
		}

		private static ServicePoint FindServicePointHelper(Uri address, bool isProxyServicePoint)
		{
			if (isProxyServicePoint && address.Scheme != Uri.UriSchemeHttp)
			{
				Exception ex = new NotSupportedException(SR.GetString("net_proxyschemenotsupported", address.Scheme));
				throw ex;
			}
			string text = MakeQueryString(address, isProxyServicePoint);
			ServicePoint servicePoint = null;
			lock (s_ServicePointTable)
			{
				WeakReference weakReference = s_ServicePointTable[text] as WeakReference;
				if (weakReference != null)
				{
					servicePoint = (ServicePoint)weakReference.Target;
				}
				if (servicePoint == null)
				{
					if (s_MaxServicePoints <= 0 || s_ServicePointTable.Count < s_MaxServicePoints)
					{
						int defaultConnectionLimit = InternalConnectionLimit;
						string key = MakeQueryString(address);
						bool userChangedLimit = s_UserChangedLimit;
						if (ConfigTable.ContainsKey(key))
						{
							defaultConnectionLimit = (int)ConfigTable[key];
							userChangedLimit = true;
						}
						servicePoint = new ServicePoint(address, s_ServicePointIdlingQueue, defaultConnectionLimit, text, userChangedLimit, isProxyServicePoint);
						weakReference = new WeakReference(servicePoint);
						s_ServicePointTable[text] = weakReference;
						return servicePoint;
					}
					Exception ex2 = new InvalidOperationException(SR.GetString("net_maxsrvpoints"));
					throw ex2;
				}
				return servicePoint;
			}
		}

		internal static ServicePoint FindServicePoint(string host, int port)
		{
			if (host == null)
			{
				throw new ArgumentNullException("address");
			}
			string text = null;
			bool proxyServicePoint = false;
			text = "ByHost:" + host + ":" + port.ToString(CultureInfo.InvariantCulture);
			ServicePoint servicePoint = null;
			lock (s_ServicePointTable)
			{
				WeakReference weakReference = s_ServicePointTable[text] as WeakReference;
				if (weakReference != null)
				{
					servicePoint = (ServicePoint)weakReference.Target;
				}
				if (servicePoint == null)
				{
					if (s_MaxServicePoints <= 0 || s_ServicePointTable.Count < s_MaxServicePoints)
					{
						int defaultConnectionLimit = InternalConnectionLimit;
						bool userChangedLimit = s_UserChangedLimit;
						string key = host + ":" + port.ToString(CultureInfo.InvariantCulture);
						if (ConfigTable.ContainsKey(key))
						{
							defaultConnectionLimit = (int)ConfigTable[key];
							userChangedLimit = true;
						}
						servicePoint = new ServicePoint(host, port, s_ServicePointIdlingQueue, defaultConnectionLimit, text, userChangedLimit, proxyServicePoint);
						weakReference = new WeakReference(servicePoint);
						s_ServicePointTable[text] = weakReference;
						return servicePoint;
					}
					Exception ex = new InvalidOperationException(SR.GetString("net_maxsrvpoints"));
					throw ex;
				}
				return servicePoint;
			}
		}

		public static void SetTcpKeepAlive(bool enabled, int keepAliveTime, int keepAliveInterval)
		{
			if (enabled)
			{
				s_UseTcpKeepAlive = true;
				if (keepAliveTime <= 0)
				{
					throw new ArgumentOutOfRangeException("keepAliveTime");
				}
				if (keepAliveInterval <= 0)
				{
					throw new ArgumentOutOfRangeException("keepAliveInterval");
				}
				s_TcpKeepAliveTime = keepAliveTime;
				s_TcpKeepAliveInterval = keepAliveInterval;
			}
			else
			{
				s_UseTcpKeepAlive = false;
				s_TcpKeepAliveTime = 0;
				s_TcpKeepAliveInterval = 0;
			}
		}

		private static void EnsureConfigurationLoaded()
		{
			if (configurationLoaded)
			{
				return;
			}
			lock (configurationLoadedLock)
			{
				if (!configurationLoaded)
				{
					s_useHttpPipeliningAndBufferPooling = TryInitialize(LoadUseHttpPipeliningAndBufferPoolingConfiguration, fallbackDefault: false);
					s_useStrictRfcInterimResponseHandling = TryInitialize(LoadUseStrictRfcInterimResponseHandlingConfiguration, fallbackDefault: true);
					s_allowDangerousUnicodeDecompositions = TryInitialize(LoadAllowDangerousUnicodeDecompositionsConfiguration, fallbackDefault: false);
					s_useStrictIPv6AddressParsing = TryInitialize(LoadUseStrictIPv6AddressParsingConfiguration, fallbackDefault: true);
					s_allowAllUriEncodingExpansion = TryInitialize(LoadAllowAllUriEncodingExpansionConfiguration, fallbackDefault: false);
					s_disableStrongCrypto = TryInitialize(LoadDisableStrongCryptoConfiguration, fallbackDefault: true);
					s_disableSendAuxRecord = TryInitialize(LoadDisableSendAuxRecordConfiguration, fallbackDefault: false);
					s_disableSystemDefaultTlsVersions = TryInitialize(LoadDisableSystemDefaultTlsVersionsConfiguration, fallbackDefault: true);
					s_dontCheckCertificateEKUs = TryInitialize(LoadDisableCertificateEKUsConfiguration, fallbackDefault: false);
					s_defaultSslProtocols = TryInitialize(LoadSecureProtocolConfiguration, SslProtocols.Default);
					s_SecurityProtocolType = (SecurityProtocolType)s_defaultSslProtocols;
					configurationLoaded = true;
				}
			}
		}

		private static T TryInitialize<T>(ConfigurationLoaderDelegate<T> loadConfiguration, T fallbackDefault)
		{
			try
			{
				return loadConfiguration(fallbackDefault);
			}
			catch (Exception exception)
			{
				if (NclUtilities.IsFatal(exception))
				{
					throw;
				}
				return fallbackDefault;
			}
		}

		private static bool LoadDisableStrongCryptoConfiguration(bool disable)
		{
			int num = 0;
			num = RegistryConfiguration.GlobalConfigReadInt("SchUseStrongCrypto", 0);
			disable = num != 1;
			return disable;
		}

		private static bool LoadDisableSendAuxRecordConfiguration(bool disable)
		{
			if (RegistryConfiguration.AppConfigReadInt("System.Net.ServicePointManager.SchSendAuxRecord", 1) == 0)
			{
				return true;
			}
			if (RegistryConfiguration.GlobalConfigReadInt("SchSendAuxRecord", 1) == 0)
			{
				return true;
			}
			return disable;
		}

		private static bool LoadDisableSystemDefaultTlsVersionsConfiguration(bool disable)
		{
			int num = RegistryConfiguration.GlobalConfigReadInt("SystemDefaultTlsVersions", 0);
			disable = num != 1;
			if (!disable)
			{
				int num2 = RegistryConfiguration.AppConfigReadInt("System.Net.ServicePointManager.SystemDefaultTlsVersions", 1);
				disable = num2 != 1;
			}
			return disable;
		}

		private static SslProtocols LoadSecureProtocolConfiguration(SslProtocols defaultValue)
		{
			if (!s_disableSystemDefaultTlsVersions)
			{
				defaultValue = SslProtocols.None;
			}
			if (!s_disableStrongCrypto || !s_disableSystemDefaultTlsVersions)
			{
				string value = RegistryConfiguration.AppConfigReadString("System.Net.ServicePointManager.SecurityProtocol", null);
				try
				{
					SecurityProtocolType securityProtocolType = (SecurityProtocolType)Enum.Parse(typeof(SecurityProtocolType), value);
					ValidateSecurityProtocol(securityProtocolType);
					defaultValue = (SslProtocols)securityProtocolType;
					return defaultValue;
				}
				catch (ArgumentNullException)
				{
					return defaultValue;
				}
				catch (ArgumentException)
				{
					return defaultValue;
				}
				catch (NotSupportedException)
				{
					return defaultValue;
				}
				catch (OverflowException)
				{
					return defaultValue;
				}
			}
			return defaultValue;
		}

		private static bool LoadDisableCertificateEKUsConfiguration(bool disable)
		{
			if (RegistryConfiguration.AppConfigReadInt("System.Net.ServicePointManager.RequireCertificateEKUs", 1) == 0)
			{
				return true;
			}
			if (RegistryConfiguration.GlobalConfigReadInt("RequireCertificateEKUs", 1) == 0)
			{
				return true;
			}
			return disable;
		}

		private static bool LoadUseHttpPipeliningAndBufferPoolingConfiguration(bool useFeature)
		{
			int num = RegistryConfiguration.AppConfigReadInt("System.Net.ServicePointManager.UseHttpPipeliningAndBufferPooling", 0);
			if (num == 1)
			{
				return true;
			}
			num = RegistryConfiguration.GlobalConfigReadInt("UseHttpPipeliningAndBufferPooling", 0);
			if (num == 1)
			{
				return true;
			}
			return useFeature;
		}

		private static bool LoadUseStrictRfcInterimResponseHandlingConfiguration(bool useFeature)
		{
			if (RegistryConfiguration.AppConfigReadInt("System.Net.ServicePointManager.UseStrictRfcInterimResponseHandling", 1) == 0)
			{
				return false;
			}
			if (RegistryConfiguration.GlobalConfigReadInt("UseStrictRfcInterimResponseHandling", 1) == 0)
			{
				return false;
			}
			return useFeature;
		}

		private static bool LoadAllowDangerousUnicodeDecompositionsConfiguration(bool useFeature)
		{
			int num = RegistryConfiguration.AppConfigReadInt("System.Uri.AllowDangerousUnicodeDecompositions", 0);
			if (num == 1)
			{
				return true;
			}
			num = RegistryConfiguration.GlobalConfigReadInt("AllowDangerousUnicodeDecompositions", 0);
			if (num == 1)
			{
				return true;
			}
			return useFeature;
		}

		private static bool LoadUseStrictIPv6AddressParsingConfiguration(bool useFeature)
		{
			if (RegistryConfiguration.AppConfigReadInt("System.Uri.UseStrictIPv6AddressParsing", 1) == 0)
			{
				return false;
			}
			if (RegistryConfiguration.GlobalConfigReadInt("UseStrictIPv6AddressParsing", 1) == 0)
			{
				return false;
			}
			return useFeature;
		}

		private static bool LoadAllowAllUriEncodingExpansionConfiguration(bool useFeature)
		{
			int num = RegistryConfiguration.AppConfigReadInt("System.Uri.AllowAllUriEncodingExpansion", 0);
			if (num == 1)
			{
				return true;
			}
			num = RegistryConfiguration.GlobalConfigReadInt("AllowAllUriEncodingExpansion", 0);
			if (num == 1)
			{
				return true;
			}
			return useFeature;
		}
	}
}
