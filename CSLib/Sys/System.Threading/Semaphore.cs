using System.IO.Ports;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public sealed class Semaphore : WaitHandle
	{
		private static int MAX_PATH = 260;

		public Semaphore(int initialCount, int maximumCount)
			: this(initialCount, maximumCount, null)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Semaphore(int initialCount, int maximumCount, string name)
		{
			if (initialCount < 0)
			{
				throw new ArgumentOutOfRangeException("initialCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
			}
			if (maximumCount < 1)
			{
				throw new ArgumentOutOfRangeException("maximumCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
			}
			if (initialCount > maximumCount)
			{
				throw new ArgumentException(SR.GetString("Argument_SemaphoreInitialMaximum"));
			}
			if (name != null && MAX_PATH < name.Length)
			{
				throw new ArgumentException(SR.GetString("Argument_WaitHandleNameTooLong"));
			}
			SafeWaitHandle safeWaitHandle = Microsoft.Win32.SafeNativeMethods.CreateSemaphore(null, initialCount, maximumCount, name);
			if (safeWaitHandle.IsInvalid)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (name != null && name.Length != 0 && 6 == lastWin32Error)
				{
					throw new WaitHandleCannotBeOpenedException(SR.GetString("WaitHandleCannotBeOpenedException_InvalidHandle", name));
				}
				InternalResources.WinIOError();
			}
			base.SafeWaitHandle = safeWaitHandle;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Semaphore(int initialCount, int maximumCount, string name, out bool createdNew)
			: this(initialCount, maximumCount, name, out createdNew, null)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public unsafe Semaphore(int initialCount, int maximumCount, string name, out bool createdNew, SemaphoreSecurity semaphoreSecurity)
		{
			if (initialCount < 0)
			{
				throw new ArgumentOutOfRangeException("initialCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
			}
			if (maximumCount < 1)
			{
				throw new ArgumentOutOfRangeException("maximumCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
			}
			if (initialCount > maximumCount)
			{
				throw new ArgumentException(SR.GetString("Argument_SemaphoreInitialMaximum"));
			}
			if (name != null && MAX_PATH < name.Length)
			{
				throw new ArgumentException(SR.GetString("Argument_WaitHandleNameTooLong"));
			}
			SafeWaitHandle safeWaitHandle;
			if (semaphoreSecurity != null)
			{
				NativeMethods.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = null;
				sECURITY_ATTRIBUTES = new NativeMethods.SECURITY_ATTRIBUTES();
				sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
				fixed (byte* value = semaphoreSecurity.GetSecurityDescriptorBinaryForm())
				{
					sECURITY_ATTRIBUTES.lpSecurityDescriptor = new SafeLocalMemHandle((IntPtr)value, ownsHandle: false);
					safeWaitHandle = Microsoft.Win32.SafeNativeMethods.CreateSemaphore(sECURITY_ATTRIBUTES, initialCount, maximumCount, name);
				}
			}
			else
			{
				safeWaitHandle = Microsoft.Win32.SafeNativeMethods.CreateSemaphore(null, initialCount, maximumCount, name);
			}
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (safeWaitHandle.IsInvalid)
			{
				if (name != null && name.Length != 0 && 6 == lastWin32Error)
				{
					throw new WaitHandleCannotBeOpenedException(SR.GetString("WaitHandleCannotBeOpenedException_InvalidHandle", name));
				}
				InternalResources.WinIOError();
			}
			createdNew = lastWin32Error != 183;
			base.SafeWaitHandle = safeWaitHandle;
		}

		private Semaphore(SafeWaitHandle handle)
		{
			base.SafeWaitHandle = handle;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Semaphore OpenExisting(string name)
		{
			return OpenExisting(name, SemaphoreRights.Modify | SemaphoreRights.Synchronize);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Semaphore OpenExisting(string name, SemaphoreRights rights)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", "name"), "name");
			}
			if (name != null && MAX_PATH < name.Length)
			{
				throw new ArgumentException(SR.GetString("Argument_WaitHandleNameTooLong"));
			}
			SafeWaitHandle safeWaitHandle = Microsoft.Win32.SafeNativeMethods.OpenSemaphore((int)rights, inheritHandle: false, name);
			if (safeWaitHandle.IsInvalid)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (2 == lastWin32Error || 123 == lastWin32Error)
				{
					throw new WaitHandleCannotBeOpenedException();
				}
				if (name != null && name.Length != 0 && 6 == lastWin32Error)
				{
					throw new WaitHandleCannotBeOpenedException(SR.GetString("WaitHandleCannotBeOpenedException_InvalidHandle", name));
				}
				InternalResources.WinIOError();
			}
			return new Semaphore(safeWaitHandle);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[PrePrepareMethod]
		public int Release()
		{
			return Release(1);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public int Release(int releaseCount)
		{
			if (releaseCount < 1)
			{
				throw new ArgumentOutOfRangeException("releaseCount", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
			}
			if (!Microsoft.Win32.SafeNativeMethods.ReleaseSemaphore(base.SafeWaitHandle, releaseCount, out var previousCount))
			{
				throw new SemaphoreFullException();
			}
			return previousCount;
		}

		public SemaphoreSecurity GetAccessControl()
		{
			return new SemaphoreSecurity(base.SafeWaitHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public void SetAccessControl(SemaphoreSecurity semaphoreSecurity)
		{
			if (semaphoreSecurity == null)
			{
				throw new ArgumentNullException("semaphoreSecurity");
			}
			semaphoreSecurity.Persist(base.SafeWaitHandle);
		}
	}
}
