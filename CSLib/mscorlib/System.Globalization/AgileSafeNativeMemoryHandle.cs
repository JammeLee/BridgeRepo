using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Globalization
{
	internal sealed class AgileSafeNativeMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private const int PAGE_READONLY = 2;

		private const int SECTION_MAP_READ = 4;

		private unsafe byte* bytes;

		private long fileSize;

		private bool mode;

		internal long FileSize => fileSize;

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal AgileSafeNativeMemoryHandle()
			: base(ownsHandle: true)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		internal AgileSafeNativeMemoryHandle(IntPtr handle, bool ownsHandle)
			: base(ownsHandle)
		{
			SetHandle(handle);
		}

		internal AgileSafeNativeMemoryHandle(string fileName)
			: this(fileName, null)
		{
		}

		internal unsafe AgileSafeNativeMemoryHandle(string fileName, string fileMappingName)
			: base(ownsHandle: true)
		{
			mode = true;
			SafeFileHandle safeFileHandle = Win32Native.UnsafeCreateFile(fileName, int.MinValue, FileShare.Read, null, FileMode.Open, 0, IntPtr.Zero);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (safeFileHandle.IsInvalid)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidOperation_UnexpectedWin32Error"), lastWin32Error));
			}
			int highSize;
			int num = Win32Native.GetFileSize(safeFileHandle, out highSize);
			if (num == -1)
			{
				safeFileHandle.Close();
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidOperation_UnexpectedWin32Error"), lastWin32Error));
			}
			fileSize = ((long)highSize << 32) | (uint)num;
			if (fileSize == 0)
			{
				safeFileHandle.Close();
				return;
			}
			SafeFileMappingHandle safeFileMappingHandle = Win32Native.CreateFileMapping(safeFileHandle, IntPtr.Zero, 2u, 0u, 0u, fileMappingName);
			lastWin32Error = Marshal.GetLastWin32Error();
			safeFileHandle.Close();
			if (safeFileMappingHandle.IsInvalid)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidOperation_UnexpectedWin32Error"), lastWin32Error));
			}
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				handle = Win32Native.MapViewOfFile(safeFileMappingHandle, 4u, 0u, 0u, UIntPtr.Zero);
			}
			lastWin32Error = Marshal.GetLastWin32Error();
			if (handle == IntPtr.Zero)
			{
				safeFileMappingHandle.Close();
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidOperation_UnexpectedWin32Error"), lastWin32Error));
			}
			bytes = (byte*)(void*)DangerousGetHandle();
			safeFileMappingHandle.Close();
		}

		internal unsafe byte* GetBytePtr()
		{
			return bytes;
		}

		protected override bool ReleaseHandle()
		{
			if (!IsInvalid)
			{
				if (!mode)
				{
					Marshal.FreeHGlobal(handle);
					handle = IntPtr.Zero;
					return true;
				}
				if (Win32Native.UnmapViewOfFile(handle))
				{
					handle = IntPtr.Zero;
					return true;
				}
			}
			return false;
		}
	}
}
