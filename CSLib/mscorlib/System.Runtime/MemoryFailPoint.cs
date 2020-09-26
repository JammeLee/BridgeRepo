using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32;

namespace System.Runtime
{
	public sealed class MemoryFailPoint : CriticalFinalizerObject, IDisposable
	{
		private const int CheckThreshold = 10000;

		private const int LowMemoryFudgeFactor = 16777216;

		private static readonly ulong TopOfMemory;

		private static long LastKnownFreeAddressSpace;

		private static long LastTimeCheckingAddressSpace;

		private static readonly uint GCSegmentSize;

		private ulong _reservedMemory;

		private bool _mustSubtractReservation;

		static MemoryFailPoint()
		{
			GetMemorySettings(out GCSegmentSize, out TopOfMemory);
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public unsafe MemoryFailPoint(int sizeInMegabytes)
		{
			if (sizeInMegabytes <= 0)
			{
				throw new ArgumentOutOfRangeException("sizeInMegabytes", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			ulong num = (_reservedMemory = (ulong)((long)sizeInMegabytes << 20));
			ulong num2 = (ulong)(Math.Ceiling((float)num / (float)GCSegmentSize) * (double)GCSegmentSize);
			if (num2 >= TopOfMemory)
			{
				throw new InsufficientMemoryException(Environment.GetResourceString("InsufficientMemory_MemFailPoint_TooBig"));
			}
			ulong availPageFile = 0uL;
			ulong totalAddressSpaceFree = 0uL;
			for (int i = 0; i < 3; i++)
			{
				CheckForAvailableMemory(out availPageFile, out totalAddressSpaceFree);
				ulong memoryFailPointReservedMemory = SharedStatics.MemoryFailPointReservedMemory;
				ulong num3 = num2 + memoryFailPointReservedMemory;
				bool flag = num3 < num2 || num3 < memoryFailPointReservedMemory;
				bool flag2 = availPageFile < num3 + 16777216 || flag;
				bool flag3 = totalAddressSpaceFree < num3 || flag;
				long num4 = Environment.TickCount;
				if (num4 > LastTimeCheckingAddressSpace + 10000 || num4 < LastTimeCheckingAddressSpace || LastKnownFreeAddressSpace < (long)num2)
				{
					CheckForFreeAddressSpace(num2, shouldThrow: false);
				}
				bool flag4 = (ulong)LastKnownFreeAddressSpace < num2;
				if (!flag2 && !flag3 && !flag4)
				{
					break;
				}
				switch (i)
				{
				case 0:
					GC.Collect();
					break;
				case 1:
					if (!flag2)
					{
						break;
					}
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						UIntPtr numBytes = new UIntPtr(num2);
						void* ptr = Win32Native.VirtualAlloc(null, numBytes, 4096, 4);
						if (ptr != null && !Win32Native.VirtualFree(ptr, UIntPtr.Zero, 32768))
						{
							__Error.WinIOError();
						}
					}
					break;
				case 2:
					if (flag2 || flag3)
					{
						InsufficientMemoryException ex = new InsufficientMemoryException(Environment.GetResourceString("InsufficientMemory_MemFailPoint"));
						throw ex;
					}
					if (flag4)
					{
						InsufficientMemoryException ex2 = new InsufficientMemoryException(Environment.GetResourceString("InsufficientMemory_MemFailPoint_VAFrag"));
						throw ex2;
					}
					break;
				}
			}
			Interlocked.Add(ref LastKnownFreeAddressSpace, (long)(0L - num));
			if (LastKnownFreeAddressSpace < 0)
			{
				CheckForFreeAddressSpace(num2, shouldThrow: true);
			}
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				SharedStatics.AddMemoryFailPointReservation((long)num);
				_mustSubtractReservation = true;
			}
		}

		private static void CheckForAvailableMemory(out ulong availPageFile, out ulong totalAddressSpaceFree)
		{
			if (Environment.IsWin9X())
			{
				Win32Native.MEMORYSTATUS mEMORYSTATUS = new Win32Native.MEMORYSTATUS();
				if (!Win32Native.GlobalMemoryStatus(mEMORYSTATUS))
				{
					__Error.WinIOError();
				}
				availPageFile = mEMORYSTATUS.availPageFile;
				totalAddressSpaceFree = mEMORYSTATUS.availVirtual;
			}
			else
			{
				Win32Native.MEMORYSTATUSEX mEMORYSTATUSEX = new Win32Native.MEMORYSTATUSEX();
				if (!Win32Native.GlobalMemoryStatusEx(mEMORYSTATUSEX))
				{
					__Error.WinIOError();
				}
				availPageFile = mEMORYSTATUSEX.availPageFile;
				totalAddressSpaceFree = mEMORYSTATUSEX.availVirtual;
			}
		}

		private static bool CheckForFreeAddressSpace(ulong size, bool shouldThrow)
		{
			ulong num = (ulong)(LastKnownFreeAddressSpace = (long)MemFreeAfterAddress(null, size));
			LastTimeCheckingAddressSpace = Environment.TickCount;
			if (num < size && shouldThrow)
			{
				throw new InsufficientMemoryException(Environment.GetResourceString("InsufficientMemory_MemFailPoint_VAFrag"));
			}
			return num >= size;
		}

		private unsafe static ulong MemFreeAfterAddress(void* address, ulong size)
		{
			if (size >= TopOfMemory)
			{
				return 0uL;
			}
			ulong num = 0uL;
			Win32Native.MEMORY_BASIC_INFORMATION buffer = default(Win32Native.MEMORY_BASIC_INFORMATION);
			IntPtr sizeOfBuffer = (IntPtr)Marshal.SizeOf(buffer);
			while ((ulong)((long)address + (long)size) < TopOfMemory)
			{
				IntPtr value = Win32Native.VirtualQuery(address, ref buffer, sizeOfBuffer);
				if (value == IntPtr.Zero)
				{
					__Error.WinIOError();
				}
				ulong num2 = buffer.RegionSize.ToUInt64();
				if (buffer.State == 65536)
				{
					if (num2 >= size)
					{
						return num2;
					}
					num = Math.Max(num, num2);
				}
				address = (void*)((ulong)address + num2);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetMemorySettings(out uint maxGCSegmentSize, out ulong topOfMemory);

		~MemoryFailPoint()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private void Dispose(bool disposing)
		{
			if (_mustSubtractReservation)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					SharedStatics.AddMemoryFailPointReservation((long)(0L - _reservedMemory));
					_mustSubtractReservation = false;
				}
			}
		}
	}
}
