using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO
{
	[ComVisible(true)]
	public static class File
	{
		private const int GetFileExInfoStandard = 0;

		private const int ERROR_INVALID_PARAMETER = 87;

		private const int ERROR_ACCESS_DENIED = 5;

		public static StreamReader OpenText(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return new StreamReader(path);
		}

		public static StreamWriter CreateText(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return new StreamWriter(path, append: false);
		}

		public static StreamWriter AppendText(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return new StreamWriter(path, append: true);
		}

		public static void Copy(string sourceFileName, string destFileName)
		{
			Copy(sourceFileName, destFileName, overwrite: false);
		}

		public static void Copy(string sourceFileName, string destFileName, bool overwrite)
		{
			InternalCopy(sourceFileName, destFileName, overwrite);
		}

		internal static string InternalCopy(string sourceFileName, string destFileName, bool overwrite)
		{
			if (sourceFileName == null || destFileName == null)
			{
				throw new ArgumentNullException((sourceFileName == null) ? "sourceFileName" : "destFileName", Environment.GetResourceString("ArgumentNull_FileName"));
			}
			if (sourceFileName.Length == 0 || destFileName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), (sourceFileName.Length == 0) ? "sourceFileName" : "destFileName");
			}
			string fullPathInternal = Path.GetFullPathInternal(sourceFileName);
			new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			string fullPathInternal2 = Path.GetFullPathInternal(destFileName);
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				fullPathInternal2
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (!Win32Native.CopyFile(fullPathInternal, fullPathInternal2, !overwrite))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				string maybeFullPath = destFileName;
				if (lastWin32Error != 80)
				{
					using (SafeFileHandle safeFileHandle = Win32Native.UnsafeCreateFile(fullPathInternal, int.MinValue, FileShare.Read, null, FileMode.Open, 0, IntPtr.Zero))
					{
						if (safeFileHandle.IsInvalid)
						{
							maybeFullPath = sourceFileName;
						}
					}
					if (lastWin32Error == 5 && Directory.InternalExists(fullPathInternal2))
					{
						throw new IOException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_FileIsDirectory_Name"), destFileName), 5, fullPathInternal2);
					}
				}
				__Error.WinIOError(lastWin32Error, maybeFullPath);
			}
			return fullPathInternal2;
		}

		public static FileStream Create(string path)
		{
			return Create(path, 4096, FileOptions.None);
		}

		public static FileStream Create(string path, int bufferSize)
		{
			return Create(path, bufferSize, FileOptions.None);
		}

		public static FileStream Create(string path, int bufferSize, FileOptions options)
		{
			return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
		}

		public static FileStream Create(string path, int bufferSize, FileOptions options, FileSecurity fileSecurity)
		{
			return new FileStream(path, FileMode.Create, FileSystemRights.Read | FileSystemRights.Write, FileShare.None, bufferSize, options, fileSecurity);
		}

		public static void Delete(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (Environment.IsWin9X() && Directory.InternalExists(fullPathInternal))
			{
				throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("UnauthorizedAccess_IODenied_Path"), path));
			}
			if (!Win32Native.DeleteFile(fullPathInternal))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 2)
				{
					__Error.WinIOError(lastWin32Error, fullPathInternal);
				}
			}
		}

		public static void Decrypt(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (!Environment.RunningOnWinNT)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (Win32Native.DecryptFile(fullPathInternal, 0))
			{
				return;
			}
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error == 5)
			{
				DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(fullPathInternal));
				if (!string.Equals("NTFS", driveInfo.DriveFormat))
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_EncryptionNeedsNTFS"));
				}
			}
			__Error.WinIOError(lastWin32Error, fullPathInternal);
		}

		public static void Encrypt(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (!Environment.RunningOnWinNT)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (Win32Native.EncryptFile(fullPathInternal))
			{
				return;
			}
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error == 5)
			{
				DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(fullPathInternal));
				if (!string.Equals("NTFS", driveInfo.DriveFormat))
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_EncryptionNeedsNTFS"));
				}
			}
			__Error.WinIOError(lastWin32Error, fullPathInternal);
		}

		public static bool Exists(string path)
		{
			try
			{
				if (path == null)
				{
					return false;
				}
				if (path.Length == 0)
				{
					return false;
				}
				path = Path.GetFullPathInternal(path);
				new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
				{
					path
				}, checkForDuplicates: false, needFullPath: false).Demand();
				return InternalExists(path);
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
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			return false;
		}

		internal static bool InternalExists(string path)
		{
			Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
			if (FillAttributeInfo(path, ref data, tryagain: false, returnErrorOnNotFound: true) == 0 && data.fileAttributes != -1)
			{
				return (data.fileAttributes & 0x10) == 0;
			}
			return false;
		}

		public static FileStream Open(string path, FileMode mode)
		{
			return Open(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access)
		{
			return Open(path, mode, access, FileShare.None);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return new FileStream(path, mode, access, share);
		}

		public static void SetCreationTime(string path, DateTime creationTime)
		{
			SetCreationTimeUtc(path, creationTime.ToUniversalTime());
		}

		public unsafe static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		{
			SafeFileHandle handle;
			using (OpenFile(path, FileAccess.Write, out handle))
			{
				Win32Native.FILE_TIME fILE_TIME = new Win32Native.FILE_TIME(creationTimeUtc.ToFileTimeUtc());
				if (!Win32Native.SetFileTime(handle, &fILE_TIME, null, null))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					__Error.WinIOError(lastWin32Error, path);
				}
			}
		}

		public static DateTime GetCreationTime(string path)
		{
			return GetCreationTimeUtc(path).ToLocalTime();
		}

		public static DateTime GetCreationTimeUtc(string path)
		{
			string fullPathInternal = Path.GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
			int num = FillAttributeInfo(fullPathInternal, ref data, tryagain: false, returnErrorOnNotFound: false);
			if (num != 0)
			{
				__Error.WinIOError(num, fullPathInternal);
			}
			long fileTime = (long)(((ulong)data.ftCreationTimeHigh << 32) | data.ftCreationTimeLow);
			return DateTime.FromFileTimeUtc(fileTime);
		}

		public static void SetLastAccessTime(string path, DateTime lastAccessTime)
		{
			SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());
		}

		public unsafe static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		{
			SafeFileHandle handle;
			using (OpenFile(path, FileAccess.Write, out handle))
			{
				Win32Native.FILE_TIME fILE_TIME = new Win32Native.FILE_TIME(lastAccessTimeUtc.ToFileTimeUtc());
				if (!Win32Native.SetFileTime(handle, null, &fILE_TIME, null))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					__Error.WinIOError(lastWin32Error, path);
				}
			}
		}

		public static DateTime GetLastAccessTime(string path)
		{
			return GetLastAccessTimeUtc(path).ToLocalTime();
		}

		public static DateTime GetLastAccessTimeUtc(string path)
		{
			string fullPathInternal = Path.GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
			int num = FillAttributeInfo(fullPathInternal, ref data, tryagain: false, returnErrorOnNotFound: false);
			if (num != 0)
			{
				__Error.WinIOError(num, fullPathInternal);
			}
			long fileTime = (long)(((ulong)data.ftLastAccessTimeHigh << 32) | data.ftLastAccessTimeLow);
			return DateTime.FromFileTimeUtc(fileTime);
		}

		public static void SetLastWriteTime(string path, DateTime lastWriteTime)
		{
			SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());
		}

		public unsafe static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			SafeFileHandle handle;
			using (OpenFile(path, FileAccess.Write, out handle))
			{
				Win32Native.FILE_TIME fILE_TIME = new Win32Native.FILE_TIME(lastWriteTimeUtc.ToFileTimeUtc());
				if (!Win32Native.SetFileTime(handle, null, null, &fILE_TIME))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					__Error.WinIOError(lastWin32Error, path);
				}
			}
		}

		public static DateTime GetLastWriteTime(string path)
		{
			return GetLastWriteTimeUtc(path).ToLocalTime();
		}

		public static DateTime GetLastWriteTimeUtc(string path)
		{
			string fullPathInternal = Path.GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
			int num = FillAttributeInfo(fullPathInternal, ref data, tryagain: false, returnErrorOnNotFound: false);
			if (num != 0)
			{
				__Error.WinIOError(num, fullPathInternal);
			}
			long fileTime = (long)(((ulong)data.ftLastWriteTimeHigh << 32) | data.ftLastWriteTimeLow);
			return DateTime.FromFileTimeUtc(fileTime);
		}

		public static FileAttributes GetAttributes(string path)
		{
			string fullPathInternal = Path.GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
			int num = FillAttributeInfo(fullPathInternal, ref data, tryagain: false, returnErrorOnNotFound: true);
			if (num != 0)
			{
				__Error.WinIOError(num, fullPathInternal);
			}
			return (FileAttributes)data.fileAttributes;
		}

		public static void SetAttributes(string path, FileAttributes fileAttributes)
		{
			string fullPathInternal = Path.GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (!Win32Native.SetFileAttributes(fullPathInternal, (int)fileAttributes))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 87 || (lastWin32Error == 5 && Environment.IsWin9X()))
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_InvalidFileAttrs"));
				}
				__Error.WinIOError(lastWin32Error, fullPathInternal);
			}
		}

		public static FileSecurity GetAccessControl(string path)
		{
			return GetAccessControl(path, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public static FileSecurity GetAccessControl(string path, AccessControlSections includeSections)
		{
			return new FileSecurity(path, includeSections);
		}

		public static void SetAccessControl(string path, FileSecurity fileSecurity)
		{
			if (fileSecurity == null)
			{
				throw new ArgumentNullException("fileSecurity");
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			fileSecurity.Persist(fullPathInternal);
		}

		public static FileStream OpenRead(string path)
		{
			return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public static FileStream OpenWrite(string path)
		{
			return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}

		public static string ReadAllText(string path)
		{
			return ReadAllText(path, Encoding.UTF8);
		}

		public static string ReadAllText(string path, Encoding encoding)
		{
			using StreamReader streamReader = new StreamReader(path, encoding);
			return streamReader.ReadToEnd();
		}

		public static void WriteAllText(string path, string contents)
		{
			WriteAllText(path, contents, StreamWriter.UTF8NoBOM);
		}

		public static void WriteAllText(string path, string contents, Encoding encoding)
		{
			using StreamWriter streamWriter = new StreamWriter(path, append: false, encoding);
			streamWriter.Write(contents);
		}

		public static byte[] ReadAllBytes(string path)
		{
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			int num = 0;
			long length = fileStream.Length;
			if (length > int.MaxValue)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_FileTooLong2GB"));
			}
			int num2 = (int)length;
			byte[] array = new byte[num2];
			while (num2 > 0)
			{
				int num3 = fileStream.Read(array, num, num2);
				if (num3 == 0)
				{
					__Error.EndOfFile();
				}
				num += num3;
				num2 -= num3;
			}
			return array;
		}

		public static void WriteAllBytes(string path, byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			fileStream.Write(bytes, 0, bytes.Length);
		}

		public static string[] ReadAllLines(string path)
		{
			return ReadAllLines(path, Encoding.UTF8);
		}

		public static string[] ReadAllLines(string path, Encoding encoding)
		{
			ArrayList arrayList = new ArrayList();
			using (StreamReader streamReader = new StreamReader(path, encoding))
			{
				string value;
				while ((value = streamReader.ReadLine()) != null)
				{
					arrayList.Add(value);
				}
			}
			return (string[])arrayList.ToArray(typeof(string));
		}

		public static void WriteAllLines(string path, string[] contents)
		{
			WriteAllLines(path, contents, StreamWriter.UTF8NoBOM);
		}

		public static void WriteAllLines(string path, string[] contents, Encoding encoding)
		{
			if (contents == null)
			{
				throw new ArgumentNullException("contents");
			}
			using StreamWriter streamWriter = new StreamWriter(path, append: false, encoding);
			foreach (string value in contents)
			{
				streamWriter.WriteLine(value);
			}
		}

		public static void AppendAllText(string path, string contents)
		{
			AppendAllText(path, contents, StreamWriter.UTF8NoBOM);
		}

		public static void AppendAllText(string path, string contents, Encoding encoding)
		{
			using StreamWriter streamWriter = new StreamWriter(path, append: true, encoding);
			streamWriter.Write(contents);
		}

		public static void Move(string sourceFileName, string destFileName)
		{
			if (sourceFileName == null || destFileName == null)
			{
				throw new ArgumentNullException((sourceFileName == null) ? "sourceFileName" : "destFileName", Environment.GetResourceString("ArgumentNull_FileName"));
			}
			if (sourceFileName.Length == 0 || destFileName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), (sourceFileName.Length == 0) ? "sourceFileName" : "destFileName");
			}
			string fullPathInternal = Path.GetFullPathInternal(sourceFileName);
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			string fullPathInternal2 = Path.GetFullPathInternal(destFileName);
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				fullPathInternal2
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (!InternalExists(fullPathInternal))
			{
				__Error.WinIOError(2, fullPathInternal);
			}
			if (!Win32Native.MoveFile(fullPathInternal, fullPathInternal2))
			{
				__Error.WinIOError();
			}
		}

		public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
		{
			Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors: false);
		}

		public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
		{
			if (sourceFileName == null)
			{
				throw new ArgumentNullException("sourceFileName");
			}
			if (destinationFileName == null)
			{
				throw new ArgumentNullException("destinationFileName");
			}
			string fullPathInternal = Path.GetFullPathInternal(sourceFileName);
			string fullPathInternal2 = Path.GetFullPathInternal(destinationFileName);
			string text = null;
			if (destinationBackupFileName != null)
			{
				text = Path.GetFullPathInternal(destinationBackupFileName);
			}
			FileIOPermission fileIOPermission = new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, new string[2]
			{
				fullPathInternal,
				fullPathInternal2
			});
			if (destinationBackupFileName != null)
			{
				fileIOPermission.AddPathList(FileIOPermissionAccess.Write, text);
			}
			fileIOPermission.Demand();
			if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
			}
			int num = 1;
			if (ignoreMetadataErrors)
			{
				num |= 2;
			}
			if (!Win32Native.ReplaceFile(fullPathInternal2, fullPathInternal, text, num, IntPtr.Zero, IntPtr.Zero))
			{
				__Error.WinIOError();
			}
		}

		internal static int FillAttributeInfo(string path, ref Win32Native.WIN32_FILE_ATTRIBUTE_DATA data, bool tryagain, bool returnErrorOnNotFound)
		{
			int num = 0;
			if (Environment.OSInfo == Environment.OSName.Win95 || tryagain)
			{
				Win32Native.WIN32_FIND_DATA wIN32_FIND_DATA = new Win32Native.WIN32_FIND_DATA();
				string fileName = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				int errorMode = Win32Native.SetErrorMode(1);
				try
				{
					bool flag = false;
					SafeFindHandle safeFindHandle = Win32Native.FindFirstFile(fileName, wIN32_FIND_DATA);
					try
					{
						if (safeFindHandle.IsInvalid)
						{
							flag = true;
							num = Marshal.GetLastWin32Error();
							if ((num == 2 || num == 3 || num == 21) && !returnErrorOnNotFound)
							{
								num = 0;
								data.fileAttributes = -1;
							}
							return num;
						}
					}
					finally
					{
						try
						{
							safeFindHandle.Close();
						}
						catch
						{
							if (!flag)
							{
								__Error.WinIOError();
							}
						}
					}
				}
				finally
				{
					Win32Native.SetErrorMode(errorMode);
				}
				data.fileAttributes = wIN32_FIND_DATA.dwFileAttributes;
				data.ftCreationTimeLow = (uint)wIN32_FIND_DATA.ftCreationTime_dwLowDateTime;
				data.ftCreationTimeHigh = (uint)wIN32_FIND_DATA.ftCreationTime_dwHighDateTime;
				data.ftLastAccessTimeLow = (uint)wIN32_FIND_DATA.ftLastAccessTime_dwLowDateTime;
				data.ftLastAccessTimeHigh = (uint)wIN32_FIND_DATA.ftLastAccessTime_dwHighDateTime;
				data.ftLastWriteTimeLow = (uint)wIN32_FIND_DATA.ftLastWriteTime_dwLowDateTime;
				data.ftLastWriteTimeHigh = (uint)wIN32_FIND_DATA.ftLastWriteTime_dwHighDateTime;
				data.fileSizeHigh = wIN32_FIND_DATA.nFileSizeHigh;
				data.fileSizeLow = wIN32_FIND_DATA.nFileSizeLow;
			}
			else
			{
				bool flag2 = false;
				int errorMode2 = Win32Native.SetErrorMode(1);
				try
				{
					flag2 = Win32Native.GetFileAttributesEx(path, 0, ref data);
				}
				finally
				{
					Win32Native.SetErrorMode(errorMode2);
				}
				if (!flag2)
				{
					num = Marshal.GetLastWin32Error();
					if (num != 2 && num != 3 && num != 21)
					{
						return FillAttributeInfo(path, ref data, tryagain: true, returnErrorOnNotFound);
					}
					if (!returnErrorOnNotFound)
					{
						num = 0;
						data.fileAttributes = -1;
					}
				}
			}
			return num;
		}

		private static FileStream OpenFile(string path, FileAccess access, out SafeFileHandle handle)
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, access, FileShare.ReadWrite, 1);
			handle = fileStream.SafeFileHandle;
			if (handle.IsInvalid)
			{
				int num = Marshal.GetLastWin32Error();
				string fullPathInternal = Path.GetFullPathInternal(path);
				if (num == 3 && fullPathInternal.Equals(Directory.GetDirectoryRoot(fullPathInternal)))
				{
					num = 5;
				}
				__Error.WinIOError(num, path);
			}
			return fileStream;
		}
	}
}
