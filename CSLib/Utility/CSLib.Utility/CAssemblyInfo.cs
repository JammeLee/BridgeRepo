using System;
using System.Reflection;

namespace CSLib.Utility
{
	public class CAssemblyInfo : CSingleton<CAssemblyInfo>
	{
		public static string GetFileVersion(Assembly assembly)
		{
			//Discarded unreachable code: IL_0019
			object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				if (true)
				{
				}
				return "";
			}
			return ((AssemblyFileVersionAttribute)customAttributes[0]).Version;
		}

		public static string GetBuildTime(Assembly assembly)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			Version version = assembly.GetName().Version;
			return new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2).ToString();
		}
	}
}
