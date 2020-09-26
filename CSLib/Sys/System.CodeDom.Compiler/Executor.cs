using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.CodeDom.Compiler
{
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public sealed class Executor
	{
		private const int ProcessTimeOut = 600000;

		private Executor()
		{
		}

		internal static string GetRuntimeInstallDirectory()
		{
			return RuntimeEnvironment.GetRuntimeDirectory();
		}

		private static FileStream CreateInheritedFile(string file)
		{
			return new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.Read | FileShare.Inheritable);
		}

		public static void ExecWait(string cmd, TempFileCollection tempFiles)
		{
			string outputName = null;
			string errorName = null;
			ExecWaitWithCapture(null, cmd, tempFiles, ref outputName, ref errorName);
		}

		public static int ExecWaitWithCapture(string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			return ExecWaitWithCapture(null, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
		}

		public static int ExecWaitWithCapture(string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			return ExecWaitWithCapture(null, cmd, currentDir, tempFiles, ref outputName, ref errorName, null);
		}

		public static int ExecWaitWithCapture(IntPtr userToken, string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			return ExecWaitWithCapture(new SafeUserTokenHandle(userToken, ownsHandle: false), cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
		}

		public static int ExecWaitWithCapture(IntPtr userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
		{
			return ExecWaitWithCapture(new SafeUserTokenHandle(userToken, ownsHandle: false), cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
		}

		internal static int ExecWaitWithCapture(SafeUserTokenHandle userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName, string trueCmdLine)
		{
			int num = 0;
			try
			{
				WindowsImpersonationContext impersonation = RevertImpersonation();
				try
				{
					return ExecWaitWithCaptureUnimpersonated(userToken, cmd, currentDir, tempFiles, ref outputName, ref errorName, trueCmdLine);
				}
				finally
				{
					ReImpersonate(impersonation);
				}
			}
			catch
			{
				throw;
			}
		}

		private unsafe static int ExecWaitWithCaptureUnimpersonated(SafeUserTokenHandle userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName, string trueCmdLine)
		{
			IntSecurity.UnmanagedCode.Demand();
			int num = 0;
			if (outputName == null || outputName.Length == 0)
			{
				outputName = tempFiles.AddExtension("out");
			}
			if (errorName == null || errorName.Length == 0)
			{
				errorName = tempFiles.AddExtension("err");
			}
			FileStream fileStream = CreateInheritedFile(outputName);
			FileStream fileStream2 = CreateInheritedFile(errorName);
			bool flag = false;
			Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION pROCESS_INFORMATION = new Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION();
			Microsoft.Win32.SafeHandles.SafeProcessHandle safeProcessHandle = new Microsoft.Win32.SafeHandles.SafeProcessHandle();
			Microsoft.Win32.SafeHandles.SafeThreadHandle safeThreadHandle = new Microsoft.Win32.SafeHandles.SafeThreadHandle();
			SafeUserTokenHandle hNewToken = null;
			try
			{
				StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
				streamWriter.Write(currentDir);
				streamWriter.Write("> ");
				streamWriter.WriteLine((trueCmdLine != null) ? trueCmdLine : cmd);
				streamWriter.WriteLine();
				streamWriter.WriteLine();
				streamWriter.Flush();
				NativeMethods.STARTUPINFO sTARTUPINFO = new NativeMethods.STARTUPINFO();
				sTARTUPINFO.cb = Marshal.SizeOf(sTARTUPINFO);
				sTARTUPINFO.dwFlags = 257;
				sTARTUPINFO.wShowWindow = 0;
				sTARTUPINFO.hStdOutput = fileStream.SafeFileHandle;
				sTARTUPINFO.hStdError = fileStream2.SafeFileHandle;
				sTARTUPINFO.hStdInput = new SafeFileHandle(Microsoft.Win32.UnsafeNativeMethods.GetStdHandle(-10), ownsHandle: false);
				StringDictionary stringDictionary = new StringDictionary();
				foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
				{
					stringDictionary.Add((string)environmentVariable.Key, (string)environmentVariable.Value);
				}
				stringDictionary["_ClrRestrictSecAttributes"] = "1";
				byte[] array = EnvironmentBlock.ToByteArray(stringDictionary, unicode: false);
				try
				{
					fixed (byte* value = array)
					{
						IntPtr intPtr = new IntPtr(value);
						if (userToken == null || userToken.IsInvalid)
						{
							RuntimeHelpers.PrepareConstrainedRegions();
							try
							{
							}
							finally
							{
								flag = NativeMethods.CreateProcess(null, new StringBuilder(cmd), null, null, bInheritHandles: true, 0, intPtr, currentDir, sTARTUPINFO, pROCESS_INFORMATION);
								if (pROCESS_INFORMATION.hProcess != (IntPtr)0 && pROCESS_INFORMATION.hProcess != NativeMethods.INVALID_HANDLE_VALUE)
								{
									safeProcessHandle.InitialSetHandle(pROCESS_INFORMATION.hProcess);
								}
								if (pROCESS_INFORMATION.hThread != (IntPtr)0 && pROCESS_INFORMATION.hThread != NativeMethods.INVALID_HANDLE_VALUE)
								{
									safeThreadHandle.InitialSetHandle(pROCESS_INFORMATION.hThread);
								}
							}
						}
						else
						{
							flag = SafeUserTokenHandle.DuplicateTokenEx(userToken, 983551, null, 2, 1, out hNewToken);
							if (flag)
							{
								RuntimeHelpers.PrepareConstrainedRegions();
								try
								{
								}
								finally
								{
									flag = NativeMethods.CreateProcessAsUser(hNewToken, null, cmd, null, null, bInheritHandles: true, 0, new HandleRef(null, intPtr), currentDir, sTARTUPINFO, pROCESS_INFORMATION);
									if (pROCESS_INFORMATION.hProcess != (IntPtr)0 && pROCESS_INFORMATION.hProcess != NativeMethods.INVALID_HANDLE_VALUE)
									{
										safeProcessHandle.InitialSetHandle(pROCESS_INFORMATION.hProcess);
									}
									if (pROCESS_INFORMATION.hThread != (IntPtr)0 && pROCESS_INFORMATION.hThread != NativeMethods.INVALID_HANDLE_VALUE)
									{
										safeThreadHandle.InitialSetHandle(pROCESS_INFORMATION.hThread);
									}
								}
							}
						}
					}
				}
				finally
				{
				}
			}
			finally
			{
				if (!flag && hNewToken != null && !hNewToken.IsInvalid)
				{
					hNewToken.Close();
					hNewToken = null;
				}
				fileStream.Close();
				fileStream2.Close();
			}
			if (flag)
			{
				try
				{
					switch (NativeMethods.WaitForSingleObject(safeProcessHandle, 600000))
					{
					case 258:
						throw new ExternalException(SR.GetString("ExecTimeout", cmd), 258);
					default:
						throw new ExternalException(SR.GetString("ExecBadreturn", cmd), Marshal.GetLastWin32Error());
					case 0:
					{
						int exitCode = 259;
						if (!NativeMethods.GetExitCodeProcess(safeProcessHandle, out exitCode))
						{
							throw new ExternalException(SR.GetString("ExecCantGetRetCode", cmd), Marshal.GetLastWin32Error());
						}
						return exitCode;
					}
					}
				}
				finally
				{
					safeProcessHandle.Close();
					safeThreadHandle.Close();
					if (hNewToken != null && !hNewToken.IsInvalid)
					{
						hNewToken.Close();
					}
				}
			}
			throw new ExternalException(SR.GetString("ExecCantExec", cmd), Marshal.GetLastWin32Error());
		}

		internal static WindowsImpersonationContext RevertImpersonation()
		{
			new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert();
			return WindowsIdentity.Impersonate(new IntPtr(0));
		}

		internal static void ReImpersonate(WindowsImpersonationContext impersonation)
		{
			impersonation.Undo();
		}
	}
}
