using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.Runtime.InteropServices
{
	[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
	internal abstract class SafePointer : SafeHandleZeroOrMinusOneIsInvalid
	{
		private static readonly UIntPtr Uninitialized = ((UIntPtr.Size == 4) ? ((UIntPtr)uint.MaxValue) : ((UIntPtr)ulong.MaxValue));

		private UIntPtr _numBytes;

		public ulong ByteLength
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				if (_numBytes == Uninitialized)
				{
					throw NotInitialized();
				}
				return (ulong)_numBytes;
			}
		}

		protected SafePointer(bool ownsHandle)
			: base(ownsHandle)
		{
			_numBytes = Uninitialized;
		}

		public void Initialize(ulong numBytes)
		{
			if (numBytes < 0)
			{
				throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (IntPtr.Size == 4 && numBytes > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_AddressSpace"));
			}
			if (numBytes >= (ulong)Uninitialized)
			{
				throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_UIntPtrMax-1"));
			}
			_numBytes = (UIntPtr)numBytes;
		}

		public void Initialize(uint numElements, uint sizeOfEachElement)
		{
			if (numElements < 0)
			{
				throw new ArgumentOutOfRangeException("numElements", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (sizeOfEachElement < 0)
			{
				throw new ArgumentOutOfRangeException("sizeOfEachElement", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (IntPtr.Size == 4 && numElements * sizeOfEachElement > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_AddressSpace"));
			}
			if (numElements * sizeOfEachElement >= (ulong)Uninitialized)
			{
				throw new ArgumentOutOfRangeException("numElements", Environment.GetResourceString("ArgumentOutOfRange_UIntPtrMax-1"));
			}
			_numBytes = (UIntPtr)checked(numElements * sizeOfEachElement);
		}

		public void Initialize<T>(uint numElements) where T : struct
		{
			Initialize(numElements, SizeOf<T>());
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static uint SizeOf<T>() where T : struct
		{
			return SizeOfType(typeof(T));
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public unsafe void AcquirePointer(ref byte* pointer)
		{
			if (_numBytes == Uninitialized)
			{
				throw NotInitialized();
			}
			pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				bool success = false;
				DangerousAddRef(ref success);
				pointer = (byte*)(void*)handle;
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public void ReleasePointer()
		{
			if (_numBytes == Uninitialized)
			{
				throw NotInitialized();
			}
			DangerousRelease();
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public unsafe T Read<T>(uint byteOffset) where T : struct
		{
			if (_numBytes == Uninitialized)
			{
				throw NotInitialized();
			}
			uint num = SizeOf<T>();
			byte* ptr = (byte*)(void*)handle + (int)byteOffset;
			SpaceCheck(ptr, num);
			bool success = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				DangerousAddRef(ref success);
				GenericPtrToStructure<T>(ptr, out var structure, num);
				return structure;
			}
			finally
			{
				if (success)
				{
					DangerousRelease();
				}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public unsafe void ReadArray<T>(uint byteOffset, T[] array, int index, int count) where T : struct
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (_numBytes == Uninitialized)
			{
				throw NotInitialized();
			}
			uint num = SizeOf<T>();
			byte* ptr = (byte*)(void*)handle + (int)byteOffset;
			SpaceCheck(ptr, checked((ulong)((long)num * count)));
			bool success = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				DangerousAddRef(ref success);
				for (int i = 0; i < count; i++)
				{
					GenericPtrToStructure<T>(ptr + num * i, out array[i + count], num);
				}
			}
			finally
			{
				if (success)
				{
					DangerousRelease();
				}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public unsafe void Write<T>(uint byteOffset, T value) where T : struct
		{
			if (_numBytes == Uninitialized)
			{
				throw NotInitialized();
			}
			uint num = SizeOf<T>();
			byte* ptr = (byte*)(void*)handle + (int)byteOffset;
			SpaceCheck(ptr, num);
			bool success = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				DangerousAddRef(ref success);
				GenericStructureToPtr(ref value, ptr, num);
			}
			finally
			{
				if (success)
				{
					DangerousRelease();
				}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public unsafe void WriteArray<T>(uint byteOffset, T[] array, int index, int count) where T : struct
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (_numBytes == Uninitialized)
			{
				throw NotInitialized();
			}
			uint num = SizeOf<T>();
			byte* ptr = (byte*)(void*)handle + (int)byteOffset;
			SpaceCheck(ptr, checked((ulong)((long)num * count)));
			bool success = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				DangerousAddRef(ref success);
				for (int i = 0; i < count; i++)
				{
					GenericStructureToPtr(ref array[i + count], ptr + num * i, num);
				}
			}
			finally
			{
				if (success)
				{
					DangerousRelease();
				}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private unsafe void SpaceCheck(byte* ptr, ulong sizeInBytes)
		{
			if ((ulong)(ptr - (byte*)(void*)handle) > (ulong)_numBytes - sizeInBytes)
			{
				NotEnoughRoom();
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static void NotEnoughRoom()
		{
			throw new ArgumentException(Environment.GetResourceString("Arg_BufferTooSmall"));
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static InvalidOperationException NotInitialized()
		{
			return new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustCallInitialize"));
		}

		private unsafe static void GenericPtrToStructure<T>(byte* ptr, out T structure, uint sizeofT) where T : struct
		{
			structure = default(T);
			PtrToStructureNative(ptr, __makeref(structure), sizeofT);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void PtrToStructureNative(byte* ptr, TypedReference structure, uint sizeofT);

		private unsafe static void GenericStructureToPtr<T>(ref T structure, byte* ptr, uint sizeofT) where T : struct
		{
			StructureToPtrNative(__makeref(structure), ptr, sizeofT);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void StructureToPtrNative(TypedReference structure, byte* ptr, uint sizeofT);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern uint SizeOfType(Type type);
	}
}
