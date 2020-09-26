using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Diagnostics
{
	internal static class SharedUtils
	{
		internal const int UnknownEnvironment = 0;

		internal const int W2kEnvironment = 1;

		internal const int NtEnvironment = 2;

		internal const int NonNtEnvironment = 3;

		private static int environment;

		private static object s_InternalSyncObject;

		private static object InternalSyncObject
		{
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

		internal static int CurrentEnvironment
		{
			get
			{
				if (environment == 0)
				{
					lock (InternalSyncObject)
					{
						if (environment == 0)
						{
							if (Environment.OSVersion.Platform == PlatformID.Win32NT)
							{
								if (Environment.OSVersion.Version.Major >= 5)
								{
									environment = 1;
								}
								else
								{
									environment = 2;
								}
							}
							else
							{
								environment = 3;
							}
						}
					}
				}
				return environment;
			}
		}

		internal static Win32Exception CreateSafeWin32Exception()
		{
			return CreateSafeWin32Exception(0);
		}

		internal static Win32Exception CreateSafeWin32Exception(int error)
		{
			Win32Exception ex = null;
			SecurityPermission securityPermission = new SecurityPermission(PermissionState.Unrestricted);
			securityPermission.Assert();
			try
			{
				if (error == 0)
				{
					return new Win32Exception();
				}
				return new Win32Exception(error);
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
		}

		internal static void CheckEnvironment()
		{
			if (CurrentEnvironment == 3)
			{
				throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
			}
		}

		internal static void CheckNtEnvironment()
		{
			if (CurrentEnvironment == 2)
			{
				throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
			}
		}

		internal static void EnterMutex(string name, ref Mutex mutex)
		{
			string text = null;
			text = ((CurrentEnvironment != 1) ? name : ("Global\\" + name));
			EnterMutexWithoutGlobal(text, ref mutex);
		}

		internal static void EnterMutexWithoutGlobal(string mutexName, ref Mutex mutex)
		{
			MutexSecurity mutexSecurity = new MutexSecurity();
			SecurityIdentifier identity = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
			mutexSecurity.AddAccessRule(new MutexAccessRule(identity, MutexRights.Modify | MutexRights.Synchronize, AccessControlType.Allow));
			bool createdNew;
			Mutex mutexIn = new Mutex(initiallyOwned: false, mutexName, out createdNew, mutexSecurity);
			SafeWaitForMutex(mutexIn, ref mutex);
		}

		private static bool SafeWaitForMutex(Mutex mutexIn, ref Mutex mutexOut)
		{
			while (true)
			{
				if (!SafeWaitForMutexOnce(mutexIn, ref mutexOut))
				{
					return false;
				}
				if (mutexOut != null)
				{
					break;
				}
				Thread.Sleep(0);
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool SafeWaitForMutexOnce(Mutex mutexIn, ref Mutex mutexOut)
		{
			RuntimeHelpers.PrepareConstrainedRegions();
			bool result;
			try
			{
			}
			finally
			{
				Thread.BeginCriticalRegion();
				Thread.BeginThreadAffinity();
				switch (WaitForSingleObjectDontCallThis(mutexIn.SafeWaitHandle, 500))
				{
				case 0:
				case 128:
					mutexOut = mutexIn;
					result = true;
					break;
				case 258:
					result = true;
					break;
				default:
					result = false;
					break;
				}
				if (mutexOut == null)
				{
					Thread.EndThreadAffinity();
					Thread.EndCriticalRegion();
				}
			}
			return result;
		}

		[DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", ExactSpelling = true, SetLastError = true)]
		[SuppressUnmanagedCodeSecurity]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern int WaitForSingleObjectDontCallThis(SafeWaitHandle handle, int timeout);

		internal static string GetLatestBuildDllDirectory(string machineName)
		{
			string result = "";
			RegistryKey registryKey = null;
			RegistryKey registryKey2 = null;
			RegistryPermission registryPermission = new RegistryPermission(PermissionState.Unrestricted);
			registryPermission.Assert();
			try
			{
				if (machineName.Equals("."))
				{
					return GetLocalBuildDirectory();
				}
				registryKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machineName);
				if (registryKey == null)
				{
					throw new InvalidOperationException(SR.GetString("RegKeyMissingShort", "HKEY_LOCAL_MACHINE", machineName));
				}
				registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework");
				if (registryKey2 != null)
				{
					string text = (string)registryKey2.GetValue("InstallRoot");
					if (text != null)
					{
						if (text != string.Empty)
						{
							string text2 = null;
							Version version = Environment.Version;
							text2 = "v" + version.ToString(2);
							string text3 = null;
							RegistryKey registryKey3 = registryKey2.OpenSubKey("policy\\" + text2);
							if (registryKey3 != null)
							{
								try
								{
									text3 = (string)registryKey3.GetValue("Version");
									if (text3 == null)
									{
										string[] valueNames = registryKey3.GetValueNames();
										for (int i = 0; i < valueNames.Length; i++)
										{
											string text4 = text2 + "." + valueNames[i].Replace('-', '.');
											if (string.Compare(text4, text3, StringComparison.Ordinal) > 0)
											{
												text3 = text4;
											}
										}
									}
								}
								finally
								{
									registryKey3.Close();
								}
								if (text3 != null)
								{
									if (text3 != string.Empty)
									{
										StringBuilder stringBuilder = new StringBuilder();
										stringBuilder.Append(text);
										if (!text.EndsWith("\\", StringComparison.Ordinal))
										{
											stringBuilder.Append("\\");
										}
										stringBuilder.Append(text3);
										stringBuilder.Append("\\");
										result = stringBuilder.ToString();
										return result;
									}
									return result;
								}
								return result;
							}
							return result;
						}
						return result;
					}
					return result;
				}
				return result;
			}
			catch
			{
				return result;
			}
			finally
			{
				registryKey2?.Close();
				registryKey?.Close();
				CodeAccessPermission.RevertAssert();
			}
		}

		private static string GetLocalBuildDirectory()
		{
			int num = 264;
			int num2 = 25;
			StringBuilder stringBuilder = new StringBuilder(num);
			StringBuilder stringBuilder2 = new StringBuilder(num2);
			uint dwDirectoryLength;
			uint dwlength;
			uint requestedRuntimeInfo = NativeMethods.GetRequestedRuntimeInfo(null, null, null, 0u, 65u, stringBuilder, num, out dwDirectoryLength, stringBuilder2, num2, out dwlength);
			while (true)
			{
				switch (requestedRuntimeInfo)
				{
				case 122u:
					break;
				default:
					throw CreateSafeWin32Exception();
				case 0u:
					stringBuilder.Append(stringBuilder2);
					return stringBuilder.ToString();
				}
				num *= 2;
				num2 *= 2;
				stringBuilder = new StringBuilder(num);
				stringBuilder2 = new StringBuilder(num2);
				requestedRuntimeInfo = NativeMethods.GetRequestedRuntimeInfo(null, null, null, 0u, 0u, stringBuilder, num, out dwDirectoryLength, stringBuilder2, num2, out dwlength);
			}
		}
	}
}
