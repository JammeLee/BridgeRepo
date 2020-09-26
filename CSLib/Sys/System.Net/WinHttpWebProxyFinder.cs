using System.Collections.Generic;
using System.Globalization;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Net
{
	internal sealed class WinHttpWebProxyFinder : BaseWebProxyFinder
	{
		private SafeInternetHandle session;

		private bool autoDetectFailed;

		public WinHttpWebProxyFinder(AutoWebProxyScriptEngine engine)
			: base(engine)
		{
			session = UnsafeNclNativeMethods.WinHttp.WinHttpOpen(null, UnsafeNclNativeMethods.WinHttp.AccessType.NoProxy, null, null, 0);
			if (session == null || session.IsInvalid)
			{
				int lastWin32Error = GetLastWin32Error();
				if (Logging.On)
				{
					Logging.PrintError(Logging.Web, string.Format(CultureInfo.InvariantCulture, "Can't open WinHttp session. Error code: {0}.", lastWin32Error));
				}
				return;
			}
			int downloadTimeout = SettingsSectionInternal.Section.DownloadTimeout;
			if (!UnsafeNclNativeMethods.WinHttp.WinHttpSetTimeouts(session, downloadTimeout, downloadTimeout, downloadTimeout, downloadTimeout))
			{
				int lastWin32Error2 = GetLastWin32Error();
				if (Logging.On)
				{
					Logging.PrintError(Logging.Web, string.Format(CultureInfo.InvariantCulture, "Can't specify proxy discovery timeout. Error code: {0}.", lastWin32Error2));
				}
			}
		}

		public override bool GetProxies(Uri destination, out IList<string> proxyList)
		{
			proxyList = null;
			if (session == null || session.IsInvalid)
			{
				return false;
			}
			if (base.State == AutoWebProxyState.UnrecognizedScheme)
			{
				return false;
			}
			string proxyListString = null;
			int num = 12180;
			if (base.Engine.AutomaticallyDetectSettings && !autoDetectFailed)
			{
				num = GetProxies(destination, null, out proxyListString);
				autoDetectFailed = IsErrorFatalForAutoDetect(num);
				if (num == 12006)
				{
					base.State = AutoWebProxyState.UnrecognizedScheme;
					return false;
				}
			}
			if (base.Engine.AutomaticConfigurationScript != null && IsRecoverableAutoProxyError(num))
			{
				num = GetProxies(destination, base.Engine.AutomaticConfigurationScript, out proxyListString);
			}
			base.State = GetStateFromErrorCode(num);
			if (base.State == AutoWebProxyState.Completed)
			{
				if (string.IsNullOrEmpty(proxyListString))
				{
					string[] array = (string[])(proxyList = new string[1]);
				}
				else
				{
					proxyListString = RemoveWhitespaces(proxyListString);
					proxyList = proxyListString.Split(';');
				}
				return true;
			}
			return false;
		}

		public override void Abort()
		{
		}

		public override void Reset()
		{
			base.Reset();
			autoDetectFailed = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && session != null && !session.IsInvalid)
			{
				session.Close();
			}
		}

		private int GetProxies(Uri destination, Uri scriptLocation, out string proxyListString)
		{
			int num = 0;
			proxyListString = null;
			UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions = default(UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS);
			autoProxyOptions.AutoLogonIfChallenged = false;
			if (scriptLocation == null)
			{
				autoProxyOptions.Flags = UnsafeNclNativeMethods.WinHttp.AutoProxyFlags.AutoDetect;
				autoProxyOptions.AutoConfigUrl = null;
				autoProxyOptions.AutoDetectFlags = UnsafeNclNativeMethods.WinHttp.AutoDetectType.Dhcp | UnsafeNclNativeMethods.WinHttp.AutoDetectType.DnsA;
			}
			else
			{
				autoProxyOptions.Flags = UnsafeNclNativeMethods.WinHttp.AutoProxyFlags.AutoProxyConfigUrl;
				autoProxyOptions.AutoConfigUrl = scriptLocation.ToString();
				autoProxyOptions.AutoDetectFlags = UnsafeNclNativeMethods.WinHttp.AutoDetectType.None;
			}
			if (!WinHttpGetProxyForUrl(destination.ToString(), ref autoProxyOptions, out proxyListString))
			{
				num = GetLastWin32Error();
				if (num == 12015 && base.Engine.Credentials != null)
				{
					autoProxyOptions.AutoLogonIfChallenged = true;
					if (!WinHttpGetProxyForUrl(destination.ToString(), ref autoProxyOptions, out proxyListString))
					{
						num = GetLastWin32Error();
					}
				}
				if (Logging.On)
				{
					Logging.PrintError(Logging.Web, string.Format(CultureInfo.InvariantCulture, "Can't retrieve proxy settings for Uri '{0}'. Error code: {1}.", destination, num));
				}
			}
			return num;
		}

		private bool WinHttpGetProxyForUrl(string destination, ref UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions, out string proxyListString)
		{
			proxyListString = null;
			bool flag = false;
			UnsafeNclNativeMethods.WinHttp.WINHTTP_PROXY_INFO proxyInfo = default(UnsafeNclNativeMethods.WinHttp.WINHTTP_PROXY_INFO);
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				flag = UnsafeNclNativeMethods.WinHttp.WinHttpGetProxyForUrl(session, destination, ref autoProxyOptions, out proxyInfo);
				if (flag)
				{
					proxyListString = Marshal.PtrToStringUni(proxyInfo.Proxy);
					return flag;
				}
				return flag;
			}
			finally
			{
				Marshal.FreeHGlobal(proxyInfo.Proxy);
				Marshal.FreeHGlobal(proxyInfo.ProxyBypass);
			}
		}

		private static int GetLastWin32Error()
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error == 8)
			{
				throw new OutOfMemoryException();
			}
			return lastWin32Error;
		}

		private static bool IsRecoverableAutoProxyError(int errorCode)
		{
			switch (errorCode)
			{
			case 12002:
			case 12006:
			case 12015:
			case 12017:
			case 12166:
			case 12167:
			case 12178:
			case 12180:
				return true;
			default:
				return false;
			}
		}

		private static AutoWebProxyState GetStateFromErrorCode(int errorCode)
		{
			if ((long)errorCode == 0)
			{
				return AutoWebProxyState.Completed;
			}
			switch (errorCode)
			{
			case 12180:
				return AutoWebProxyState.DiscoveryFailure;
			case 12167:
				return AutoWebProxyState.DownloadFailure;
			case 12006:
				return AutoWebProxyState.UnrecognizedScheme;
			case 12005:
			case 12166:
			case 12178:
				return AutoWebProxyState.Completed;
			default:
				return AutoWebProxyState.CompilationFailure;
			}
		}

		private static string RemoveWhitespaces(string value)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in value)
			{
				if (!char.IsWhiteSpace(c))
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		private static bool IsErrorFatalForAutoDetect(int errorCode)
		{
			switch (errorCode)
			{
			case 0:
			case 12005:
			case 12166:
			case 12178:
				return false;
			default:
				return true;
			}
		}
	}
}
