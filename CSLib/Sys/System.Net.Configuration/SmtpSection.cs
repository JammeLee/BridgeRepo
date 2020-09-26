using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Net.Mail;

namespace System.Net.Configuration
{
	public sealed class SmtpSection : ConfigurationSection
	{
		private class SmtpDeliveryMethodTypeConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof(string))
				{
					return true;
				}
				return base.CanConvertFrom(context, sourceType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				string text = value as string;
				if (text != null)
				{
					switch (text.ToLower(CultureInfo.InvariantCulture))
					{
					case "network":
						return SmtpDeliveryMethod.Network;
					case "specifiedpickupdirectory":
						return SmtpDeliveryMethod.SpecifiedPickupDirectory;
					case "pickupdirectoryfromiis":
						return SmtpDeliveryMethod.PickupDirectoryFromIis;
					}
				}
				return base.ConvertFrom(context, culture, value);
			}
		}

		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty from = new ConfigurationProperty("from", typeof(string), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty network = new ConfigurationProperty("network", typeof(SmtpNetworkElement), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty specifiedPickupDirectory = new ConfigurationProperty("specifiedPickupDirectory", typeof(SmtpSpecifiedPickupDirectoryElement), null, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty deliveryMethod = new ConfigurationProperty("deliveryMethod", typeof(SmtpDeliveryMethod), SmtpDeliveryMethod.Network, new SmtpDeliveryMethodTypeConverter(), null, ConfigurationPropertyOptions.None);

		[ConfigurationProperty("deliveryMethod", DefaultValue = SmtpDeliveryMethod.Network)]
		public SmtpDeliveryMethod DeliveryMethod
		{
			get
			{
				return (SmtpDeliveryMethod)base[deliveryMethod];
			}
			set
			{
				base[deliveryMethod] = value;
			}
		}

		[ConfigurationProperty("from")]
		public string From
		{
			get
			{
				return (string)base[from];
			}
			set
			{
				base[from] = value;
			}
		}

		[ConfigurationProperty("network")]
		public SmtpNetworkElement Network => (SmtpNetworkElement)base[network];

		[ConfigurationProperty("specifiedPickupDirectory")]
		public SmtpSpecifiedPickupDirectoryElement SpecifiedPickupDirectory => (SmtpSpecifiedPickupDirectoryElement)base[specifiedPickupDirectory];

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		public SmtpSection()
		{
			properties.Add(deliveryMethod);
			properties.Add(from);
			properties.Add(network);
			properties.Add(specifiedPickupDirectory);
		}
	}
}
