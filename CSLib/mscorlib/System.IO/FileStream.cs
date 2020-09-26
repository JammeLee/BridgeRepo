using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO
{
	[ComVisible(true)]
	public class FileStream : Stream
	{
		internal const int DefaultBufferSize = 4096;

		private const int FILE_ATTRIBUTE_NORMAL = 128;

		private const int FILE_ATTRIBUTE_ENCRYPTED = 16384;

		private const int FILE_FLAG_OVERLAPPED = 1073741824;

		internal const int GENERIC_READ = int.MinValue;

		private const int GENERIC_WRITE = 1073741824;

		private const int FILE_BEGIN = 0;

		private const int FILE_CURRENT = 1;

		private const int FILE_END = 2;

		private const int ERROR_BROKEN_PIPE = 109;

		private const int ERROR_NO_DATA = 232;

		private const int ERROR_HANDLE_EOF = 38;

		private const int ERROR_INVALID_PARAMETER = 87;

		private const int ERROR_IO_PENDING = 997;

		private static readonly bool _canUseAsync = Environment.RunningOnWinNT;

		private static readonly IOCompletionCallback IOCallback = AsyncFSCallback;

		private byte[] _buffer;

		private string _fileName;

		private bool _isAsync;

		private bool _canRead;

		private bool _canWrite;

		private bool _canSeek;

		private bool _exposedHandle;

		private bool _isPipe;

		private int _readPos;

		private int _readLen;

		private int _writePos;

		private int _bufferSize;

		private SafeFileHandle _handle;

		private long _pos;

		private long _appendStart;

		public override bool CanRead => _canRead;

		public override bool CanWrite => _canWrite;

		public override bool CanSeek => _canSeek;

		public virtual bool IsAsync => _isAsync;

		public override long Length
		{
			get
			{
				if (_handle.IsClosed)
				{
					__Error.FileNotOpen();
				}
				if (!CanSeek)
				{
					__Error.SeekNotSupported();
				}
				int highSize = 0;
				int num = 0;
				num = Win32Native.GetFileSize(_handle, out highSize);
				if (num == -1)
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error != 0)
					{
						__Error.WinIOError(lastWin32Error, string.Empty);
					}
				}
				long num2 = ((long)highSize << 32) | (uint)num;
				if (_writePos > 0 && _pos + _writePos > num2)
				{
					num2 = _writePos + _pos;
				}
				return num2;
			}
		}

		public string Name
		{
			get
			{
				if (_fileName == null)
				{
					return Environment.GetResourceString("IO_UnknownFileName");
				}
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[1]
				{
					_fileName
				}, checkForDuplicates: false, needFullPath: false).Demand();
				return _fileName;
			}
		}

		internal string NameInternal
		{
			get
			{
				if (_fileName == null)
				{
					return "<UnknownFileName>";
				}
				return _fileName;
			}
		}

		public override long Position
		{
			get
			{
				if (_handle.IsClosed)
				{
					__Error.FileNotOpen();
				}
				if (!CanSeek)
				{
					__Error.SeekNotSupported();
				}
				if (_exposedHandle)
				{
					VerifyOSHandlePosition();
				}
				return _pos + (_readPos - _readLen + _writePos);
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_writePos > 0)
				{
					FlushWrite(calledFromFinalizer: false);
				}
				_readPos = 0;
				_readLen = 0;
				Seek(value, SeekOrigin.Begin);
			}
		}

		[Obsolete("This property has been deprecated.  Please use FileStream's SafeFileHandle property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public virtual IntPtr Handle
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				Flush();
				_readPos = 0;
				_readLen = 0;
				_writePos = 0;
				_exposedHandle = true;
				return _handle.DangerousGetHandle();
			}
		}

		public virtual SafeFileHandle SafeFileHandle
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				Flush();
				_readPos = 0;
				_readLen = 0;
				_writePos = 0;
				_exposedHandle = true;
				return _handle;
			}
		}

		internal FileStream()
		{
			_fileName = null;
			_handle = null;
		}

		public FileStream(string path, FileMode mode)
			: this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.None, Path.GetFileName(path), bFromProxy: false)
		{
		}

		public FileStream(string path, FileMode mode, FileAccess access)
			: this(path, mode, access, FileShare.Read, 4096, FileOptions.None, Path.GetFileName(path), bFromProxy: false)
		{
		}

		public FileStream(string path, FileMode mode, FileAccess access, FileShare share)
			: this(path, mode, access, share, 4096, FileOptions.None, Path.GetFileName(path), bFromProxy: false)
		{
		}

		public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
			: this(path, mode, access, share, bufferSize, FileOptions.None, Path.GetFileName(path), bFromProxy: false)
		{
		}

		public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
			: this(path, mode, access, share, bufferSize, options, Path.GetFileName(path), bFromProxy: false)
		{
		}

		public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
			: this(path, mode, access, share, bufferSize, useAsync ? FileOptions.Asynchronous : FileOptions.None, Path.GetFileName(path), bFromProxy: false)
		{
		}

		public FileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity fileSecurity)
		{
			object pinningHandle;
			Win32Native.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(share, fileSecurity, out pinningHandle);
			try
			{
				Init(path, mode, (FileAccess)0, (int)rights, useRights: true, share, bufferSize, options, secAttrs, Path.GetFileName(path), bFromProxy: false);
			}
			finally
			{
				if (pinningHandle != null)
				{
					((GCHandle)pinningHandle).Free();
				}
			}
		}

		public FileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options)
		{
			Win32Native.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(share);
			Init(path, mode, (FileAccess)0, (int)rights, useRights: true, share, bufferSize, options, secAttrs, Path.GetFileName(path), bFromProxy: false);
		}

		internal FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, string msgPath, bool bFromProxy)
		{
			Win32Native.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(share);
			Init(path, mode, access, 0, useRights: false, share, bufferSize, options, secAttrs, msgPath, bFromProxy);
		}

		internal unsafe void Init(string path, FileMode mode, FileAccess access, int rights, bool useRights, FileShare share, int bufferSize, FileOptions options, Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy)
		{
			_fileName = msgPath;
			_exposedHandle = false;
			if (path == null)
			{
				throw new ArgumentNullException("path", Environment.GetResourceString("ArgumentNull_Path"));
			}
			if (path.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
			}
			if (Environment.IsWin9X())
			{
				if ((share & FileShare.Delete) != 0)
				{
					throw new PlatformNotSupportedException(Environment.GetResourceString("NotSupported_FileShareDeleteOnWin9x"));
				}
				if (useRights)
				{
					throw new PlatformNotSupportedException(Environment.GetResourceString("NotSupported_FileSystemRightsOnWin9x"));
				}
			}
			FileShare fileShare = share & ~FileShare.Inheritable;
			string text = null;
			if (mode < FileMode.CreateNew || mode > FileMode.Append)
			{
				text = "mode";
			}
			else if (!useRights && (access < FileAccess.Read || access > FileAccess.ReadWrite))
			{
				text = "access";
			}
			else if (useRights && (rights < 1 || rights > 2032127))
			{
				text = "rights";
			}
			else if ((fileShare < FileShare.None) || fileShare > (FileShare.ReadWrite | FileShare.Delete))
			{
				text = "share";
			}
			if (text != null)
			{
				throw new ArgumentOutOfRangeException(text, Environment.GetResourceString("ArgumentOutOfRange_Enum"));
			}
			if (options != 0 && (options & (FileOptions)67092479) != 0)
			{
				throw new ArgumentOutOfRangeException("options", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
			}
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			if (((!useRights && (access & FileAccess.Write) == 0) || (useRights && (rights & 0x116) == 0)) && (mode == FileMode.Truncate || mode == FileMode.CreateNew || mode == FileMode.Create || mode == FileMode.Append))
			{
				if (!useRights)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidFileMode&AccessCombo"), mode, access));
				}
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidFileMode&RightsCombo"), mode, (FileSystemRights)rights));
			}
			if (useRights && mode == FileMode.Truncate)
			{
				if (rights != 278)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidFileModeTruncate&RightsCombo"), mode, (FileSystemRights)rights));
				}
				useRights = false;
				access = FileAccess.Write;
			}
			int dwDesiredAccess = (useRights ? rights : (access switch
			{
				FileAccess.Write => 1073741824, 
				FileAccess.Read => int.MinValue, 
				_ => -1073741824, 
			}));
			string text2 = (_fileName = Path.GetFullPathInternal(path));
			if (text2.StartsWith("\\\\.\\", StringComparison.Ordinal))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_DevicesNotSupported"));
			}
			FileIOPermissionAccess fileIOPermissionAccess = FileIOPermissionAccess.NoAccess;
			if ((!useRights && (access & FileAccess.Read) != 0) || (useRights && ((uint)rights & 0x200A9u) != 0))
			{
				if (mode == FileMode.Append)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAppendMode"));
				}
				fileIOPermissionAccess |= FileIOPermissionAccess.Read;
			}
			if ((!useRights && (access & FileAccess.Write) != 0) || (useRights && ((uint)rights & 0xD0156u) != 0))
			{
				fileIOPermissionAccess = ((mode != FileMode.Append) ? (fileIOPermissionAccess | FileIOPermissionAccess.Write) : (fileIOPermissionAccess | FileIOPermissionAccess.Append));
			}
			AccessControlActions control = ((secAttrs != null && secAttrs.pSecurityDescriptor != null) ? AccessControlActions.Change : AccessControlActions.None);
			new FileIOPermission(fileIOPermissionAccess, control, new string[1]
			{
				text2
			}, checkForDuplicates: false, needFullPath: false).Demand();
			share &= ~FileShare.Inheritable;
			bool flag = mode == FileMode.Append;
			if (mode == FileMode.Append)
			{
				mode = FileMode.OpenOrCreate;
			}
			if (_canUseAsync && (options & FileOptions.Asynchronous) != 0)
			{
				_isAsync = true;
			}
			else
			{
				options &= ~FileOptions.Asynchronous;
			}
			int num = (int)options;
			num |= 0x100000;
			int errorMode = Win32Native.SetErrorMode(1);
			try
			{
				_handle = Win32Native.SafeCreateFile(text2, dwDesiredAccess, share, secAttrs, mode, num, Win32Native.NULL);
				if (_handle.IsInvalid)
				{
					int num2 = Marshal.GetLastWin32Error();
					if (num2 == 3 && text2.Equals(Directory.InternalGetDirectoryRoot(text2)))
					{
						num2 = 5;
					}
					bool flag2 = false;
					if (!bFromProxy)
					{
						try
						{
							new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[1]
							{
								_fileName
							}, checkForDuplicates: false, needFullPath: false).Demand();
							flag2 = true;
						}
						catch (SecurityException)
						{
						}
					}
					if (flag2)
					{
						__Error.WinIOError(num2, _fileName);
					}
					else
					{
						__Error.WinIOError(num2, msgPath);
					}
				}
			}
			finally
			{
				Win32Native.SetErrorMode(errorMode);
			}
			int fileType = Win32Native.GetFileType(_handle);
			if (fileType != 1)
			{
				_handle.Close();
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FileStreamOnNonFiles"));
			}
			if (_isAsync)
			{
				bool flag3 = false;
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
				try
				{
					flag3 = ThreadPool.BindHandle(_handle);
				}
				finally
				{
					CodeAccessPermission.RevertAssert();
					if (!flag3)
					{
						_handle.Close();
					}
				}
				if (!flag3)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_BindHandleFailed"));
				}
			}
			if (!useRights)
			{
				_canRead = (access & FileAccess.Read) != 0;
				_canWrite = (access & FileAccess.Write) != 0;
			}
			else
			{
				_canRead = (rights & 1) != 0;
				_canWrite = ((uint)rights & 2u) != 0 || (rights & 4) != 0;
			}
			_canSeek = true;
			_isPipe = false;
			_pos = 0L;
			_bufferSize = bufferSize;
			_readPos = 0;
			_readLen = 0;
			_writePos = 0;
			if (flag)
			{
				_appendStart = SeekCore(0L, SeekOrigin.End);
			}
			else
			{
				_appendStart = -1L;
			}
		}

		[Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public FileStream(IntPtr handle, FileAccess access)
			: this(handle, access, ownsHandle: true, 4096, isAsync: false)
		{
		}

		[Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public FileStream(IntPtr handle, FileAccess access, bool ownsHandle)
			: this(handle, access, ownsHandle, 4096, isAsync: false)
		{
		}

		[Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access, int bufferSize) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
			: this(handle, access, ownsHandle, bufferSize, isAsync: false)
		{
		}

		[Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync)
			: this(new SafeFileHandle(handle, ownsHandle), access, bufferSize, isAsync)
		{
		}

		public FileStream(SafeFileHandle handle, FileAccess access)
			: this(handle, access, 4096, isAsync: false)
		{
		}

		public FileStream(SafeFileHandle handle, FileAccess access, int bufferSize)
			: this(handle, access, bufferSize, isAsync: false)
		{
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
		{
			if (handle.IsInvalid)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidHandle"), "handle");
			}
			_handle = handle;
			_exposedHandle = true;
			if (access < FileAccess.Read || access > FileAccess.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("access", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
			}
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			int fileType = Win32Native.GetFileType(_handle);
			_isAsync = isAsync && _canUseAsync;
			_canRead = 0 != (access & FileAccess.Read);
			_canWrite = 0 != (access & FileAccess.Write);
			_canSeek = fileType == 1;
			_bufferSize = bufferSize;
			_readPos = 0;
			_readLen = 0;
			_writePos = 0;
			_fileName = null;
			_isPipe = fileType == 3;
			if (_isAsync)
			{
				bool flag = false;
				try
				{
					flag = ThreadPool.BindHandle(_handle);
				}
				catch (ApplicationException)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_HandleNotAsync"));
				}
				if (!flag)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_BindHandleFailed"));
				}
			}
			else if (fileType != 3)
			{
				VerifyHandleIsSync();
			}
			if (_canSeek)
			{
				SeekCore(0L, SeekOrigin.Current);
			}
			else
			{
				_pos = 0L;
			}
		}

		private static Win32Native.SECURITY_ATTRIBUTES GetSecAttrs(FileShare share)
		{
			Win32Native.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = null;
			if ((share & FileShare.Inheritable) != 0)
			{
				sECURITY_ATTRIBUTES = new Win32Native.SECURITY_ATTRIBUTES();
				sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
				sECURITY_ATTRIBUTES.bInheritHandle = 1;
			}
			return sECURITY_ATTRIBUTES;
		}

		private unsafe static Win32Native.SECURITY_ATTRIBUTES GetSecAttrs(FileShare share, FileSecurity fileSecurity, out object pinningHandle)
		{
			pinningHandle = null;
			Win32Native.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = null;
			if ((share & FileShare.Inheritable) != 0 || fileSecurity != null)
			{
				sECURITY_ATTRIBUTES = new Win32Native.SECURITY_ATTRIBUTES();
				sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
				if ((share & FileShare.Inheritable) != 0)
				{
					sECURITY_ATTRIBUTES.bInheritHandle = 1;
				}
				if (fileSecurity != null)
				{
					byte[] securityDescriptorBinaryForm = fileSecurity.GetSecurityDescriptorBinaryForm();
					pinningHandle = GCHandle.Alloc(securityDescriptorBinaryForm, GCHandleType.Pinned);
					fixed (byte* pSecurityDescriptor = securityDescriptorBinaryForm)
					{
						sECURITY_ATTRIBUTES.pSecurityDescriptor = pSecurityDescriptor;
					}
				}
			}
			return sECURITY_ATTRIBUTES;
		}

		private void VerifyHandleIsSync()
		{
			byte[] bytes = new byte[1];
			int hr = 0;
			if (CanRead)
			{
				ReadFileNative(_handle, bytes, 0, 0, null, out hr);
			}
			else if (CanWrite)
			{
				WriteFileNative(_handle, bytes, 0, 0, null, out hr);
			}
			switch (hr)
			{
			case 87:
				throw new ArgumentException(Environment.GetResourceString("Arg_HandleNotSync"));
			case 6:
				__Error.WinIOError(hr, "<OS handle>");
				break;
			}
		}

		public FileSecurity GetAccessControl()
		{
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			return new FileSecurity(_handle, _fileName, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public void SetAccessControl(FileSecurity fileSecurity)
		{
			if (fileSecurity == null)
			{
				throw new ArgumentNullException("fileSecurity");
			}
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			fileSecurity.Persist(_handle, _fileName);
		}

		private unsafe static void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
		{
			Overlapped overlapped = Overlapped.Unpack(pOverlapped);
			FileStreamAsyncResult fileStreamAsyncResult = (FileStreamAsyncResult)overlapped.AsyncResult;
			fileStreamAsyncResult._numBytes = (int)numBytes;
			if (errorCode == 109 || errorCode == 232)
			{
				errorCode = 0u;
			}
			fileStreamAsyncResult._errorCode = (int)errorCode;
			fileStreamAsyncResult._completedSynchronously = false;
			fileStreamAsyncResult._isComplete = true;
			ManualResetEvent waitHandle = fileStreamAsyncResult._waitHandle;
			if (waitHandle != null && !waitHandle.Set())
			{
				__Error.WinIOError();
			}
			fileStreamAsyncResult._userCallback?.Invoke(fileStreamAsyncResult);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (_handle != null && !_handle.IsClosed && _writePos > 0)
				{
					FlushWrite(!disposing);
				}
			}
			finally
			{
				if (_handle != null && !_handle.IsClosed)
				{
					_handle.Dispose();
				}
				_canRead = false;
				_canWrite = false;
				_canSeek = false;
				base.Dispose(disposing);
			}
		}

		~FileStream()
		{
			if (_handle != null)
			{
				Dispose(disposing: false);
			}
		}

		public override void Flush()
		{
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (_writePos > 0)
			{
				FlushWrite(calledFromFinalizer: false);
			}
			else if (_readPos < _readLen && CanSeek)
			{
				FlushRead();
			}
			_readPos = 0;
			_readLen = 0;
		}

		private void FlushRead()
		{
			if (_readPos - _readLen != 0)
			{
				SeekCore(_readPos - _readLen, SeekOrigin.Current);
			}
			_readPos = 0;
			_readLen = 0;
		}

		private void FlushWrite(bool calledFromFinalizer)
		{
			if (_isAsync)
			{
				IAsyncResult asyncResult = BeginWriteCore(_buffer, 0, _writePos, null, null);
				if (!calledFromFinalizer)
				{
					EndWrite(asyncResult);
				}
			}
			else
			{
				WriteCore(_buffer, 0, _writePos);
			}
			_writePos = 0;
		}

		public override void SetLength(long value)
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (!CanSeek)
			{
				__Error.SeekNotSupported();
			}
			if (!CanWrite)
			{
				__Error.WriteNotSupported();
			}
			if (_writePos > 0)
			{
				FlushWrite(calledFromFinalizer: false);
			}
			else if (_readPos < _readLen)
			{
				FlushRead();
			}
			_readPos = 0;
			_readLen = 0;
			if (_appendStart != -1 && value < _appendStart)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_SetLengthAppendTruncate"));
			}
			SetLengthCore(value);
		}

		private void SetLengthCore(long value)
		{
			long pos = _pos;
			if (_exposedHandle)
			{
				VerifyOSHandlePosition();
			}
			if (_pos != value)
			{
				SeekCore(value, SeekOrigin.Begin);
			}
			if (!Win32Native.SetEndOfFile(_handle))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 87)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_FileLengthTooBig"));
				}
				__Error.WinIOError(lastWin32Error, string.Empty);
			}
			if (pos != value)
			{
				if (pos < value)
				{
					SeekCore(pos, SeekOrigin.Begin);
				}
				else
				{
					SeekCore(0L, SeekOrigin.End);
				}
			}
		}

		public override int Read([In][Out] byte[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			bool flag = false;
			int num = _readLen - _readPos;
			if (num == 0)
			{
				if (!CanRead)
				{
					__Error.ReadNotSupported();
				}
				if (_writePos > 0)
				{
					FlushWrite(calledFromFinalizer: false);
				}
				if (!CanSeek || count >= _bufferSize)
				{
					num = ReadCore(array, offset, count);
					_readPos = 0;
					_readLen = 0;
					return num;
				}
				if (_buffer == null)
				{
					_buffer = new byte[_bufferSize];
				}
				num = ReadCore(_buffer, 0, _bufferSize);
				if (num == 0)
				{
					return 0;
				}
				flag = num < _bufferSize;
				_readPos = 0;
				_readLen = num;
			}
			if (num > count)
			{
				num = count;
			}
			Buffer.InternalBlockCopy(_buffer, _readPos, array, offset, num);
			_readPos += num;
			if (!_isPipe && num < count && !flag)
			{
				int num2 = ReadCore(array, offset + num, count - num);
				num += num2;
				_readPos = 0;
				_readLen = 0;
			}
			return num;
		}

		private int ReadCore(byte[] buffer, int offset, int count)
		{
			if (_isAsync)
			{
				IAsyncResult asyncResult = BeginReadCore(buffer, offset, count, null, null, 0);
				return EndRead(asyncResult);
			}
			if (_exposedHandle)
			{
				VerifyOSHandlePosition();
			}
			int hr = 0;
			int num = ReadFileNative(_handle, buffer, offset, count, null, out hr);
			if (num == -1)
			{
				switch (hr)
				{
				case 109:
					num = 0;
					break;
				case 87:
					throw new ArgumentException(Environment.GetResourceString("Arg_HandleNotSync"));
				default:
					__Error.WinIOError(hr, string.Empty);
					break;
				}
			}
			_pos += num;
			return num;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin < SeekOrigin.Begin || origin > SeekOrigin.End)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSeekOrigin"));
			}
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (!CanSeek)
			{
				__Error.SeekNotSupported();
			}
			if (_writePos > 0)
			{
				FlushWrite(calledFromFinalizer: false);
			}
			else if (origin == SeekOrigin.Current)
			{
				offset -= _readLen - _readPos;
			}
			if (_exposedHandle)
			{
				VerifyOSHandlePosition();
			}
			long num = _pos + (_readPos - _readLen);
			long num2 = SeekCore(offset, origin);
			if (_appendStart != -1 && num2 < _appendStart)
			{
				SeekCore(num, SeekOrigin.Begin);
				throw new IOException(Environment.GetResourceString("IO.IO_SeekAppendOverwrite"));
			}
			if (_readLen > 0)
			{
				if (num == num2)
				{
					if (_readPos > 0)
					{
						Buffer.InternalBlockCopy(_buffer, _readPos, _buffer, 0, _readLen - _readPos);
						_readLen -= _readPos;
						_readPos = 0;
					}
					if (_readLen > 0)
					{
						SeekCore(_readLen, SeekOrigin.Current);
					}
				}
				else if (num - _readPos < num2 && num2 < num + _readLen - _readPos)
				{
					int num3 = (int)(num2 - num);
					Buffer.InternalBlockCopy(_buffer, _readPos + num3, _buffer, 0, _readLen - (_readPos + num3));
					_readLen -= _readPos + num3;
					_readPos = 0;
					if (_readLen > 0)
					{
						SeekCore(_readLen, SeekOrigin.Current);
					}
				}
				else
				{
					_readPos = 0;
					_readLen = 0;
				}
			}
			return num2;
		}

		private long SeekCore(long offset, SeekOrigin origin)
		{
			int hr = 0;
			long num = 0L;
			num = Win32Native.SetFilePointer(_handle, offset, origin, out hr);
			if (num == -1)
			{
				if (hr == 6 && !_handle.IsInvalid)
				{
					_handle.Dispose();
				}
				__Error.WinIOError(hr, string.Empty);
			}
			_pos = num;
			return num;
		}

		private void VerifyOSHandlePosition()
		{
			if (!CanSeek)
			{
				return;
			}
			long pos = _pos;
			long num = SeekCore(0L, SeekOrigin.Current);
			if (num != pos)
			{
				_readPos = 0;
				_readLen = 0;
				if (_writePos > 0)
				{
					_writePos = 0;
					throw new IOException(Environment.GetResourceString("IO.IO_FileStreamHandlePosition"));
				}
			}
		}

		public override void Write(byte[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (_writePos == 0)
			{
				if (!CanWrite)
				{
					__Error.WriteNotSupported();
				}
				if (_readPos < _readLen)
				{
					FlushRead();
				}
				_readPos = 0;
				_readLen = 0;
			}
			if (_writePos > 0)
			{
				int num = _bufferSize - _writePos;
				if (num > 0)
				{
					if (num > count)
					{
						num = count;
					}
					Buffer.InternalBlockCopy(array, offset, _buffer, _writePos, num);
					_writePos += num;
					if (count == num)
					{
						return;
					}
					offset += num;
					count -= num;
				}
				if (_isAsync)
				{
					IAsyncResult asyncResult = BeginWriteCore(_buffer, 0, _writePos, null, null);
					EndWrite(asyncResult);
				}
				else
				{
					WriteCore(_buffer, 0, _writePos);
				}
				_writePos = 0;
			}
			if (count >= _bufferSize)
			{
				WriteCore(array, offset, count);
			}
			else if (count != 0)
			{
				if (_buffer == null)
				{
					_buffer = new byte[_bufferSize];
				}
				Buffer.InternalBlockCopy(array, offset, _buffer, _writePos, count);
				_writePos = count;
			}
		}

		private void WriteCore(byte[] buffer, int offset, int count)
		{
			if (_isAsync)
			{
				IAsyncResult asyncResult = BeginWriteCore(buffer, offset, count, null, null);
				EndWrite(asyncResult);
				return;
			}
			if (_exposedHandle)
			{
				VerifyOSHandlePosition();
			}
			int hr = 0;
			int num = WriteFileNative(_handle, buffer, offset, count, null, out hr);
			if (num == -1)
			{
				switch (hr)
				{
				case 232:
					num = 0;
					break;
				case 87:
					throw new IOException(Environment.GetResourceString("IO.IO_FileTooLongOrHandleNotSync"));
				default:
					__Error.WinIOError(hr, string.Empty);
					break;
				}
			}
			_pos += num;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (numBytes < 0)
			{
				throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - offset < numBytes)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (!_isAsync)
			{
				return base.BeginRead(array, offset, numBytes, userCallback, stateObject);
			}
			if (!CanRead)
			{
				__Error.ReadNotSupported();
			}
			FileStreamAsyncResult fileStreamAsyncResult = null;
			if (_isPipe)
			{
				if (_readPos < _readLen)
				{
					int num = _readLen - _readPos;
					if (num > numBytes)
					{
						num = numBytes;
					}
					Buffer.InternalBlockCopy(_buffer, _readPos, array, offset, num);
					_readPos += num;
					fileStreamAsyncResult = FileStreamAsyncResult.CreateBufferedReadResult(num, userCallback, stateObject);
					fileStreamAsyncResult.CallUserCallback();
					return fileStreamAsyncResult;
				}
				return BeginReadCore(array, offset, numBytes, userCallback, stateObject, 0);
			}
			if (_writePos > 0)
			{
				FlushWrite(calledFromFinalizer: false);
			}
			if (_readPos == _readLen)
			{
				if (numBytes < _bufferSize)
				{
					if (_buffer == null)
					{
						_buffer = new byte[_bufferSize];
					}
					IAsyncResult asyncResult = BeginReadCore(_buffer, 0, _bufferSize, null, null, 0);
					_readLen = EndRead(asyncResult);
					int num2 = _readLen;
					if (num2 > numBytes)
					{
						num2 = numBytes;
					}
					Buffer.InternalBlockCopy(_buffer, 0, array, offset, num2);
					_readPos = num2;
					fileStreamAsyncResult = FileStreamAsyncResult.CreateBufferedReadResult(num2, userCallback, stateObject);
					fileStreamAsyncResult.CallUserCallback();
					return fileStreamAsyncResult;
				}
				_readPos = 0;
				_readLen = 0;
				return BeginReadCore(array, offset, numBytes, userCallback, stateObject, 0);
			}
			int num3 = _readLen - _readPos;
			if (num3 > numBytes)
			{
				num3 = numBytes;
			}
			Buffer.InternalBlockCopy(_buffer, _readPos, array, offset, num3);
			_readPos += num3;
			if (num3 >= numBytes || _isPipe)
			{
				fileStreamAsyncResult = FileStreamAsyncResult.CreateBufferedReadResult(num3, userCallback, stateObject);
				fileStreamAsyncResult.CallUserCallback();
				return fileStreamAsyncResult;
			}
			_readPos = 0;
			_readLen = 0;
			return BeginReadCore(array, offset + num3, numBytes - num3, userCallback, stateObject, num3);
		}

		private unsafe FileStreamAsyncResult BeginReadCore(byte[] bytes, int offset, int numBytes, AsyncCallback userCallback, object stateObject, int numBufferedBytesRead)
		{
			FileStreamAsyncResult fileStreamAsyncResult = new FileStreamAsyncResult();
			fileStreamAsyncResult._handle = _handle;
			fileStreamAsyncResult._userCallback = userCallback;
			fileStreamAsyncResult._userStateObject = stateObject;
			fileStreamAsyncResult._isWrite = false;
			fileStreamAsyncResult._numBufferedBytes = numBufferedBytesRead;
			ManualResetEvent manualResetEvent = (fileStreamAsyncResult._waitHandle = new ManualResetEvent(initialState: false));
			Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, fileStreamAsyncResult);
			NativeOverlapped* ptr = (fileStreamAsyncResult._overlapped = ((userCallback == null) ? overlapped.UnsafePack(null, bytes) : overlapped.Pack(IOCallback, bytes)));
			if (CanSeek)
			{
				long length = Length;
				if (_exposedHandle)
				{
					VerifyOSHandlePosition();
				}
				if (_pos + numBytes > length)
				{
					numBytes = (int)((_pos <= length) ? (length - _pos) : 0);
				}
				ptr->OffsetLow = (int)_pos;
				ptr->OffsetHigh = (int)(_pos >> 32);
				SeekCore(numBytes, SeekOrigin.Current);
			}
			int hr = 0;
			int num = ReadFileNative(_handle, bytes, offset, numBytes, ptr, out hr);
			if (num == -1 && numBytes != -1)
			{
				switch (hr)
				{
				case 109:
					ptr->InternalLow = IntPtr.Zero;
					fileStreamAsyncResult.CallUserCallback();
					break;
				default:
					if (!_handle.IsClosed && CanSeek)
					{
						SeekCore(0L, SeekOrigin.Current);
					}
					if (hr == 38)
					{
						__Error.EndOfFile();
					}
					else
					{
						__Error.WinIOError(hr, string.Empty);
					}
					break;
				case 997:
					break;
				}
			}
			return fileStreamAsyncResult;
		}

		public unsafe override int EndRead(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			if (!_isAsync)
			{
				return base.EndRead(asyncResult);
			}
			FileStreamAsyncResult fileStreamAsyncResult = asyncResult as FileStreamAsyncResult;
			if (fileStreamAsyncResult == null || fileStreamAsyncResult._isWrite)
			{
				__Error.WrongAsyncResult();
			}
			if (1 == Interlocked.CompareExchange(ref fileStreamAsyncResult._EndXxxCalled, 1, 0))
			{
				__Error.EndReadCalledTwice();
			}
			WaitHandle waitHandle = fileStreamAsyncResult._waitHandle;
			if (waitHandle != null)
			{
				try
				{
					waitHandle.WaitOne();
				}
				finally
				{
					waitHandle.Close();
				}
			}
			NativeOverlapped* overlapped = fileStreamAsyncResult._overlapped;
			if (overlapped != null)
			{
				Overlapped.Free(overlapped);
			}
			if (fileStreamAsyncResult._errorCode != 0)
			{
				__Error.WinIOError(fileStreamAsyncResult._errorCode, Path.GetFileName(_fileName));
			}
			return fileStreamAsyncResult._numBytes + fileStreamAsyncResult._numBufferedBytes;
		}

		public override int ReadByte()
		{
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (_readLen == 0 && !CanRead)
			{
				__Error.ReadNotSupported();
			}
			if (_readPos == _readLen)
			{
				if (_writePos > 0)
				{
					FlushWrite(calledFromFinalizer: false);
				}
				if (_buffer == null)
				{
					_buffer = new byte[_bufferSize];
				}
				_readLen = ReadCore(_buffer, 0, _bufferSize);
				_readPos = 0;
			}
			if (_readPos == _readLen)
			{
				return -1;
			}
			int result = _buffer[_readPos];
			_readPos++;
			return result;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (numBytes < 0)
			{
				throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - offset < numBytes)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (!_isAsync)
			{
				return base.BeginWrite(array, offset, numBytes, userCallback, stateObject);
			}
			if (!CanWrite)
			{
				__Error.WriteNotSupported();
			}
			if (_isPipe)
			{
				if (_writePos > 0)
				{
					FlushWrite(calledFromFinalizer: false);
				}
				return BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
			}
			if (_writePos == 0)
			{
				if (_readPos < _readLen)
				{
					FlushRead();
				}
				_readPos = 0;
				_readLen = 0;
			}
			int num = _bufferSize - _writePos;
			if (numBytes <= num)
			{
				if (_writePos == 0)
				{
					_buffer = new byte[_bufferSize];
				}
				Buffer.InternalBlockCopy(array, offset, _buffer, _writePos, numBytes);
				_writePos += numBytes;
				FileStreamAsyncResult fileStreamAsyncResult = new FileStreamAsyncResult();
				fileStreamAsyncResult._userCallback = userCallback;
				fileStreamAsyncResult._userStateObject = stateObject;
				fileStreamAsyncResult._waitHandle = null;
				fileStreamAsyncResult._isWrite = true;
				fileStreamAsyncResult._numBufferedBytes = numBytes;
				fileStreamAsyncResult.CallUserCallback();
				return fileStreamAsyncResult;
			}
			if (_writePos > 0)
			{
				FlushWrite(calledFromFinalizer: false);
			}
			return BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
		}

		private unsafe FileStreamAsyncResult BeginWriteCore(byte[] bytes, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			FileStreamAsyncResult fileStreamAsyncResult = new FileStreamAsyncResult();
			fileStreamAsyncResult._handle = _handle;
			fileStreamAsyncResult._userCallback = userCallback;
			fileStreamAsyncResult._userStateObject = stateObject;
			fileStreamAsyncResult._isWrite = true;
			ManualResetEvent manualResetEvent = (fileStreamAsyncResult._waitHandle = new ManualResetEvent(initialState: false));
			Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, fileStreamAsyncResult);
			NativeOverlapped* ptr = (fileStreamAsyncResult._overlapped = ((userCallback == null) ? overlapped.UnsafePack(null, bytes) : overlapped.Pack(IOCallback, bytes)));
			if (CanSeek)
			{
				long length = Length;
				if (_exposedHandle)
				{
					VerifyOSHandlePosition();
				}
				if (_pos + numBytes > length)
				{
					SetLengthCore(_pos + numBytes);
				}
				ptr->OffsetLow = (int)_pos;
				ptr->OffsetHigh = (int)(_pos >> 32);
				SeekCore(numBytes, SeekOrigin.Current);
			}
			int hr = 0;
			int num = WriteFileNative(_handle, bytes, offset, numBytes, ptr, out hr);
			if (num == -1 && numBytes != -1)
			{
				switch (hr)
				{
				case 232:
					fileStreamAsyncResult.CallUserCallback();
					break;
				default:
					if (!_handle.IsClosed && CanSeek)
					{
						SeekCore(0L, SeekOrigin.Current);
					}
					if (hr == 38)
					{
						__Error.EndOfFile();
					}
					else
					{
						__Error.WinIOError(hr, string.Empty);
					}
					break;
				case 997:
					break;
				}
			}
			return fileStreamAsyncResult;
		}

		public unsafe override void EndWrite(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			if (!_isAsync)
			{
				base.EndWrite(asyncResult);
				return;
			}
			FileStreamAsyncResult fileStreamAsyncResult = asyncResult as FileStreamAsyncResult;
			if (fileStreamAsyncResult == null || !fileStreamAsyncResult._isWrite)
			{
				__Error.WrongAsyncResult();
			}
			if (1 == Interlocked.CompareExchange(ref fileStreamAsyncResult._EndXxxCalled, 1, 0))
			{
				__Error.EndWriteCalledTwice();
			}
			WaitHandle waitHandle = fileStreamAsyncResult._waitHandle;
			if (waitHandle != null)
			{
				try
				{
					waitHandle.WaitOne();
				}
				finally
				{
					waitHandle.Close();
				}
			}
			NativeOverlapped* overlapped = fileStreamAsyncResult._overlapped;
			if (overlapped != null)
			{
				Overlapped.Free(overlapped);
			}
			if (fileStreamAsyncResult._errorCode != 0)
			{
				__Error.WinIOError(fileStreamAsyncResult._errorCode, Path.GetFileName(_fileName));
			}
		}

		public override void WriteByte(byte value)
		{
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (_writePos == 0)
			{
				if (!CanWrite)
				{
					__Error.WriteNotSupported();
				}
				if (_readPos < _readLen)
				{
					FlushRead();
				}
				_readPos = 0;
				_readLen = 0;
				if (_buffer == null)
				{
					_buffer = new byte[_bufferSize];
				}
			}
			if (_writePos == _bufferSize)
			{
				FlushWrite(calledFromFinalizer: false);
			}
			_buffer[_writePos] = value;
			_writePos++;
		}

		public virtual void Lock(long position, long length)
		{
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (position < 0 || length < 0)
			{
				throw new ArgumentOutOfRangeException((position < 0) ? "position" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			int offsetLow = (int)position;
			int offsetHigh = (int)(position >> 32);
			int countLow = (int)length;
			int countHigh = (int)(length >> 32);
			if (!Win32Native.LockFile(_handle, offsetLow, offsetHigh, countLow, countHigh))
			{
				__Error.WinIOError();
			}
		}

		public virtual void Unlock(long position, long length)
		{
			if (_handle.IsClosed)
			{
				__Error.FileNotOpen();
			}
			if (position < 0 || length < 0)
			{
				throw new ArgumentOutOfRangeException((position < 0) ? "position" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			int offsetLow = (int)position;
			int offsetHigh = (int)(position >> 32);
			int countLow = (int)length;
			int countHigh = (int)(length >> 32);
			if (!Win32Native.UnlockFile(_handle, offsetLow, offsetHigh, countLow, countHigh))
			{
				__Error.WinIOError();
			}
		}

		private unsafe int ReadFileNative(SafeFileHandle handle, byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
		{
			if (bytes.Length - offset < count)
			{
				throw new IndexOutOfRangeException(Environment.GetResourceString("IndexOutOfRange_IORaceCondition"));
			}
			if (bytes.Length == 0)
			{
				hr = 0;
				return 0;
			}
			int num = 0;
			int numBytesRead = 0;
			fixed (byte* ptr = bytes)
			{
				num = ((!_isAsync) ? Win32Native.ReadFile(handle, ptr + offset, count, out numBytesRead, IntPtr.Zero) : Win32Native.ReadFile(handle, ptr + offset, count, IntPtr.Zero, overlapped));
			}
			if (num == 0)
			{
				hr = Marshal.GetLastWin32Error();
				if (hr == 109 || hr == 233)
				{
					return -1;
				}
				if (hr == 6 && !_handle.IsInvalid)
				{
					_handle.Dispose();
				}
				return -1;
			}
			hr = 0;
			return numBytesRead;
		}

		private unsafe int WriteFileNative(SafeFileHandle handle, byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
		{
			if (bytes.Length - offset < count)
			{
				throw new IndexOutOfRangeException(Environment.GetResourceString("IndexOutOfRange_IORaceCondition"));
			}
			if (bytes.Length == 0)
			{
				hr = 0;
				return 0;
			}
			int numBytesWritten = 0;
			int num = 0;
			fixed (byte* ptr = bytes)
			{
				num = ((!_isAsync) ? Win32Native.WriteFile(handle, ptr + offset, count, out numBytesWritten, IntPtr.Zero) : Win32Native.WriteFile(handle, ptr + offset, count, IntPtr.Zero, overlapped));
			}
			if (num == 0)
			{
				hr = Marshal.GetLastWin32Error();
				if (hr == 232)
				{
					return -1;
				}
				if (hr == 6 && !_handle.IsInvalid)
				{
					_handle.Dispose();
				}
				return -1;
			}
			hr = 0;
			return numBytesWritten;
		}
	}
}
