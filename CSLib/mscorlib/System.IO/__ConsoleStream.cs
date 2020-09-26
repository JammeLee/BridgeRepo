using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO
{
	internal sealed class __ConsoleStream : Stream
	{
		internal const int DefaultBufferSize = 128;

		private const int ERROR_BROKEN_PIPE = 109;

		private const int ERROR_NO_DATA = 232;

		private SafeFileHandle _handle;

		private bool _canRead;

		private bool _canWrite;

		public override bool CanRead => _canRead;

		public override bool CanWrite => _canWrite;

		public override bool CanSeek => false;

		public override long Length
		{
			get
			{
				__Error.SeekNotSupported();
				return 0L;
			}
		}

		public override long Position
		{
			get
			{
				__Error.SeekNotSupported();
				return 0L;
			}
			set
			{
				__Error.SeekNotSupported();
			}
		}

		internal __ConsoleStream(SafeFileHandle handle, FileAccess access)
		{
			_handle = handle;
			_canRead = access == FileAccess.Read;
			_canWrite = access == FileAccess.Write;
		}

		protected override void Dispose(bool disposing)
		{
			if (_handle != null)
			{
				_handle = null;
			}
			_canRead = false;
			_canWrite = false;
			base.Dispose(disposing);
		}

		public override void Flush()
		{
			if (_handle == null)
			{
				__Error.FileNotOpen();
			}
			if (!CanWrite)
			{
				__Error.WriteNotSupported();
			}
		}

		public override void SetLength(long value)
		{
			__Error.SeekNotSupported();
		}

		public override int Read([In][Out] byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((offset < 0) ? "offset" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (buffer.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (!_canRead)
			{
				__Error.ReadNotSupported();
			}
			int errorCode = 0;
			int num = ReadFileNative(_handle, buffer, offset, count, 0, out errorCode);
			if (num == -1)
			{
				__Error.WinIOError(errorCode, string.Empty);
			}
			return num;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			__Error.SeekNotSupported();
			return 0L;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((offset < 0) ? "offset" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (buffer.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (!_canWrite)
			{
				__Error.WriteNotSupported();
			}
			int errorCode = 0;
			int num = WriteFileNative(_handle, buffer, offset, count, 0, out errorCode);
			if (num == -1)
			{
				__Error.WinIOError(errorCode, string.Empty);
			}
		}

		private unsafe static int ReadFileNative(SafeFileHandle hFile, byte[] bytes, int offset, int count, int mustBeZero, out int errorCode)
		{
			if (bytes.Length - offset < count)
			{
				throw new IndexOutOfRangeException(Environment.GetResourceString("IndexOutOfRange_IORaceCondition"));
			}
			if (bytes.Length == 0)
			{
				errorCode = 0;
				return 0;
			}
			int num;
			int numBytesRead;
			fixed (byte* ptr = bytes)
			{
				num = ReadFile(hFile, ptr + offset, count, out numBytesRead, Win32Native.NULL);
			}
			if (num == 0)
			{
				errorCode = Marshal.GetLastWin32Error();
				if (errorCode == 109)
				{
					return 0;
				}
				return -1;
			}
			errorCode = 0;
			return numBytesRead;
		}

		private unsafe static int WriteFileNative(SafeFileHandle hFile, byte[] bytes, int offset, int count, int mustBeZero, out int errorCode)
		{
			if (bytes.Length == 0)
			{
				errorCode = 0;
				return 0;
			}
			int numBytesWritten = 0;
			int num;
			fixed (byte* ptr = bytes)
			{
				num = WriteFile(hFile, ptr + offset, count, out numBytesWritten, Win32Native.NULL);
			}
			if (num == 0)
			{
				errorCode = Marshal.GetLastWin32Error();
				if (errorCode == 232 || errorCode == 109)
				{
					return 0;
				}
				return -1;
			}
			errorCode = 0;
			return numBytesWritten;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[SuppressUnmanagedCodeSecurity]
		private unsafe static extern int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);

		[DllImport("kernel32.dll", SetLastError = true)]
		[SuppressUnmanagedCodeSecurity]
		internal unsafe static extern int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);
	}
}
