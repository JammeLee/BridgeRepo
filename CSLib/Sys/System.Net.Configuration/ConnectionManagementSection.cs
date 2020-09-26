using System.Configuration;

namespace System.Net.Configuration
{
	public sealed class ConnectionManagementSection : ConfigurationSection
	{
		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty connectionManagement = new ConfigurationProperty(null, typeof(ConnectionManagementElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

		[ConfigurationProperty("", IsDefaultCollection = true)]
		public ConnectionManagementElementCollection ConnectionManagement => (ConnectionManagementElementCollection)base[connectionManagement];

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		public ConnectionManagementSection()
		{
			properties.Add(connectionManagement);
		}
	}
}
