using System.Collections;
using System.Configuration;

namespace System.Diagnostics
{
	internal class SwitchElement : ConfigurationElement
	{
		private static readonly ConfigurationPropertyCollection _properties;

		private static readonly ConfigurationProperty _propName;

		private static readonly ConfigurationProperty _propValue;

		private Hashtable _attributes;

		public Hashtable Attributes
		{
			get
			{
				if (_attributes == null)
				{
					_attributes = new Hashtable(StringComparer.OrdinalIgnoreCase);
				}
				return _attributes;
			}
		}

		[ConfigurationProperty("name", DefaultValue = "", IsRequired = true, IsKey = true)]
		public string Name => (string)base[_propName];

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return _properties;
			}
		}

		[ConfigurationProperty("value", IsRequired = true)]
		public string Value => (string)base[_propValue];

		static SwitchElement()
		{
			_propName = new ConfigurationProperty("name", typeof(string), "", ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			_propValue = new ConfigurationProperty("value", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
			_properties = new ConfigurationPropertyCollection();
			_properties.Add(_propName);
			_properties.Add(_propValue);
		}

		protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
		{
			ConfigurationProperty configurationProperty = new ConfigurationProperty(name, typeof(string), value);
			_properties.Add(configurationProperty);
			base[configurationProperty] = value;
			Attributes.Add(name, value);
			return true;
		}

		internal void ResetProperties()
		{
			if (_attributes != null)
			{
				_attributes.Clear();
				_properties.Clear();
				_properties.Add(_propName);
				_properties.Add(_propValue);
			}
		}
	}
}
