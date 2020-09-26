using System.Collections;
using System.Configuration;
using System.Threading;

namespace System.Net.Configuration
{
	internal sealed class WebRequestModulesSectionInternal
	{
		private static object classSyncObject;

		private ArrayList webRequestModules;

		internal static object ClassSyncObject
		{
			get
			{
				if (classSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref classSyncObject, value, null);
				}
				return classSyncObject;
			}
		}

		internal ArrayList WebRequestModules
		{
			get
			{
				ArrayList arrayList = webRequestModules;
				if (arrayList == null)
				{
					arrayList = new ArrayList(0);
				}
				return arrayList;
			}
		}

		internal WebRequestModulesSectionInternal(WebRequestModulesSection section)
		{
			if (section.WebRequestModules.Count <= 0)
			{
				return;
			}
			webRequestModules = new ArrayList(section.WebRequestModules.Count);
			foreach (WebRequestModuleElement webRequestModule in section.WebRequestModules)
			{
				try
				{
					webRequestModules.Add(new WebRequestPrefixElement(webRequestModule.Prefix, webRequestModule.Type));
				}
				catch (Exception ex)
				{
					if (NclUtilities.IsFatal(ex))
					{
						throw;
					}
					throw new ConfigurationErrorsException(SR.GetString("net_config_webrequestmodules"), ex);
				}
				catch
				{
					throw new ConfigurationErrorsException(ConfigurationStrings.WebRequestModulesSectionPath, new Exception(SR.GetString("net_nonClsCompliantException")));
				}
			}
		}

		internal static WebRequestModulesSectionInternal GetSection()
		{
			lock (ClassSyncObject)
			{
				WebRequestModulesSection webRequestModulesSection = System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.WebRequestModulesSectionPath) as WebRequestModulesSection;
				if (webRequestModulesSection == null)
				{
					return null;
				}
				return new WebRequestModulesSectionInternal(webRequestModulesSection);
			}
		}
	}
}
