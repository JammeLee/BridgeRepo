using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.IO
{
	internal static class __Error
	{
		internal const int ERROR_FILE_NOT_FOUND = 2;

		internal const int ERROR_PATH_NOT_FOUND = 3;

		internal const int ERROR_ACCESS_DENIED = 5;

		internal const int ERROR_INVALID_PARAMETER = 87;

		internal static void EndOfFile()
		{
			throw new EndOfStreamException(Environment.GetResourceString("IO.EOF_ReadBeyondEOF"));
		}

		internal static void FileNotOpen()
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_FileClosed"));
		}

		internal static void StreamIsClosed()
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_StreamClosed"));
		}

		internal static void MemoryStreamNotExpandable()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_MemStreamNotExpandable"));
		}

		internal static void ReaderClosed()
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ReaderClosed"));
		}

		internal static void ReadNotSupported()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnreadableStream"));
		}

		internal static void SeekNotSupported()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
		}

		internal static void WrongAsyncResult()
		{
			throw new ArgumentException(Environment.GetResourceString("Arg_WrongAsyncResult"));
		}

		internal static void EndReadCalledTwice()
		{
			throw new ArgumentException(Environment.GetResourceString("InvalidOperation_EndReadCalledMultiple"));
		}

		internal static void EndWriteCalledTwice()
		{
			throw new ArgumentException(Environment.GetResourceString("InvalidOperation_EndWriteCalledMultiple"));
		}

		internal static string GetDisplayablePath(string path, bool isInvalidPath)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}
			bool flag = false;
			if (path.Length < 2)
			{
				return path;
			}
			if (Path.IsDirectorySeparator(path[0]) && Path.IsDirectorySeparator(path[1]))
			{
				flag = true;
			}
			else if (path[1] == Path.VolumeSeparatorChar)
			{
				flag = true;
			}
			if (!flag && !isInvalidPath)
			{
				return path;
			}
			bool flag2 = false;
			try
			{
				if (!isInvalidPath)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[1]
					{
						path
					}, checkForDuplicates: false, needFullPath: false).Demand();
					flag2 = true;
				}
			}
			catch (ArgumentException)
			{
			}
			catch (NotSupportedException)
			{
			}
			catch (SecurityException)
			{
			}
			if (!flag2)
			{
				path = ((!Path.IsDirectorySeparator(path[path.Length - 1])) ? Path.GetFileName(path) : Environment.GetResourceString("IO.IO_NoPermissionToDirectoryName"));
			}
			return path;
		}

		internal static void WinIOError()
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			WinIOError(lastWin32Error, string.Empty);
		}

		internal static void WinIOError(int errorCode, string maybeFullPath)
		{
			bool isInvalidPath = errorCode == 123 || errorCode == 161;
			string displayablePath = GetDisplayablePath(maybeFullPath, isInvalidPath);
			switch (errorCode)
			{
			case 2:
				if (displayablePath.Length == 0)
				{
					throw new FileNotFoundException(Environment.GetResourceString("IO.FileNotFound"));
				}
				throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.FileNotFound_FileName"), displayablePath), displayablePath);
			case 3:
				if (displayablePath.Length == 0)
				{
					throw new DirectoryNotFoundException(Environment.GetResourceString("IO.PathNotFound_NoPathName"));
				}
				throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.PathNotFound_Path"), displayablePath));
			case 5:
				if (displayablePath.Length == 0)
				{
					throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_IODenied_NoPathName"));
				}
				throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("UnauthorizedAccess_IODenied_Path"), displayablePath));
			case 183:
				if (displayablePath.Length != 0)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_AlreadyExists_Name", displayablePath), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
				}
				break;
			case 206:
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			case 15:
				throw new DriveNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.DriveNotFound_Drive"), displayablePath));
			case 87:
				throw new IOException(Win32Native.GetMessage(errorCode), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
			case 32:
				if (displayablePath.Length == 0)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_SharingViolation_NoFileName"), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
				}
				throw new IOException(Environment.GetResourceString("IO.IO_SharingViolation_File", displayablePath), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
			case 80:
				if (displayablePath.Length != 0)
				{
					throw new IOException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.IO_FileExists_Name"), displayablePath), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
				}
				break;
			case 995:
				throw new OperationCanceledException();
			}
			throw new IOException(Win32Native.GetMessage(errorCode), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
		}

		internal static void WinIODriveError(string driveName)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			WinIODriveError(driveName, lastWin32Error);
		}

		internal static void WinIODriveError(string driveName, int errorCode)
		{
			if (errorCode == 3 || errorCode == 15)
			{
				throw new DriveNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.DriveNotFound_Drive"), driveName));
			}
			WinIOError(errorCode, driveName);
		}

		internal static void WriteNotSupported()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnwritableStream"));
		}

		internal static void WriterClosed()
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_WriterClosed"));
		}
	}
}
