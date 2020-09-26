using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Xml.Serialization;

namespace System.Diagnostics
{
	public abstract class Switch
	{
		private SwitchElementsCollection switchSettings;

		private string description;

		private string displayName;

		private int switchSetting;

		private bool initialized;

		private bool initializing;

		private string switchValueString = string.Empty;

		private StringDictionary attributes;

		private string defaultValue;

		private static List<WeakReference> switches = new List<WeakReference>();

		[XmlIgnore]
		public StringDictionary Attributes
		{
			get
			{
				Initialize();
				if (attributes == null)
				{
					attributes = new StringDictionary();
				}
				return attributes;
			}
		}

		public string DisplayName => displayName;

		public string Description
		{
			get
			{
				if (description != null)
				{
					return description;
				}
				return string.Empty;
			}
		}

		protected int SwitchSetting
		{
			get
			{
				if (!initialized)
				{
					if (!InitializeWithStatus())
					{
						return 0;
					}
					OnSwitchSettingChanged();
				}
				return switchSetting;
			}
			set
			{
				initialized = true;
				if (switchSetting != value)
				{
					switchSetting = value;
					OnSwitchSettingChanged();
				}
			}
		}

		protected string Value
		{
			get
			{
				Initialize();
				return switchValueString;
			}
			set
			{
				Initialize();
				switchValueString = value;
				try
				{
					OnValueChanged();
				}
				catch (ArgumentException inner)
				{
					throw new ConfigurationErrorsException(SR.GetString("BadConfigSwitchValue", DisplayName), inner);
				}
				catch (FormatException inner2)
				{
					throw new ConfigurationErrorsException(SR.GetString("BadConfigSwitchValue", DisplayName), inner2);
				}
				catch (OverflowException inner3)
				{
					throw new ConfigurationErrorsException(SR.GetString("BadConfigSwitchValue", DisplayName), inner3);
				}
			}
		}

		protected Switch(string displayName, string description)
			: this(displayName, description, "0")
		{
		}

		protected Switch(string displayName, string description, string defaultSwitchValue)
		{
			if (displayName == null)
			{
				displayName = string.Empty;
			}
			this.displayName = displayName;
			this.description = description;
			lock (switches)
			{
				switches.Add(new WeakReference(this));
			}
			defaultValue = defaultSwitchValue;
		}

		private void Initialize()
		{
			InitializeWithStatus();
		}

		private bool InitializeWithStatus()
		{
			if (!initialized)
			{
				if (initializing)
				{
					return false;
				}
				initializing = true;
				if (switchSettings == null && !InitializeConfigSettings())
				{
					return false;
				}
				if (switchSettings != null)
				{
					SwitchElement switchElement = switchSettings[displayName];
					if (switchElement != null)
					{
						string value = switchElement.Value;
						if (value != null)
						{
							Value = value;
						}
						else
						{
							Value = defaultValue;
						}
						try
						{
							TraceUtils.VerifyAttributes(switchElement.Attributes, GetSupportedAttributes(), this);
						}
						catch (ConfigurationException)
						{
							initialized = false;
							initializing = false;
							throw;
						}
						attributes = new StringDictionary();
						attributes.contents = switchElement.Attributes;
					}
					else
					{
						switchValueString = defaultValue;
						OnValueChanged();
					}
				}
				else
				{
					switchValueString = defaultValue;
					OnValueChanged();
				}
				initialized = true;
				initializing = false;
			}
			return true;
		}

		private bool InitializeConfigSettings()
		{
			if (switchSettings != null)
			{
				return true;
			}
			if (!DiagnosticsConfiguration.CanInitialize())
			{
				return false;
			}
			switchSettings = DiagnosticsConfiguration.SwitchSettings;
			return true;
		}

		protected internal virtual string[] GetSupportedAttributes()
		{
			return null;
		}

		protected virtual void OnSwitchSettingChanged()
		{
		}

		protected virtual void OnValueChanged()
		{
			SwitchSetting = int.Parse(Value, CultureInfo.InvariantCulture);
		}

		internal static void RefreshAll()
		{
			lock (switches)
			{
				for (int i = 0; i < switches.Count; i++)
				{
					((Switch)switches[i].Target)?.Refresh();
				}
			}
		}

		internal void Refresh()
		{
			initialized = false;
			switchSettings = null;
			Initialize();
		}
	}
}
