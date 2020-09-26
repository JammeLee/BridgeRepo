using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Security.AccessControl
{
	internal sealed class Privilege
	{
		private sealed class TlsContents : IDisposable
		{
			private bool disposed;

			private int referenceCount = 1;

			private SafeTokenHandle threadHandle = new SafeTokenHandle(IntPtr.Zero);

			private bool isImpersonating;

			private static SafeTokenHandle processHandle = new SafeTokenHandle(IntPtr.Zero);

			private static readonly object syncRoot = new object();

			public int ReferenceCountValue => referenceCount;

			public SafeTokenHandle ThreadHandle => threadHandle;

			public bool IsImpersonating => isImpersonating;

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			public TlsContents()
			{
				int num = 0;
				int num2 = 0;
				bool flag = true;
				if (processHandle.IsInvalid)
				{
					lock (syncRoot)
					{
						if (processHandle.IsInvalid && !Win32Native.OpenProcessToken(Win32Native.GetCurrentProcess(), TokenAccessLevels.Duplicate, ref processHandle))
						{
							num2 = Marshal.GetLastWin32Error();
							flag = false;
						}
					}
				}
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					try
					{
						SafeTokenHandle safeTokenHandle = threadHandle;
						num = System.Security.Principal.Win32.OpenThreadToken(TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges, WinSecurityContext.Process, out threadHandle);
						num &= 0x7FF8FFFF;
						if (num != 0)
						{
							if (flag)
							{
								threadHandle = safeTokenHandle;
								if (num != 1008)
								{
									flag = false;
								}
								if (flag)
								{
									num = 0;
									if (!Win32Native.DuplicateTokenEx(processHandle, TokenAccessLevels.Impersonate | TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges, IntPtr.Zero, Win32Native.SECURITY_IMPERSONATION_LEVEL.Impersonation, System.Security.Principal.TokenType.TokenImpersonation, ref threadHandle))
									{
										num = Marshal.GetLastWin32Error();
										flag = false;
									}
								}
								if (flag)
								{
									num = System.Security.Principal.Win32.SetThreadToken(threadHandle);
									num &= 0x7FF8FFFF;
									if (num != 0)
									{
										flag = false;
									}
								}
								if (flag)
								{
									isImpersonating = true;
								}
							}
							else
							{
								num = num2;
							}
						}
						else
						{
							flag = true;
						}
					}
					finally
					{
						if (!flag)
						{
							Dispose();
						}
					}
				}
				switch (num)
				{
				case 8:
					throw new OutOfMemoryException();
				case 5:
				case 1347:
					throw new UnauthorizedAccessException();
				default:
					throw new InvalidOperationException();
				case 0:
					break;
				}
			}

			~TlsContents()
			{
				if (!disposed)
				{
					Dispose(disposing: false);
				}
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing)
			{
				if (!disposed)
				{
					if (disposing && threadHandle != null)
					{
						threadHandle.Dispose();
						threadHandle = null;
					}
					if (isImpersonating)
					{
						System.Security.Principal.Win32.RevertToSelf();
					}
					disposed = true;
				}
			}

			public void IncrementReferenceCount()
			{
				referenceCount++;
			}

			public int DecrementReferenceCount()
			{
				int num = --referenceCount;
				if (num == 0)
				{
					Dispose();
				}
				return num;
			}
		}

		public const string CreateToken = "SeCreateTokenPrivilege";

		public const string AssignPrimaryToken = "SeAssignPrimaryTokenPrivilege";

		public const string LockMemory = "SeLockMemoryPrivilege";

		public const string IncreaseQuota = "SeIncreaseQuotaPrivilege";

		public const string UnsolicitedInput = "SeUnsolicitedInputPrivilege";

		public const string MachineAccount = "SeMachineAccountPrivilege";

		public const string TrustedComputingBase = "SeTcbPrivilege";

		public const string Security = "SeSecurityPrivilege";

		public const string TakeOwnership = "SeTakeOwnershipPrivilege";

		public const string LoadDriver = "SeLoadDriverPrivilege";

		public const string SystemProfile = "SeSystemProfilePrivilege";

		public const string SystemTime = "SeSystemtimePrivilege";

		public const string ProfileSingleProcess = "SeProfileSingleProcessPrivilege";

		public const string IncreaseBasePriority = "SeIncreaseBasePriorityPrivilege";

		public const string CreatePageFile = "SeCreatePagefilePrivilege";

		public const string CreatePermanent = "SeCreatePermanentPrivilege";

		public const string Backup = "SeBackupPrivilege";

		public const string Restore = "SeRestorePrivilege";

		public const string Shutdown = "SeShutdownPrivilege";

		public const string Debug = "SeDebugPrivilege";

		public const string Audit = "SeAuditPrivilege";

		public const string SystemEnvironment = "SeSystemEnvironmentPrivilege";

		public const string ChangeNotify = "SeChangeNotifyPrivilege";

		public const string RemoteShutdown = "SeRemoteShutdownPrivilege";

		public const string Undock = "SeUndockPrivilege";

		public const string SyncAgent = "SeSyncAgentPrivilege";

		public const string EnableDelegation = "SeEnableDelegationPrivilege";

		public const string ManageVolume = "SeManageVolumePrivilege";

		public const string Impersonate = "SeImpersonatePrivilege";

		public const string CreateGlobal = "SeCreateGlobalPrivilege";

		public const string TrustedCredentialManagerAccess = "SeTrustedCredManAccessPrivilege";

		public const string ReserveProcessor = "SeReserveProcessorPrivilege";

		private static LocalDataStoreSlot tlsSlot = Thread.AllocateDataSlot();

		private static Hashtable privileges = new Hashtable();

		private static Hashtable luids = new Hashtable();

		private static ReaderWriterLock privilegeLock = new ReaderWriterLock();

		private bool needToRevert;

		private bool initialState;

		private bool stateWasChanged;

		private Win32Native.LUID luid;

		private readonly Thread currentThread = Thread.CurrentThread;

		private TlsContents tlsContents;

		public bool NeedToRevert => needToRevert;

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static Win32Native.LUID LuidFromPrivilege(string privilege)
		{
			Win32Native.LUID Luid = default(Win32Native.LUID);
			Luid.LowPart = 0u;
			Luid.HighPart = 0u;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				privilegeLock.AcquireReaderLock(-1);
				if (luids.Contains(privilege))
				{
					Luid = (Win32Native.LUID)luids[privilege];
					privilegeLock.ReleaseReaderLock();
				}
				else
				{
					privilegeLock.ReleaseReaderLock();
					if (!Win32Native.LookupPrivilegeValue(null, privilege, ref Luid))
					{
						switch (Marshal.GetLastWin32Error())
						{
						case 8:
							throw new OutOfMemoryException();
						case 5:
							throw new UnauthorizedAccessException();
						case 1313:
							throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPrivilegeName", privilege));
						default:
							throw new InvalidOperationException();
						}
					}
					privilegeLock.AcquireWriterLock(-1);
				}
			}
			finally
			{
				if (privilegeLock.IsReaderLockHeld)
				{
					privilegeLock.ReleaseReaderLock();
				}
				if (privilegeLock.IsWriterLockHeld)
				{
					if (!luids.Contains(privilege))
					{
						luids[privilege] = Luid;
						privileges[Luid] = privilege;
					}
					privilegeLock.ReleaseWriterLock();
				}
			}
			return Luid;
		}

		public Privilege(string privilegeName)
		{
			if (!WindowsIdentity.RunningOnWin2K)
			{
				throw new NotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresNT"));
			}
			if (privilegeName == null)
			{
				throw new ArgumentNullException("privilegeName");
			}
			luid = LuidFromPrivilege(privilegeName);
		}

		~Privilege()
		{
			if (needToRevert)
			{
				Revert();
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public void Enable()
		{
			ToggleState(enable: true);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private void ToggleState(bool enable)
		{
			int num = 0;
			if (!currentThread.Equals(Thread.CurrentThread))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustBeSameThread"));
			}
			if (needToRevert)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustRevertPrivilege"));
			}
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				try
				{
					tlsContents = Thread.GetData(tlsSlot) as TlsContents;
					if (tlsContents == null)
					{
						tlsContents = new TlsContents();
						Thread.SetData(tlsSlot, tlsContents);
					}
					else
					{
						tlsContents.IncrementReferenceCount();
					}
					Win32Native.TOKEN_PRIVILEGE NewState = default(Win32Native.TOKEN_PRIVILEGE);
					NewState.PrivilegeCount = 1u;
					NewState.Privilege.Luid = luid;
					NewState.Privilege.Attributes = (enable ? 2u : 0u);
					Win32Native.TOKEN_PRIVILEGE PreviousState = default(Win32Native.TOKEN_PRIVILEGE);
					uint ReturnLength = 0u;
					if (!Win32Native.AdjustTokenPrivileges(tlsContents.ThreadHandle, DisableAllPrivileges: false, ref NewState, (uint)Marshal.SizeOf(PreviousState), ref PreviousState, ref ReturnLength))
					{
						num = Marshal.GetLastWin32Error();
					}
					else if (1300 == Marshal.GetLastWin32Error())
					{
						num = 1300;
					}
					else
					{
						initialState = (PreviousState.Privilege.Attributes & 2) != 0;
						stateWasChanged = initialState != enable;
						needToRevert = tlsContents.IsImpersonating || stateWasChanged;
					}
				}
				finally
				{
					if (!needToRevert)
					{
						Reset();
					}
				}
			}
			switch (num)
			{
			case 1300:
				throw new PrivilegeNotHeldException(privileges[luid] as string);
			case 8:
				throw new OutOfMemoryException();
			case 5:
			case 1347:
				throw new UnauthorizedAccessException();
			default:
				throw new InvalidOperationException();
			case 0:
				break;
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public void Revert()
		{
			int num = 0;
			if (!currentThread.Equals(Thread.CurrentThread))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustBeSameThread"));
			}
			if (!NeedToRevert)
			{
				return;
			}
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				bool flag = true;
				try
				{
					if (stateWasChanged && (tlsContents.ReferenceCountValue > 1 || !tlsContents.IsImpersonating))
					{
						Win32Native.TOKEN_PRIVILEGE NewState = default(Win32Native.TOKEN_PRIVILEGE);
						NewState.PrivilegeCount = 1u;
						NewState.Privilege.Luid = luid;
						NewState.Privilege.Attributes = (initialState ? 2u : 0u);
						Win32Native.TOKEN_PRIVILEGE PreviousState = default(Win32Native.TOKEN_PRIVILEGE);
						uint ReturnLength = 0u;
						if (!Win32Native.AdjustTokenPrivileges(tlsContents.ThreadHandle, DisableAllPrivileges: false, ref NewState, (uint)Marshal.SizeOf(PreviousState), ref PreviousState, ref ReturnLength))
						{
							num = Marshal.GetLastWin32Error();
							flag = false;
						}
					}
				}
				finally
				{
					if (flag)
					{
						Reset();
					}
				}
			}
			switch (num)
			{
			case 8:
				throw new OutOfMemoryException();
			case 5:
				throw new UnauthorizedAccessException();
			default:
				throw new InvalidOperationException();
			case 0:
				break;
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private void Reset()
		{
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				stateWasChanged = false;
				initialState = false;
				needToRevert = false;
				if (tlsContents != null && tlsContents.DecrementReferenceCount() == 0)
				{
					tlsContents = null;
					Thread.SetData(tlsSlot, null);
				}
			}
		}
	}
}
