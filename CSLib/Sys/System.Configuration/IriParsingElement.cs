namespace System.Configuration
{
	public sealed class IriParsingElement : ConfigurationElement
	{
		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty enabled = new ConfigurationProperty("enabled", typeof(bool), false, ConfigurationPropertyOptions.None);

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		[ConfigurationProperty("enabled", DefaultValue = false)]
		public bool Enabled
		{
			get
			{
				return (bool)base[enabled];
			}
			set
			{
				base[enabled] = value;
			}
		}

		public IriParsingElement()
		{
			properties.Add(enabled);
		}
	}
}
