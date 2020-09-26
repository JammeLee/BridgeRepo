using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace System.CodeDom.Compiler
{
	internal static class RedistVersionInfo
	{
		internal const string NameTag = "CompilerVersion";

		internal const string DefaultVersion = "v2.0";

		internal const string InPlaceVersion = "v2.0";

		internal const string RedistVersion = "v3.5";

		private const string dotNetFrameworkSdkInstallKeyValueV35 = "MSBuildToolsPath";

		private const string dotNetFrameworkRegistryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\MSBuild\\ToolsVersions\\3.5";

		public static string GetCompilerPath(IDictionary<string, string> provOptions, string compilerExecutable)
		{
			string text = Executor.GetRuntimeInstallDirectory();
			if (provOptions != null && provOptions.TryGetValue("CompilerVersion", out var value))
			{
				switch (value)
				{
				case "v3.5":
					text = GetOrcasPath();
					break;
				default:
					text = null;
					break;
				case "v2.0":
					break;
				}
			}
			if (text == null)
			{
				throw new InvalidOperationException(SR.GetString("CompilerNotFound", compilerExecutable));
			}
			return text;
		}

		private static string GetOrcasPath()
		{
			string text = null;
			string environmentVariable = Environment.GetEnvironmentVariable("COMPLUS_InstallRoot");
			string environmentVariable2 = Environment.GetEnvironmentVariable("COMPLUS_Version");
			if (!string.IsNullOrEmpty(environmentVariable) && !string.IsNullOrEmpty(environmentVariable2))
			{
				text = Path.Combine(environmentVariable, environmentVariable2);
				if (Directory.Exists(text))
				{
					return text;
				}
			}
			text = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\MSBuild\\ToolsVersions\\3.5", "MSBuildToolsPath", null) as string;
			if (text != null && Directory.Exists(text))
			{
				return text;
			}
			return null;
		}
	}
}
