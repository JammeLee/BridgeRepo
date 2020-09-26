using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32
{
	[SuppressUnmanagedCodeSecurity]
	internal static class Win32Native
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal class OSVERSIONINFO
		{
			internal int OSVersionInfoSize;

			internal int MajorVersion;

			internal int MinorVersion;

			internal int BuildNumber;

			internal int PlatformId;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			internal string CSDVersion;

			internal OSVERSIONINFO()
			{
				OSVersionInfoSize = Marshal.SizeOf(this);
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal class OSVERSIONINFOEX
		{
			internal int OSVersionInfoSize;

			internal int MajorVersion;

			internal int MinorVersion;

			internal int BuildNumber;

			internal int PlatformId;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			internal string CSDVersion;

			internal ushort ServicePackMajor;

			internal ushort ServicePackMinor;

			internal short SuiteMask;

			internal byte ProductType;

			internal byte Reserved;

			public OSVERSIONINFOEX()
			{
				OSVersionInfoSize = Marshal.SizeOf(this);
			}
		}

		internal struct SYSTEM_INFO
		{
			internal int dwOemId;

			internal int dwPageSize;

			internal IntPtr lpMinimumApplicationAddress;

			internal IntPtr lpMaximumApplicationAddress;

			internal IntPtr dwActiveProcessorMask;

			internal int dwNumberOfProcessors;

			internal int dwProcessorType;

			internal int dwAllocationGranularity;

			internal short wProcessorLevel;

			internal short wProcessorRevision;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class SECURITY_ATTRIBUTES
		{
			internal int nLength;

			internal unsafe byte* pSecurityDescriptor = null;

			internal int bInheritHandle;
		}

		[Serializable]
		internal struct WIN32_FILE_ATTRIBUTE_DATA
		{
			internal int fileAttributes;

			internal uint ftCreationTimeLow;

			internal uint ftCreationTimeHigh;

			internal uint ftLastAccessTimeLow;

			internal uint ftLastAccessTimeHigh;

			internal uint ftLastWriteTimeLow;

			internal uint ftLastWriteTimeHigh;

			internal int fileSizeHigh;

			internal int fileSizeLow;
		}

		internal struct FILE_TIME
		{
			internal uint ftTimeLow;

			internal uint ftTimeHigh;

			public FILE_TIME(long fileTime)
			{
				ftTimeLow = (uint)fileTime;
				ftTimeHigh = (uint)(fileTime >> 32);
			}

			public long ToTicks()
			{
				return (long)(((ulong)ftTimeHigh << 32) + ftTimeLow);
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct KERB_S4U_LOGON
		{
			internal uint MessageType;

			internal uint Flags;

			internal UNICODE_INTPTR_STRING ClientUpn;

			internal UNICODE_INTPTR_STRING ClientRealm;
		}

		internal struct LSA_OBJECT_ATTRIBUTES
		{
			internal int Length;

			internal IntPtr RootDirectory;

			internal IntPtr ObjectName;

			internal int Attributes;

			internal IntPtr SecurityDescriptor;

			internal IntPtr SecurityQualityOfService;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct UNICODE_STRING
		{
			internal ushort Length;

			internal ushort MaximumLength;

			[MarshalAs(UnmanagedType.LPWStr)]
			internal string Buffer;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct UNICODE_INTPTR_STRING
		{
			internal ushort Length;

			internal ushort MaxLength;

			internal IntPtr Buffer;

			internal UNICODE_INTPTR_STRING(int length, int maximumLength, IntPtr buffer)
			{
				Length = (ushort)length;
				MaxLength = (ushort)maximumLength;
				Buffer = buffer;
			}
		}

		internal struct LSA_TRANSLATED_NAME
		{
			internal int Use;

			internal UNICODE_INTPTR_STRING Name;

			internal int DomainIndex;
		}

		internal struct LSA_TRANSLATED_SID
		{
			internal int Use;

			internal uint Rid;

			internal int DomainIndex;
		}

		internal struct LSA_TRANSLATED_SID2
		{
			internal int Use;

			internal IntPtr Sid;

			internal int DomainIndex;

			private uint Flags;
		}

		internal struct LSA_TRUST_INFORMATION
		{
			internal UNICODE_INTPTR_STRING Name;

			internal IntPtr Sid;
		}

		internal struct LSA_REFERENCED_DOMAIN_LIST
		{
			internal int Entries;

			internal IntPtr Domains;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct LUID
		{
			internal uint LowPart;

			internal uint HighPart;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct LUID_AND_ATTRIBUTES
		{
			internal LUID Luid;

			internal uint Attributes;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct QUOTA_LIMITS
		{
			internal IntPtr PagedPoolLimit;

			internal IntPtr NonPagedPoolLimit;

			internal IntPtr MinimumWorkingSetSize;

			internal IntPtr MaximumWorkingSetSize;

			internal IntPtr PagefileLimit;

			internal IntPtr TimeLimit;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct SECURITY_LOGON_SESSION_DATA
		{
			internal uint Size;

			internal LUID LogonId;

			internal UNICODE_INTPTR_STRING UserName;

			internal UNICODE_INTPTR_STRING LogonDomain;

			internal UNICODE_INTPTR_STRING AuthenticationPackage;

			internal uint LogonType;

			internal uint Session;

			internal IntPtr Sid;

			internal long LogonTime;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct SID_AND_ATTRIBUTES
		{
			internal IntPtr Sid;

			internal uint Attributes;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TOKEN_GROUPS
		{
			internal uint GroupCount;

			internal SID_AND_ATTRIBUTES Groups;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TOKEN_PRIVILEGE
		{
			internal uint PrivilegeCount;

			internal LUID_AND_ATTRIBUTES Privilege;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TOKEN_SOURCE
		{
			private const int TOKEN_SOURCE_LENGTH = 8;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			internal char[] Name;

			internal LUID SourceIdentifier;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TOKEN_STATISTICS
		{
			internal LUID TokenId;

			internal LUID AuthenticationId;

			internal long ExpirationTime;

			internal uint TokenType;

			internal uint ImpersonationLevel;

			internal uint DynamicCharged;

			internal uint DynamicAvailable;

			internal uint GroupCount;

			internal uint PrivilegeCount;

			internal LUID ModifiedId;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TOKEN_USER
		{
			internal SID_AND_ATTRIBUTES User;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class MEMORYSTATUSEX
		{
			internal int length;

			internal int memoryLoad;

			internal ulong totalPhys;

			internal ulong availPhys;

			internal ulong totalPageFile;

			internal ulong availPageFile;

			internal ulong totalVirtual;

			internal ulong availVirtual;

			internal ulong availExtendedVirtual;

			internal MEMORYSTATUSEX()
			{
				length = Marshal.SizeOf(this);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class MEMORYSTATUS
		{
			internal int length;

			internal int memoryLoad;

			internal uint totalPhys;

			internal uint availPhys;

			internal uint totalPageFile;

			internal uint availPageFile;

			internal uint totalVirtual;

			internal uint availVirtual;

			internal MEMORYSTATUS()
			{
				length = Marshal.SizeOf(this);
			}
		}

		internal struct MEMORY_BASIC_INFORMATION
		{
			internal unsafe void* BaseAddress;

			internal unsafe void* AllocationBase;

			internal uint AllocationProtect;

			internal UIntPtr RegionSize;

			internal uint State;

			internal uint Protect;

			internal uint Type;
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		[BestFitMapping(false)]
		internal class WIN32_FIND_DATA
		{
			internal int dwFileAttributes;

			internal int ftCreationTime_dwLowDateTime;

			internal int ftCreationTime_dwHighDateTime;

			internal int ftLastAccessTime_dwLowDateTime;

			internal int ftLastAccessTime_dwHighDateTime;

			internal int ftLastWriteTime_dwLowDateTime;

			internal int ftLastWriteTime_dwHighDateTime;

			internal int nFileSizeHigh;

			internal int nFileSizeLow;

			internal int dwReserved0;

			internal int dwReserved1;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			internal string cFileName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			internal string cAlternateFileName;
		}

		internal delegate bool ConsoleCtrlHandlerRoutine(int controlType);

		internal struct COORD
		{
			internal short X;

			internal short Y;
		}

		internal struct SMALL_RECT
		{
			internal short Left;

			internal short Top;

			internal short Right;

			internal short Bottom;
		}

		internal struct CONSOLE_SCREEN_BUFFER_INFO
		{
			internal COORD dwSize;

			internal COORD dwCursorPosition;

			internal short wAttributes;

			internal SMALL_RECT srWindow;

			internal COORD dwMaximumWindowSize;
		}

		internal struct CONSOLE_CURSOR_INFO
		{
			internal int dwSize;

			internal bool bVisible;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct KeyEventRecord
		{
			internal bool keyDown;

			internal short repeatCount;

			internal short virtualKeyCode;

			internal short virtualScanCode;

			internal char uChar;

			internal int controlKeyState;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct InputRecord
		{
			internal short eventType;

			internal KeyEventRecord keyEvent;
		}

		[Serializable]
		[Flags]
		internal enum Color : short
		{
			Black = 0x0,
			ForegroundBlue = 0x1,
			ForegroundGreen = 0x2,
			ForegroundRed = 0x4,
			ForegroundYellow = 0x6,
			ForegroundIntensity = 0x8,
			BackgroundBlue = 0x10,
			BackgroundGreen = 0x20,
			BackgroundRed = 0x40,
			BackgroundYellow = 0x60,
			BackgroundIntensity = 0x80,
			ForegroundMask = 0xF,
			BackgroundMask = 0xF0,
			ColorMask = 0xFF
		}

		internal struct CHAR_INFO
		{
			private ushort charData;

			private short attributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class USEROBJECTFLAGS
		{
			internal int fInherit;

			internal int fReserved;

			internal int dwFlags;
		}

		internal enum SECURITY_IMPERSONATION_LEVEL
		{
			Anonymous,
			Identification,
			Impersonation,
			Delegation
		}

		internal const int KEY_QUERY_VALUE = 1;

		internal const int KEY_SET_VALUE = 2;

		internal const int KEY_CREATE_SUB_KEY = 4;

		internal const int KEY_ENUMERATE_SUB_KEYS = 8;

		internal const int KEY_NOTIFY = 16;

		internal const int KEY_CREATE_LINK = 32;

		internal const int KEY_READ = 131097;

		internal const int KEY_WRITE = 131078;

		internal const int REG_NONE = 0;

		internal const int REG_SZ = 1;

		internal const int REG_EXPAND_SZ = 2;

		internal const int REG_BINARY = 3;

		internal const int REG_DWORD = 4;

		internal const int REG_DWORD_LITTLE_ENDIAN = 4;

		internal const int REG_DWORD_BIG_ENDIAN = 5;

		internal const int REG_LINK = 6;

		internal const int REG_MULTI_SZ = 7;

		internal const int REG_RESOURCE_LIST = 8;

		internal const int REG_FULL_RESOURCE_DESCRIPTOR = 9;

		internal const int REG_RESOURCE_REQUIREMENTS_LIST = 10;

		internal const int REG_QWORD = 11;

		internal const int HWND_BROADCAST = 65535;

		internal const int WM_SETTINGCHANGE = 26;

		internal const uint CRYPTPROTECTMEMORY_BLOCK_SIZE = 16u;

		internal const uint CRYPTPROTECTMEMORY_SAME_PROCESS = 0u;

		internal const uint CRYPTPROTECTMEMORY_CROSS_PROCESS = 1u;

		internal const uint CRYPTPROTECTMEMORY_SAME_LOGON = 2u;

		internal const int SECURITY_ANONYMOUS = 0;

		internal const int SECURITY_SQOS_PRESENT = 1048576;

		internal const string MICROSOFT_KERBEROS_NAME = "Kerberos";

		internal const uint ANONYMOUS_LOGON_LUID = 998u;

		internal const int SECURITY_ANONYMOUS_LOGON_RID = 7;

		internal const int SECURITY_AUTHENTICATED_USER_RID = 11;

		internal const int SECURITY_LOCAL_SYSTEM_RID = 18;

		internal const int SECURITY_BUILTIN_DOMAIN_RID = 32;

		internal const int DOMAIN_USER_RID_GUEST = 501;

		internal const uint SE_PRIVILEGE_DISABLED = 0u;

		internal const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1u;

		internal const uint SE_PRIVILEGE_ENABLED = 2u;

		internal const uint SE_PRIVILEGE_USED_FOR_ACCESS = 2147483648u;

		internal const uint SE_GROUP_MANDATORY = 1u;

		internal const uint SE_GROUP_ENABLED_BY_DEFAULT = 2u;

		internal const uint SE_GROUP_ENABLED = 4u;

		internal const uint SE_GROUP_OWNER = 8u;

		internal const uint SE_GROUP_USE_FOR_DENY_ONLY = 16u;

		internal const uint SE_GROUP_LOGON_ID = 3221225472u;

		internal const uint SE_GROUP_RESOURCE = 536870912u;

		internal const uint DUPLICATE_CLOSE_SOURCE = 1u;

		internal const uint DUPLICATE_SAME_ACCESS = 2u;

		internal const uint DUPLICATE_SAME_ATTRIBUTES = 4u;

		internal const int READ_CONTROL = 131072;

		internal const int SYNCHRONIZE = 1048576;

		internal const int STANDARD_RIGHTS_READ = 131072;

		internal const int STANDARD_RIGHTS_WRITE = 131072;

		internal const int SEMAPHORE_MODIFY_STATE = 2;

		internal const int EVENT_MODIFY_STATE = 2;

		internal const int MUTEX_MODIFY_STATE = 1;

		internal const int MUTEX_ALL_ACCESS = 2031617;

		internal const int LMEM_FIXED = 0;

		internal const int LMEM_ZEROINIT = 64;

		internal const int LPTR = 64;

		internal const string KERNEL32 = "kernel32.dll";

		internal const string USER32 = "user32.dll";

		internal const string ADVAPI32 = "advapi32.dll";

		internal const string OLE32 = "ole32.dll";

		internal const string OLEAUT32 = "oleaut32.dll";

		internal const string SHFOLDER = "shfolder.dll";

		internal const string SHIM = "mscoree.dll";

		internal const string CRYPT32 = "crypt32.dll";

		internal const string SECUR32 = "secur32.dll";

		internal const string MSCORWKS = "mscorwks.dll";

		internal const string LSTRCPY = "lstrcpy";

		internal const string LSTRCPYN = "lstrcpyn";

		internal const string LSTRLEN = "lstrlen";

		internal const string LSTRLENA = "lstrlenA";

		internal const string LSTRLENW = "lstrlenW";

		internal const string MOVEMEMORY = "RtlMoveMemory";

		internal const int SEM_FAILCRITICALERRORS = 1;

		internal const int LCMAP_SORTKEY = 1024;

		internal const int FIND_STARTSWITH = 1048576;

		internal const int FIND_ENDSWITH = 2097152;

		internal const int FIND_FROMSTART = 4194304;

		internal const int FIND_FROMEND = 8388608;

		internal const int STD_INPUT_HANDLE = -10;

		internal const int STD_OUTPUT_HANDLE = -11;

		internal const int STD_ERROR_HANDLE = -12;

		internal const int CTRL_C_EVENT = 0;

		internal const int CTRL_BREAK_EVENT = 1;

		internal const int CTRL_CLOSE_EVENT = 2;

		internal const int CTRL_LOGOFF_EVENT = 5;

		internal const int CTRL_SHUTDOWN_EVENT = 6;

		internal const short KEY_EVENT = 1;

		internal const int FILE_TYPE_DISK = 1;

		internal const int FILE_TYPE_CHAR = 2;

		internal const int FILE_TYPE_PIPE = 3;

		internal const int REPLACEFILE_WRITE_THROUGH = 1;

		internal const int REPLACEFILE_IGNORE_MERGE_ERRORS = 2;

		private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;

		private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

		private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;

		internal const int FILE_ATTRIBUTE_READONLY = 1;

		internal const int FILE_ATTRIBUTE_DIRECTORY = 16;

		internal const int FILE_ATTRIBUTE_REPARSE_POINT = 1024;

		internal const int IO_REPARSE_TAG_MOUNT_POINT = -1610612733;

		internal const int PAGE_READWRITE = 4;

		internal const int MEM_COMMIT = 4096;

		internal const int MEM_RESERVE = 8192;

		internal const int MEM_RELEASE = 32768;

		internal const int MEM_FREE = 65536;

		internal const int ERROR_SUCCESS = 0;

		internal const int ERROR_INVALID_FUNCTION = 1;

		internal const int ERROR_FILE_NOT_FOUND = 2;

		internal const int ERROR_PATH_NOT_FOUND = 3;

		internal const int ERROR_ACCESS_DENIED = 5;

		internal const int ERROR_INVALID_HANDLE = 6;

		internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

		internal const int ERROR_INVALID_DATA = 13;

		internal const int ERROR_INVALID_DRIVE = 15;

		internal const int ERROR_NO_MORE_FILES = 18;

		internal const int ERROR_NOT_READY = 21;

		internal const int ERROR_BAD_LENGTH = 24;

		internal const int ERROR_SHARING_VIOLATION = 32;

		internal const int ERROR_NOT_SUPPORTED = 50;

		internal const int ERROR_FILE_EXISTS = 80;

		internal const int ERROR_INVALID_PARAMETER = 87;

		internal const int ERROR_CALL_NOT_IMPLEMENTED = 120;

		internal const int ERROR_INSUFFICIENT_BUFFER = 122;

		internal const int ERROR_INVALID_NAME = 123;

		internal const int ERROR_BAD_PATHNAME = 161;

		internal const int ERROR_ALREADY_EXISTS = 183;

		internal const int ERROR_ENVVAR_NOT_FOUND = 203;

		internal const int ERROR_FILENAME_EXCED_RANGE = 206;

		internal const int ERROR_NO_DATA = 232;

		internal const int ERROR_PIPE_NOT_CONNECTED = 233;

		internal const int ERROR_MORE_DATA = 234;

		internal const int ERROR_OPERATION_ABORTED = 995;

		internal const int ERROR_NO_TOKEN = 1008;

		internal const int ERROR_DLL_INIT_FAILED = 1114;

		internal const int ERROR_NON_ACCOUNT_SID = 1257;

		internal const int ERROR_NOT_ALL_ASSIGNED = 1300;

		internal const int ERROR_UNKNOWN_REVISION = 1305;

		internal const int ERROR_INVALID_OWNER = 1307;

		internal const int ERROR_INVALID_PRIMARY_GROUP = 1308;

		internal const int ERROR_NO_SUCH_PRIVILEGE = 1313;

		internal const int ERROR_PRIVILEGE_NOT_HELD = 1314;

		internal const int ERROR_NONE_MAPPED = 1332;

		internal const int ERROR_INVALID_ACL = 1336;

		internal const int ERROR_INVALID_SID = 1337;

		internal const int ERROR_INVALID_SECURITY_DESCR = 1338;

		internal const int ERROR_BAD_IMPERSONATION_LEVEL = 1346;

		internal const int ERROR_CANT_OPEN_ANONYMOUS = 1347;

		internal const int ERROR_NO_SECURITY_ON_OBJECT = 1350;

		internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789;

		internal const uint STATUS_SUCCESS = 0u;

		internal const uint STATUS_SOME_NOT_MAPPED = 263u;

		internal const uint STATUS_NO_MEMORY = 3221225495u;

		internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 3221225524u;

		internal const uint STATUS_NONE_MAPPED = 3221225587u;

		internal const uint STATUS_INSUFFICIENT_RESOURCES = 3221225626u;

		internal const uint STATUS_ACCESS_DENIED = 3221225506u;

		internal const int INVALID_FILE_SIZE = -1;

		internal const int STATUS_ACCOUNT_RESTRICTION = -1073741714;

		internal const int LCID_SUPPORTED = 2;

		internal const int ENABLE_PROCESSED_INPUT = 1;

		internal const int ENABLE_LINE_INPUT = 2;

		internal const int ENABLE_ECHO_INPUT = 4;

		internal const int VER_PLATFORM_WIN32s = 0;

		internal const int VER_PLATFORM_WIN32_WINDOWS = 1;

		internal const int VER_PLATFORM_WIN32_NT = 2;

		internal const int VER_PLATFORM_WINCE = 3;

		internal const int SHGFP_TYPE_CURRENT = 0;

		internal const int UOI_FLAGS = 1;

		internal const int WSF_VISIBLE = 1;

		internal const int CSIDL_APPDATA = 26;

		internal const int CSIDL_COMMON_APPDATA = 35;

		internal const int CSIDL_LOCAL_APPDATA = 28;

		internal const int CSIDL_COOKIES = 33;

		internal const int CSIDL_FAVORITES = 6;

		internal const int CSIDL_HISTORY = 34;

		internal const int CSIDL_INTERNET_CACHE = 32;

		internal const int CSIDL_PROGRAMS = 2;

		internal const int CSIDL_RECENT = 8;

		internal const int CSIDL_SENDTO = 9;

		internal const int CSIDL_STARTMENU = 11;

		internal const int CSIDL_STARTUP = 7;

		internal const int CSIDL_SYSTEM = 37;

		internal const int CSIDL_TEMPLATES = 21;

		internal const int CSIDL_DESKTOPDIRECTORY = 16;

		internal const int CSIDL_PERSONAL = 5;

		internal const int CSIDL_PROGRAM_FILES = 38;

		internal const int CSIDL_PROGRAM_FILES_COMMON = 43;

		internal const int CSIDL_DESKTOP = 0;

		internal const int CSIDL_DRIVES = 17;

		internal const int CSIDL_MYMUSIC = 13;

		internal const int CSIDL_MYPICTURES = 39;

		internal const int NameSamCompatible = 2;

		internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

		internal static readonly IntPtr NULL = IntPtr.Zero;

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern void SetLastError(int errorCode);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetVersionEx([In][Out] OSVERSIONINFO ver);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetVersionEx([In][Out] OSVERSIONINFOEX ver);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

		[DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Auto)]
		internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);

		internal static string GetMessage(int errorCode)
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			if (FormatMessage(12800, NULL, errorCode, 0, stringBuilder, stringBuilder.Capacity, NULL) != 0)
			{
				return stringBuilder.ToString();
			}
			return Environment.GetResourceString("UnknownError_Num", errorCode);
		}

		[DllImport("kernel32.dll", EntryPoint = "LocalAlloc")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static extern IntPtr LocalAlloc_NoSafeHandle(int uFlags, IntPtr sizetdwBytes);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern SafeLocalAllocHandle LocalAlloc([In] int uFlags, [In] IntPtr sizetdwBytes);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern IntPtr LocalFree(IntPtr handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern void ZeroMemory(IntPtr handle, uint length);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool GlobalMemoryStatusEx([In][Out] MEMORYSTATUSEX buffer);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool GlobalMemoryStatus([In][Out] MEMORYSTATUS buffer);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern IntPtr VirtualQuery(void* address, ref MEMORY_BASIC_INFORMATION buffer, IntPtr sizeOfBuffer);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal unsafe static extern void* VirtualAlloc(void* address, UIntPtr numBytes, int commitOrReserve, int pageProtectionMode);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal unsafe static extern bool VirtualFree(void* address, UIntPtr numBytes, int pageFreeMode);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern uint GetTempPath(int bufferLen, StringBuilder buffer);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern IntPtr lstrcpy(IntPtr dst, string src);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern IntPtr lstrcpy(StringBuilder dst, IntPtr src);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		internal static extern int lstrlen(sbyte[] ptr);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		internal static extern int lstrlen(IntPtr ptr);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		internal static extern int lstrlenA(IntPtr ptr);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern int lstrlenW(IntPtr ptr);

		[DllImport("oleaut32.dll", CharSet = CharSet.Unicode)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static extern IntPtr SysAllocStringLen(string src, int len);

		[DllImport("oleaut32.dll")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern int SysStringLen(IntPtr bstr);

		[DllImport("oleaut32.dll")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern void SysFreeString(IntPtr bstr);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "RtlMoveMemory")]
		internal static extern void CopyMemoryUni(IntPtr pdst, string psrc, IntPtr sizetcb);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "RtlMoveMemory")]
		internal static extern void CopyMemoryUni(StringBuilder pdst, IntPtr psrc, IntPtr sizetcb);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, EntryPoint = "RtlMoveMemory")]
		internal static extern void CopyMemoryAnsi(IntPtr pdst, string psrc, IntPtr sizetcb);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, EntryPoint = "RtlMoveMemory")]
		internal static extern void CopyMemoryAnsi(StringBuilder pdst, IntPtr psrc, IntPtr sizetcb);

		[DllImport("kernel32.dll")]
		internal static extern int GetACP();

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetEvent(SafeWaitHandle handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool ResetEvent(SafeWaitHandle handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] handles, bool bWaitAll, uint dwMilliseconds);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern SafeWaitHandle CreateEvent(SECURITY_ATTRIBUTES lpSecurityAttributes, bool isManualReset, bool initialState, string name);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern SafeWaitHandle OpenEvent(int desiredAccess, bool inheritHandle, string name);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static extern SafeWaitHandle CreateMutex(SECURITY_ATTRIBUTES lpSecurityAttributes, bool initialOwner, string name);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern SafeWaitHandle OpenMutex(int desiredAccess, bool inheritHandle, string name);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static extern bool ReleaseMutex(SafeWaitHandle handle);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetFullPathName([In] char[] path, int numBufferChars, [Out] char[] buffer, IntPtr mustBeZero);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal unsafe static extern int GetFullPathName(char* path, int numBufferChars, char* buffer, IntPtr mustBeZero);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetLongPathName(string path, StringBuilder longPathBuffer, int bufferLength);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetLongPathName([In] char[] path, [Out] char[] longPathBuffer, int bufferLength);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal unsafe static extern int GetLongPathName(char* path, char* longPathBuffer, int bufferLength);

		internal static SafeFileHandle SafeCreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
		{
			SafeFileHandle safeFileHandle = CreateFile(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
			if (!safeFileHandle.IsInvalid)
			{
				int fileType = GetFileType(safeFileHandle);
				if (fileType != 1)
				{
					safeFileHandle.Dispose();
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_FileStreamOnNonFiles"));
				}
			}
			return safeFileHandle;
		}

		internal static SafeFileHandle UnsafeCreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
		{
			return CreateFile(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern SafeFileMappingHandle CreateFileMapping(SafeFileHandle hFile, IntPtr lpAttributes, uint fProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern IntPtr MapViewOfFile(SafeFileMappingHandle handle, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumerOfBytesToMap);

		[DllImport("kernel32.dll", ExactSpelling = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll")]
		internal static extern int GetFileType(SafeFileHandle handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetEndOfFile(SafeFileHandle hFile);

		[DllImport("kernel32.dll", EntryPoint = "SetFilePointer", SetLastError = true)]
		private unsafe static extern int SetFilePointerWin32(SafeFileHandle handle, int lo, int* hi, int origin);

		internal unsafe static long SetFilePointer(SafeFileHandle handle, long offset, SeekOrigin origin, out int hr)
		{
			hr = 0;
			int lo = (int)offset;
			int num = (int)(offset >> 32);
			lo = SetFilePointerWin32(handle, lo, &num, (int)origin);
			if (lo == -1 && (hr = Marshal.GetLastWin32Error()) != 0)
			{
				return -1L;
			}
			return (long)(((ulong)(uint)num << 32) | (uint)lo);
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead_mustBeZero, NativeOverlapped* overlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetDiskFreeSpaceEx(string drive, out long freeBytesForUser, out long totalBytes, out long freeBytes);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetDriveType(string drive);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetVolumeInformation(string drive, StringBuilder volumeName, int volumeNameBufLen, out int volSerialNumber, out int maxFileNameLen, out int fileSystemFlags, StringBuilder fileSystemName, int fileSystemNameBufLen);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetVolumeLabel(string driveLetter, string volumeName);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetWindowsDirectory(StringBuilder sb, int length);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		internal unsafe static extern int LCMapStringW(int lcid, int flags, char* src, int cchSrc, char* target, int cchTarget);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		internal unsafe static extern int FindNLSString(int Locale, int dwFindFlags, char* lpStringSource, int cchSource, char* lpStringValue, int cchValue, IntPtr pcchFound);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetSystemDirectory(StringBuilder sb, int length);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern bool SetFileTime(SafeFileHandle hFile, FILE_TIME* creationTime, FILE_TIME* lastAccessTime, FILE_TIME* lastWriteTime);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int GetFileSize(SafeFileHandle hFile, out int highSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool LockFile(SafeFileHandle handle, int offsetLow, int offsetHigh, int countLow, int countHigh);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool UnlockFile(SafeFileHandle handle, int offsetLow, int offsetHigh, int countLow, int countHigh);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr GetStdHandle(int nStdHandle);

		internal static int MakeHRFromErrorCode(int errorCode)
		{
			return -2147024896 | errorCode;
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool CopyFile(string src, string dst, bool failIfExists);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool CreateDirectory(string path, SECURITY_ATTRIBUTES lpSecurityAttributes);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool DeleteFile(string path);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool ReplaceFile(string replacedFileName, string replacementFileName, string backupFileName, int dwReplaceFlags, IntPtr lpExclude, IntPtr lpReserved);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool DecryptFile(string path, int reservedMustBeZero);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool EncryptFile(string path);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern SafeFindHandle FindFirstFile(string fileName, [In][Out] WIN32_FIND_DATA data);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool FindNextFile(SafeFindHandle hndFindFile, [In][Out][MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern bool FindClose(IntPtr handle);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetCurrentDirectory(int nBufferLength, StringBuilder lpBuffer);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetFileAttributes(string name, int attr);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int GetLogicalDrives();

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern uint GetTempFileName(string tmpPath, string prefix, uint uniqueIdOrZero, StringBuilder tmpFileName);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool MoveFile(string src, string dst);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool DeleteVolumeMountPoint(string mountPoint);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool RemoveDirectory(string path);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetCurrentDirectory(string path);

		[DllImport("kernel32.dll")]
		internal static extern int SetErrorMode(int newMode);

		[DllImport("kernel32.dll")]
		internal unsafe static extern int WideCharToMultiByte(uint cp, uint flags, char* pwzSource, int cchSource, byte* pbDestBuffer, int cbDestBuffer, IntPtr null1, IntPtr null2);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerRoutine handler, bool addOrRemove);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetEnvironmentVariable(string lpName, string lpValue);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetEnvironmentVariable(string lpName, StringBuilder lpValue, int size);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern uint GetCurrentProcessId();

		[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
		internal static extern bool GetUserName(StringBuilder lpBuffer, ref int nSize);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int GetComputerName(StringBuilder nameBuffer, ref int bufferSize);

		[DllImport("ole32.dll")]
		internal static extern IntPtr CoTaskMemAlloc(int cb);

		[DllImport("ole32.dll")]
		internal static extern IntPtr CoTaskMemRealloc(IntPtr pv, int cb);

		[DllImport("ole32.dll")]
		internal static extern void CoTaskMemFree(IntPtr ptr);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool Beep(int frequency, int duration);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, COORD size);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern COORD GetLargestConsoleWindowSize(IntPtr hConsoleOutput);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool FillConsoleOutputCharacter(IntPtr hConsoleOutput, char character, int nLength, COORD dwWriteCoord, out int pNumCharsWritten);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool FillConsoleOutputAttribute(IntPtr hConsoleOutput, short wColorAttribute, int numCells, COORD startCoord, out int pNumBytesWritten);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern bool SetConsoleWindowInfo(IntPtr hConsoleOutput, bool absolute, SMALL_RECT* consoleWindow);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, short attributes);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD cursorPosition);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool GetConsoleCursorInfo(IntPtr hConsoleOutput, out CONSOLE_CURSOR_INFO cci);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetConsoleCursorInfo(IntPtr hConsoleOutput, ref CONSOLE_CURSOR_INFO cci);

		[DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetConsoleTitle(StringBuilder sb, int capacity);

		[DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetConsoleTitle(string title);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool ReadConsoleInput(IntPtr hConsoleInput, out InputRecord buffer, int numInputRecords_UseOne, out int numEventsRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool PeekConsoleInput(IntPtr hConsoleInput, out InputRecord buffer, int numInputRecords_UseOne, out int numEventsRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern bool ReadConsoleOutput(IntPtr hConsoleOutput, CHAR_INFO* pBuffer, COORD bufferSize, COORD bufferCoord, ref SMALL_RECT readRegion);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal unsafe static extern bool WriteConsoleOutput(IntPtr hConsoleOutput, CHAR_INFO* buffer, COORD bufferSize, COORD bufferCoord, ref SMALL_RECT writeRegion);

		[DllImport("user32.dll")]
		internal static extern short GetKeyState(int virtualKeyCode);

		[DllImport("kernel32.dll")]
		internal static extern uint GetConsoleCP();

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetConsoleCP(uint codePage);

		[DllImport("kernel32.dll")]
		internal static extern uint GetConsoleOutputCP();

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool SetConsoleOutputCP(uint codePage);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegConnectRegistry(string machineName, SafeRegistryHandle key, out SafeRegistryHandle result);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegCreateKeyEx(SafeRegistryHandle hKey, string lpSubKey, int Reserved, string lpClass, int dwOptions, int samDesigner, SECURITY_ATTRIBUTES lpSecurityAttributes, out SafeRegistryHandle hkResult, out int lpdwDisposition);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegDeleteKey(SafeRegistryHandle hKey, string lpSubKey);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegDeleteValue(SafeRegistryHandle hKey, string lpValueName);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegEnumKeyEx(SafeRegistryHandle hKey, int dwIndex, StringBuilder lpName, out int lpcbName, int[] lpReserved, StringBuilder lpClass, int[] lpcbClass, long[] lpftLastWriteTime);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegEnumValue(SafeRegistryHandle hKey, int dwIndex, StringBuilder lpValueName, ref int lpcbValueName, IntPtr lpReserved_MustBeZero, int[] lpType, byte[] lpData, int[] lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Ansi)]
		internal static extern int RegEnumValueA(SafeRegistryHandle hKey, int dwIndex, StringBuilder lpValueName, ref int lpcbValueName, IntPtr lpReserved_MustBeZero, int[] lpType, byte[] lpData, int[] lpcbData);

		[DllImport("advapi32.dll")]
		internal static extern int RegFlushKey(SafeRegistryHandle hKey);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegOpenKeyEx(SafeRegistryHandle hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegQueryInfoKey(SafeRegistryHandle hKey, StringBuilder lpClass, int[] lpcbClass, IntPtr lpReserved_MustBeZero, ref int lpcSubKeys, int[] lpcbMaxSubKeyLen, int[] lpcbMaxClassLen, ref int lpcValues, int[] lpcbMaxValueNameLen, int[] lpcbMaxValueLen, int[] lpcbSecurityDescriptor, int[] lpftLastWriteTime);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] byte[] lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, ref int lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, ref long lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] char[] lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, StringBuilder lpData, ref int lpcbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, byte[] lpData, int cbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, ref int lpData, int cbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, ref long lpData, int cbData);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int RegSetValueEx(SafeRegistryHandle hKey, string lpValueName, int Reserved, RegistryValueKind dwType, string lpData, int cbData);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int ExpandEnvironmentStrings(string lpSrc, StringBuilder lpDst, int nSize);

		[DllImport("kernel32.dll")]
		internal static extern IntPtr LocalReAlloc(IntPtr handle, IntPtr sizetcbBytes, int uFlags);

		[DllImport("shfolder.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

		[DllImport("secur32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern byte GetUserNameEx(int format, StringBuilder domainName, ref int domainNameLen);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool LookupAccountName(string machineName, string accountName, byte[] sid, ref int sidLen, StringBuilder domainName, ref int domainNameLen, out int peUse);

		[DllImport("user32.dll", ExactSpelling = true)]
		internal static extern IntPtr GetProcessWindowStation();

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, [MarshalAs(UnmanagedType.LPStruct)] USEROBJECTFLAGS pvBuffer, int nLength, ref int lpnLengthNeeded);

		[DllImport("user32.dll", BestFitMapping = false, SetLastError = true)]
		internal static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, IntPtr lpdwResult);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern int SystemFunction040([In][Out] SafeBSTRHandle pDataIn, [In] uint cbDataIn, [In] uint dwFlags);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SystemFunction041([In][Out] SafeBSTRHandle pDataIn, [In] uint cbDataIn, [In] uint dwFlags);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int LsaNtStatusToWinError([In] int status);

		[DllImport("bcrypt.dll")]
		internal static extern uint BCryptGetFipsAlgorithmMode([MarshalAs(UnmanagedType.U1)] out bool pfEnabled);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static extern bool AdjustTokenPrivileges([In] SafeTokenHandle TokenHandle, [In] bool DisableAllPrivileges, [In] ref TOKEN_PRIVILEGE NewState, [In] uint BufferLength, [In][Out] ref TOKEN_PRIVILEGE PreviousState, [In][Out] ref uint ReturnLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool AllocateLocallyUniqueId([In][Out] ref LUID Luid);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool CheckTokenMembership([In] SafeTokenHandle TokenHandle, [In] byte[] SidToCheck, [In][Out] ref bool IsMember);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "ConvertSecurityDescriptorToStringSecurityDescriptorW", SetLastError = true)]
		internal static extern int ConvertSdToStringSd(byte[] securityDescriptor, uint requestedRevision, uint securityInformation, out IntPtr resultString, ref uint resultStringLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "ConvertStringSecurityDescriptorToSecurityDescriptorW", SetLastError = true)]
		internal static extern int ConvertStringSdToSd(string stringSd, uint stringSdRevision, out IntPtr resultSd, ref uint resultSdLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "ConvertStringSidToSidW", SetLastError = true)]
		internal static extern int ConvertStringSidToSid(string stringSid, out IntPtr ByteArray);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int CreateWellKnownSid(int sidType, byte[] domainSid, [Out] byte[] resultSid, ref uint resultSidLength);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool DuplicateHandle([In] IntPtr hSourceProcessHandle, [In] IntPtr hSourceHandle, [In] IntPtr hTargetProcessHandle, [In][Out] ref SafeTokenHandle lpTargetHandle, [In] uint dwDesiredAccess, [In] bool bInheritHandle, [In] uint dwOptions);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static extern bool DuplicateHandle([In] IntPtr hSourceProcessHandle, [In] SafeTokenHandle hSourceHandle, [In] IntPtr hTargetProcessHandle, [In][Out] ref SafeTokenHandle lpTargetHandle, [In] uint dwDesiredAccess, [In] bool bInheritHandle, [In] uint dwOptions);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static extern bool DuplicateTokenEx([In] SafeTokenHandle ExistingTokenHandle, [In] TokenAccessLevels DesiredAccess, [In] IntPtr TokenAttributes, [In] SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, [In] System.Security.Principal.TokenType TokenType, [In][Out] ref SafeTokenHandle DuplicateTokenHandle);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool DuplicateTokenEx([In] SafeTokenHandle hExistingToken, [In] uint dwDesiredAccess, [In] IntPtr lpTokenAttributes, [In] uint ImpersonationLevel, [In] uint TokenType, [In][Out] ref SafeTokenHandle phNewToken);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "EqualDomainSid", SetLastError = true)]
		internal static extern int IsEqualDomainSid(byte[] sid1, byte[] sid2, out bool result);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr GetCurrentProcess();

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint GetSecurityDescriptorLength(IntPtr byteArray);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetSecurityInfo", SetLastError = true)]
		internal static extern uint GetSecurityInfoByHandle(SafeHandle handle, uint objectType, uint securityInformation, out IntPtr sidOwner, out IntPtr sidGroup, out IntPtr dacl, out IntPtr sacl, out IntPtr securityDescriptor);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetNamedSecurityInfoW", SetLastError = true)]
		internal static extern uint GetSecurityInfoByName(string name, uint objectType, uint securityInformation, out IntPtr sidOwner, out IntPtr sidGroup, out IntPtr dacl, out IntPtr sacl, out IntPtr securityDescriptor);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetTokenInformation([In] IntPtr TokenHandle, [In] uint TokenInformationClass, [In] SafeLocalAllocHandle TokenInformation, [In] uint TokenInformationLength, out uint ReturnLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetTokenInformation([In] SafeTokenHandle TokenHandle, [In] uint TokenInformationClass, [In] SafeLocalAllocHandle TokenInformation, [In] uint TokenInformationLength, out uint ReturnLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int GetWindowsAccountDomainSid(byte[] sid, [Out] byte[] resultSid, ref uint resultSidLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int IsWellKnownSid(byte[] sid, int type);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint LsaOpenPolicy(string systemName, ref LSA_OBJECT_ATTRIBUTES attributes, int accessMask, out SafeLsaPolicyHandle handle);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, EntryPoint = "LookupPrivilegeValueW", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static extern bool LookupPrivilegeValue([In] string lpSystemName, [In] string lpName, [In][Out] ref LUID Luid);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint LsaLookupSids(SafeLsaPolicyHandle handle, int count, IntPtr[] sids, ref SafeLsaMemoryHandle referencedDomains, ref SafeLsaMemoryHandle names);

		[DllImport("advapi32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern int LsaFreeMemory(IntPtr handle);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint LsaLookupNames(SafeLsaPolicyHandle handle, int count, UNICODE_STRING[] names, ref SafeLsaMemoryHandle referencedDomains, ref SafeLsaMemoryHandle sids);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint LsaLookupNames2(SafeLsaPolicyHandle handle, int flags, int count, UNICODE_STRING[] names, ref SafeLsaMemoryHandle referencedDomains, ref SafeLsaMemoryHandle sids);

		[DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int LsaConnectUntrusted([In][Out] ref SafeLsaLogonProcessHandle LsaHandle);

		[DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int LsaGetLogonSessionData([In] ref LUID LogonId, [In][Out] ref SafeLsaReturnBufferHandle ppLogonSessionData);

		[DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int LsaLogonUser([In] SafeLsaLogonProcessHandle LsaHandle, [In] ref UNICODE_INTPTR_STRING OriginName, [In] uint LogonType, [In] uint AuthenticationPackage, [In] IntPtr AuthenticationInformation, [In] uint AuthenticationInformationLength, [In] IntPtr LocalGroups, [In] ref TOKEN_SOURCE SourceContext, [In][Out] ref SafeLsaReturnBufferHandle ProfileBuffer, [In][Out] ref uint ProfileBufferLength, [In][Out] ref LUID LogonId, [In][Out] ref SafeTokenHandle Token, [In][Out] ref QUOTA_LIMITS Quotas, [In][Out] ref int SubStatus);

		[DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int LsaLookupAuthenticationPackage([In] SafeLsaLogonProcessHandle LsaHandle, [In] ref UNICODE_INTPTR_STRING PackageName, [In][Out] ref uint AuthenticationPackage);

		[DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int LsaRegisterLogonProcess([In] ref UNICODE_INTPTR_STRING LogonProcessName, [In][Out] ref SafeLsaLogonProcessHandle LsaHandle, [In][Out] ref IntPtr SecurityMode);

		[DllImport("secur32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern int LsaDeregisterLogonProcess(IntPtr handle);

		[DllImport("advapi32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern int LsaClose(IntPtr handle);

		[DllImport("secur32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern int LsaFreeReturnBuffer(IntPtr handle);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool OpenProcessToken([In] IntPtr ProcessToken, [In] TokenAccessLevels DesiredAccess, [In][Out] ref SafeTokenHandle TokenHandle);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetNamedSecurityInfoW", SetLastError = true)]
		internal static extern uint SetSecurityInfoByName(string name, uint objectType, uint securityInformation, byte[] owner, byte[] group, byte[] dacl, byte[] sacl);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetSecurityInfo", SetLastError = true)]
		internal static extern uint SetSecurityInfoByHandle(SafeHandle handle, uint objectType, uint securityInformation, byte[] owner, byte[] group, byte[] dacl, byte[] sacl);

		[DllImport("mscorwks.dll", CharSet = CharSet.Unicode)]
		internal static extern int CreateAssemblyNameObject(out IAssemblyName ppEnum, string szAssemblyName, uint dwFlags, IntPtr pvReserved);

		[DllImport("mscorwks.dll", CharSet = CharSet.Auto)]
		internal static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum, IApplicationContext pAppCtx, IAssemblyName pName, uint dwFlags, IntPtr pvReserved);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int GetCalendarInfo(int Locale, int Calendar, int CalType, StringBuilder lpCalData, int cchData, IntPtr lpValue);
	}
}
