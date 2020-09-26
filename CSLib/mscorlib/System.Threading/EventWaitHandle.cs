using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
	[ComVisible(true)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public class EventWaitHandle : WaitHandle
	{
		public EventWaitHandle(bool initialState, EventResetMode mode)
			: this(initialState, mode, null)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public EventWaitHandle(bool initialState, EventResetMode mode, string name)
		{
			if (name != null && 260 < name.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", name));
			}
			SafeWaitHandle safeWaitHandle = null;
			safeWaitHandle = mode switch
			{
				EventResetMode.ManualReset => Win32Native.CreateEvent(null, isManualReset: true, initialState, name), 
				EventResetMode.AutoReset => Win32Native.CreateEvent(null, isManualReset: false, initialState, name), 
				_ => throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag", name)), 
			};
			if (safeWaitHandle.IsInvalid)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				safeWaitHandle.SetHandleAsInvalid();
				if (name != null && name.Length != 0 && 6 == lastWin32Error)
				{
					throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name));
				}
				__Error.WinIOError(lastWin32Error, "");
			}
			SetHandleInternal(safeWaitHandle);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public EventWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew)
			: this(initialState, mode, name, out createdNew, null)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public unsafe EventWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew, EventWaitHandleSecurity eventSecurity)
		{
			if (name != null && 260 < name.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", name));
			}
			Win32Native.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = null;
			if (eventSecurity != null)
			{
				sECURITY_ATTRIBUTES = new Win32Native.SECURITY_ATTRIBUTES();
				sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
				byte[] securityDescriptorBinaryForm = eventSecurity.GetSecurityDescriptorBinaryForm();
				byte* ptr = stackalloc byte[1 * securityDescriptorBinaryForm.Length];
				Buffer.memcpy(securityDescriptorBinaryForm, 0, ptr, 0, securityDescriptorBinaryForm.Length);
				sECURITY_ATTRIBUTES.pSecurityDescriptor = ptr;
			}
			SafeWaitHandle safeWaitHandle = null;
			safeWaitHandle = Win32Native.CreateEvent(sECURITY_ATTRIBUTES, mode switch
			{
				EventResetMode.ManualReset => true, 
				EventResetMode.AutoReset => false, 
				_ => throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag", name)), 
			}, initialState, name);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (safeWaitHandle.IsInvalid)
			{
				safeWaitHandle.SetHandleAsInvalid();
				if (name != null && name.Length != 0 && 6 == lastWin32Error)
				{
					throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name));
				}
				__Error.WinIOError(lastWin32Error, name);
			}
			createdNew = lastWin32Error != 183;
			SetHandleInternal(safeWaitHandle);
		}

		private EventWaitHandle(SafeWaitHandle handle)
		{
			SetHandleInternal(handle);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static EventWaitHandle OpenExisting(string name)
		{
			return OpenExisting(name, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static EventWaitHandle OpenExisting(string name, EventWaitHandleRights rights)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name", Environment.GetResourceString("ArgumentNull_WithParamName"));
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			if (name != null && 260 < name.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", name));
			}
			SafeWaitHandle safeWaitHandle = Win32Native.OpenEvent((int)rights, inheritHandle: false, name);
			if (safeWaitHandle.IsInvalid)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (2 == lastWin32Error || 123 == lastWin32Error)
				{
					throw new WaitHandleCannotBeOpenedException();
				}
				if (name != null && name.Length != 0 && 6 == lastWin32Error)
				{
					throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name));
				}
				__Error.WinIOError(lastWin32Error, "");
			}
			return new EventWaitHandle(safeWaitHandle);
		}

		public bool Reset()
		{
			bool flag = Win32Native.ResetEvent(safeWaitHandle);
			if (!flag)
			{
				__Error.WinIOError();
			}
			return flag;
		}

		public bool Set()
		{
			bool flag = Win32Native.SetEvent(safeWaitHandle);
			if (!flag)
			{
				__Error.WinIOError();
			}
			return flag;
		}

		public EventWaitHandleSecurity GetAccessControl()
		{
			return new EventWaitHandleSecurity(safeWaitHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public void SetAccessControl(EventWaitHandleSecurity eventSecurity)
		{
			if (eventSecurity == null)
			{
				throw new ArgumentNullException("eventSecurity");
			}
			eventSecurity.Persist(safeWaitHandle);
		}
	}
}
