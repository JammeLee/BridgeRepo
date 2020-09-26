using System.Collections;
using System.Configuration;

namespace System.Diagnostics
{
	internal class ListenerElement : TypedElement
	{
		private static readonly ConfigurationProperty _propFilter;

		private static readonly ConfigurationProperty _propName;

		private static readonly ConfigurationProperty _propOutputOpts;

		private ConfigurationProperty _propListenerTypeName;

		private bool _allowReferences;

		private Hashtable _attributes;

		internal bool _isAddedByDefault;

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

		[ConfigurationProperty("filter")]
		public FilterElement Filter => (FilterElement)base[_propFilter];

		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name
		{
			get
			{
				return (string)base[_propName];
			}
			set
			{
				base[_propName] = value;
			}
		}

		[ConfigurationProperty("traceOutputOptions", DefaultValue = TraceOptions.None)]
		public TraceOptions TraceOutputOptions
		{
			get
			{
				return (TraceOptions)base[_propOutputOpts];
			}
			set
			{
				base[_propOutputOpts] = value;
			}
		}

		[ConfigurationProperty("type")]
		public override string TypeName
		{
			get
			{
				return (string)base[_propListenerTypeName];
			}
			set
			{
				base[_propListenerTypeName] = value;
			}
		}

		static ListenerElement()
		{
			_propFilter = new ConfigurationProperty("filter", typeof(FilterElement), null, ConfigurationPropertyOptions.None);
			_propName = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			_propOutputOpts = new ConfigurationProperty("traceOutputOptions", typeof(TraceOptions), TraceOptions.None, ConfigurationPropertyOptions.None);
		}

		public ListenerElement(bool allowReferences)
			: base(typeof(TraceListener))
		{
			_allowReferences = allowReferences;
			ConfigurationPropertyOptions configurationPropertyOptions = ConfigurationPropertyOptions.None;
			if (!_allowReferences)
			{
				configurationPropertyOptions |= ConfigurationPropertyOptions.IsRequired;
			}
			_propListenerTypeName = new ConfigurationProperty("type", typeof(string), null, configurationPropertyOptions);
			_properties.Remove("type");
			_properties.Add(_propListenerTypeName);
			_properties.Add(_propFilter);
			_properties.Add(_propName);
			_properties.Add(_propOutputOpts);
		}

		public override bool Equals(object compareTo)
		{
			if (Name.Equals("Default") && TypeName.Equals(typeof(DefaultTraceListener).FullName))
			{
				ListenerElement listenerElement = compareTo as ListenerElement;
				if (listenerElement != null && listenerElement.Name.Equals("Default"))
				{
					return listenerElement.TypeName.Equals(typeof(DefaultTraceListener).FullName);
				}
				return false;
			}
			return base.Equals(compareTo);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public TraceListener GetRuntimeObject()
		{
			if (_runtimeObject != null)
			{
				return (TraceListener)_runtimeObject;
			}
			try
			{
				string typeName = TypeName;
				if (string.IsNullOrEmpty(typeName))
				{
					if (_attributes != null || base.ElementInformation.Properties[_propFilter.Name].ValueOrigin == PropertyValueOrigin.SetHere || TraceOutputOptions != 0 || !string.IsNullOrEmpty(base.InitData))
					{
						throw new ConfigurationErrorsException(SR.GetString("Reference_listener_cant_have_properties", Name));
					}
					if (DiagnosticsConfiguration.SharedListeners == null)
					{
						throw new ConfigurationErrorsException(SR.GetString("Reference_to_nonexistent_listener", Name));
					}
					ListenerElement listenerElement = DiagnosticsConfiguration.SharedListeners[Name];
					if (listenerElement == null)
					{
						throw new ConfigurationErrorsException(SR.GetString("Reference_to_nonexistent_listener", Name));
					}
					_runtimeObject = listenerElement.GetRuntimeObject();
					return (TraceListener)_runtimeObject;
				}
				TraceListener traceListener = (TraceListener)BaseGetRuntimeObject();
				traceListener.initializeData = base.InitData;
				traceListener.Name = Name;
				traceListener.SetAttributes(Attributes);
				traceListener.TraceOutputOptions = TraceOutputOptions;
				if (Filter != null && Filter.TypeName != null && Filter.TypeName.Length != 0)
				{
					traceListener.Filter = Filter.GetRuntimeObject();
				}
				_runtimeObject = traceListener;
				return traceListener;
			}
			catch (ArgumentException inner)
			{
				throw new ConfigurationErrorsException(SR.GetString("Could_not_create_listener", Name), inner);
			}
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
				_properties.Add(_propListenerTypeName);
				_properties.Add(_propFilter);
				_properties.Add(_propName);
				_properties.Add(_propOutputOpts);
			}
		}

		internal TraceListener RefreshRuntimeObject(TraceListener listener)
		{
			_runtimeObject = null;
			try
			{
				string typeName = TypeName;
				if (string.IsNullOrEmpty(typeName))
				{
					if (_attributes != null || base.ElementInformation.Properties[_propFilter.Name].ValueOrigin == PropertyValueOrigin.SetHere || TraceOutputOptions != 0 || !string.IsNullOrEmpty(base.InitData))
					{
						throw new ConfigurationErrorsException(SR.GetString("Reference_listener_cant_have_properties", Name));
					}
					if (DiagnosticsConfiguration.SharedListeners == null)
					{
						throw new ConfigurationErrorsException(SR.GetString("Reference_to_nonexistent_listener", Name));
					}
					ListenerElement listenerElement = DiagnosticsConfiguration.SharedListeners[Name];
					if (listenerElement == null)
					{
						throw new ConfigurationErrorsException(SR.GetString("Reference_to_nonexistent_listener", Name));
					}
					_runtimeObject = listenerElement.RefreshRuntimeObject(listener);
					return (TraceListener)_runtimeObject;
				}
				if (Type.GetType(typeName) != listener.GetType() || base.InitData != listener.initializeData)
				{
					return GetRuntimeObject();
				}
				listener.SetAttributes(Attributes);
				listener.TraceOutputOptions = TraceOutputOptions;
				if (base.ElementInformation.Properties[_propFilter.Name].ValueOrigin == PropertyValueOrigin.SetHere)
				{
					listener.Filter = Filter.RefreshRuntimeObject(listener.Filter);
				}
				else
				{
					listener.Filter = null;
				}
				_runtimeObject = listener;
				return listener;
			}
			catch (ArgumentException inner)
			{
				throw new ConfigurationErrorsException(SR.GetString("Could_not_create_listener", Name), inner);
			}
		}
	}
}
