using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace System.Security
{
	public sealed class SecureString : IDisposable
	{
		private const int BlockSize = 8;

		private const int MaxLength = 65536;

		private const uint ProtectionScope = 0u;

		private SafeBSTRHandle m_buffer;

		private int m_length;

		private bool m_readOnly;

		private bool m_enrypted;

		private static bool supportedOnCurrentPlatform = EncryptionSupported();

		public int Length
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				EnsureNotDisposed();
				return m_length;
			}
		}

		private int BufferLength => m_buffer.Length;

		private static bool EncryptionSupported()
		{
			bool result = true;
			try
			{
				Win32Native.SystemFunction041(SafeBSTRHandle.Allocate(null, 16u), 16u, 0u);
				return result;
			}
			catch (EntryPointNotFoundException)
			{
				return false;
			}
		}

		internal SecureString(SecureString str)
		{
			AllocateBuffer(str.BufferLength);
			SafeBSTRHandle.Copy(str.m_buffer, m_buffer);
			m_length = str.m_length;
			m_enrypted = str.m_enrypted;
		}

		public SecureString()
		{
			CheckSupportedOnCurrentPlatform();
			AllocateBuffer(8);
			m_length = 0;
		}

		[CLSCompliant(false)]
		public unsafe SecureString(char* value, int length)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (length > 65536)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Length"));
			}
			CheckSupportedOnCurrentPlatform();
			AllocateBuffer(length);
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				m_buffer.AcquirePointer(ref pointer);
				Buffer.memcpyimpl((byte*)value, pointer, length * 2);
			}
			finally
			{
				if (pointer != null)
				{
					m_buffer.ReleasePointer();
				}
			}
			m_length = length;
			ProtectMemory();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AppendChar(char c)
		{
			EnsureNotDisposed();
			EnsureNotReadOnly();
			EnsureCapacity(m_length + 1);
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				UnProtectMemory();
				m_buffer.Write((uint)(m_length * 2), c);
				m_length++;
			}
			finally
			{
				ProtectMemory();
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Clear()
		{
			EnsureNotDisposed();
			EnsureNotReadOnly();
			m_length = 0;
			m_buffer.ClearBuffer();
			m_enrypted = false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public SecureString Copy()
		{
			EnsureNotDisposed();
			return new SecureString(this);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Dispose()
		{
			if (m_buffer != null && !m_buffer.IsInvalid)
			{
				m_buffer.Close();
				m_buffer = null;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public unsafe void InsertAt(int index, char c)
		{
			EnsureNotDisposed();
			EnsureNotReadOnly();
			if (index < 0 || index > m_length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_IndexString"));
			}
			EnsureCapacity(m_length + 1);
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				UnProtectMemory();
				m_buffer.AcquirePointer(ref pointer);
				char* ptr = (char*)pointer;
				for (int num = m_length; num > index; num--)
				{
					ptr[num] = ptr[num - 1];
				}
				ptr[index] = c;
				m_length++;
			}
			finally
			{
				ProtectMemory();
				if (pointer != null)
				{
					m_buffer.ReleasePointer();
				}
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool IsReadOnly()
		{
			EnsureNotDisposed();
			return m_readOnly;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void MakeReadOnly()
		{
			EnsureNotDisposed();
			m_readOnly = true;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public unsafe void RemoveAt(int index)
		{
			EnsureNotDisposed();
			EnsureNotReadOnly();
			if (index < 0 || index >= m_length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_IndexString"));
			}
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				UnProtectMemory();
				m_buffer.AcquirePointer(ref pointer);
				char* ptr = (char*)pointer;
				for (int i = index; i < m_length - 1; i++)
				{
					ptr[i] = ptr[i + 1];
				}
				ptr[--m_length] = '\0';
			}
			finally
			{
				ProtectMemory();
				if (pointer != null)
				{
					m_buffer.ReleasePointer();
				}
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void SetAt(int index, char c)
		{
			EnsureNotDisposed();
			EnsureNotReadOnly();
			if (index < 0 || index >= m_length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_IndexString"));
			}
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				UnProtectMemory();
				m_buffer.Write((uint)(index * 2), c);
			}
			finally
			{
				ProtectMemory();
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private void AllocateBuffer(int size)
		{
			uint alignedSize = GetAlignedSize(size);
			m_buffer = SafeBSTRHandle.Allocate(null, alignedSize);
			if (m_buffer.IsInvalid)
			{
				throw new OutOfMemoryException();
			}
		}

		private void CheckSupportedOnCurrentPlatform()
		{
			if (!supportedOnCurrentPlatform)
			{
				throw new NotSupportedException(Environment.GetResourceString("Arg_PlatformSecureString"));
			}
		}

		private void EnsureCapacity(int capacity)
		{
			if (capacity > m_buffer.Length)
			{
				if (capacity > 65536)
				{
					throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_Capacity"));
				}
				SafeBSTRHandle safeBSTRHandle = SafeBSTRHandle.Allocate(null, GetAlignedSize(capacity));
				if (safeBSTRHandle.IsInvalid)
				{
					throw new OutOfMemoryException();
				}
				SafeBSTRHandle.Copy(m_buffer, safeBSTRHandle);
				m_buffer.Close();
				m_buffer = safeBSTRHandle;
			}
		}

		private void EnsureNotDisposed()
		{
			if (m_buffer == null)
			{
				throw new ObjectDisposedException(null);
			}
		}

		private void EnsureNotReadOnly()
		{
			if (m_readOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static uint GetAlignedSize(int size)
		{
			uint num = (uint)size / 8u * 8;
			if (size % 8 != 0 || size == 0)
			{
				num += 8;
			}
			return num;
		}

		private unsafe int GetAnsiByteCount()
		{
			uint flags = 1024u;
			uint num = 63u;
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				m_buffer.AcquirePointer(ref pointer);
				return Win32Native.WideCharToMultiByte(0u, flags, (char*)pointer, m_length, null, 0, IntPtr.Zero, new IntPtr(&num));
			}
			finally
			{
				if (pointer != null)
				{
					m_buffer.ReleasePointer();
				}
			}
		}

		private unsafe void GetAnsiBytes(byte* ansiStrPtr, int byteCount)
		{
			uint flags = 1024u;
			uint num = 63u;
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				m_buffer.AcquirePointer(ref pointer);
				Win32Native.WideCharToMultiByte(0u, flags, (char*)pointer, m_length, ansiStrPtr, byteCount - 1, IntPtr.Zero, new IntPtr(&num));
				*(ansiStrPtr + byteCount - 1) = 0;
			}
			finally
			{
				if (pointer != null)
				{
					m_buffer.ReleasePointer();
				}
			}
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		private void ProtectMemory()
		{
			if (m_length == 0 || m_enrypted)
			{
				return;
			}
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				int num = Win32Native.SystemFunction040(m_buffer, (uint)(m_buffer.Length * 2), 0u);
				if (num < 0)
				{
					throw new CryptographicException(Win32Native.LsaNtStatusToWinError(num));
				}
				m_enrypted = true;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal unsafe IntPtr ToBSTR()
		{
			EnsureNotDisposed();
			int length = m_length;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr intPtr2 = IntPtr.Zero;
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					intPtr = Win32Native.SysAllocStringLen(null, length);
				}
				if (intPtr == IntPtr.Zero)
				{
					throw new OutOfMemoryException();
				}
				UnProtectMemory();
				m_buffer.AcquirePointer(ref pointer);
				Buffer.memcpyimpl(pointer, (byte*)intPtr.ToPointer(), length * 2);
				intPtr2 = intPtr;
			}
			finally
			{
				ProtectMemory();
				if (intPtr2 == IntPtr.Zero && intPtr != IntPtr.Zero)
				{
					Win32Native.ZeroMemory(intPtr, (uint)(length * 2));
					Win32Native.SysFreeString(intPtr);
				}
				if (pointer != null)
				{
					m_buffer.ReleasePointer();
				}
			}
			return intPtr2;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal unsafe IntPtr ToUniStr(bool allocateFromHeap)
		{
			EnsureNotDisposed();
			int length = m_length;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr intPtr2 = IntPtr.Zero;
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					intPtr = ((!allocateFromHeap) ? Marshal.AllocCoTaskMem((length + 1) * 2) : Marshal.AllocHGlobal((length + 1) * 2));
				}
				if (intPtr == IntPtr.Zero)
				{
					throw new OutOfMemoryException();
				}
				UnProtectMemory();
				m_buffer.AcquirePointer(ref pointer);
				Buffer.memcpyimpl(pointer, (byte*)intPtr.ToPointer(), length * 2);
				char* ptr = (char*)intPtr.ToPointer();
				ptr[length] = '\0';
				intPtr2 = intPtr;
			}
			finally
			{
				ProtectMemory();
				if (intPtr2 == IntPtr.Zero && intPtr != IntPtr.Zero)
				{
					Win32Native.ZeroMemory(intPtr, (uint)(length * 2));
					if (allocateFromHeap)
					{
						Marshal.FreeHGlobal(intPtr);
					}
					else
					{
						Marshal.FreeCoTaskMem(intPtr);
					}
				}
				if (pointer != null)
				{
					m_buffer.ReleasePointer();
				}
			}
			return intPtr2;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal unsafe IntPtr ToAnsiStr(bool allocateFromHeap)
		{
			EnsureNotDisposed();
			IntPtr intPtr = IntPtr.Zero;
			IntPtr intPtr2 = IntPtr.Zero;
			int num = 0;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				UnProtectMemory();
				num = GetAnsiByteCount() + 1;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					intPtr = ((!allocateFromHeap) ? Marshal.AllocCoTaskMem(num) : Marshal.AllocHGlobal(num));
				}
				if (intPtr == IntPtr.Zero)
				{
					throw new OutOfMemoryException();
				}
				GetAnsiBytes((byte*)intPtr.ToPointer(), num);
				intPtr2 = intPtr;
			}
			finally
			{
				ProtectMemory();
				if (intPtr2 == IntPtr.Zero && intPtr != IntPtr.Zero)
				{
					Win32Native.ZeroMemory(intPtr, (uint)num);
					if (allocateFromHeap)
					{
						Marshal.FreeHGlobal(intPtr);
					}
					else
					{
						Marshal.FreeCoTaskMem(intPtr);
					}
				}
			}
			return intPtr2;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private void UnProtectMemory()
		{
			if (m_length == 0)
			{
				return;
			}
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				if (m_enrypted)
				{
					int num = Win32Native.SystemFunction041(m_buffer, (uint)(m_buffer.Length * 2), 0u);
					if (num < 0)
					{
						throw new CryptographicException(Win32Native.LsaNtStatusToWinError(num));
					}
					m_enrypted = false;
				}
			}
		}
	}
}
