using System.Configuration;

namespace System.Net.Configuration
{
	public sealed class SocketElement : ConfigurationElement
	{
		private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationProperty alwaysUseCompletionPortsForConnect = new ConfigurationProperty("alwaysUseCompletionPortsForConnect", typeof(bool), false, ConfigurationPropertyOptions.None);

		private readonly ConfigurationProperty alwaysUseCompletionPortsForAccept = new ConfigurationProperty("alwaysUseCompletionPortsForAccept", typeof(bool), false, ConfigurationPropertyOptions.None);

		[ConfigurationProperty("alwaysUseCompletionPortsForAccept", DefaultValue = false)]
		public bool AlwaysUseCompletionPortsForAccept
		{
			get
			{
				return (bool)base[alwaysUseCompletionPortsForAccept];
			}
			set
			{
				base[alwaysUseCompletionPortsForAccept] = value;
			}
		}

		[ConfigurationProperty("alwaysUseCompletionPortsForConnect", DefaultValue = false)]
		public bool AlwaysUseCompletionPortsForConnect
		{
			get
			{
				return (bool)base[alwaysUseCompletionPortsForConnect];
			}
			set
			{
				base[alwaysUseCompletionPortsForConnect] = value;
			}
		}

		protected internal override ConfigurationPropertyCollection Properties
		{
			protected get
			{
				return properties;
			}
		}

		public SocketElement()
		{
			properties.Add(alwaysUseCompletionPortsForAccept);
			properties.Add(alwaysUseCompletionPortsForConnect);
		}

		protected override void PostDeserialize()
		{
			if (!base.EvaluationContext.IsMachineLevel)
			{
				try
				{
					ExceptionHelper.UnrestrictedSocketPermission.Demand();
				}
				catch (Exception inner)
				{
					throw new ConfigurationErrorsException(SR.GetString("net_config_element_permission", "socket"), inner);
				}
			}
		}
	}
}
