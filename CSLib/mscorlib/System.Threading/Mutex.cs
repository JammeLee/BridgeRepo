using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
	[ComVisible(true)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public sealed class Mutex : WaitHandle
	{
		internal class MutexCleanupInfo
		{
			internal SafeWaitHandle mutexHandle;

			internal bool inCriticalRegion;

			internal MutexCleanupInfo(SafeWaitHandle mutexHandle, bool inCriticalRegion)
			{
				this.mutexHandle = mutexHandle;
				this.inCriticalRegion = inCriticalRegion;
			}
		}

		private const int WAIT_OBJECT_0 = 0;

		private const int WAIT_ABANDONED_0 = 128;

		private const uint WAIT_FAILED = uint.MaxValue;

		private static bool dummyBool;

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Mutex(bool initiallyOwned, string name, out bool createdNew)
			: this(initiallyOwned, name, out createdNew, null)
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public unsafe Mutex(bool initiallyOwned, string name, out bool createdNew, MutexSecurity mutexSecurity)
		{
			bool initiallyOwned2 = initiallyOwned;
			string name2 = name;
			base._002Ector();
			bool initiallyOwned3 = initiallyOwned;
			string name3 = name;
			if (name2 != null && 260 < name2.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", name2));
			}
			Win32Native.SECURITY_ATTRIBUTES secAttrs = null;
			if (mutexSecurity != null)
			{
				secAttrs = new Win32Native.SECURITY_ATTRIBUTES();
				secAttrs.nLength = Marshal.SizeOf(secAttrs);
				byte[] securityDescriptorBinaryForm = mutexSecurity.GetSecurityDescriptorBinaryForm();
				byte* ptr = stackalloc byte[1 * securityDescriptorBinaryForm.Length];
				Buffer.memcpy(securityDescriptorBinaryForm, 0, ptr, 0, securityDescriptorBinaryForm.Length);
				secAttrs.pSecurityDescriptor = ptr;
			}
			SafeWaitHandle mutexHandle = null;
			bool newMutex = false;
			RuntimeHelpers.CleanupCode backoutCode = MutexCleanupCode;
			MutexCleanupInfo cleanupInfo = new MutexCleanupInfo(mutexHandle, inCriticalRegion: false);
			RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(delegate
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					if (initiallyOwned2)
					{
						cleanupInfo.inCriticalRegion = true;
						Thread.BeginThreadAffinity();
						Thread.BeginCriticalRegion();
					}
				}
				int num = 0;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					num = CreateMutexHandle(initiallyOwned2, name2, secAttrs, out mutexHandle);
				}
				if (mutexHandle.IsInvalid)
				{
					mutexHandle.SetHandleAsInvalid();
					if (name2 != null && name2.Length != 0 && 6 == num)
					{
						throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name2));
					}
					__Error.WinIOError(num, name2);
				}
				newMutex = num != 183;
				SetHandleInternal(mutexHandle);
				hasThreadAffinity = true;
			}, backoutCode, cleanupInfo);
			createdNew = newMutex;
		}

		[PrePrepareMethod]
		private void MutexCleanupCode(object userData, bool exceptionThrown)
		{
			MutexCleanupInfo mutexCleanupInfo = (MutexCleanupInfo)userData;
			if (hasThreadAffinity)
			{
				return;
			}
			if (mutexCleanupInfo.mutexHandle != null && !mutexCleanupInfo.mutexHandle.IsInvalid)
			{
				if (mutexCleanupInfo.inCriticalRegion)
				{
					Win32Native.ReleaseMutex(mutexCleanupInfo.mutexHandle);
				}
				mutexCleanupInfo.mutexHandle.Dispose();
			}
			if (mutexCleanupInfo.inCriticalRegion)
			{
				Thread.EndCriticalRegion();
				Thread.EndThreadAffinity();
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Mutex(bool initiallyOwned, string name)
			: this(initiallyOwned, name, out dummyBool)
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public Mutex(bool initiallyOwned)
			: this(initiallyOwned, null, out dummyBool)
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public Mutex()
			: this(initiallyOwned: false, null, out dummyBool)
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private Mutex(SafeWaitHandle handle)
		{
			SetHandleInternal(handle);
			hasThreadAffinity = true;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Mutex OpenExisting(string name)
		{
			return OpenExisting(name, MutexRights.Modify | MutexRights.Synchronize);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Mutex OpenExisting(string name, MutexRights rights)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name", Environment.GetResourceString("ArgumentNull_WithParamName"));
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			if (260 < name.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", name));
			}
			SafeWaitHandle safeWaitHandle = Win32Native.OpenMutex((int)rights, inheritHandle: false, name);
			int num = 0;
			if (safeWaitHandle.IsInvalid)
			{
				num = Marshal.GetLastWin32Error();
				if (2 == num || 123 == num)
				{
					throw new WaitHandleCannotBeOpenedException();
				}
				if (name != null && name.Length != 0 && 6 == num)
				{
					throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name));
				}
				__Error.WinIOError(num, name);
			}
			return new Mutex(safeWaitHandle);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public void ReleaseMutex()
		{
			if (Win32Native.ReleaseMutex(safeWaitHandle))
			{
				Thread.EndCriticalRegion();
				Thread.EndThreadAffinity();
				return;
			}
			throw new ApplicationException(Environment.GetResourceString("Arg_SynchronizationLockException"));
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static int CreateMutexHandle(bool initiallyOwned, string name, Win32Native.SECURITY_ATTRIBUTES securityAttribute, out SafeWaitHandle mutexHandle)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			int num;
			while (true)
			{
				flag2 = false;
				flag3 = false;
				mutexHandle = Win32Native.CreateMutex(securityAttribute, initiallyOwned, name);
				num = Marshal.GetLastWin32Error();
				if (!mutexHandle.IsInvalid || num != 5)
				{
					break;
				}
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						Thread.BeginThreadAffinity();
						flag = true;
					}
					mutexHandle = Win32Native.OpenMutex(1048577, inheritHandle: false, name);
					if (!mutexHandle.IsInvalid)
					{
						num = 183;
						if (Environment.IsW2k3)
						{
							SafeWaitHandle safeWaitHandle = Win32Native.OpenMutex(1048577, inheritHandle: false, name);
							if (!safeWaitHandle.IsInvalid)
							{
								RuntimeHelpers.PrepareConstrainedRegions();
								try
								{
									uint num2 = 0u;
									IntPtr intPtr = mutexHandle.DangerousGetHandle();
									IntPtr intPtr2 = safeWaitHandle.DangerousGetHandle();
									IntPtr[] array = new IntPtr[2]
									{
										intPtr,
										intPtr2
									};
									num2 = Win32Native.WaitForMultipleObjects(2u, array, bWaitAll: true, 0u);
									GC.KeepAlive(array);
									if (num2 == uint.MaxValue)
									{
										uint lastWin32Error = (uint)Marshal.GetLastWin32Error();
										if (lastWin32Error != 87)
										{
											mutexHandle.Dispose();
											flag3 = true;
										}
									}
									else
									{
										flag2 = true;
										if (num2 >= 0 && num2 < 2)
										{
											Win32Native.ReleaseMutex(mutexHandle);
											Win32Native.ReleaseMutex(safeWaitHandle);
										}
										else if (num2 >= 128 && num2 < 130)
										{
											Win32Native.ReleaseMutex(mutexHandle);
											Win32Native.ReleaseMutex(safeWaitHandle);
										}
										mutexHandle.Dispose();
									}
								}
								finally
								{
									safeWaitHandle.Dispose();
								}
							}
							else
							{
								mutexHandle.Dispose();
								flag3 = true;
							}
						}
					}
					else
					{
						num = Marshal.GetLastWin32Error();
					}
				}
				finally
				{
					if (flag)
					{
						Thread.EndThreadAffinity();
					}
				}
				if (!flag2 && !flag3)
				{
					switch (num)
					{
					case 2:
						continue;
					case 0:
						num = 183;
						break;
					}
					break;
				}
			}
			return num;
		}

		public MutexSecurity GetAccessControl()
		{
			return new MutexSecurity(safeWaitHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public void SetAccessControl(MutexSecurity mutexSecurity)
		{
			if (mutexSecurity == null)
			{
				throw new ArgumentNullException("mutexSecurity");
			}
			mutexSecurity.Persist(safeWaitHandle);
		}
	}
}
