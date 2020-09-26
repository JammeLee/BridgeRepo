using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace System.Diagnostics
{
	internal class MainWindowFinder
	{
		private IntPtr bestHandle;

		private int processId;

		public IntPtr FindMainWindow(int processId)
		{
			bestHandle = (IntPtr)0;
			this.processId = processId;
			NativeMethods.EnumThreadWindowsCallback enumThreadWindowsCallback = EnumWindowsCallback;
			NativeMethods.EnumWindows(enumThreadWindowsCallback, IntPtr.Zero);
			GC.KeepAlive(enumThreadWindowsCallback);
			return bestHandle;
		}

		private bool IsMainWindow(IntPtr handle)
		{
			if (NativeMethods.GetWindow(new HandleRef(this, handle), 4) != (IntPtr)0 || !NativeMethods.IsWindowVisible(new HandleRef(this, handle)))
			{
				return false;
			}
			return true;
		}

		private bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
		{
			NativeMethods.GetWindowThreadProcessId(new HandleRef(this, handle), out var num);
			if (num == processId && IsMainWindow(handle))
			{
				bestHandle = handle;
				return false;
			}
			return true;
		}
	}
}
