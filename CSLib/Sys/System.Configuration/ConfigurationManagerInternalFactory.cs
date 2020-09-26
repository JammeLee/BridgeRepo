using System.Configuration.Internal;

namespace System.Configuration
{
	internal static class ConfigurationManagerInternalFactory
	{
		private const string ConfigurationManagerInternalTypeString = "System.Configuration.Internal.ConfigurationManagerInternal, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

		private static IConfigurationManagerInternal s_instance;

		internal static IConfigurationManagerInternal Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = (IConfigurationManagerInternal)TypeUtil.CreateInstanceWithReflectionPermission("System.Configuration.Internal.ConfigurationManagerInternal, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
				}
				return s_instance;
			}
		}
	}
}
