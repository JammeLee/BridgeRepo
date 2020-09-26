using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace System.Security
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeBSTRHandle : SafePointer
	{
		internal unsafe int Length
		{
			get
			{
				byte* pointer = null;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					AcquirePointer(ref pointer);
					return Win32Native.SysStringLen((IntPtr)pointer);
				}
				finally
				{
					if (pointer != null)
					{
						ReleasePointer();
					}
				}
			}
		}

		internal SafeBSTRHandle()
			: base(ownsHandle: true)
		{
		}

		internal static SafeBSTRHandle Allocate(string src, uint len)
		{
			SafeBSTRHandle safeBSTRHandle = SysAllocStringLen(src, len);
			safeBSTRHandle.Initialize(len * 2);
			return safeBSTRHandle;
		}

		[DllImport("oleaut32.dll", CharSet = CharSet.Unicode)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern SafeBSTRHandle SysAllocStringLen(string src, uint len);

		protected override bool ReleaseHandle()
		{
			Win32Native.ZeroMemory(handle, (uint)(Win32Native.SysStringLen(handle) * 2));
			Win32Native.SysFreeString(handle);
			return true;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal unsafe void ClearBuffer()
		{
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				AcquirePointer(ref pointer);
				Win32Native.ZeroMemory((IntPtr)pointer, (uint)(Win32Native.SysStringLen((IntPtr)pointer) * 2));
			}
			finally
			{
				if (pointer != null)
				{
					ReleasePointer();
				}
			}
		}

		internal unsafe static void Copy(SafeBSTRHandle source, SafeBSTRHandle target)
		{
			byte* pointer = null;
			byte* pointer2 = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				source.AcquirePointer(ref pointer);
				target.AcquirePointer(ref pointer2);
				Buffer.memcpyimpl(pointer, pointer2, Win32Native.SysStringLen((IntPtr)pointer) * 2);
			}
			finally
			{
				if (pointer != null)
				{
					source.ReleasePointer();
				}
				if (pointer2 != null)
				{
					target.ReleasePointer();
				}
			}
		}
	}
}
