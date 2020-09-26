using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System
{
	[ComVisible(true)]
	public static class Environment
	{
		internal sealed class ResourceHelper
		{
			internal class GetResourceStringUserData
			{
				public ResourceHelper m_resourceHelper;

				public string m_key;

				public string m_retVal;

				public bool m_lockWasTaken;

				public GetResourceStringUserData(ResourceHelper resourceHelper, string key)
				{
					m_resourceHelper = resourceHelper;
					m_key = key;
				}
			}

			private ResourceManager SystemResMgr;

			private Stack currentlyLoading;

			internal bool resourceManagerInited;

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal string GetResourceString(string key)
			{
				if (key == null || key.Length == 0)
				{
					return "[Resource lookup failed - null or empty resource name]";
				}
				GetResourceStringUserData getResourceStringUserData = new GetResourceStringUserData(this, key);
				RuntimeHelpers.TryCode code = GetResourceStringCode;
				RuntimeHelpers.CleanupCode backoutCode = GetResourceStringBackoutCode;
				RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(code, backoutCode, getResourceStringUserData);
				return getResourceStringUserData.m_retVal;
			}

			private void GetResourceStringCode(object userDataIn)
			{
				GetResourceStringUserData getResourceStringUserData = (GetResourceStringUserData)userDataIn;
				ResourceHelper resourceHelper = getResourceStringUserData.m_resourceHelper;
				string key = getResourceStringUserData.m_key;
				Monitor.ReliableEnter(resourceHelper, ref getResourceStringUserData.m_lockWasTaken);
				if (resourceHelper.currentlyLoading != null && resourceHelper.currentlyLoading.Count > 0 && resourceHelper.currentlyLoading.Contains(key))
				{
					try
					{
						StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
						stackTrace.ToString(System.Diagnostics.StackTrace.TraceFormat.NoResourceLookup);
					}
					catch (StackOverflowException)
					{
					}
					catch (NullReferenceException)
					{
					}
					catch (OutOfMemoryException)
					{
					}
					getResourceStringUserData.m_retVal = "[Resource lookup failed - infinite recursion or critical failure detected.]";
					return;
				}
				if (resourceHelper.currentlyLoading == null)
				{
					resourceHelper.currentlyLoading = new Stack(4);
				}
				if (!resourceHelper.resourceManagerInited)
				{
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						RuntimeHelpers.RunClassConstructor(typeof(ResourceManager).TypeHandle);
						RuntimeHelpers.RunClassConstructor(typeof(ResourceReader).TypeHandle);
						RuntimeHelpers.RunClassConstructor(typeof(RuntimeResourceSet).TypeHandle);
						RuntimeHelpers.RunClassConstructor(typeof(BinaryReader).TypeHandle);
						resourceHelper.resourceManagerInited = true;
					}
				}
				resourceHelper.currentlyLoading.Push(key);
				if (resourceHelper.SystemResMgr == null)
				{
					resourceHelper.SystemResMgr = new ResourceManager("mscorlib", typeof(object).Assembly);
				}
				string @string = resourceHelper.SystemResMgr.GetString(key, null);
				resourceHelper.currentlyLoading.Pop();
				getResourceStringUserData.m_retVal = @string;
			}

			[PrePrepareMethod]
			private void GetResourceStringBackoutCode(object userDataIn, bool exceptionThrown)
			{
				GetResourceStringUserData getResourceStringUserData = (GetResourceStringUserData)userDataIn;
				ResourceHelper resourceHelper = getResourceStringUserData.m_resourceHelper;
				if (exceptionThrown && getResourceStringUserData.m_lockWasTaken)
				{
					resourceHelper.SystemResMgr = null;
					resourceHelper.currentlyLoading = null;
				}
				if (getResourceStringUserData.m_lockWasTaken)
				{
					Monitor.Exit(resourceHelper);
				}
			}
		}

		[Serializable]
		internal enum OSName
		{
			Invalid = 0,
			Unknown = 1,
			Win9x = 0x40,
			Win95 = 65,
			Win98 = 66,
			WinMe = 67,
			WinNT = 0x80,
			Nt4 = 129,
			Win2k = 130
		}

		[ComVisible(true)]
		public enum SpecialFolder
		{
			ApplicationData = 26,
			CommonApplicationData = 35,
			LocalApplicationData = 28,
			Cookies = 33,
			Desktop = 0,
			Favorites = 6,
			History = 34,
			InternetCache = 0x20,
			Programs = 2,
			MyComputer = 17,
			MyMusic = 13,
			MyPictures = 39,
			Recent = 8,
			SendTo = 9,
			StartMenu = 11,
			Startup = 7,
			System = 37,
			Templates = 21,
			DesktopDirectory = 0x10,
			Personal = 5,
			MyDocuments = 5,
			ProgramFiles = 38,
			CommonProgramFiles = 43
		}

		private const int MaximumLength = 32767;

		private const int MaxMachineNameLength = 256;

		private static ResourceHelper m_resHelper;

		private static bool s_IsW2k3;

		private static volatile bool s_CheckedOSW2k3;

		private static object s_InternalSyncObject;

		private static OperatingSystem m_os;

		private static OSName m_osname;

		private static IntPtr processWinStation;

		private static bool isUserNonInteractive;

		private static object InternalSyncObject
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		public static int TickCount => nativeGetTickCount();

		public static int ExitCode
		{
			get
			{
				return nativeGetExitCode();
			}
			set
			{
				nativeSetExitCode(value);
			}
		}

		public static string CommandLine
		{
			get
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Demand();
				return GetCommandLineNative();
			}
		}

		public static string CurrentDirectory
		{
			get
			{
				return Directory.GetCurrentDirectory();
			}
			set
			{
				Directory.SetCurrentDirectory(value);
			}
		}

		public static string SystemDirectory
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(260);
				if (Win32Native.GetSystemDirectory(stringBuilder, 260) == 0)
				{
					__Error.WinIOError();
				}
				string text = stringBuilder.ToString();
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, text).Demand();
				return text;
			}
		}

		internal static string InternalWindowsDirectory
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(260);
				if (Win32Native.GetWindowsDirectory(stringBuilder, 260) == 0)
				{
					__Error.WinIOError();
				}
				return stringBuilder.ToString();
			}
		}

		public static string MachineName
		{
			get
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, "COMPUTERNAME").Demand();
				StringBuilder stringBuilder = new StringBuilder(256);
				int bufferSize = 256;
				if (Win32Native.GetComputerName(stringBuilder, ref bufferSize) == 0)
				{
					throw new InvalidOperationException(GetResourceString("InvalidOperation_ComputerName"));
				}
				return stringBuilder.ToString();
			}
		}

		public static int ProcessorCount
		{
			get
			{
				Win32Native.SYSTEM_INFO lpSystemInfo = default(Win32Native.SYSTEM_INFO);
				Win32Native.GetSystemInfo(ref lpSystemInfo);
				return lpSystemInfo.dwNumberOfProcessors;
			}
		}

		public static string NewLine => "\r\n";

		public static Version Version => new Version("2.0.50727.9044");

		public static long WorkingSet
		{
			get
			{
				new EnvironmentPermission(PermissionState.Unrestricted).Demand();
				return nativeGetWorkingSet();
			}
		}

		public static OperatingSystem OSVersion
		{
			get
			{
				if (m_os == null)
				{
					Win32Native.OSVERSIONINFO oSVERSIONINFO = new Win32Native.OSVERSIONINFO();
					if (!Win32Native.GetVersionEx(oSVERSIONINFO))
					{
						throw new InvalidOperationException(GetResourceString("InvalidOperation_GetVersion"));
					}
					Win32Native.OSVERSIONINFOEX oSVERSIONINFOEX = new Win32Native.OSVERSIONINFOEX();
					if (oSVERSIONINFO.PlatformId != 1 && !Win32Native.GetVersionEx(oSVERSIONINFOEX))
					{
						throw new InvalidOperationException(GetResourceString("InvalidOperation_GetVersion"));
					}
					PlatformID platform = oSVERSIONINFO.PlatformId switch
					{
						2 => PlatformID.Win32NT, 
						1 => PlatformID.Win32Windows, 
						0 => PlatformID.Win32S, 
						3 => PlatformID.WinCE, 
						_ => throw new InvalidOperationException(GetResourceString("InvalidOperation_InvalidPlatformID")), 
					};
					Version version = new Version(oSVERSIONINFO.MajorVersion, oSVERSIONINFO.MinorVersion, oSVERSIONINFO.BuildNumber, (oSVERSIONINFOEX.ServicePackMajor << 16) | oSVERSIONINFOEX.ServicePackMinor);
					m_os = new OperatingSystem(platform, version, oSVERSIONINFO.CSDVersion);
				}
				return m_os;
			}
		}

		internal static bool IsW2k3
		{
			get
			{
				if (!s_CheckedOSW2k3)
				{
					OperatingSystem oSVersion = OSVersion;
					s_IsW2k3 = oSVersion.Platform == PlatformID.Win32NT && oSVersion.Version.Major == 5 && oSVersion.Version.Minor == 2;
					s_CheckedOSW2k3 = true;
				}
				return s_IsW2k3;
			}
		}

		internal static bool RunningOnWinNT => OSVersion.Platform == PlatformID.Win32NT;

		internal static OSName OSInfo
		{
			get
			{
				if (m_osname == OSName.Invalid)
				{
					lock (InternalSyncObject)
					{
						if (m_osname == OSName.Invalid)
						{
							Win32Native.OSVERSIONINFO oSVERSIONINFO = new Win32Native.OSVERSIONINFO();
							if (!Win32Native.GetVersionEx(oSVERSIONINFO))
							{
								throw new InvalidOperationException(GetResourceString("InvalidOperation_GetVersion"));
							}
							switch (oSVERSIONINFO.PlatformId)
							{
							case 2:
								switch (oSVERSIONINFO.MajorVersion)
								{
								case 5:
									m_osname = OSName.Win2k;
									break;
								case 4:
									m_osname = OSName.Nt4;
									break;
								default:
									m_osname = OSName.WinNT;
									break;
								}
								break;
							case 1:
								switch (oSVERSIONINFO.MajorVersion)
								{
								case 5:
									m_osname = OSName.WinMe;
									break;
								case 4:
									if (oSVERSIONINFO.MinorVersion == 0)
									{
										m_osname = OSName.Win95;
									}
									else
									{
										m_osname = OSName.Win98;
									}
									break;
								default:
									m_osname = OSName.Win9x;
									break;
								}
								break;
							default:
								m_osname = OSName.Unknown;
								break;
							}
						}
					}
				}
				return m_osname;
			}
		}

		public static string StackTrace
		{
			get
			{
				new EnvironmentPermission(PermissionState.Unrestricted).Demand();
				return GetStackTrace(null, needFileInfo: true);
			}
		}

		public static bool HasShutdownStarted => nativeHasShutdownStarted();

		public static string UserName
		{
			get
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, "UserName").Demand();
				StringBuilder stringBuilder = new StringBuilder(256);
				int nSize = stringBuilder.Capacity;
				Win32Native.GetUserName(stringBuilder, ref nSize);
				return stringBuilder.ToString();
			}
		}

		public static bool UserInteractive
		{
			get
			{
				if ((OSInfo & OSName.WinNT) == OSName.WinNT)
				{
					IntPtr processWindowStation = Win32Native.GetProcessWindowStation();
					if (processWindowStation != IntPtr.Zero && processWinStation != processWindowStation)
					{
						int lpnLengthNeeded = 0;
						Win32Native.USEROBJECTFLAGS uSEROBJECTFLAGS = new Win32Native.USEROBJECTFLAGS();
						if (Win32Native.GetUserObjectInformation(processWindowStation, 1, uSEROBJECTFLAGS, Marshal.SizeOf(uSEROBJECTFLAGS), ref lpnLengthNeeded) && (uSEROBJECTFLAGS.dwFlags & 1) == 0)
						{
							isUserNonInteractive = true;
						}
						processWinStation = processWindowStation;
					}
				}
				return !isUserNonInteractive;
			}
		}

		public static string UserDomainName
		{
			get
			{
				new EnvironmentPermission(EnvironmentPermissionAccess.Read, "UserDomain").Demand();
				byte[] array = new byte[1024];
				int sidLen = array.Length;
				StringBuilder stringBuilder = new StringBuilder(1024);
				int domainNameLen = stringBuilder.Capacity;
				if (OSVersion.Platform == PlatformID.Win32NT)
				{
					byte userNameEx = Win32Native.GetUserNameEx(2, stringBuilder, ref domainNameLen);
					if (userNameEx == 1)
					{
						string text = stringBuilder.ToString();
						int num = text.IndexOf('\\');
						if (num != -1)
						{
							return text.Substring(0, num);
						}
					}
					domainNameLen = stringBuilder.Capacity;
				}
				if (!Win32Native.LookupAccountName(null, UserName, array, ref sidLen, stringBuilder, ref domainNameLen, out var _))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error == 120)
					{
						throw new PlatformNotSupportedException(GetResourceString("PlatformNotSupported_Win9x"));
					}
					throw new InvalidOperationException(GetResourceString("InvalidOperation_UserDomainName"));
				}
				return stringBuilder.ToString();
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int nativeGetTickCount();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void ExitNative(int exitCode);

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Exit(int exitCode)
		{
			ExitNative(exitCode);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void nativeSetExitCode(int exitCode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int nativeGetExitCode();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern void FailFast(string message);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string GetCommandLineNative();

		public static string ExpandEnvironmentVariables(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				return name;
			}
			string[] array = name.Split('%');
			StringBuilder stringBuilder = new StringBuilder();
			int num = 100;
			StringBuilder stringBuilder2 = new StringBuilder(num);
			int num2;
			for (int i = 1; i < array.Length - 1; i++)
			{
				if (array[i].Length == 0)
				{
					continue;
				}
				stringBuilder2.Length = 0;
				string text = "%" + array[i] + "%";
				num2 = Win32Native.ExpandEnvironmentStrings(text, stringBuilder2, num);
				if (num2 == 0)
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}
				while (num2 > num)
				{
					num = (stringBuilder2.Capacity = num2);
					stringBuilder2.Length = 0;
					num2 = Win32Native.ExpandEnvironmentStrings(text, stringBuilder2, num);
					if (num2 == 0)
					{
						Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
					}
				}
				string a = stringBuilder2.ToString();
				if (a != text)
				{
					stringBuilder.Append(array[i]);
					stringBuilder.Append(';');
				}
			}
			new EnvironmentPermission(EnvironmentPermissionAccess.Read, stringBuilder.ToString()).Demand();
			stringBuilder2.Length = 0;
			num2 = Win32Native.ExpandEnvironmentStrings(name, stringBuilder2, num);
			if (num2 == 0)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
			while (num2 > num)
			{
				num = (stringBuilder2.Capacity = num2);
				stringBuilder2.Length = 0;
				num2 = Win32Native.ExpandEnvironmentStrings(name, stringBuilder2, num);
				if (num2 == 0)
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}
			}
			return stringBuilder2.ToString();
		}

		public static string[] GetCommandLineArgs()
		{
			new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Demand();
			return GetCommandLineArgsNative();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string[] GetCommandLineArgsNative();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nativeGetEnvironmentVariable(string variable);

		public static string GetEnvironmentVariable(string variable)
		{
			if (variable == null)
			{
				throw new ArgumentNullException("variable");
			}
			new EnvironmentPermission(EnvironmentPermissionAccess.Read, variable).Demand();
			StringBuilder stringBuilder = new StringBuilder(128);
			int environmentVariable = Win32Native.GetEnvironmentVariable(variable, stringBuilder, stringBuilder.Capacity);
			if (environmentVariable == 0 && Marshal.GetLastWin32Error() == 203)
			{
				return null;
			}
			while (environmentVariable > stringBuilder.Capacity)
			{
				stringBuilder.Capacity = environmentVariable;
				stringBuilder.Length = 0;
				environmentVariable = Win32Native.GetEnvironmentVariable(variable, stringBuilder, stringBuilder.Capacity);
			}
			return stringBuilder.ToString();
		}

		public static string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
		{
			if (target == EnvironmentVariableTarget.Process)
			{
				return GetEnvironmentVariable(variable);
			}
			if (variable == null)
			{
				throw new ArgumentNullException("variable");
			}
			if (IsWin9X())
			{
				throw new NotSupportedException(GetResourceString("PlatformNotSupported_Win9x"));
			}
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			switch (target)
			{
			case EnvironmentVariableTarget.Machine:
			{
				using RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\Session Manager\\Environment", writable: false);
				if (registryKey2 == null)
				{
					return null;
				}
				return registryKey2.GetValue(variable) as string;
			}
			case EnvironmentVariableTarget.User:
			{
				using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Environment", writable: false);
				if (registryKey == null)
				{
					return null;
				}
				return registryKey.GetValue(variable) as string;
			}
			default:
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, GetResourceString("Arg_EnumIllegalVal"), (int)target));
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern char[] nativeGetEnvironmentCharArray();

		public static IDictionary GetEnvironmentVariables()
		{
			char[] array = nativeGetEnvironmentCharArray();
			if (array == null)
			{
				throw new OutOfMemoryException();
			}
			Hashtable hashtable = new Hashtable(20);
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			for (int i = 0; i < array.Length; i++)
			{
				int num = i;
				for (; array[i] != '=' && array[i] != 0; i++)
				{
				}
				if (array[i] == '\0')
				{
					continue;
				}
				if (i - num == 0)
				{
					for (; array[i] != 0; i++)
					{
					}
					continue;
				}
				string text = new string(array, num, i - num);
				i++;
				int num2 = i;
				for (; array[i] != 0; i++)
				{
				}
				string text3 = (string)(hashtable[text] = new string(array, num2, i - num2));
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(';');
				}
				stringBuilder.Append(text);
			}
			new EnvironmentPermission(EnvironmentPermissionAccess.Read, stringBuilder.ToString()).Demand();
			return hashtable;
		}

		internal static IDictionary GetRegistryKeyNameValuePairs(RegistryKey registryKey)
		{
			Hashtable hashtable = new Hashtable(20);
			if (registryKey != null)
			{
				string[] valueNames = registryKey.GetValueNames();
				string[] array = valueNames;
				foreach (string text in array)
				{
					string value = registryKey.GetValue(text, "").ToString();
					hashtable.Add(text, value);
				}
			}
			return hashtable;
		}

		public static IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target)
		{
			if (target == EnvironmentVariableTarget.Process)
			{
				return GetEnvironmentVariables();
			}
			if (IsWin9X())
			{
				throw new NotSupportedException(GetResourceString("PlatformNotSupported_Win9x"));
			}
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			switch (target)
			{
			case EnvironmentVariableTarget.Machine:
			{
				using RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\Session Manager\\Environment", writable: false);
				return GetRegistryKeyNameValuePairs(registryKey2);
			}
			case EnvironmentVariableTarget.User:
			{
				using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Environment", writable: false);
				return GetRegistryKeyNameValuePairs(registryKey);
			}
			default:
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, GetResourceString("Arg_EnumIllegalVal"), (int)target));
			}
		}

		public static void SetEnvironmentVariable(string variable, string value)
		{
			CheckEnvironmentVariableName(variable);
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (string.IsNullOrEmpty(value) || value[0] == '\0')
			{
				value = null;
			}
			else if (value.Length >= 32767)
			{
				throw new ArgumentException(GetResourceString("Argument_LongEnvVarValue"));
			}
			if (!Win32Native.SetEnvironmentVariable(variable, value))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				switch (lastWin32Error)
				{
				case 203:
					break;
				case 206:
					throw new ArgumentException(GetResourceString("Argument_LongEnvVarValue"));
				default:
					throw new ArgumentException(Win32Native.GetMessage(lastWin32Error));
				}
			}
		}

		private static void CheckEnvironmentVariableName(string variable)
		{
			if (variable == null)
			{
				throw new ArgumentNullException("variable");
			}
			if (variable.Length == 0)
			{
				throw new ArgumentException(GetResourceString("Argument_StringZeroLength"), "variable");
			}
			if (variable[0] == '\0')
			{
				throw new ArgumentException(GetResourceString("Argument_StringFirstCharIsZero"), "variable");
			}
			if (variable.Length >= 32767)
			{
				throw new ArgumentException(GetResourceString("Argument_LongEnvVarValue"));
			}
			if (variable.IndexOf('=') != -1)
			{
				throw new ArgumentException(GetResourceString("Argument_IllegalEnvVarName"));
			}
		}

		public static void SetEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target)
		{
			if (target == EnvironmentVariableTarget.Process)
			{
				SetEnvironmentVariable(variable, value);
				return;
			}
			CheckEnvironmentVariableName(variable);
			if (variable.Length >= 255)
			{
				throw new ArgumentException(GetResourceString("Argument_LongEnvVarName"));
			}
			if (IsWin9X())
			{
				throw new NotSupportedException(GetResourceString("PlatformNotSupported_Win9x"));
			}
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (string.IsNullOrEmpty(value) || value[0] == '\0')
			{
				value = null;
			}
			switch (target)
			{
			case EnvironmentVariableTarget.Machine:
			{
				using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\Session Manager\\Environment", writable: true))
				{
					if (registryKey2 != null)
					{
						if (value == null)
						{
							registryKey2.DeleteValue(variable, throwOnMissingValue: false);
						}
						else
						{
							registryKey2.SetValue(variable, value);
						}
					}
				}
				break;
			}
			case EnvironmentVariableTarget.User:
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Environment", writable: true))
				{
					if (registryKey != null)
					{
						if (value == null)
						{
							registryKey.DeleteValue(variable, throwOnMissingValue: false);
						}
						else
						{
							registryKey.SetValue(variable, value);
						}
					}
				}
				break;
			}
			default:
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, GetResourceString("Arg_EnumIllegalVal"), (int)target));
			}
			IntPtr value2 = Win32Native.SendMessageTimeout(new IntPtr(65535), 26, IntPtr.Zero, "Environment", 0u, 1000u, IntPtr.Zero);
			_ = value2 == IntPtr.Zero;
		}

		public static string[] GetLogicalDrives()
		{
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			int logicalDrives = Win32Native.GetLogicalDrives();
			if (logicalDrives == 0)
			{
				__Error.WinIOError();
			}
			uint num = (uint)logicalDrives;
			int num2 = 0;
			while (num != 0)
			{
				if ((num & (true ? 1u : 0u)) != 0)
				{
					num2++;
				}
				num >>= 1;
			}
			string[] array = new string[num2];
			char[] array2 = new char[3]
			{
				'A',
				':',
				'\\'
			};
			num = (uint)logicalDrives;
			num2 = 0;
			while (num != 0)
			{
				if ((num & (true ? 1u : 0u)) != 0)
				{
					array[num2++] = new string(array2);
				}
				num >>= 1;
				array2[0] += '\u0001';
			}
			return array;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern long nativeGetWorkingSet();

		internal static bool IsWin9X()
		{
			return OSVersion.Platform == PlatformID.Win32Windows;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool nativeIsWin9x();

		internal static string GetStackTrace(Exception e, bool needFileInfo)
		{
			StackTrace stackTrace = ((e != null) ? new StackTrace(e, needFileInfo) : new StackTrace(needFileInfo));
			return stackTrace.ToString(System.Diagnostics.StackTrace.TraceFormat.Normal);
		}

		private static void InitResourceHelper()
		{
			bool flag = false;
			bool flag2 = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					Thread.BeginCriticalRegion();
					flag = true;
					Monitor.Enter(InternalSyncObject);
					flag2 = true;
				}
				if (m_resHelper == null)
				{
					ResourceHelper resHelper = new ResourceHelper();
					Thread.MemoryBarrier();
					m_resHelper = resHelper;
				}
			}
			finally
			{
				if (flag2)
				{
					Monitor.Exit(InternalSyncObject);
				}
				if (flag)
				{
					Thread.EndCriticalRegion();
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetResourceFromDefault(string key);

		internal static string GetResourceStringLocal(string key)
		{
			if (m_resHelper == null)
			{
				InitResourceHelper();
			}
			return m_resHelper.GetResourceString(key);
		}

		internal static string GetResourceString(string key)
		{
			return GetResourceFromDefault(key);
		}

		internal static string GetResourceString(string key, params object[] values)
		{
			string resourceFromDefault = GetResourceFromDefault(key);
			return string.Format(CultureInfo.CurrentCulture, resourceFromDefault, values);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool nativeHasShutdownStarted();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool GetCompatibilityFlag(CompatibilityFlag flag);

		public static string GetFolderPath(SpecialFolder folder)
		{
			if (!Enum.IsDefined(typeof(SpecialFolder), folder))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, GetResourceString("Arg_EnumIllegalVal"), (int)folder));
			}
			StringBuilder stringBuilder = new StringBuilder(260);
			Win32Native.SHGetFolderPath(IntPtr.Zero, (int)folder, IntPtr.Zero, 0, stringBuilder);
			string text = stringBuilder.ToString();
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, text).Demand();
			return text;
		}
	}
}
