using System.Collections;
using System.Net.Cache;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNclNativeMethods
	{
		internal static class ErrorCodes
		{
			internal const uint ERROR_SUCCESS = 0u;

			internal const uint ERROR_HANDLE_EOF = 38u;

			internal const uint ERROR_NOT_SUPPORTED = 50u;

			internal const uint ERROR_INVALID_PARAMETER = 87u;

			internal const uint ERROR_ALREADY_EXISTS = 183u;

			internal const uint ERROR_MORE_DATA = 234u;

			internal const uint ERROR_OPERATION_ABORTED = 995u;

			internal const uint ERROR_IO_PENDING = 997u;

			internal const uint ERROR_NOT_FOUND = 1168u;
		}

		internal static class NTStatus
		{
			internal const uint STATUS_SUCCESS = 0u;

			internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 3221225524u;
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class RegistryHelper
		{
			internal const uint REG_NOTIFY_CHANGE_LAST_SET = 4u;

			internal const uint REG_BINARY = 3u;

			internal const uint KEY_READ = 131097u;

			internal static readonly IntPtr HKEY_CURRENT_USER = (IntPtr)(-2147483647);

			internal static readonly IntPtr HKEY_LOCAL_MACHINE = (IntPtr)(-2147483646);

			[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern uint RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint samDesired, out SafeRegistryHandle resultSubKey);

			[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern uint RegOpenKeyEx(SafeRegistryHandle key, string subKey, uint ulOptions, uint samDesired, out SafeRegistryHandle resultSubKey);

			[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern uint RegCloseKey(IntPtr key);

			[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern uint RegNotifyChangeKeyValue(SafeRegistryHandle key, bool watchSubTree, uint notifyFilter, SafeWaitHandle regEvent, bool async);

			[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern uint RegOpenCurrentUser(uint samDesired, out SafeRegistryHandle resultKey);

			[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern uint RegQueryValueEx(SafeRegistryHandle key, string valueName, IntPtr reserved, out uint type, [Out] byte[] data, [In][Out] ref uint size);
		}

		[SuppressUnmanagedCodeSecurity]
		internal class RasHelper
		{
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
			private struct RASCONN
			{
				internal uint dwSize;

				internal IntPtr hrasconn;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
				internal string szEntryName;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
				internal string szDeviceType;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
				internal string szDeviceName;
			}

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			private struct RASCONNSTATUS
			{
				internal uint dwSize;

				internal RASCONNSTATE rasconnstate;

				internal uint dwError;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
				internal string szDeviceType;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
				internal string szDeviceName;
			}

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			private struct RASDIALPARAMS
			{
				internal uint dwSize;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
				internal string szEntryName;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
				internal string szPhoneNumber;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
				internal string szCallbackNumber;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
				internal string szUserName;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
				internal string szPassword;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
				internal string szDomain;
			}

			private enum RASCONNSTATE
			{
				RASCS_OpenPort = 0,
				RASCS_PortOpened = 1,
				RASCS_ConnectDevice = 2,
				RASCS_DeviceConnected = 3,
				RASCS_AllDevicesConnected = 4,
				RASCS_Authenticate = 5,
				RASCS_AuthNotify = 6,
				RASCS_AuthRetry = 7,
				RASCS_AuthCallback = 8,
				RASCS_AuthChangePassword = 9,
				RASCS_AuthProject = 10,
				RASCS_AuthLinkSpeed = 11,
				RASCS_AuthAck = 12,
				RASCS_ReAuthenticate = 13,
				RASCS_Authenticated = 14,
				RASCS_PrepareForCallback = 0xF,
				RASCS_WaitForModemReset = 0x10,
				RASCS_WaitForCallback = 17,
				RASCS_Projected = 18,
				RASCS_StartAuthentication = 19,
				RASCS_CallbackComplete = 20,
				RASCS_LogonNetwork = 21,
				RASCS_SubEntryConnected = 22,
				RASCS_SubEntryDisconnected = 23,
				RASCS_Interactive = 0x1000,
				RASCS_RetryAuthentication = 4097,
				RASCS_CallbackSetByCaller = 4098,
				RASCS_PasswordExpired = 4099,
				RASCS_InvokeEapUI = 4100,
				RASCS_Connected = 0x2000,
				RASCS_Disconnected = 8193
			}

			private const int RAS_MaxEntryName = 256;

			private const int RAS_MaxDeviceType = 16;

			private const int RAS_MaxDeviceName = 128;

			private const int RAS_MaxPhoneNumber = 128;

			private const int RAS_MaxCallbackNumber = 128;

			private const uint RASCN_Connection = 1u;

			private const uint RASCN_Disconnection = 2u;

			private const int UNLEN = 256;

			private const int PWLEN = 256;

			private const int DNLEN = 15;

			private const int MAX_PATH = 260;

			private const uint RASBASE = 600u;

			private const uint ERROR_DIAL_ALREADY_IN_PROGRESS = 756u;

			private const uint ERROR_BUFFER_TOO_SMALL = 603u;

			private const int RASCS_PAUSED = 4096;

			private const int RASCS_DONE = 8192;

			private static bool s_RasSupported;

			private ManualResetEvent m_RasEvent;

			private bool m_Suppressed;

			internal static bool RasSupported => s_RasSupported;

			internal bool HasChanged
			{
				get
				{
					if (m_Suppressed)
					{
						return false;
					}
					ManualResetEvent rasEvent = m_RasEvent;
					if (rasEvent == null)
					{
						throw new ObjectDisposedException(GetType().FullName);
					}
					return rasEvent.WaitOne(0, exitContext: false);
				}
			}

			static RasHelper()
			{
				InitRasSupported();
			}

			internal RasHelper()
			{
				if (!s_RasSupported)
				{
					throw new InvalidOperationException(SR.GetString("net_log_proxy_ras_notsupported_exception"));
				}
				m_RasEvent = new ManualResetEvent(initialState: false);
				if (RasConnectionNotification((IntPtr)(-1), m_RasEvent.SafeWaitHandle, 3u) != 0)
				{
					m_Suppressed = true;
					m_RasEvent.Close();
					m_RasEvent = null;
				}
			}

			internal void Reset()
			{
				if (!m_Suppressed)
				{
					ManualResetEvent rasEvent = m_RasEvent;
					if (rasEvent == null)
					{
						throw new ObjectDisposedException(GetType().FullName);
					}
					rasEvent.Reset();
				}
			}

			internal static string GetCurrentConnectoid()
			{
				uint num = (uint)Marshal.SizeOf(typeof(RASCONN));
				if (!s_RasSupported)
				{
					return null;
				}
				uint lpcConnections = 4u;
				uint num2 = 0u;
				RASCONN[] array = null;
				while (true)
				{
					uint lpcb = checked(num * lpcConnections);
					array = new RASCONN[lpcConnections];
					array[0].dwSize = num;
					num2 = RasEnumConnections(array, ref lpcb, ref lpcConnections);
					if (num2 != 603)
					{
						break;
					}
					lpcConnections = checked(lpcb + num - 1u) / num;
				}
				if (lpcConnections == 0 || num2 != 0)
				{
					return null;
				}
				for (uint num3 = 0u; num3 < lpcConnections; num3++)
				{
					RASCONNSTATUS lprasconnstatus = default(RASCONNSTATUS);
					lprasconnstatus.dwSize = (uint)Marshal.SizeOf(lprasconnstatus);
					if (RasGetConnectStatus(array[num3].hrasconn, ref lprasconnstatus) == 0 && lprasconnstatus.rasconnstate == RASCONNSTATE.RASCS_Connected)
					{
						return array[num3].szEntryName;
					}
				}
				return null;
			}

			private static void InitRasSupported()
			{
				if (ComNetOS.InstallationType == WindowsInstallationType.ServerCore)
				{
					s_RasSupported = false;
				}
				else
				{
					s_RasSupported = true;
				}
			}

			[DllImport("rasapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, ThrowOnUnmappableChar = true)]
			private static extern uint RasEnumConnections([In][Out] RASCONN[] lprasconn, ref uint lpcb, ref uint lpcConnections);

			[DllImport("rasapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, ThrowOnUnmappableChar = true)]
			private static extern uint RasGetConnectStatus([In] IntPtr hrasconn, [In][Out] ref RASCONNSTATUS lprasconnstatus);

			[DllImport("rasapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, ThrowOnUnmappableChar = true)]
			private static extern uint RasConnectionNotification([In] IntPtr hrasconn, [In] SafeWaitHandle hEvent, uint dwFlags);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandles_SECUR32
		{
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int FreeContextBuffer([In] IntPtr contextBuffer);

			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int FreeCredentialsHandle(ref SSPIHandle handlePtr);

			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int DeleteSecurityContext(ref SSPIHandle handlePtr);

			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int AcceptSecurityContext(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] SecurityBufferDescriptor inputBuffer, [In] ContextFlags inFlags, [In] Endianness endianness, ref SSPIHandle outContextPtr, [In][Out] SecurityBufferDescriptor outputBuffer, [In][Out] ref ContextFlags attributes, out long timeStamp);

			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int QueryContextAttributesA(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);

			[DllImport("secur32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal unsafe static extern int AcquireCredentialsHandleA([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref AuthIdentity authdata, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			[DllImport("secur32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal unsafe static extern int AcquireCredentialsHandleA([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] IntPtr zero, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int InitializeSecurityContextA(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] ContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecurityBufferDescriptor inputBuffer, [In] int reservedII, ref SSPIHandle outContextPtr, [In][Out] SecurityBufferDescriptor outputBuffer, [In][Out] ref ContextFlags attributes, out long timeStamp);

			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int EnumerateSecurityPackagesA(out int pkgnum, out SafeFreeContextBuffer_SECUR32 handle);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandles_SECURITY
		{
			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int FreeContextBuffer([In] IntPtr contextBuffer);

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int FreeCredentialsHandle(ref SSPIHandle handlePtr);

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int DeleteSecurityContext(ref SSPIHandle handlePtr);

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int AcceptSecurityContext(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] SecurityBufferDescriptor inputBuffer, [In] ContextFlags inFlags, [In] Endianness endianness, ref SSPIHandle outContextPtr, [In][Out] SecurityBufferDescriptor outputBuffer, [In][Out] ref ContextFlags attributes, out long timeStamp);

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int QueryContextAttributesW(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int EnumerateSecurityPackagesW(out int pkgnum, out SafeFreeContextBuffer_SECURITY handle);

			[DllImport("security.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref AuthIdentity authdata, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			[DllImport("security.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] IntPtr zero, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			[DllImport("security.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SecureCredential authData, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int InitializeSecurityContextW(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] ContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecurityBufferDescriptor inputBuffer, [In] int reservedII, ref SSPIHandle outContextPtr, [In][Out] SecurityBufferDescriptor outputBuffer, [In][Out] ref ContextFlags attributes, out long timeStamp);

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int CompleteAuthToken([In] void* inContextPtr, [In][Out] SecurityBufferDescriptor inputBuffers);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandles_SCHANNEL
		{
			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int FreeContextBuffer([In] IntPtr contextBuffer);

			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int QueryContextAttributesA(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);

			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int EnumerateSecurityPackagesA(out int pkgnum, out SafeFreeContextBuffer_SCHANNEL handle);

			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int InitializeSecurityContextA(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] ContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecurityBufferDescriptor inputBuffer, [In] int reservedII, ref SSPIHandle outContextPtr, [In][Out] SecurityBufferDescriptor outputBuffer, [In][Out] ref ContextFlags attributes, out long timeStamp);

			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int AcceptSecurityContext(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] SecurityBufferDescriptor inputBuffer, [In] ContextFlags inFlags, [In] Endianness endianness, ref SSPIHandle outContextPtr, [In][Out] SecurityBufferDescriptor outputBuffer, [In][Out] ref ContextFlags attributes, out long timeStamp);

			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int DeleteSecurityContext(ref SSPIHandle handlePtr);

			[DllImport("schannel.dll", BestFitMapping = false, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal unsafe static extern int AcquireCredentialsHandleA([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SecureCredential authData, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern int FreeCredentialsHandle(ref SSPIHandle handlePtr);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandlesSafeOverlappedFree
		{
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SafeOverlappedFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandlesXPOrLater
		{
			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern int getaddrinfo([In] string nodename, [In] string servicename, [In] ref AddressInfo hints, out SafeFreeAddrInfo handle);

			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern void freeaddrinfo([In] IntPtr info);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandles
		{
			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int QuerySecurityContextToken(ref SSPIHandle phContext, out SafeCloseHandle handle);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpCreateHttpHandle(out SafeCloseHandle pReqQueueHandle, uint options);

			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern bool CloseHandle(IntPtr handle);

			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SafeLocalFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);

			[DllImport("kernel32.dll", EntryPoint = "LocalAlloc", SetLastError = true)]
			internal static extern SafeLocalFreeChannelBinding LocalAllocChannelBinding(int uFlags, UIntPtr sizetdwBytes);

			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern IntPtr LocalFree(IntPtr handle);

			[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal unsafe static extern SafeLoadLibrary LoadLibraryExA([In] string lpwLibFileName, [In] void* hFile, [In] uint dwFlags);

			[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern SafeLoadLibrary LoadLibraryExW([In] string lpwLibFileName, [In] void* hFile, [In] uint dwFlags);

			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern bool FreeLibrary([In] IntPtr hModule);

			[DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern void CertFreeCertificateChain([In] IntPtr pChainContext);

			[DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern bool CertFreeCertificateContext([In] IntPtr certContext);

			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern IntPtr GlobalFree(IntPtr handle);

			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SafeCloseSocket.InnerSafeCloseSocket accept([In] IntPtr socketHandle, [Out] byte[] socketAddress, [In][Out] ref int socketAddressSize);

			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern SocketError closesocket([In] IntPtr socketHandle);

			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern SocketError ioctlsocket([In] IntPtr handle, [In] int cmd, [In][Out] ref int argp);

			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern SocketError WSAEventSelect([In] IntPtr handle, [In] IntPtr Event, [In] AsyncEventBits NetworkEvents);

			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern SocketError setsockopt([In] IntPtr handle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref Linger linger, [In] int optionLength);

			[DllImport("wininet.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern bool RetrieveUrlCacheEntryFileW([In] char* urlName, [In] byte* entryPtr, [In][Out] ref int entryBufSize, [In] int dwReserved);

			[DllImport("wininet.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern bool UnlockUrlCacheEntryFileW([In] char* urlName, [In] int dwReserved);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class OSSOCK
		{
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			internal struct WSAPROTOCOLCHAIN
			{
				internal int ChainLen;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
				internal uint[] ChainEntries;
			}

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			internal struct WSAPROTOCOL_INFO
			{
				internal uint dwServiceFlags1;

				internal uint dwServiceFlags2;

				internal uint dwServiceFlags3;

				internal uint dwServiceFlags4;

				internal uint dwProviderFlags;

				private Guid ProviderId;

				internal uint dwCatalogEntryId;

				private WSAPROTOCOLCHAIN ProtocolChain;

				internal int iVersion;

				internal AddressFamily iAddressFamily;

				internal int iMaxSockAddr;

				internal int iMinSockAddr;

				internal int iSocketType;

				internal int iProtocol;

				internal int iProtocolMaxOffset;

				internal int iNetworkByteOrder;

				internal int iSecurityScheme;

				internal uint dwMessageSize;

				internal uint dwProviderReserved;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
				internal string szProtocol;
			}

			internal struct ControlData
			{
				internal UIntPtr length;

				internal uint level;

				internal uint type;

				internal uint address;

				internal uint index;
			}

			internal struct ControlDataIPv6
			{
				internal UIntPtr length;

				internal uint level;

				internal uint type;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
				internal byte[] address;

				internal uint index;
			}

			internal struct WSAMsg
			{
				internal IntPtr socketAddress;

				internal uint addressLength;

				internal IntPtr buffers;

				internal uint count;

				internal WSABuffer controlBuffer;

				internal SocketFlags flags;
			}

			[Flags]
			internal enum TransmitPacketsElementFlags : uint
			{
				None = 0x0u,
				Memory = 0x1u,
				File = 0x2u,
				EndOfPacket = 0x4u
			}

			[StructLayout(LayoutKind.Explicit)]
			internal struct TransmitPacketsElement
			{
				[FieldOffset(0)]
				internal TransmitPacketsElementFlags flags;

				[FieldOffset(4)]
				internal uint length;

				[FieldOffset(8)]
				internal long fileOffset;

				[FieldOffset(8)]
				internal IntPtr buffer;

				[FieldOffset(16)]
				internal IntPtr fileHandle;
			}

			internal struct SOCKET_ADDRESS
			{
				internal IntPtr lpSockAddr;

				internal int iSockaddrLength;
			}

			internal struct SOCKET_ADDRESS_LIST
			{
				internal int iAddressCount;

				internal SOCKET_ADDRESS Addresses;
			}

			internal struct TransmitFileBuffersStruct
			{
				internal IntPtr preBuffer;

				internal int preBufferLength;

				internal IntPtr postBuffer;

				internal int postBufferLength;
			}

			private const string WS2_32 = "ws2_32.dll";

			private const string mswsock = "mswsock.dll";

			[DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal static extern SafeCloseSocket.InnerSafeCloseSocket WSASocket([In] AddressFamily addressFamily, [In] SocketType socketType, [In] ProtocolType protocolType, [In] IntPtr protocolInfo, [In] uint group, [In] SocketConstructorFlags flags);

			[DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal unsafe static extern SafeCloseSocket.InnerSafeCloseSocket WSASocket([In] AddressFamily addressFamily, [In] SocketType socketType, [In] ProtocolType protocolType, [In] byte* pinnedBuffer, [In] uint group, [In] SocketConstructorFlags flags);

			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError WSAStartup([In] short wVersionRequested, out WSAData lpWSAData);

			[DllImport("ws2_32.dll", SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal static extern SocketError ioctlsocket([In] SafeCloseSocket socketHandle, [In] int cmd, [In][Out] ref int argp);

			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern IntPtr gethostbyname([In] string host);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern IntPtr gethostbyaddr([In] ref int addr, [In] int len, [In] ProtocolFamily type);

			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError gethostname([Out] StringBuilder hostName, [In] int bufferLength);

			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern int inet_addr([In] string cp);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getpeername([In] SafeCloseSocket socketHandle, [Out] byte[] socketAddress, [In][Out] ref int socketAddressSize);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out int optionValue, [In][Out] ref int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [Out] byte[] optionValue, [In][Out] ref int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out Linger optionValue, [In][Out] ref int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out IPMulticastRequest optionValue, [In][Out] ref int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out IPv6MulticastRequest optionValue, [In][Out] ref int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref int optionValue, [In] int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] byte[] optionValue, [In] int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IntPtr pointer, [In] int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref Linger linger, [In] int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IPMulticastRequest mreq, [In] int optionLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IPv6MulticastRequest mreq, [In] int optionLength);

			[DllImport("mswsock.dll", SetLastError = true)]
			internal static extern bool AcceptEx([In] SafeCloseSocket listenSocketHandle, [In] SafeCloseSocket acceptSocketHandle, [In] IntPtr buffer, [In] int len, [In] int localAddressLength, [In] int remoteAddressLength, out int bytesReceived, [In] SafeHandle overlapped);

			[DllImport("mswsock.dll", SetLastError = true)]
			internal static extern bool TransmitFile([In] SafeCloseSocket socket, [In] SafeHandle fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);

			[DllImport("mswsock.dll", SetLastError = true)]
			internal static extern bool TransmitFile([In] SafeCloseSocket socket, [In] SafeHandle fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] IntPtr overlapped, [In] IntPtr buffers, [In] TransmitFileOptions flags);

			[DllImport("mswsock.dll", SetLastError = true)]
			internal static extern bool TransmitFile([In] SafeCloseSocket socket, [In] IntPtr fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] IntPtr overlapped, [In] IntPtr buffers, [In] TransmitFileOptions flags);

			[DllImport("mswsock.dll", EntryPoint = "TransmitFile", SetLastError = true)]
			internal static extern bool TransmitFile2([In] SafeCloseSocket socket, [In] IntPtr fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);

			[DllImport("mswsock.dll", EntryPoint = "TransmitFile", SetLastError = true)]
			internal static extern bool TransmitFile_Blocking([In] IntPtr socket, [In] SafeHandle fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);

			[DllImport("mswsock.dll", EntryPoint = "TransmitFile", SetLastError = true)]
			internal static extern bool TransmitFile_Blocking2([In] IntPtr socket, [In] IntPtr fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);

			[DllImport("mswsock.dll", SetLastError = true)]
			internal static extern void GetAcceptExSockaddrs([In] IntPtr buffer, [In] int receiveDataLength, [In] int localAddressLength, [In] int remoteAddressLength, out IntPtr localSocketAddress, out int localSocketAddressLength, out IntPtr remoteSocketAddress, out int remoteSocketAddressLength);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int send([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int recv([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError listen([In] SafeCloseSocket socketHandle, [In] int backlog);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError bind([In] SafeCloseSocket socketHandle, [In] byte[] socketAddress, [In] int socketAddressSize);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError shutdown([In] SafeCloseSocket socketHandle, [In] int how);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int sendto([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [In] byte[] socketAddress, [In] int socketAddressSize);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int recvfrom([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [Out] byte[] socketAddress, [In][Out] ref int socketAddressSize);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockname([In] SafeCloseSocket socketHandle, [Out] byte[] socketAddress, [In][Out] ref int socketAddressSize);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern int select([In] int ignoredParameter, [In][Out] IntPtr[] readfds, [In][Out] IntPtr[] writefds, [In][Out] IntPtr[] exceptfds, [In] ref TimeValue timeout);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern int select([In] int ignoredParameter, [In][Out] IntPtr[] readfds, [In][Out] IntPtr[] writefds, [In][Out] IntPtr[] exceptfds, [In] IntPtr nullTimeout);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAConnect([In] IntPtr socketHandle, [In] byte[] socketAddress, [In] int socketAddressSize, [In] IntPtr inBuffer, [In] IntPtr outBuffer, [In] IntPtr sQOS, [In] IntPtr gQOS);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASend([In] SafeCloseSocket socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASend([In] SafeCloseSocket socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASend([In] SafeCloseSocket socketHandle, [In] IntPtr buffers, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", EntryPoint = "WSASend", SetLastError = true)]
			internal static extern SocketError WSASend_Blocking([In] IntPtr socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASendTo([In] SafeCloseSocket socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr socketAddress, [In] int socketAddressSize, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASendTo([In] SafeCloseSocket socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr socketAddress, [In] int socketAddressSize, [In] SafeNativeOverlapped overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecv([In] SafeCloseSocket socketHandle, [In][Out] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In][Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecv([In] SafeCloseSocket socketHandle, [In][Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In][Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecv([In] SafeCloseSocket socketHandle, [In] IntPtr buffers, [In] int bufferCount, out int bytesTransferred, [In][Out] ref SocketFlags socketFlags, [In] IntPtr overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", EntryPoint = "WSARecv", SetLastError = true)]
			internal static extern SocketError WSARecv_Blocking([In] IntPtr socketHandle, [In][Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In][Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecvFrom([In] SafeCloseSocket socketHandle, [In][Out] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In][Out] ref SocketFlags socketFlags, [In] IntPtr socketAddressPointer, [In] IntPtr socketAddressSizePointer, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecvFrom([In] SafeCloseSocket socketHandle, [In][Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In][Out] ref SocketFlags socketFlags, [In] IntPtr socketAddressPointer, [In] IntPtr socketAddressSizePointer, [In] SafeNativeOverlapped overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAEventSelect([In] SafeCloseSocket socketHandle, [In] SafeHandle Event, [In] AsyncEventBits NetworkEvents);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAEventSelect([In] SafeCloseSocket socketHandle, [In] IntPtr Event, [In] AsyncEventBits NetworkEvents);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAIoctl([In] SafeCloseSocket socketHandle, [In] int ioControlCode, [In][Out] ref Guid guid, [In] int guidSize, out IntPtr funcPtr, [In] int funcPtrSize, out int bytesTransferred, [In] IntPtr shouldBeNull, [In] IntPtr shouldBeNull2);

			[DllImport("ws2_32.dll", EntryPoint = "WSAIoctl", SetLastError = true)]
			internal static extern SocketError WSAIoctl_Blocking([In] IntPtr socketHandle, [In] int ioControlCode, [In] byte[] inBuffer, [In] int inBufferSize, [Out] byte[] outBuffer, [In] int outBufferSize, out int bytesTransferred, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", EntryPoint = "WSAIoctl", SetLastError = true)]
			internal static extern SocketError WSAIoctl_Blocking_Internal([In] IntPtr socketHandle, [In] uint ioControlCode, [In] IntPtr inBuffer, [In] int inBufferSize, [Out] IntPtr outBuffer, [In] int outBufferSize, out int bytesTransferred, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAEnumNetworkEvents([In] SafeCloseSocket socketHandle, [In] SafeWaitHandle Event, [In][Out] ref NetworkEvents networkEvents);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int WSADuplicateSocket([In] SafeCloseSocket socketHandle, [In] uint targetProcessID, [In] byte* pinnedBuffer);

			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern bool WSAGetOverlappedResult([In] SafeCloseSocket socketHandle, [In] SafeHandle overlapped, out uint bytesTransferred, [In] bool wait, out SocketFlags socketFlags);

			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError WSAStringToAddress([In] string addressString, [In] AddressFamily addressFamily, [In] IntPtr lpProtocolInfo, [Out] byte[] socketAddress, [In][Out] ref int socketAddressSize);

			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError WSAAddressToString([In] byte[] socketAddress, [In] int socketAddressSize, [In] IntPtr lpProtocolInfo, [Out] StringBuilder addressString, [In][Out] ref int addressStringLength);

			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError getnameinfo([In] byte[] sa, [In] int salen, [In][Out] StringBuilder host, [In] int hostlen, [In][Out] StringBuilder serv, [In] int servlen, [In] int flags);

			[DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal static extern int WSAEnumProtocols([In][MarshalAs(UnmanagedType.LPArray)] int[] lpiProtocols, [In] SafeLocalFree lpProtocolBuffer, [In][Out] ref uint lpdwBufferLength);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class NativePKI
		{
			private const string CRYPT32 = "crypt32.dll";

			[DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int CertVerifyCertificateChainPolicy([In] IntPtr policy, [In] SafeFreeCertChain chainContext, [In] ref ChainPolicyParameter cpp, [In][Out] ref ChainPolicyStatus ps);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class NativeNTSSPI
		{
			private const string SECURITY = "security.dll";

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal static extern int EncryptMessage(ref SSPIHandle contextHandle, [In] uint qualityOfProtection, [In][Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber);

			[DllImport("security.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal unsafe static extern int DecryptMessage([In] ref SSPIHandle contextHandle, [In][Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber, uint* qualityOfProtection);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class NativeSSLWin9xSSPI
		{
			private const string SCHANNEL = "schannel.dll";

			private const string SECUR32 = "secur32.dll";

			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal static extern int SealMessage(ref SSPIHandle contextHandle, [In] uint qualityOfProtection, [In][Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber);

			[DllImport("schannel.dll", ExactSpelling = true, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal static extern int UnsealMessage([In] ref SSPIHandle contextHandle, [In][Out] SecurityBufferDescriptor inputOutput, [In] IntPtr qualityOfProtection, [In] uint sequenceNumber);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class WinInet
		{
			[DllImport("wininet.dll", BestFitMapping = false, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern bool DetectAutoProxyUrl([Out] StringBuilder autoProxyUrl, [In] int autoProxyUrlLength, [In] int detectFlags);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class WinHttp
		{
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			internal struct WINHTTP_CURRENT_USER_IE_PROXY_CONFIG
			{
				public bool AutoDetect;

				public IntPtr AutoConfigUrl;

				public IntPtr Proxy;

				public IntPtr ProxyBypass;
			}

			[Flags]
			internal enum AutoProxyFlags
			{
				AutoDetect = 0x1,
				AutoProxyConfigUrl = 0x2,
				RunInProcess = 0x10000,
				RunOutProcessOnly = 0x20000
			}

			internal enum AccessType
			{
				DefaultProxy = 0,
				NoProxy = 1,
				NamedProxy = 3
			}

			[Flags]
			internal enum AutoDetectType
			{
				None = 0x0,
				Dhcp = 0x1,
				DnsA = 0x2
			}

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			internal struct WINHTTP_AUTOPROXY_OPTIONS
			{
				public AutoProxyFlags Flags;

				public AutoDetectType AutoDetectFlags;

				[MarshalAs(UnmanagedType.LPWStr)]
				public string AutoConfigUrl;

				private IntPtr lpvReserved;

				private int dwReserved;

				public bool AutoLogonIfChallenged;
			}

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			internal struct WINHTTP_PROXY_INFO
			{
				public AccessType AccessType;

				public IntPtr Proxy;

				public IntPtr ProxyBypass;
			}

			internal enum ErrorCodes
			{
				Success = 0,
				OutOfHandles = 12001,
				Timeout = 12002,
				InternalError = 12004,
				InvalidUrl = 12005,
				UnrecognizedScheme = 12006,
				NameNotResolved = 12007,
				InvalidOption = 12009,
				OptionNotSettable = 12011,
				Shutdown = 12012,
				LoginFailure = 12015,
				OperationCancelled = 12017,
				IncorrectHandleType = 12018,
				IncorrectHandleState = 12019,
				CannotConnect = 12029,
				ConnectionError = 12030,
				ResendRequest = 12032,
				AuthCertNeeded = 12044,
				CannotCallBeforeOpen = 12100,
				CannotCallBeforeSend = 12101,
				CannotCallAfterSend = 12102,
				CannotCallAfterOpen = 12103,
				HeaderNotFound = 12150,
				InvalidServerResponse = 12152,
				InvalidHeader = 12153,
				InvalidQueryRequest = 12154,
				HeaderAlreadyExists = 12155,
				RedirectFailed = 12156,
				AutoProxyServiceError = 12178,
				BadAutoProxyScript = 12166,
				UnableToDownloadScript = 12167,
				NotInitialized = 12172,
				SecureFailure = 12175,
				SecureCertDateInvalid = 12037,
				SecureCertCNInvalid = 12038,
				SecureInvalidCA = 12045,
				SecureCertRevFailed = 12057,
				SecureChannelError = 12157,
				SecureInvalidCert = 12169,
				SecureCertRevoked = 12170,
				SecureCertWrongUsage = 12179,
				AudodetectionFailed = 12180,
				HeaderCountExceeded = 12181,
				HeaderSizeOverflow = 12182,
				ChunkedEncodingHeaderSizeOverflow = 12183,
				ResponseDrainOverflow = 12184,
				ClientCertNoPrivateKey = 12185,
				ClientCertNoAccessPrivateKey = 12186
			}

			[DllImport("winhttp.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern bool WinHttpDetectAutoProxyConfigUrl(AutoDetectType autoDetectFlags, out SafeGlobalFree autoConfigUrl);

			[DllImport("winhttp.dll", SetLastError = true)]
			internal static extern bool WinHttpGetIEProxyConfigForCurrentUser(ref WINHTTP_CURRENT_USER_IE_PROXY_CONFIG proxyConfig);

			[DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern SafeInternetHandle WinHttpOpen(string userAgent, AccessType accessType, string proxyName, string proxyBypass, int dwFlags);

			[DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern bool WinHttpSetTimeouts(SafeInternetHandle session, int resolveTimeout, int connectTimeout, int sendTimeout, int receiveTimeout);

			[DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern bool WinHttpGetProxyForUrl(SafeInternetHandle session, string url, [In] ref WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions, out WINHTTP_PROXY_INFO proxyInfo);

			[DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal static extern bool WinHttpCloseHandle(IntPtr httpSession);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class UnsafeWinInetCache
		{
			public const int MAX_PATH = 260;

			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern bool CreateUrlCacheEntryW([In] string urlName, [In] int expectedFileSize, [In] string fileExtension, [Out] StringBuilder fileName, [In] int dwReserved);

			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern bool CommitUrlCacheEntryW([In] string urlName, [In] string localFileName, [In] _WinInetCache.FILETIME expireTime, [In] _WinInetCache.FILETIME lastModifiedTime, [In] _WinInetCache.EntryType EntryType, [In] byte* headerInfo, [In] int headerSizeTChars, [In] string fileExtension, [In] string originalUrl);

			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern bool GetUrlCacheEntryInfoW([In] string urlName, [In] byte* entryPtr, [In][Out] ref int bufferSz);

			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern bool SetUrlCacheEntryInfoW([In] string lpszUrlName, [In] byte* EntryPtr, [In] _WinInetCache.Entry_FC fieldControl);

			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern bool DeleteUrlCacheEntryW([In] string urlName);

			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern bool UnlockUrlCacheEntryFileW([In] string urlName, [In] int dwReserved);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class HttpApi
		{
			internal struct HTTP_VERSION
			{
				internal ushort MajorVersion;

				internal ushort MinorVersion;
			}

			internal struct HTTP_KNOWN_HEADER
			{
				internal ushort RawValueLength;

				internal unsafe sbyte* pRawValue;
			}

			[StructLayout(LayoutKind.Sequential, Size = 32)]
			internal struct HTTP_DATA_CHUNK
			{
				internal HTTP_DATA_CHUNK_TYPE DataChunkType;

				internal uint p0;

				internal unsafe byte* pBuffer;

				internal uint BufferLength;
			}

			internal struct HTTPAPI_VERSION
			{
				internal ushort HttpApiMajorVersion;

				internal ushort HttpApiMinorVersion;
			}

			internal struct HTTP_COOKED_URL
			{
				internal ushort FullUrlLength;

				internal ushort HostLength;

				internal ushort AbsPathLength;

				internal ushort QueryStringLength;

				internal unsafe ushort* pFullUrl;

				internal unsafe ushort* pHost;

				internal unsafe ushort* pAbsPath;

				internal unsafe ushort* pQueryString;
			}

			internal struct SOCKADDR
			{
				internal ushort sa_family;

				internal byte sa_data;

				internal byte sa_data_02;

				internal byte sa_data_03;

				internal byte sa_data_04;

				internal byte sa_data_05;

				internal byte sa_data_06;

				internal byte sa_data_07;

				internal byte sa_data_08;

				internal byte sa_data_09;

				internal byte sa_data_10;

				internal byte sa_data_11;

				internal byte sa_data_12;

				internal byte sa_data_13;

				internal byte sa_data_14;
			}

			internal struct HTTP_TRANSPORT_ADDRESS
			{
				internal unsafe SOCKADDR* pRemoteAddress;

				internal unsafe SOCKADDR* pLocalAddress;
			}

			internal struct HTTP_SSL_CLIENT_CERT_INFO
			{
				internal uint CertFlags;

				internal uint CertEncodedSize;

				internal unsafe byte* pCertEncoded;

				internal unsafe void* Token;

				internal byte CertDeniedByMapper;
			}

			internal enum HTTP_SERVICE_BINDING_TYPE : uint
			{
				HttpServiceBindingTypeNone,
				HttpServiceBindingTypeW,
				HttpServiceBindingTypeA
			}

			internal struct HTTP_SERVICE_BINDING_BASE
			{
				internal HTTP_SERVICE_BINDING_TYPE Type;
			}

			internal struct HTTP_REQUEST_CHANNEL_BIND_STATUS
			{
				internal IntPtr ServiceName;

				internal IntPtr ChannelToken;

				internal uint ChannelTokenSize;

				internal uint Flags;
			}

			internal struct HTTP_UNKNOWN_HEADER
			{
				internal ushort NameLength;

				internal ushort RawValueLength;

				internal unsafe sbyte* pName;

				internal unsafe sbyte* pRawValue;
			}

			internal struct HTTP_SSL_INFO
			{
				internal ushort ServerCertKeySize;

				internal ushort ConnectionKeySize;

				internal uint ServerCertIssuerSize;

				internal uint ServerCertSubjectSize;

				internal unsafe sbyte* pServerCertIssuer;

				internal unsafe sbyte* pServerCertSubject;

				internal unsafe HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo;

				internal uint SslClientCertNegotiated;
			}

			internal struct HTTP_RESPONSE_HEADERS
			{
				internal ushort UnknownHeaderCount;

				internal unsafe HTTP_UNKNOWN_HEADER* pUnknownHeaders;

				internal ushort TrailerCount;

				internal unsafe HTTP_UNKNOWN_HEADER* pTrailers;

				internal HTTP_KNOWN_HEADER KnownHeaders;

				internal HTTP_KNOWN_HEADER KnownHeaders_02;

				internal HTTP_KNOWN_HEADER KnownHeaders_03;

				internal HTTP_KNOWN_HEADER KnownHeaders_04;

				internal HTTP_KNOWN_HEADER KnownHeaders_05;

				internal HTTP_KNOWN_HEADER KnownHeaders_06;

				internal HTTP_KNOWN_HEADER KnownHeaders_07;

				internal HTTP_KNOWN_HEADER KnownHeaders_08;

				internal HTTP_KNOWN_HEADER KnownHeaders_09;

				internal HTTP_KNOWN_HEADER KnownHeaders_10;

				internal HTTP_KNOWN_HEADER KnownHeaders_11;

				internal HTTP_KNOWN_HEADER KnownHeaders_12;

				internal HTTP_KNOWN_HEADER KnownHeaders_13;

				internal HTTP_KNOWN_HEADER KnownHeaders_14;

				internal HTTP_KNOWN_HEADER KnownHeaders_15;

				internal HTTP_KNOWN_HEADER KnownHeaders_16;

				internal HTTP_KNOWN_HEADER KnownHeaders_17;

				internal HTTP_KNOWN_HEADER KnownHeaders_18;

				internal HTTP_KNOWN_HEADER KnownHeaders_19;

				internal HTTP_KNOWN_HEADER KnownHeaders_20;

				internal HTTP_KNOWN_HEADER KnownHeaders_21;

				internal HTTP_KNOWN_HEADER KnownHeaders_22;

				internal HTTP_KNOWN_HEADER KnownHeaders_23;

				internal HTTP_KNOWN_HEADER KnownHeaders_24;

				internal HTTP_KNOWN_HEADER KnownHeaders_25;

				internal HTTP_KNOWN_HEADER KnownHeaders_26;

				internal HTTP_KNOWN_HEADER KnownHeaders_27;

				internal HTTP_KNOWN_HEADER KnownHeaders_28;

				internal HTTP_KNOWN_HEADER KnownHeaders_29;

				internal HTTP_KNOWN_HEADER KnownHeaders_30;
			}

			internal struct HTTP_REQUEST_HEADERS
			{
				internal ushort UnknownHeaderCount;

				internal unsafe HTTP_UNKNOWN_HEADER* pUnknownHeaders;

				internal ushort TrailerCount;

				internal unsafe HTTP_UNKNOWN_HEADER* pTrailers;

				internal HTTP_KNOWN_HEADER KnownHeaders;

				internal HTTP_KNOWN_HEADER KnownHeaders_02;

				internal HTTP_KNOWN_HEADER KnownHeaders_03;

				internal HTTP_KNOWN_HEADER KnownHeaders_04;

				internal HTTP_KNOWN_HEADER KnownHeaders_05;

				internal HTTP_KNOWN_HEADER KnownHeaders_06;

				internal HTTP_KNOWN_HEADER KnownHeaders_07;

				internal HTTP_KNOWN_HEADER KnownHeaders_08;

				internal HTTP_KNOWN_HEADER KnownHeaders_09;

				internal HTTP_KNOWN_HEADER KnownHeaders_10;

				internal HTTP_KNOWN_HEADER KnownHeaders_11;

				internal HTTP_KNOWN_HEADER KnownHeaders_12;

				internal HTTP_KNOWN_HEADER KnownHeaders_13;

				internal HTTP_KNOWN_HEADER KnownHeaders_14;

				internal HTTP_KNOWN_HEADER KnownHeaders_15;

				internal HTTP_KNOWN_HEADER KnownHeaders_16;

				internal HTTP_KNOWN_HEADER KnownHeaders_17;

				internal HTTP_KNOWN_HEADER KnownHeaders_18;

				internal HTTP_KNOWN_HEADER KnownHeaders_19;

				internal HTTP_KNOWN_HEADER KnownHeaders_20;

				internal HTTP_KNOWN_HEADER KnownHeaders_21;

				internal HTTP_KNOWN_HEADER KnownHeaders_22;

				internal HTTP_KNOWN_HEADER KnownHeaders_23;

				internal HTTP_KNOWN_HEADER KnownHeaders_24;

				internal HTTP_KNOWN_HEADER KnownHeaders_25;

				internal HTTP_KNOWN_HEADER KnownHeaders_26;

				internal HTTP_KNOWN_HEADER KnownHeaders_27;

				internal HTTP_KNOWN_HEADER KnownHeaders_28;

				internal HTTP_KNOWN_HEADER KnownHeaders_29;

				internal HTTP_KNOWN_HEADER KnownHeaders_30;

				internal HTTP_KNOWN_HEADER KnownHeaders_31;

				internal HTTP_KNOWN_HEADER KnownHeaders_32;

				internal HTTP_KNOWN_HEADER KnownHeaders_33;

				internal HTTP_KNOWN_HEADER KnownHeaders_34;

				internal HTTP_KNOWN_HEADER KnownHeaders_35;

				internal HTTP_KNOWN_HEADER KnownHeaders_36;

				internal HTTP_KNOWN_HEADER KnownHeaders_37;

				internal HTTP_KNOWN_HEADER KnownHeaders_38;

				internal HTTP_KNOWN_HEADER KnownHeaders_39;

				internal HTTP_KNOWN_HEADER KnownHeaders_40;

				internal HTTP_KNOWN_HEADER KnownHeaders_41;
			}

			internal enum HTTP_VERB
			{
				HttpVerbUnparsed,
				HttpVerbUnknown,
				HttpVerbInvalid,
				HttpVerbOPTIONS,
				HttpVerbGET,
				HttpVerbHEAD,
				HttpVerbPOST,
				HttpVerbPUT,
				HttpVerbDELETE,
				HttpVerbTRACE,
				HttpVerbCONNECT,
				HttpVerbTRACK,
				HttpVerbMOVE,
				HttpVerbCOPY,
				HttpVerbPROPFIND,
				HttpVerbPROPPATCH,
				HttpVerbMKCOL,
				HttpVerbLOCK,
				HttpVerbUNLOCK,
				HttpVerbSEARCH,
				HttpVerbMaximum
			}

			internal enum HTTP_DATA_CHUNK_TYPE
			{
				HttpDataChunkFromMemory,
				HttpDataChunkFromFileHandle,
				HttpDataChunkFromFragmentCache,
				HttpDataChunkMaximum
			}

			internal struct HTTP_RESPONSE
			{
				internal uint Flags;

				internal HTTP_VERSION Version;

				internal ushort StatusCode;

				internal ushort ReasonLength;

				internal unsafe sbyte* pReason;

				internal HTTP_RESPONSE_HEADERS Headers;

				internal ushort EntityChunkCount;

				internal unsafe HTTP_DATA_CHUNK* pEntityChunks;
			}

			internal struct HTTP_REQUEST
			{
				internal uint Flags;

				internal ulong ConnectionId;

				internal ulong RequestId;

				internal ulong UrlContext;

				internal HTTP_VERSION Version;

				internal HTTP_VERB Verb;

				internal ushort UnknownVerbLength;

				internal ushort RawUrlLength;

				internal unsafe sbyte* pUnknownVerb;

				internal unsafe sbyte* pRawUrl;

				internal HTTP_COOKED_URL CookedUrl;

				internal HTTP_TRANSPORT_ADDRESS Address;

				internal HTTP_REQUEST_HEADERS Headers;

				internal ulong BytesReceived;

				internal ushort EntityChunkCount;

				internal unsafe HTTP_DATA_CHUNK* pEntityChunks;

				internal ulong RawConnectionId;

				internal unsafe HTTP_SSL_INFO* pSslInfo;
			}

			[Flags]
			internal enum HTTP_FLAGS : uint
			{
				NONE = 0x0u,
				HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY = 0x1u,
				HTTP_RECEIVE_SECURE_CHANNEL_TOKEN = 0x1u,
				HTTP_SEND_RESPONSE_FLAG_DISCONNECT = 0x1u,
				HTTP_SEND_RESPONSE_FLAG_MORE_DATA = 0x2u,
				HTTP_SEND_RESPONSE_FLAG_RAW_HEADER = 0x4u,
				HTTP_SEND_REQUEST_FLAG_MORE_DATA = 0x1u,
				HTTP_INITIALIZE_SERVER = 0x1u,
				HTTP_INITIALIZE_CBT = 0x4u
			}

			internal static class HTTP_REQUEST_HEADER_ID
			{
				private static Hashtable m_Hashtable;

				private static string[] m_Strings;

				static HTTP_REQUEST_HEADER_ID()
				{
					m_Strings = new string[41]
					{
						"Cache-Control",
						"Connection",
						"Date",
						"Keep-Alive",
						"Pragma",
						"Trailer",
						"Transfer-Encoding",
						"Upgrade",
						"Via",
						"Warning",
						"Allow",
						"Content-Length",
						"Content-Type",
						"Content-Encoding",
						"Content-Language",
						"Content-Location",
						"Content-MD5",
						"Content-Range",
						"Expires",
						"Last-Modified",
						"Accept",
						"Accept-Charset",
						"Accept-Encoding",
						"Accept-Language",
						"Authorization",
						"Cookie",
						"Expect",
						"From",
						"Host",
						"If-Match",
						"If-Modified-Since",
						"If-None-Match",
						"If-Range",
						"If-Unmodified-Since",
						"Max-Forwards",
						"Proxy-Authorization",
						"Referer",
						"Range",
						"Te",
						"Translate",
						"User-Agent"
					};
					m_Hashtable = new Hashtable(41);
					for (int i = 0; i < 40; i++)
					{
						m_Hashtable.Add(m_Strings[i], i);
					}
				}

				internal static string ToString(int position)
				{
					return m_Strings[position];
				}
			}

			internal static class HTTP_RESPONSE_HEADER_ID
			{
				internal enum Enum
				{
					HttpHeaderCacheControl = 0,
					HttpHeaderConnection = 1,
					HttpHeaderDate = 2,
					HttpHeaderKeepAlive = 3,
					HttpHeaderPragma = 4,
					HttpHeaderTrailer = 5,
					HttpHeaderTransferEncoding = 6,
					HttpHeaderUpgrade = 7,
					HttpHeaderVia = 8,
					HttpHeaderWarning = 9,
					HttpHeaderAllow = 10,
					HttpHeaderContentLength = 11,
					HttpHeaderContentType = 12,
					HttpHeaderContentEncoding = 13,
					HttpHeaderContentLanguage = 14,
					HttpHeaderContentLocation = 0xF,
					HttpHeaderContentMd5 = 0x10,
					HttpHeaderContentRange = 17,
					HttpHeaderExpires = 18,
					HttpHeaderLastModified = 19,
					HttpHeaderAcceptRanges = 20,
					HttpHeaderAge = 21,
					HttpHeaderEtag = 22,
					HttpHeaderLocation = 23,
					HttpHeaderProxyAuthenticate = 24,
					HttpHeaderRetryAfter = 25,
					HttpHeaderServer = 26,
					HttpHeaderSetCookie = 27,
					HttpHeaderVary = 28,
					HttpHeaderWwwAuthenticate = 29,
					HttpHeaderResponseMaximum = 30,
					HttpHeaderMaximum = 41
				}

				private static Hashtable m_Hashtable;

				private static string[] m_Strings;

				static HTTP_RESPONSE_HEADER_ID()
				{
					m_Strings = new string[30]
					{
						"Cache-Control",
						"Connection",
						"Date",
						"Keep-Alive",
						"Pragma",
						"Trailer",
						"Transfer-Encoding",
						"Upgrade",
						"Via",
						"Warning",
						"Allow",
						"Content-Length",
						"Content-Type",
						"Content-Encoding",
						"Content-Language",
						"Content-Location",
						"Content-MD5",
						"Content-Range",
						"Expires",
						"Last-Modified",
						"Accept-Ranges",
						"Age",
						"ETag",
						"Location",
						"Proxy-Authenticate",
						"Retry-After",
						"Server",
						"Set-Cookie",
						"Vary",
						"WWW-Authenticate"
					};
					m_Hashtable = new Hashtable(30);
					for (int i = 0; i < 29; i++)
					{
						m_Hashtable.Add(m_Strings[i], i);
					}
				}

				internal static int IndexOfKnownHeader(string HeaderName)
				{
					object obj = m_Hashtable[HeaderName];
					if (obj != null)
					{
						return (int)obj;
					}
					return -1;
				}

				internal static string ToString(int position)
				{
					return m_Strings[position];
				}
			}

			private const string HTTPAPI = "httpapi.dll";

			private const int HttpHeaderRequestMaximum = 41;

			private const int HttpHeaderResponseMaximum = 30;

			internal static readonly string[] HttpVerbs;

			private static bool extendedProtectionSupported;

			private static bool supported;

			internal static bool ExtendedProtectionSupported => extendedProtectionSupported;

			internal static bool Supported => supported;

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpInitialize(HTTPAPI_VERSION Version, uint Flags, void* Reserved);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveRequestEntityBody(SafeCloseHandle RequestQueueHandle, ulong RequestId, uint Flags, void* pEntityBuffer, uint EntityBufferLength, uint* pBytesReturned, NativeOverlapped* pOverlapped);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveClientCertificate(SafeCloseHandle RequestQueueHandle, ulong ConnectionId, uint Flags, HTTP_SSL_CLIENT_CERT_INFO* pSslClientCertInfo, uint SslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveClientCertificate(SafeCloseHandle RequestQueueHandle, ulong ConnectionId, uint Flags, byte* pSslClientCertInfo, uint SslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveHttpRequest(SafeCloseHandle RequestQueueHandle, ulong RequestId, uint Flags, HTTP_REQUEST* pRequestBuffer, uint RequestBufferLength, uint* pBytesReturned, NativeOverlapped* pOverlapped);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpAddUrl(SafeCloseHandle RequestQueueHandle, ushort* pFullyQualifiedUrl, void* pReserved);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpRemoveUrl(SafeCloseHandle RequestQueueHandle, ushort* pFullyQualifiedUrl);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpSendHttpResponse(SafeCloseHandle RequestQueueHandle, ulong RequestId, uint Flags, HTTP_RESPONSE* pHttpResponse, void* pCachePolicy, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint RequestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpSendResponseEntityBody(SafeCloseHandle RequestQueueHandle, ulong RequestId, uint Flags, ushort EntityChunkCount, HTTP_DATA_CHUNK* pEntityChunks, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint RequestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);

			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpWaitForDisconnect(SafeCloseHandle RequestQueueHandle, ulong ConnectionId, NativeOverlapped* pOverlapped);

			static HttpApi()
			{
				HttpVerbs = new string[20]
				{
					null,
					"Unknown",
					"Invalid",
					"OPTIONS",
					"GET",
					"HEAD",
					"POST",
					"PUT",
					"DELETE",
					"TRACE",
					"CONNECT",
					"TRACK",
					"MOVE",
					"COPY",
					"PROPFIND",
					"PROPPATCH",
					"MKCOL",
					"LOCK",
					"UNLOCK",
					"SEARCH"
				};
				SafeLoadLibrary safeLoadLibrary = SafeLoadLibrary.LoadLibraryEx("httpapi.dll");
				if (safeLoadLibrary.IsInvalid)
				{
					return;
				}
				try
				{
					HTTPAPI_VERSION version = new HTTPAPI_VERSION
					{
						HttpApiMajorVersion = 1,
						HttpApiMinorVersion = 0
					};
					uint num = 0u;
					extendedProtectionSupported = true;
					if (ComNetOS.IsWin7)
					{
						num = HttpInitialize(version, 1u, null);
					}
					else
					{
						num = HttpInitialize(version, 5u, null);
						if (num == 87)
						{
							if (Logging.On)
							{
								Logging.PrintWarning(Logging.HttpListener, SR.GetString("net_listener_cbt_not_supported"));
							}
							extendedProtectionSupported = false;
							num = HttpInitialize(version, 1u, null);
						}
					}
					supported = num == 0;
				}
				finally
				{
					safeLoadLibrary.Close();
				}
			}

			internal unsafe static WebHeaderCollection GetHeaders(byte[] memoryBlob, IntPtr originalAddress)
			{
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection(WebHeaderCollectionType.HttpListenerRequest);
				fixed (byte* ptr = memoryBlob)
				{
					HTTP_REQUEST* ptr2 = (HTTP_REQUEST*)ptr;
					long num = ptr - (byte*)(void*)originalAddress;
					if (ptr2->Headers.UnknownHeaderCount != 0)
					{
						HTTP_UNKNOWN_HEADER* ptr3 = (HTTP_UNKNOWN_HEADER*)(num + (byte*)ptr2->Headers.pUnknownHeaders);
						for (int i = 0; i < ptr2->Headers.UnknownHeaderCount; i++)
						{
							if (ptr3->pName != null && ptr3->NameLength > 0 && ptr3->pRawValue != null && ptr3->RawValueLength > 0)
							{
								string name = new string(ptr3->pName + num, 0, ptr3->NameLength);
								string value = new string(ptr3->pRawValue + num, 0, ptr3->RawValueLength);
								webHeaderCollection.AddInternal(name, value);
							}
							ptr3++;
						}
					}
					HTTP_KNOWN_HEADER* ptr4 = &ptr2->Headers.KnownHeaders;
					for (int i = 0; i < 40; i++)
					{
						if (ptr4->RawValueLength != 0 && ptr4->pRawValue != null)
						{
							string value2 = new string(ptr4->pRawValue + num, 0, ptr4->RawValueLength);
							webHeaderCollection.AddInternal(HTTP_REQUEST_HEADER_ID.ToString(i), value2);
						}
						ptr4++;
					}
				}
				return webHeaderCollection;
			}

			private unsafe static string GetKnownHeader(HTTP_REQUEST* request, long fixup, int headerIndex)
			{
				string result = null;
				HTTP_KNOWN_HEADER* ptr = &request->Headers.KnownHeaders + headerIndex;
				if (ptr->RawValueLength != 0 && ptr->pRawValue != null)
				{
					result = new string(ptr->pRawValue + fixup, 0, ptr->RawValueLength);
				}
				return result;
			}

			internal unsafe static string GetKnownHeader(HTTP_REQUEST* request, int headerIndex)
			{
				return GetKnownHeader(request, 0L, headerIndex);
			}

			internal unsafe static string GetKnownHeader(byte[] memoryBlob, IntPtr originalAddress, int headerIndex)
			{
				fixed (byte* ptr = memoryBlob)
				{
					return GetKnownHeader((HTTP_REQUEST*)ptr, ptr - (byte*)(void*)originalAddress, headerIndex);
				}
			}

			private unsafe static string GetVerb(HTTP_REQUEST* request, long fixup)
			{
				string result = null;
				if (request->Verb > HTTP_VERB.HttpVerbUnknown && request->Verb < HTTP_VERB.HttpVerbMaximum)
				{
					result = HttpVerbs[(int)request->Verb];
				}
				else if (request->Verb == HTTP_VERB.HttpVerbUnknown && request->pUnknownVerb != null)
				{
					result = new string(request->pUnknownVerb + fixup, 0, request->UnknownVerbLength);
				}
				return result;
			}

			internal unsafe static string GetVerb(HTTP_REQUEST* request)
			{
				return GetVerb(request, 0L);
			}

			internal unsafe static string GetVerb(byte[] memoryBlob, IntPtr originalAddress)
			{
				fixed (byte* ptr = memoryBlob)
				{
					return GetVerb((HTTP_REQUEST*)ptr, ptr - (byte*)(void*)originalAddress);
				}
			}

			internal unsafe static HTTP_VERB GetKnownVerb(byte[] memoryBlob, IntPtr originalAddress)
			{
				HTTP_VERB result = HTTP_VERB.HttpVerbUnknown;
				fixed (byte* ptr = memoryBlob)
				{
					HTTP_REQUEST* ptr2 = (HTTP_REQUEST*)ptr;
					if (ptr2->Verb > HTTP_VERB.HttpVerbUnparsed && ptr2->Verb < HTTP_VERB.HttpVerbMaximum)
					{
						result = ptr2->Verb;
					}
				}
				return result;
			}

			internal unsafe static uint GetChunks(byte[] memoryBlob, IntPtr originalAddress, ref int dataChunkIndex, ref uint dataChunkOffset, byte[] buffer, int offset, int size)
			{
				uint num = 0u;
				fixed (byte* ptr = memoryBlob)
				{
					HTTP_REQUEST* ptr2 = (HTTP_REQUEST*)ptr;
					long num2 = ptr - (byte*)(void*)originalAddress;
					if (ptr2->EntityChunkCount > 0 && dataChunkIndex < ptr2->EntityChunkCount && dataChunkIndex != -1)
					{
						HTTP_DATA_CHUNK* ptr3 = (HTTP_DATA_CHUNK*)(num2 + (byte*)(ptr2->pEntityChunks + dataChunkIndex));
						fixed (byte* ptr4 = buffer)
						{
							byte* ptr5 = ptr4 + offset;
							while (dataChunkIndex < ptr2->EntityChunkCount && num < size)
							{
								if (dataChunkOffset >= ptr3->BufferLength)
								{
									dataChunkOffset = 0u;
									dataChunkIndex++;
									ptr3++;
									continue;
								}
								byte* ptr6 = ptr3->pBuffer + (int)dataChunkOffset + num2;
								uint num3 = ptr3->BufferLength - dataChunkOffset;
								if (num3 > (uint)size)
								{
									num3 = (uint)size;
								}
								for (uint num4 = 0u; num4 < num3; num4++)
								{
									*(ptr5++) = *(ptr6++);
								}
								num += num3;
								dataChunkOffset += num3;
							}
						}
					}
					if (dataChunkIndex == ptr2->EntityChunkCount)
					{
						dataChunkIndex = -1;
					}
				}
				return num;
			}

			internal unsafe static IPEndPoint GetRemoteEndPoint(byte[] memoryBlob, IntPtr originalAddress)
			{
				SocketAddress v4address = new SocketAddress(AddressFamily.InterNetwork, 16);
				SocketAddress v6address = new SocketAddress(AddressFamily.InterNetworkV6, 28);
				fixed (byte* ptr = memoryBlob)
				{
					HTTP_REQUEST* ptr2 = (HTTP_REQUEST*)ptr;
					IntPtr address = ((ptr2->Address.pRemoteAddress != null) ? ((IntPtr)(ptr - (byte*)(void*)originalAddress + (byte*)ptr2->Address.pRemoteAddress)) : IntPtr.Zero);
					CopyOutAddress(address, ref v4address, ref v6address);
				}
				IPEndPoint result = null;
				if (v4address != null)
				{
					result = IPEndPoint.Any.Create(v4address) as IPEndPoint;
				}
				else if (v6address != null)
				{
					result = IPEndPoint.IPv6Any.Create(v6address) as IPEndPoint;
				}
				return result;
			}

			internal unsafe static IPEndPoint GetLocalEndPoint(byte[] memoryBlob, IntPtr originalAddress)
			{
				SocketAddress v4address = new SocketAddress(AddressFamily.InterNetwork, 16);
				SocketAddress v6address = new SocketAddress(AddressFamily.InterNetworkV6, 28);
				fixed (byte* ptr = memoryBlob)
				{
					HTTP_REQUEST* ptr2 = (HTTP_REQUEST*)ptr;
					IntPtr address = ((ptr2->Address.pLocalAddress != null) ? ((IntPtr)(ptr - (byte*)(void*)originalAddress + (byte*)ptr2->Address.pLocalAddress)) : IntPtr.Zero);
					CopyOutAddress(address, ref v4address, ref v6address);
				}
				IPEndPoint result = null;
				if (v4address != null)
				{
					result = IPEndPoint.Any.Create(v4address) as IPEndPoint;
				}
				else if (v6address != null)
				{
					result = IPEndPoint.IPv6Any.Create(v6address) as IPEndPoint;
				}
				return result;
			}

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			private unsafe static void CopyOutAddress(IntPtr address, ref SocketAddress v4address, ref SocketAddress v6address)
			{
				if (address != IntPtr.Zero)
				{
					switch (*(ushort*)(void*)address)
					{
					case 2:
						v6address = null;
						fixed (byte* ptr2 = v4address.m_Buffer)
						{
							for (int j = 2; j < 16; j++)
							{
								ptr2[j] = ((byte*)(void*)address)[j];
							}
						}
						return;
					case 23:
						v4address = null;
						fixed (byte* ptr = v6address.m_Buffer)
						{
							for (int i = 2; i < 28; i++)
							{
								ptr[i] = ((byte*)(void*)address)[i];
							}
						}
						return;
					}
				}
				v4address = null;
				v6address = null;
			}
		}

		private const string KERNEL32 = "kernel32.dll";

		private const string WS2_32 = "ws2_32.dll";

		private const string SECUR32 = "secur32.dll";

		private const string CRYPT32 = "crypt32.dll";

		private const string ADVAPI32 = "advapi32.dll";

		private const string HTTPAPI = "httpapi.dll";

		private const string SCHANNEL = "schannel.dll";

		private const string SECURITY = "security.dll";

		private const string RASAPI32 = "rasapi32.dll";

		private const string WININET = "wininet.dll";

		private const string WINHTTP = "winhttp.dll";

		private const string BCRYPT = "bcrypt.dll";

		[DllImport("kernel32.dll")]
		internal static extern IntPtr CreateSemaphore([In] IntPtr lpSemaphoreAttributes, [In] int lInitialCount, [In] int lMaximumCount, [In] IntPtr lpName);

		[DllImport("kernel32.dll")]
		internal static extern bool ReleaseSemaphore([In] IntPtr hSemaphore, [In] int lReleaseCount, [In] IntPtr lpPreviousCount);

		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal static extern uint GetCurrentThreadId();

		[DllImport("bcrypt.dll")]
		internal static extern uint BCryptGetFipsAlgorithmMode([MarshalAs(UnmanagedType.U1)] out bool pfEnabled);

		[DllImport("kernel32.dll", ExactSpelling = true)]
		internal static extern void DebugBreak();
	}
}
