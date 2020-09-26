using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Net
{
	internal static class RegistryConfiguration
	{
		private const string netFrameworkPath = "SOFTWARE\\Microsoft\\.NETFramework";

		private const string netFrameworkVersionedPath = "SOFTWARE\\Microsoft\\.NETFramework\\v{0}";

		private const string netFrameworkFullPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework";

		[RegistryPermission(SecurityAction.Assert, Read = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework")]
		public static int GlobalConfigReadInt(string configVariable, int defaultValue)
		{
			object obj = ReadConfig(GetNetFrameworkVersionedPath(), configVariable, RegistryValueKind.DWord);
			if (obj != null)
			{
				return (int)obj;
			}
			return defaultValue;
		}

		[RegistryPermission(SecurityAction.Assert, Read = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework")]
		public static string GlobalConfigReadString(string configVariable, string defaultValue)
		{
			object obj = ReadConfig(GetNetFrameworkVersionedPath(), configVariable, RegistryValueKind.String);
			if (obj != null)
			{
				return (string)obj;
			}
			return defaultValue;
		}

		[RegistryPermission(SecurityAction.Assert, Read = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework")]
		public static int AppConfigReadInt(string configVariable, int defaultValue)
		{
			object obj = ReadConfig(GetAppConfigPath(configVariable), GetAppConfigValueName(), RegistryValueKind.DWord);
			if (obj != null)
			{
				return (int)obj;
			}
			return defaultValue;
		}

		[RegistryPermission(SecurityAction.Assert, Read = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework")]
		public static string AppConfigReadString(string configVariable, string defaultValue)
		{
			object obj = ReadConfig(GetAppConfigPath(configVariable), GetAppConfigValueName(), RegistryValueKind.String);
			if (obj != null)
			{
				return (string)obj;
			}
			return defaultValue;
		}

		private static object ReadConfig(string path, string valueName, RegistryValueKind kind)
		{
			object result = null;
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(path);
				if (registryKey == null)
				{
					return result;
				}
				try
				{
					object value = registryKey.GetValue(valueName, null);
					if (value != null)
					{
						if (registryKey.GetValueKind(valueName) == kind)
						{
							result = value;
							return result;
						}
						return result;
					}
					return result;
				}
				catch (UnauthorizedAccessException)
				{
					return result;
				}
				catch (IOException)
				{
					return result;
				}
			}
			catch (SecurityException)
			{
				return result;
			}
			catch (ObjectDisposedException)
			{
				return result;
			}
		}

		private static string GetNetFrameworkVersionedPath()
		{
			return string.Format(CultureInfo.InvariantCulture, "SOFTWARE\\Microsoft\\.NETFramework\\v{0}", Environment.Version.ToString(3));
		}

		private static string GetAppConfigPath(string valueName)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", GetNetFrameworkVersionedPath(), valueName);
		}

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		private static string GetAppConfigValueName()
		{
			string text = "Unknown";
			Process currentProcess = Process.GetCurrentProcess();
			try
			{
				ProcessModule mainModule = currentProcess.MainModule;
				text = mainModule.FileName;
			}
			catch (NotSupportedException)
			{
			}
			catch (Win32Exception)
			{
			}
			catch (InvalidOperationException)
			{
			}
			try
			{
				text = Path.GetFullPath(text);
				return text;
			}
			catch (ArgumentException)
			{
				return text;
			}
			catch (SecurityException)
			{
				return text;
			}
			catch (NotSupportedException)
			{
				return text;
			}
			catch (PathTooLongException)
			{
				return text;
			}
		}
	}
}
