using System.Configuration;

namespace System.Net.Configuration
{
	public sealed class WebProxyScriptElement : ConfigurationElement
	{
		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty downloadTimeout = new ConfigurationProperty("downloadTimeout", typeof(TimeSpan), TimeSpan.FromMinutes(1.0), null, new TimeSpanValidator(new TimeSpan(0, 0, 0), TimeSpan.MaxValue, rangeIsExclusive: false), ConfigurationPropertyOptions.None);

		[ConfigurationProperty("downloadTimeout", DefaultValue = "00:01:00")]
		public TimeSpan DownloadTimeout
		{
			get
			{
				return (TimeSpan)base[downloadTimeout];
			}
			set
			{
				base[downloadTimeout] = value;
			}
		}

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		public WebProxyScriptElement()
		{
			properties.Add(downloadTimeout);
		}

		protected override void PostDeserialize()
		{
			if (!base.EvaluationContext.IsMachineLevel)
			{
				try
				{
					ExceptionHelper.WebPermissionUnrestricted.Demand();
				}
				catch (Exception inner)
				{
					throw new ConfigurationErrorsException(SR.GetString("net_config_element_permission", "webProxyScript"), inner);
				}
			}
		}
	}
}
