using System.Configuration;
using System.Net.Cache;

namespace System.Net.Configuration
{
	public sealed class SettingsSection : ConfigurationSection
	{
		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty httpWebRequest = new ConfigurationProperty("httpWebRequest", typeof(HttpWebRequestElement), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty ipv6 = new ConfigurationProperty("ipv6", typeof(Ipv6Element), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty servicePointManager = new ConfigurationProperty("servicePointManager", typeof(ServicePointManagerElement), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty socket = new ConfigurationProperty("socket", typeof(SocketElement), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty webProxyScript = new ConfigurationProperty("webProxyScript", typeof(WebProxyScriptElement), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty performanceCounters = new ConfigurationProperty("performanceCounters", typeof(PerformanceCountersElement), null, ConfigurationPropertyOptions.None);

		[ConfigurationProperty("httpWebRequest")]
		public HttpWebRequestElement HttpWebRequest => (HttpWebRequestElement)base[httpWebRequest];

		[ConfigurationProperty("ipv6")]
		public Ipv6Element Ipv6 => (Ipv6Element)base[ipv6];

		[ConfigurationProperty("servicePointManager")]
		public ServicePointManagerElement ServicePointManager => (ServicePointManagerElement)base[servicePointManager];

		[ConfigurationProperty("socket")]
		public SocketElement Socket => (SocketElement)base[socket];

		[ConfigurationProperty("webProxyScript")]
		public WebProxyScriptElement WebProxyScript => (WebProxyScriptElement)base[webProxyScript];

		[ConfigurationProperty("performanceCounters")]
		public PerformanceCountersElement PerformanceCounters => (PerformanceCountersElement)base[performanceCounters];

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		internal static void EnsureConfigLoaded()
		{
			try
			{
				AuthenticationManager.EnsureConfigLoaded();
				_ = RequestCacheManager.IsCachingEnabled;
				_ = System.Net.ServicePointManager.DefaultConnectionLimit;
				_ = System.Net.ServicePointManager.Expect100Continue;
				_ = WebRequest.PrefixList;
				_ = WebRequest.InternalDefaultWebProxy;
				NetworkingPerfCounters.Initialize();
			}
			catch
			{
			}
		}

		public SettingsSection()
		{
			properties.Add(httpWebRequest);
			properties.Add(ipv6);
			properties.Add(servicePointManager);
			properties.Add(socket);
			properties.Add(webProxyScript);
			properties.Add(performanceCounters);
		}
	}
}
