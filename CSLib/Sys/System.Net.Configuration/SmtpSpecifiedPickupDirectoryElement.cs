using System.Configuration;

namespace System.Net.Configuration
{
	public sealed class SmtpSpecifiedPickupDirectoryElement : ConfigurationElement
	{
		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty pickupDirectoryLocation = new ConfigurationProperty("pickupDirectoryLocation", typeof(string), null, ConfigurationPropertyOptions.None);

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		[ConfigurationProperty("pickupDirectoryLocation")]
		public string PickupDirectoryLocation
		{
			get
			{
				return (string)base[pickupDirectoryLocation];
			}
			set
			{
				base[pickupDirectoryLocation] = value;
			}
		}

		public SmtpSpecifiedPickupDirectoryElement()
		{
			properties.Add(pickupDirectoryLocation);
		}
	}
}
