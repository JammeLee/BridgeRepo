using System.Security;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32;

namespace System.Net
{
	internal static class ComNetOS
	{
		private const string OSInstallTypeRegKey = "Software\\Microsoft\\Windows NT\\CurrentVersion";

		private const string OSInstallTypeRegKeyPath = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion";

		private const string OSInstallTypeRegName = "InstallationType";

		private const string InstallTypeStringClient = "Client";

		private const string InstallTypeStringServer = "Server";

		private const string InstallTypeStringServerCore = "Server Core";

		private const string InstallTypeStringEmbedded = "Embedded";

		internal static readonly bool IsWin9x;

		internal static readonly bool IsWinNt;

		internal static readonly bool IsWin2K;

		internal static readonly bool IsPostWin2K;

		internal static readonly bool IsAspNetServer;

		internal static readonly bool IsWinHttp51;

		internal static readonly bool IsWin2k3;

		internal static readonly bool IsXpSp2;

		internal static readonly bool IsWin2k3Sp1;

		internal static readonly bool IsVista;

		internal static readonly bool IsWin7;

		internal static readonly WindowsInstallationType InstallationType;

		[EnvironmentPermission(SecurityAction.Assert, Unrestricted = true)]
		static ComNetOS()
		{
			OperatingSystem oSVersion = Environment.OSVersion;
			if (oSVersion.Platform == PlatformID.Win32Windows)
			{
				IsWin9x = true;
				return;
			}
			try
			{
				IsAspNetServer = Thread.GetDomain().GetData(".appDomain") != null;
			}
			catch
			{
			}
			IsWinNt = true;
			IsWin2K = true;
			if (oSVersion.Version.Major == 5 && oSVersion.Version.Minor == 0)
			{
				IsWinHttp51 = oSVersion.Version.MajorRevision >= 3;
				return;
			}
			IsPostWin2K = true;
			if ((oSVersion.Version.Major == 5 && oSVersion.Version.Minor == 1 && oSVersion.Version.MajorRevision >= 2) || oSVersion.Version.Major >= 6)
			{
				IsXpSp2 = true;
			}
			if (oSVersion.Version.Major == 5 && oSVersion.Version.Minor == 1)
			{
				IsWinHttp51 = oSVersion.Version.MajorRevision >= 1;
				return;
			}
			IsWinHttp51 = true;
			IsWin2k3 = true;
			if ((oSVersion.Version.Major == 5 && oSVersion.Version.Minor == 2 && oSVersion.Version.MajorRevision >= 1) || oSVersion.Version.Major >= 6)
			{
				IsWin2k3Sp1 = true;
			}
			if (oSVersion.Version.Major >= 6)
			{
				IsVista = true;
			}
			if (oSVersion.Version.Major >= 7 || (oSVersion.Version.Major == 6 && oSVersion.Version.Minor >= 1))
			{
				IsWin7 = true;
			}
			InstallationType = GetWindowsInstallType();
		}

		[RegistryPermission(SecurityAction.Assert, Read = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion")]
		private static WindowsInstallationType GetWindowsInstallType()
		{
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
				string text = registryKey.GetValue("InstallationType") as string;
				if (string.IsNullOrEmpty(text))
				{
					return WindowsInstallationType.Unknown;
				}
				if (string.Compare(text, "Client", StringComparison.OrdinalIgnoreCase) == 0)
				{
					return WindowsInstallationType.Client;
				}
				if (string.Compare(text, "Server", StringComparison.OrdinalIgnoreCase) == 0)
				{
					return WindowsInstallationType.Server;
				}
				if (string.Compare(text, "Server Core", StringComparison.OrdinalIgnoreCase) == 0)
				{
					return WindowsInstallationType.ServerCore;
				}
				if (string.Compare(text, "Embedded", StringComparison.OrdinalIgnoreCase) == 0)
				{
					return WindowsInstallationType.Embedded;
				}
				return WindowsInstallationType.Unknown;
			}
			catch (UnauthorizedAccessException)
			{
				return WindowsInstallationType.Unknown;
			}
			catch (SecurityException)
			{
				return WindowsInstallationType.Unknown;
			}
		}
	}
}
