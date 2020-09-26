using System.ComponentModel;
using System.Globalization;

namespace System.Configuration
{
	public sealed class IdnElement : ConfigurationElement
	{
		private class UriIdnScopeTypeConverter : TypeConverter
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
					case "all":
						return UriIdnScope.All;
					case "none":
						return UriIdnScope.None;
					case "allexceptintranet":
						return UriIdnScope.AllExceptIntranet;
					}
				}
				return base.ConvertFrom(context, culture, value);
			}
		}

		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty enabled = new ConfigurationProperty("enabled", typeof(UriIdnScope), UriIdnScope.None, new UriIdnScopeTypeConverter(), null, ConfigurationPropertyOptions.None);

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		[ConfigurationProperty("enabled", DefaultValue = UriIdnScope.None)]
		public UriIdnScope Enabled
		{
			get
			{
				return (UriIdnScope)base[enabled];
			}
			set
			{
				base[enabled] = value;
			}
		}

		public IdnElement()
		{
			properties.Add(enabled);
		}
	}
}
