using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace System.Diagnostics
{
	internal static class WinProcessManager
	{
		public static int[] GetProcessIds()
		{
			ProcessInfo[] processInfos = GetProcessInfos();
			int[] array = new int[processInfos.Length];
			for (int i = 0; i < processInfos.Length; i++)
			{
				array[i] = processInfos[i].processId;
			}
			return array;
		}

		public static ProcessInfo[] GetProcessInfos()
		{
			IntPtr intPtr = (IntPtr)(-1);
			GCHandle gCHandle = default(GCHandle);
			ArrayList arrayList = new ArrayList();
			Hashtable hashtable = new Hashtable();
			try
			{
				intPtr = NativeMethods.CreateToolhelp32Snapshot(6, 0);
				if (intPtr == (IntPtr)(-1))
				{
					throw new Win32Exception();
				}
				int num = Marshal.SizeOf(typeof(NativeMethods.WinProcessEntry));
				int num2 = num + 260;
				int[] value = new int[num2 / 4];
				gCHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
				IntPtr intPtr2 = gCHandle.AddrOfPinnedObject();
				Marshal.WriteInt32(intPtr2, num2);
				HandleRef handle = new HandleRef(null, intPtr);
				if (NativeMethods.Process32First(handle, intPtr2))
				{
					do
					{
						NativeMethods.WinProcessEntry winProcessEntry = new NativeMethods.WinProcessEntry();
						Marshal.PtrToStructure(intPtr2, winProcessEntry);
						ProcessInfo processInfo = new ProcessInfo();
						string path = Marshal.PtrToStringAnsi((IntPtr)((long)intPtr2 + num));
						processInfo.processName = Path.ChangeExtension(Path.GetFileName(path), null);
						processInfo.handleCount = winProcessEntry.cntUsage;
						processInfo.processId = winProcessEntry.th32ProcessID;
						processInfo.basePriority = winProcessEntry.pcPriClassBase;
						processInfo.mainModuleId = winProcessEntry.th32ModuleID;
						hashtable.Add(processInfo.processId, processInfo);
						Marshal.WriteInt32(intPtr2, num2);
					}
					while (NativeMethods.Process32Next(handle, intPtr2));
				}
				NativeMethods.WinThreadEntry winThreadEntry = new NativeMethods.WinThreadEntry();
				winThreadEntry.dwSize = Marshal.SizeOf(winThreadEntry);
				if (NativeMethods.Thread32First(handle, winThreadEntry))
				{
					do
					{
						ThreadInfo threadInfo = new ThreadInfo();
						threadInfo.threadId = winThreadEntry.th32ThreadID;
						threadInfo.processId = winThreadEntry.th32OwnerProcessID;
						threadInfo.basePriority = winThreadEntry.tpBasePri;
						threadInfo.currentPriority = winThreadEntry.tpBasePri + winThreadEntry.tpDeltaPri;
						arrayList.Add(threadInfo);
					}
					while (NativeMethods.Thread32Next(handle, winThreadEntry));
				}
				for (int i = 0; i < arrayList.Count; i++)
				{
					ThreadInfo threadInfo2 = (ThreadInfo)arrayList[i];
					((ProcessInfo)hashtable[threadInfo2.processId])?.threadInfoList.Add(threadInfo2);
				}
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
				if (intPtr != (IntPtr)(-1))
				{
					Microsoft.Win32.SafeNativeMethods.CloseHandle(new HandleRef(null, intPtr));
				}
			}
			ProcessInfo[] array = new ProcessInfo[hashtable.Values.Count];
			hashtable.Values.CopyTo(array, 0);
			return array;
		}

		public static ModuleInfo[] GetModuleInfos(int processId)
		{
			IntPtr intPtr = (IntPtr)(-1);
			GCHandle gCHandle = default(GCHandle);
			ArrayList arrayList = new ArrayList();
			try
			{
				intPtr = NativeMethods.CreateToolhelp32Snapshot(8, processId);
				if (intPtr == (IntPtr)(-1))
				{
					throw new Win32Exception();
				}
				int num = Marshal.SizeOf(typeof(NativeMethods.WinModuleEntry));
				int num2 = num + 260 + 256;
				int[] value = new int[num2 / 4];
				gCHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
				IntPtr intPtr2 = gCHandle.AddrOfPinnedObject();
				Marshal.WriteInt32(intPtr2, num2);
				HandleRef handle = new HandleRef(null, intPtr);
				if (NativeMethods.Module32First(handle, intPtr2))
				{
					do
					{
						NativeMethods.WinModuleEntry winModuleEntry = new NativeMethods.WinModuleEntry();
						Marshal.PtrToStructure(intPtr2, winModuleEntry);
						ModuleInfo moduleInfo = new ModuleInfo();
						moduleInfo.baseName = Marshal.PtrToStringAnsi((IntPtr)((long)intPtr2 + num));
						moduleInfo.fileName = Marshal.PtrToStringAnsi((IntPtr)((long)intPtr2 + num + 256));
						moduleInfo.baseOfDll = winModuleEntry.modBaseAddr;
						moduleInfo.sizeOfImage = winModuleEntry.modBaseSize;
						moduleInfo.Id = winModuleEntry.th32ModuleID;
						arrayList.Add(moduleInfo);
						Marshal.WriteInt32(intPtr2, num2);
					}
					while (NativeMethods.Module32Next(handle, intPtr2));
				}
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
				if (intPtr != (IntPtr)(-1))
				{
					Microsoft.Win32.SafeNativeMethods.CloseHandle(new HandleRef(null, intPtr));
				}
			}
			ModuleInfo[] array = new ModuleInfo[arrayList.Count];
			arrayList.CopyTo(array, 0);
			return array;
		}
	}
}
