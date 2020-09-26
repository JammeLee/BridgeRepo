namespace System.Configuration
{
	public sealed class UriSection : ConfigurationSection
	{
		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty idn = new ConfigurationProperty("idn", typeof(IdnElement), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty iriParsing = new ConfigurationProperty("iriParsing", typeof(IriParsingElement), null, ConfigurationPropertyOptions.None);

		[ConfigurationProperty("idn")]
		public IdnElement Idn => (IdnElement)base[idn];

		[ConfigurationProperty("iriParsing")]
		public IriParsingElement IriParsing => (IriParsingElement)base[iriParsing];

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		public UriSection()
		{
			properties.Add(idn);
			properties.Add(iriParsing);
		}
	}
}
