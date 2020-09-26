using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Diagnostics
{
	internal static class ProcessManager
	{
		public static bool IsNt => Environment.OSVersion.Platform == PlatformID.Win32NT;

		public static bool IsOSOlderThanXP
		{
			get
			{
				if (Environment.OSVersion.Version.Major >= 5)
				{
					if (Environment.OSVersion.Version.Major == 5)
					{
						return Environment.OSVersion.Version.Minor == 0;
					}
					return false;
				}
				return true;
			}
		}

		static ProcessManager()
		{
			NativeMethods.LUID lpLuid = default(NativeMethods.LUID);
			if (!NativeMethods.LookupPrivilegeValue(null, "SeDebugPrivilege", out lpLuid))
			{
				return;
			}
			IntPtr TokenHandle = IntPtr.Zero;
			try
			{
				if (NativeMethods.OpenProcessToken(new HandleRef(null, NativeMethods.GetCurrentProcess()), 32, out TokenHandle))
				{
					NativeMethods.TokenPrivileges newState = new NativeMethods.TokenPrivileges
					{
						PrivilegeCount = 1,
						Luid = lpLuid,
						Attributes = 2
					};
					NativeMethods.AdjustTokenPrivileges(new HandleRef(null, TokenHandle), DisableAllPrivileges: false, newState, 0, IntPtr.Zero, IntPtr.Zero);
				}
			}
			finally
			{
				if (TokenHandle != IntPtr.Zero)
				{
					Microsoft.Win32.SafeNativeMethods.CloseHandle(new HandleRef(null, TokenHandle));
				}
			}
		}

		public static ProcessInfo[] GetProcessInfos(string machineName)
		{
			bool flag = IsRemoteMachine(machineName);
			if (IsNt)
			{
				if (!flag && Environment.OSVersion.Version.Major >= 5)
				{
					return NtProcessInfoHelper.GetProcessInfos();
				}
				return NtProcessManager.GetProcessInfos(machineName, flag);
			}
			if (flag)
			{
				throw new PlatformNotSupportedException(SR.GetString("WinNTRequiredForRemote"));
			}
			return WinProcessManager.GetProcessInfos();
		}

		public static int[] GetProcessIds()
		{
			if (IsNt)
			{
				return NtProcessManager.GetProcessIds();
			}
			return WinProcessManager.GetProcessIds();
		}

		public static int[] GetProcessIds(string machineName)
		{
			if (IsRemoteMachine(machineName))
			{
				if (IsNt)
				{
					return NtProcessManager.GetProcessIds(machineName, isRemoteMachine: true);
				}
				throw new PlatformNotSupportedException(SR.GetString("WinNTRequiredForRemote"));
			}
			return GetProcessIds();
		}

		public static bool IsProcessRunning(int processId, string machineName)
		{
			return IsProcessRunning(processId, GetProcessIds(machineName));
		}

		public static bool IsProcessRunning(int processId)
		{
			return IsProcessRunning(processId, GetProcessIds());
		}

		private static bool IsProcessRunning(int processId, int[] processIds)
		{
			for (int i = 0; i < processIds.Length; i++)
			{
				if (processIds[i] == processId)
				{
					return true;
				}
			}
			return false;
		}

		public static int GetProcessIdFromHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle)
		{
			if (IsNt)
			{
				return NtProcessManager.GetProcessIdFromHandle(processHandle);
			}
			throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
		}

		public static IntPtr GetMainWindowHandle(ProcessInfo processInfo)
		{
			MainWindowFinder mainWindowFinder = new MainWindowFinder();
			return mainWindowFinder.FindMainWindow(processInfo.processId);
		}

		public static ModuleInfo[] GetModuleInfos(int processId)
		{
			if (IsNt)
			{
				return NtProcessManager.GetModuleInfos(processId);
			}
			return WinProcessManager.GetModuleInfos(processId);
		}

		public static Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcess(int processId, int access, bool throwIfExited)
		{
			Microsoft.Win32.SafeHandles.SafeProcessHandle safeProcessHandle = NativeMethods.OpenProcess(access, inherit: false, processId);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (!safeProcessHandle.IsInvalid)
			{
				return safeProcessHandle;
			}
			if (processId == 0)
			{
				throw new Win32Exception(5);
			}
			if (!IsProcessRunning(processId))
			{
				if (throwIfExited)
				{
					throw new InvalidOperationException(SR.GetString("ProcessHasExited", processId.ToString(CultureInfo.CurrentCulture)));
				}
				return Microsoft.Win32.SafeHandles.SafeProcessHandle.InvalidHandle;
			}
			throw new Win32Exception(lastWin32Error);
		}

		public static Microsoft.Win32.SafeHandles.SafeThreadHandle OpenThread(int threadId, int access)
		{
			try
			{
				Microsoft.Win32.SafeHandles.SafeThreadHandle safeThreadHandle = NativeMethods.OpenThread(access, inherit: false, threadId);
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (safeThreadHandle.IsInvalid)
				{
					if (lastWin32Error == 87)
					{
						throw new InvalidOperationException(SR.GetString("ThreadExited", threadId.ToString(CultureInfo.CurrentCulture)));
					}
					throw new Win32Exception(lastWin32Error);
				}
				return safeThreadHandle;
			}
			catch (EntryPointNotFoundException inner)
			{
				throw new PlatformNotSupportedException(SR.GetString("Win2000Required"), inner);
			}
		}

		public static bool IsRemoteMachine(string machineName)
		{
			if (machineName == null)
			{
				throw new ArgumentNullException("machineName");
			}
			if (machineName.Length == 0)
			{
				throw new ArgumentException(SR.GetString("InvalidParameter", "machineName", machineName));
			}
			string text = ((!machineName.StartsWith("\\", StringComparison.Ordinal)) ? machineName : machineName.Substring(2));
			if (text.Equals("."))
			{
				return false;
			}
			StringBuilder stringBuilder = new StringBuilder(256);
			Microsoft.Win32.SafeNativeMethods.GetComputerName(stringBuilder, new int[1]
			{
				stringBuilder.Capacity
			});
			string strA = stringBuilder.ToString();
			if (string.Compare(strA, text, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return false;
			}
			return true;
		}
	}
}
