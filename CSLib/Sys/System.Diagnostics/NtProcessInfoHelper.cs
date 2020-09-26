using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace System.Diagnostics
{
	internal static class NtProcessInfoHelper
	{
		[StructLayout(LayoutKind.Sequential)]
		internal class SystemProcessInformation
		{
			internal int NextEntryOffset;

			internal uint NumberOfThreads;

			private long SpareLi1;

			private long SpareLi2;

			private long SpareLi3;

			private long CreateTime;

			private long UserTime;

			private long KernelTime;

			internal ushort NameLength;

			internal ushort MaximumNameLength;

			internal IntPtr NamePtr;

			internal int BasePriority;

			internal IntPtr UniqueProcessId;

			internal IntPtr InheritedFromUniqueProcessId;

			internal uint HandleCount;

			internal uint SessionId;

			internal IntPtr PageDirectoryBase;

			internal IntPtr PeakVirtualSize;

			internal IntPtr VirtualSize;

			internal uint PageFaultCount;

			internal IntPtr PeakWorkingSetSize;

			internal IntPtr WorkingSetSize;

			internal IntPtr QuotaPeakPagedPoolUsage;

			internal IntPtr QuotaPagedPoolUsage;

			internal IntPtr QuotaPeakNonPagedPoolUsage;

			internal IntPtr QuotaNonPagedPoolUsage;

			internal IntPtr PagefileUsage;

			internal IntPtr PeakPagefileUsage;

			internal IntPtr PrivatePageCount;

			private long ReadOperationCount;

			private long WriteOperationCount;

			private long OtherOperationCount;

			private long ReadTransferCount;

			private long WriteTransferCount;

			private long OtherTransferCount;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class SystemThreadInformation
		{
			private long KernelTime;

			private long UserTime;

			private long CreateTime;

			private uint WaitTime;

			internal IntPtr StartAddress;

			internal IntPtr UniqueProcess;

			internal IntPtr UniqueThread;

			internal int Priority;

			internal int BasePriority;

			internal uint ContextSwitches;

			internal uint ThreadState;

			internal uint WaitReason;
		}

		private static int GetNewBufferSize(int existingBufferSize, int requiredSize)
		{
			if (requiredSize == 0)
			{
				int num = existingBufferSize * 2;
				if (num < existingBufferSize)
				{
					throw new OutOfMemoryException();
				}
				return num;
			}
			return requiredSize + 10240;
		}

		public static ProcessInfo[] GetProcessInfos()
		{
			int num = 131072;
			int returnedSize = 0;
			GCHandle gCHandle = default(GCHandle);
			try
			{
				int num2;
				do
				{
					byte[] value = new byte[num];
					gCHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
					num2 = NativeMethods.NtQuerySystemInformation(5, gCHandle.AddrOfPinnedObject(), num, out returnedSize);
					if (num2 == -1073741820)
					{
						if (gCHandle.IsAllocated)
						{
							gCHandle.Free();
						}
						num = GetNewBufferSize(num, returnedSize);
					}
				}
				while (num2 == -1073741820);
				if (num2 < 0)
				{
					throw new InvalidOperationException(SR.GetString("CouldntGetProcessInfos"), new Win32Exception(num2));
				}
				return GetProcessInfos(gCHandle.AddrOfPinnedObject());
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
			}
		}

		private unsafe static ProcessInfo[] GetProcessInfos(IntPtr dataPtr)
		{
			Hashtable hashtable = new Hashtable(60);
			long num = 0L;
			while (true)
			{
				IntPtr intPtr = (IntPtr)((long)dataPtr + num);
				SystemProcessInformation systemProcessInformation = new SystemProcessInformation();
				Marshal.PtrToStructure(intPtr, systemProcessInformation);
				ProcessInfo processInfo = new ProcessInfo();
				processInfo.processId = systemProcessInformation.UniqueProcessId.ToInt32();
				processInfo.handleCount = (int)systemProcessInformation.HandleCount;
				processInfo.sessionId = (int)systemProcessInformation.SessionId;
				processInfo.poolPagedBytes = (long)systemProcessInformation.QuotaPagedPoolUsage;
				processInfo.poolNonpagedBytes = (long)systemProcessInformation.QuotaNonPagedPoolUsage;
				processInfo.virtualBytes = (long)systemProcessInformation.VirtualSize;
				processInfo.virtualBytesPeak = (long)systemProcessInformation.PeakVirtualSize;
				processInfo.workingSetPeak = (long)systemProcessInformation.PeakWorkingSetSize;
				processInfo.workingSet = (long)systemProcessInformation.WorkingSetSize;
				processInfo.pageFileBytesPeak = (long)systemProcessInformation.PeakPagefileUsage;
				processInfo.pageFileBytes = (long)systemProcessInformation.PagefileUsage;
				processInfo.privateBytes = (long)systemProcessInformation.PrivatePageCount;
				processInfo.basePriority = systemProcessInformation.BasePriority;
				if (systemProcessInformation.NamePtr == IntPtr.Zero)
				{
					if (processInfo.processId == NtProcessManager.SystemProcessID)
					{
						processInfo.processName = "System";
					}
					else if (processInfo.processId == 0)
					{
						processInfo.processName = "Idle";
					}
					else
					{
						processInfo.processName = processInfo.processId.ToString(CultureInfo.InvariantCulture);
					}
				}
				else
				{
					string text = GetProcessShortName((char*)systemProcessInformation.NamePtr.ToPointer(), (int)systemProcessInformation.NameLength / 2);
					if (ProcessManager.IsOSOlderThanXP && text.Length == 15)
					{
						if (text.EndsWith(".", StringComparison.OrdinalIgnoreCase))
						{
							text = text.Substring(0, 14);
						}
						else if (text.EndsWith(".e", StringComparison.OrdinalIgnoreCase))
						{
							text = text.Substring(0, 13);
						}
						else if (text.EndsWith(".ex", StringComparison.OrdinalIgnoreCase))
						{
							text = text.Substring(0, 12);
						}
					}
					processInfo.processName = text;
				}
				hashtable[processInfo.processId] = processInfo;
				intPtr = (IntPtr)((long)intPtr + Marshal.SizeOf(systemProcessInformation));
				for (int i = 0; i < systemProcessInformation.NumberOfThreads; i++)
				{
					SystemThreadInformation systemThreadInformation = new SystemThreadInformation();
					Marshal.PtrToStructure(intPtr, systemThreadInformation);
					ThreadInfo threadInfo = new ThreadInfo();
					threadInfo.processId = (int)systemThreadInformation.UniqueProcess;
					threadInfo.threadId = (int)systemThreadInformation.UniqueThread;
					threadInfo.basePriority = systemThreadInformation.BasePriority;
					threadInfo.currentPriority = systemThreadInformation.Priority;
					threadInfo.startAddress = systemThreadInformation.StartAddress;
					threadInfo.threadState = (ThreadState)systemThreadInformation.ThreadState;
					threadInfo.threadWaitReason = NtProcessManager.GetThreadWaitReason((int)systemThreadInformation.WaitReason);
					processInfo.threadInfoList.Add(threadInfo);
					intPtr = (IntPtr)((long)intPtr + Marshal.SizeOf(systemThreadInformation));
				}
				if (systemProcessInformation.NextEntryOffset == 0)
				{
					break;
				}
				num += systemProcessInformation.NextEntryOffset;
			}
			ProcessInfo[] array = new ProcessInfo[hashtable.Values.Count];
			hashtable.Values.CopyTo(array, 0);
			return array;
		}

		internal unsafe static string GetProcessShortName(char* name, int length)
		{
			char* ptr = name;
			char* ptr2 = name;
			char* ptr3 = name;
			int num = 0;
			while (*ptr3 != 0)
			{
				if (*ptr3 == '\\')
				{
					ptr = ptr3;
				}
				else if (*ptr3 == '.')
				{
					ptr2 = ptr3;
				}
				ptr3++;
				num++;
				if (num >= length)
				{
					break;
				}
			}
			if (ptr2 == name)
			{
				ptr2 = ptr3;
			}
			else
			{
				string b = new string(ptr2);
				if (!string.Equals(".exe", b, StringComparison.OrdinalIgnoreCase))
				{
					ptr2 = ptr3;
				}
			}
			if (*ptr == '\\')
			{
				ptr++;
			}
			int length2 = (int)(ptr2 - ptr);
			return new string(ptr, 0, length2);
		}
	}
}
