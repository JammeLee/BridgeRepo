using System.Configuration;
using System.Threading;

namespace System.Net.Configuration
{
	internal sealed class SettingsSectionInternal
	{
		private static object s_InternalSyncObject;

		private static SettingsSectionInternal s_settings;

		private bool alwaysUseCompletionPortsForAccept;

		private bool alwaysUseCompletionPortsForConnect;

		private bool checkCertificateName;

		private bool checkCertificateRevocationList;

		private int downloadTimeout;

		private int dnsRefreshTimeout;

		private bool enableDnsRoundRobin;

		private bool expect100Continue;

		private bool ipv6Enabled;

		private int maximumResponseHeadersLength;

		private int maximumErrorResponseLength;

		private int maximumUnauthorizedUploadLength;

		private bool useUnsafeHeaderParsing;

		private bool useNagleAlgorithm;

		private bool performanceCountersEnabled;

		internal static SettingsSectionInternal Section
		{
			get
			{
				if (s_settings == null)
				{
					lock (InternalSyncObject)
					{
						if (s_settings == null)
						{
							s_settings = new SettingsSectionInternal((SettingsSection)System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SettingsSectionPath));
						}
					}
				}
				return s_settings;
			}
		}

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

		internal bool AlwaysUseCompletionPortsForAccept => alwaysUseCompletionPortsForAccept;

		internal bool AlwaysUseCompletionPortsForConnect => alwaysUseCompletionPortsForConnect;

		internal bool CheckCertificateName => checkCertificateName;

		internal bool CheckCertificateRevocationList
		{
			get
			{
				return checkCertificateRevocationList;
			}
			set
			{
				checkCertificateRevocationList = value;
			}
		}

		internal int DnsRefreshTimeout
		{
			get
			{
				return dnsRefreshTimeout;
			}
			set
			{
				dnsRefreshTimeout = value;
			}
		}

		internal int DownloadTimeout => downloadTimeout;

		internal bool EnableDnsRoundRobin
		{
			get
			{
				return enableDnsRoundRobin;
			}
			set
			{
				enableDnsRoundRobin = value;
			}
		}

		internal bool Expect100Continue
		{
			get
			{
				return expect100Continue;
			}
			set
			{
				expect100Continue = value;
			}
		}

		internal bool Ipv6Enabled => ipv6Enabled;

		internal int MaximumResponseHeadersLength
		{
			get
			{
				return maximumResponseHeadersLength;
			}
			set
			{
				maximumResponseHeadersLength = value;
			}
		}

		internal int MaximumUnauthorizedUploadLength => maximumUnauthorizedUploadLength;

		internal int MaximumErrorResponseLength
		{
			get
			{
				return maximumErrorResponseLength;
			}
			set
			{
				maximumErrorResponseLength = value;
			}
		}

		internal bool UseUnsafeHeaderParsing => useUnsafeHeaderParsing;

		internal bool UseNagleAlgorithm
		{
			get
			{
				return useNagleAlgorithm;
			}
			set
			{
				useNagleAlgorithm = value;
			}
		}

		internal bool PerformanceCountersEnabled => performanceCountersEnabled;

		internal SettingsSectionInternal(SettingsSection section)
		{
			if (section == null)
			{
				section = new SettingsSection();
			}
			alwaysUseCompletionPortsForConnect = section.Socket.AlwaysUseCompletionPortsForConnect;
			alwaysUseCompletionPortsForAccept = section.Socket.AlwaysUseCompletionPortsForAccept;
			checkCertificateName = section.ServicePointManager.CheckCertificateName;
			CheckCertificateRevocationList = section.ServicePointManager.CheckCertificateRevocationList;
			DnsRefreshTimeout = section.ServicePointManager.DnsRefreshTimeout;
			ipv6Enabled = section.Ipv6.Enabled;
			EnableDnsRoundRobin = section.ServicePointManager.EnableDnsRoundRobin;
			Expect100Continue = section.ServicePointManager.Expect100Continue;
			maximumUnauthorizedUploadLength = section.HttpWebRequest.MaximumUnauthorizedUploadLength;
			maximumResponseHeadersLength = section.HttpWebRequest.MaximumResponseHeadersLength;
			maximumErrorResponseLength = section.HttpWebRequest.MaximumErrorResponseLength;
			useUnsafeHeaderParsing = section.HttpWebRequest.UseUnsafeHeaderParsing;
			UseNagleAlgorithm = section.ServicePointManager.UseNagleAlgorithm;
			TimeSpan t = section.WebProxyScript.DownloadTimeout;
			downloadTimeout = ((t == TimeSpan.MaxValue || t == TimeSpan.Zero) ? (-1) : ((int)t.TotalMilliseconds));
			performanceCountersEnabled = section.PerformanceCounters.Enabled;
			NetworkingPerfCounters.Initialize();
		}

		internal static SettingsSectionInternal GetSection()
		{
			return new SettingsSectionInternal((SettingsSection)System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SettingsSectionPath));
		}
	}
}
