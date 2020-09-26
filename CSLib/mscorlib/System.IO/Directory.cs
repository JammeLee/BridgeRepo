using System.Collections.Generic;
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
	public static class Directory
	{
		private sealed class SearchData
		{
			public string fullPath;

			public string userPath;

			public SearchOption searchOption;

			public SearchData()
			{
			}

			public SearchData(string fullPath, string userPath, SearchOption searchOption)
			{
				this.fullPath = fullPath;
				this.userPath = userPath;
				this.searchOption = searchOption;
			}
		}

		private const int FILE_ATTRIBUTE_DIRECTORY = 16;

		private const int GENERIC_WRITE = 1073741824;

		private const int FILE_SHARE_WRITE = 2;

		private const int FILE_SHARE_DELETE = 4;

		private const int OPEN_EXISTING = 3;

		private const int FILE_FLAG_BACKUP_SEMANTICS = 33554432;

		public static DirectoryInfo GetParent(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (path.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_PathEmpty"), "path");
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			string directoryName = Path.GetDirectoryName(fullPathInternal);
			if (directoryName == null)
			{
				return null;
			}
			return new DirectoryInfo(directoryName);
		}

		public static DirectoryInfo CreateDirectory(string path)
		{
			return CreateDirectory(path, null);
		}

		public static DirectoryInfo CreateDirectory(string path, DirectorySecurity directorySecurity)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (path.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_PathEmpty"));
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			string demandDir = GetDemandDir(fullPathInternal, thisDirOnly: true);
			new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
			{
				demandDir
			}, checkForDuplicates: false, needFullPath: false).Demand();
			InternalCreateDirectory(fullPathInternal, path, directorySecurity);
			return new DirectoryInfo(fullPathInternal, junk: false);
		}

		internal static string GetDemandDir(string fullPath, bool thisDirOnly)
		{
			if (thisDirOnly)
			{
				if (fullPath.EndsWith(Path.DirectorySeparatorChar) || fullPath.EndsWith(Path.AltDirectorySeparatorChar))
				{
					return fullPath + '.';
				}
				return fullPath + Path.DirectorySeparatorChar + '.';
			}
			if (!fullPath.EndsWith(Path.DirectorySeparatorChar) && !fullPath.EndsWith(Path.AltDirectorySeparatorChar))
			{
				return fullPath + Path.DirectorySeparatorChar;
			}
			return fullPath;
		}

		internal unsafe static void InternalCreateDirectory(string fullPath, string path, DirectorySecurity dirSecurity)
		{
			int num = fullPath.Length;
			if (num >= 2 && Path.IsDirectorySeparator(fullPath[num - 1]))
			{
				num--;
			}
			int rootLength = Path.GetRootLength(fullPath);
			if (num == 2 && Path.IsDirectorySeparator(fullPath[1]))
			{
				throw new IOException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.IO_CannotCreateDirectory"), path));
			}
			List<string> list = new List<string>();
			bool flag = false;
			if (num > rootLength)
			{
				for (int num2 = num - 1; num2 >= rootLength; num2--)
				{
					string text = fullPath.Substring(0, num2 + 1);
					if (!InternalExists(text))
					{
						list.Add(text);
					}
					else
					{
						flag = true;
					}
					while (num2 > rootLength && fullPath[num2] != Path.DirectorySeparatorChar && fullPath[num2] != Path.AltDirectorySeparatorChar)
					{
						num2--;
					}
				}
			}
			int count = list.Count;
			if (list.Count != 0)
			{
				string[] array = new string[list.Count];
				list.CopyTo(array, 0);
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2;
					string[] array3 = (array2 = array);
					int num3 = i;
					nint num4 = num3;
					array3[num3] = array2[num4] + "\\.";
				}
				AccessControlActions control = ((dirSecurity != null) ? AccessControlActions.Change : AccessControlActions.None);
				new FileIOPermission(FileIOPermissionAccess.Write, control, array, checkForDuplicates: false, needFullPath: false).Demand();
			}
			Win32Native.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = null;
			if (dirSecurity != null)
			{
				sECURITY_ATTRIBUTES = new Win32Native.SECURITY_ATTRIBUTES();
				sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
				byte[] securityDescriptorBinaryForm = dirSecurity.GetSecurityDescriptorBinaryForm();
				byte* ptr = stackalloc byte[1 * securityDescriptorBinaryForm.Length];
				Buffer.memcpy(securityDescriptorBinaryForm, 0, ptr, 0, securityDescriptorBinaryForm.Length);
				sECURITY_ATTRIBUTES.pSecurityDescriptor = ptr;
			}
			bool flag2 = true;
			int num5 = 0;
			string maybeFullPath = path;
			while (list.Count > 0)
			{
				string text2 = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				if (text2.Length > 248)
				{
					throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
				}
				flag2 = Win32Native.CreateDirectory(text2, sECURITY_ATTRIBUTES);
				if (flag2 || num5 != 0)
				{
					continue;
				}
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 183)
				{
					num5 = lastWin32Error;
				}
				else if (File.InternalExists(text2))
				{
					num5 = lastWin32Error;
					try
					{
						new FileIOPermission(FileIOPermissionAccess.PathDiscovery, GetDemandDir(text2, thisDirOnly: true)).Demand();
						maybeFullPath = text2;
					}
					catch (SecurityException)
					{
					}
				}
			}
			if (count == 0 && !flag)
			{
				string path2 = InternalGetDirectoryRoot(fullPath);
				if (!InternalExists(path2))
				{
					__Error.WinIOError(3, InternalGetDirectoryRoot(path));
				}
			}
			else if (!flag2 && num5 != 0)
			{
				__Error.WinIOError(num5, maybeFullPath);
			}
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
				string fullPathInternal = Path.GetFullPathInternal(path);
				string demandDir = GetDemandDir(fullPathInternal, thisDirOnly: true);
				new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
				{
					demandDir
				}, checkForDuplicates: false, needFullPath: false).Demand();
				return InternalExists(fullPathInternal);
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
			if (File.FillAttributeInfo(path, ref data, tryagain: false, returnErrorOnNotFound: true) == 0 && data.fileAttributes != -1)
			{
				return (data.fileAttributes & 0x10) != 0;
			}
			return false;
		}

		public static void SetCreationTime(string path, DateTime creationTime)
		{
			SetCreationTimeUtc(path, creationTime.ToUniversalTime());
		}

		public unsafe static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		{
			if ((Environment.OSInfo & Environment.OSName.WinNT) != Environment.OSName.WinNT)
			{
				return;
			}
			using SafeFileHandle hFile = OpenHandle(path);
			Win32Native.FILE_TIME fILE_TIME = new Win32Native.FILE_TIME(creationTimeUtc.ToFileTimeUtc());
			if (!Win32Native.SetFileTime(hFile, &fILE_TIME, null, null))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				__Error.WinIOError(lastWin32Error, path);
			}
		}

		public static DateTime GetCreationTime(string path)
		{
			return File.GetCreationTime(path);
		}

		public static DateTime GetCreationTimeUtc(string path)
		{
			return File.GetCreationTimeUtc(path);
		}

		public static void SetLastWriteTime(string path, DateTime lastWriteTime)
		{
			SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());
		}

		public unsafe static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			if ((Environment.OSInfo & Environment.OSName.WinNT) != Environment.OSName.WinNT)
			{
				return;
			}
			using SafeFileHandle hFile = OpenHandle(path);
			Win32Native.FILE_TIME fILE_TIME = new Win32Native.FILE_TIME(lastWriteTimeUtc.ToFileTimeUtc());
			if (!Win32Native.SetFileTime(hFile, null, null, &fILE_TIME))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				__Error.WinIOError(lastWin32Error, path);
			}
		}

		public static DateTime GetLastWriteTime(string path)
		{
			return File.GetLastWriteTime(path);
		}

		public static DateTime GetLastWriteTimeUtc(string path)
		{
			return File.GetLastWriteTimeUtc(path);
		}

		public static void SetLastAccessTime(string path, DateTime lastAccessTime)
		{
			SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());
		}

		public unsafe static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		{
			if ((Environment.OSInfo & Environment.OSName.WinNT) != Environment.OSName.WinNT)
			{
				return;
			}
			using SafeFileHandle hFile = OpenHandle(path);
			Win32Native.FILE_TIME fILE_TIME = new Win32Native.FILE_TIME(lastAccessTimeUtc.ToFileTimeUtc());
			if (!Win32Native.SetFileTime(hFile, null, &fILE_TIME, null))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				__Error.WinIOError(lastWin32Error, path);
			}
		}

		public static DateTime GetLastAccessTime(string path)
		{
			return File.GetLastAccessTime(path);
		}

		public static DateTime GetLastAccessTimeUtc(string path)
		{
			return File.GetLastAccessTimeUtc(path);
		}

		public static DirectorySecurity GetAccessControl(string path)
		{
			return new DirectorySecurity(path, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public static DirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
		{
			return new DirectorySecurity(path, includeSections);
		}

		public static void SetAccessControl(string path, DirectorySecurity directorySecurity)
		{
			if (directorySecurity == null)
			{
				throw new ArgumentNullException("directorySecurity");
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			directorySecurity.Persist(fullPathInternal);
		}

		public static string[] GetFiles(string path)
		{
			return GetFiles(path, "*");
		}

		public static string[] GetFiles(string path, string searchPattern)
		{
			return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			return InternalGetFileDirectoryNames(path, path, searchPattern, includeFiles: true, includeDirs: false, searchOption);
		}

		public static string[] GetDirectories(string path)
		{
			return GetDirectories(path, "*");
		}

		public static string[] GetDirectories(string path, string searchPattern)
		{
			return GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			return InternalGetFileDirectoryNames(path, path, searchPattern, includeFiles: false, includeDirs: true, searchOption);
		}

		public static string[] GetFileSystemEntries(string path)
		{
			return GetFileSystemEntries(path, "*");
		}

		public static string[] GetFileSystemEntries(string path, string searchPattern)
		{
			return GetFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		private static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			return InternalGetFileDirectoryNames(path, path, searchPattern, includeFiles: true, includeDirs: true, searchOption);
		}

		internal static string[] InternalGetFileDirectoryNames(string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
		{
			int num = 0;
			if (searchOption != 0 && searchOption != SearchOption.AllDirectories)
			{
				throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
			}
			searchPattern = searchPattern.TrimEnd();
			if (searchPattern.Length == 0)
			{
				return new string[0];
			}
			Path.CheckSearchPattern(searchPattern);
			string fullPathInternal = Path.GetFullPathInternal(path);
			string[] pathList = new string[1]
			{
				GetDemandDir(fullPathInternal, thisDirOnly: true)
			};
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathList, checkForDuplicates: false, needFullPath: false).Demand();
			string text = userPathOriginal;
			string directoryName = Path.GetDirectoryName(searchPattern);
			if (directoryName != null && directoryName.Length != 0)
			{
				pathList = new string[1]
				{
					GetDemandDir(Path.InternalCombine(fullPathInternal, directoryName), thisDirOnly: true)
				};
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathList, checkForDuplicates: false, needFullPath: false).Demand();
				text = Path.Combine(text, directoryName);
			}
			string text2 = Path.InternalCombine(fullPathInternal, searchPattern);
			char c = text2[text2.Length - 1];
			if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar)
			{
				text2 += '*';
			}
			fullPathInternal = Path.GetDirectoryName(text2);
			bool flag = false;
			bool flag2 = false;
			c = fullPathInternal[fullPathInternal.Length - 1];
			string text3 = ((c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) ? text2.Substring(fullPathInternal.Length + 1) : text2.Substring(fullPathInternal.Length));
			Win32Native.WIN32_FIND_DATA wIN32_FIND_DATA = new Win32Native.WIN32_FIND_DATA();
			SafeFindHandle safeFindHandle = null;
			SearchData searchData = new SearchData(fullPathInternal, text, searchOption);
			List<SearchData> list = new List<SearchData>();
			list.Add(searchData);
			List<string> list2 = new List<string>();
			int num2 = 0;
			int num3 = 0;
			string[] array = new string[10];
			int errorMode = Win32Native.SetErrorMode(1);
			try
			{
				while (list.Count > 0)
				{
					searchData = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
					c = searchData.fullPath[searchData.fullPath.Length - 1];
					flag = c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
					if (searchData.userPath.Length > 0)
					{
						c = searchData.userPath[searchData.userPath.Length - 1];
						flag2 = c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
					}
					if (searchData.searchOption != 0)
					{
						try
						{
							string fileName = ((!flag) ? (searchData.fullPath + Path.DirectorySeparatorChar + "*") : (searchData.fullPath + "*"));
							safeFindHandle = Win32Native.FindFirstFile(fileName, wIN32_FIND_DATA);
							if (!safeFindHandle.IsInvalid)
							{
								goto IL_02ab;
							}
							num = Marshal.GetLastWin32Error();
							if (num == 2)
							{
								continue;
							}
							__Error.WinIOError(num, searchData.fullPath);
							goto IL_02ab;
							IL_02ab:
							do
							{
								if (((uint)wIN32_FIND_DATA.dwFileAttributes & 0x10u) != 0 && !wIN32_FIND_DATA.cFileName.Equals(".") && !wIN32_FIND_DATA.cFileName.Equals(".."))
								{
									SearchData searchData2 = new SearchData();
									StringBuilder stringBuilder = new StringBuilder(searchData.fullPath);
									if (!flag)
									{
										stringBuilder.Append(Path.DirectorySeparatorChar);
									}
									stringBuilder.Append(wIN32_FIND_DATA.cFileName);
									searchData2.fullPath = stringBuilder.ToString();
									stringBuilder.Length = 0;
									stringBuilder.Append(searchData.userPath);
									if (!flag2)
									{
										stringBuilder.Append(Path.DirectorySeparatorChar);
									}
									stringBuilder.Append(wIN32_FIND_DATA.cFileName);
									searchData2.userPath = stringBuilder.ToString();
									searchData2.searchOption = searchData.searchOption;
									list.Add(searchData2);
								}
							}
							while (Win32Native.FindNextFile(safeFindHandle, wIN32_FIND_DATA));
							goto IL_03a1;
						}
						finally
						{
							safeFindHandle?.Dispose();
						}
					}
					goto IL_03a1;
					IL_03a1:
					try
					{
						string fileName = ((!flag) ? (searchData.fullPath + Path.DirectorySeparatorChar + text3) : (searchData.fullPath + text3));
						safeFindHandle = Win32Native.FindFirstFile(fileName, wIN32_FIND_DATA);
						if (!safeFindHandle.IsInvalid)
						{
							goto IL_0401;
						}
						num = Marshal.GetLastWin32Error();
						if (num == 2)
						{
							continue;
						}
						__Error.WinIOError(num, searchData.fullPath);
						goto IL_0401;
						IL_0401:
						num2 = 0;
						do
						{
							bool flag3 = false;
							if (includeFiles)
							{
								flag3 = 0 == (wIN32_FIND_DATA.dwFileAttributes & 0x10);
							}
							if (includeDirs && ((uint)wIN32_FIND_DATA.dwFileAttributes & 0x10u) != 0 && !wIN32_FIND_DATA.cFileName.Equals(".") && !wIN32_FIND_DATA.cFileName.Equals(".."))
							{
								flag3 = true;
							}
							if (flag3)
							{
								num2++;
								if (num3 == array.Length)
								{
									string[] array2 = new string[array.Length * 2];
									Array.Copy(array, 0, array2, 0, num3);
									array = array2;
								}
								array[num3++] = Path.InternalCombine(searchData.userPath, wIN32_FIND_DATA.cFileName);
							}
						}
						while (Win32Native.FindNextFile(safeFindHandle, wIN32_FIND_DATA));
						num = Marshal.GetLastWin32Error();
						if (num2 > 0)
						{
							list2.Add(GetDemandDir(searchData.fullPath, thisDirOnly: true));
						}
					}
					finally
					{
						safeFindHandle?.Dispose();
					}
				}
			}
			finally
			{
				Win32Native.SetErrorMode(errorMode);
			}
			if (num != 0 && num != 18 && num != 2)
			{
				__Error.WinIOError(num, searchData.fullPath);
			}
			if (list2.Count > 0)
			{
				pathList = new string[list2.Count];
				list2.CopyTo(pathList, 0);
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathList, checkForDuplicates: false, needFullPath: false).Demand();
			}
			if (num3 == array.Length)
			{
				return array;
			}
			string[] array3 = new string[num3];
			Array.Copy(array, 0, array3, 0, num3);
			return array3;
		}

		public static string[] GetLogicalDrives()
		{
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			int logicalDrives = Win32Native.GetLogicalDrives();
			if (logicalDrives == 0)
			{
				__Error.WinIOError();
			}
			uint num = (uint)logicalDrives;
			int num2 = 0;
			while (num != 0)
			{
				if ((num & (true ? 1u : 0u)) != 0)
				{
					num2++;
				}
				num >>= 1;
			}
			string[] array = new string[num2];
			char[] array2 = new char[3]
			{
				'A',
				':',
				'\\'
			};
			num = (uint)logicalDrives;
			num2 = 0;
			while (num != 0)
			{
				if ((num & (true ? 1u : 0u)) != 0)
				{
					array[num2++] = new string(array2);
				}
				num >>= 1;
				array2[0] += '\u0001';
			}
			return array;
		}

		public static string GetDirectoryRoot(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			string text = fullPathInternal.Substring(0, Path.GetRootLength(fullPathInternal));
			string demandDir = GetDemandDir(text, thisDirOnly: true);
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[1]
			{
				demandDir
			}, checkForDuplicates: false, needFullPath: false).Demand();
			return text;
		}

		internal static string InternalGetDirectoryRoot(string path)
		{
			return path?.Substring(0, Path.GetRootLength(path));
		}

		public static string GetCurrentDirectory()
		{
			StringBuilder stringBuilder = new StringBuilder(261);
			if (Win32Native.GetCurrentDirectory(stringBuilder.Capacity, stringBuilder) == 0)
			{
				__Error.WinIOError();
			}
			string text = stringBuilder.ToString();
			if (text.IndexOf('~') >= 0)
			{
				int longPathName = Win32Native.GetLongPathName(text, stringBuilder, stringBuilder.Capacity);
				if (longPathName == 0 || longPathName >= 260)
				{
					int num = Marshal.GetLastWin32Error();
					if (longPathName >= 260)
					{
						num = 206;
					}
					if (num != 2 && num != 3 && num != 1 && num != 5)
					{
						__Error.WinIOError(num, string.Empty);
					}
				}
				text = stringBuilder.ToString();
			}
			string demandDir = GetDemandDir(text, thisDirOnly: true);
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[1]
			{
				demandDir
			}, checkForDuplicates: false, needFullPath: false).Demand();
			return text;
		}

		public static void SetCurrentDirectory(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("value");
			}
			if (path.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_PathEmpty"));
			}
			if (path.Length >= 260)
			{
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			}
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			string fullPathInternal = Path.GetFullPathInternal(path);
			if (Environment.IsWin9X() && !InternalExists(Path.GetPathRoot(fullPathInternal)))
			{
				throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.PathNotFound_Path"), path));
			}
			if (!Win32Native.SetCurrentDirectory(fullPathInternal))
			{
				int num = Marshal.GetLastWin32Error();
				if (num == 2)
				{
					num = 3;
				}
				__Error.WinIOError(num, fullPathInternal);
			}
		}

		public static void Move(string sourceDirName, string destDirName)
		{
			if (sourceDirName == null)
			{
				throw new ArgumentNullException("sourceDirName");
			}
			if (sourceDirName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "sourceDirName");
			}
			if (destDirName == null)
			{
				throw new ArgumentNullException("destDirName");
			}
			if (destDirName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destDirName");
			}
			string fullPathInternal = Path.GetFullPathInternal(sourceDirName);
			string demandDir = GetDemandDir(fullPathInternal, thisDirOnly: false);
			if (demandDir.Length >= 249)
			{
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			}
			string fullPathInternal2 = Path.GetFullPathInternal(destDirName);
			string demandDir2 = GetDemandDir(fullPathInternal2, thisDirOnly: false);
			if (demandDir2.Length >= 249)
			{
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			}
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, new string[1]
			{
				demandDir
			}, checkForDuplicates: false, needFullPath: false).Demand();
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				demandDir2
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (CultureInfo.InvariantCulture.CompareInfo.Compare(demandDir, demandDir2, CompareOptions.IgnoreCase) == 0)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustBeDifferent"));
			}
			string pathRoot = Path.GetPathRoot(demandDir);
			string pathRoot2 = Path.GetPathRoot(demandDir2);
			if (CultureInfo.InvariantCulture.CompareInfo.Compare(pathRoot, pathRoot2, CompareOptions.IgnoreCase) != 0)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustHaveSameRoot"));
			}
			if (Environment.IsWin9X() && !InternalExists(Path.GetPathRoot(fullPathInternal2)))
			{
				throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.PathNotFound_Path"), destDirName));
			}
			if (!Win32Native.MoveFile(sourceDirName, destDirName))
			{
				int num = Marshal.GetLastWin32Error();
				if (num == 2)
				{
					num = 3;
					__Error.WinIOError(num, fullPathInternal);
				}
				if (num == 5)
				{
					throw new IOException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("UnauthorizedAccess_IODenied_Path"), sourceDirName), Win32Native.MakeHRFromErrorCode(num));
				}
				__Error.WinIOError(num, string.Empty);
			}
		}

		public static void Delete(string path)
		{
			string fullPathInternal = Path.GetFullPathInternal(path);
			Delete(fullPathInternal, path, recursive: false);
		}

		public static void Delete(string path, bool recursive)
		{
			string fullPathInternal = Path.GetFullPathInternal(path);
			Delete(fullPathInternal, path, recursive);
		}

		internal static void Delete(string fullPath, string userPath, bool recursive)
		{
			string demandDir = GetDemandDir(fullPath, !recursive);
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				demandDir
			}, checkForDuplicates: false, needFullPath: false).Demand();
			Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
			int num = File.FillAttributeInfo(fullPath, ref data, tryagain: false, returnErrorOnNotFound: true);
			if (num != 0)
			{
				if (num == 2)
				{
					num = 3;
				}
				__Error.WinIOError(num, fullPath);
			}
			if (((uint)data.fileAttributes & 0x400u) != 0)
			{
				recursive = false;
			}
			DeleteHelper(fullPath, userPath, recursive);
		}

		private static void DeleteHelper(string fullPath, string userPath, bool recursive)
		{
			Exception ex = null;
			if (recursive)
			{
				Win32Native.WIN32_FIND_DATA wIN32_FIND_DATA = new Win32Native.WIN32_FIND_DATA();
				int lastWin32Error;
				using (SafeFindHandle safeFindHandle = Win32Native.FindFirstFile(fullPath + Path.DirectorySeparatorChar + "*", wIN32_FIND_DATA))
				{
					if (safeFindHandle.IsInvalid)
					{
						lastWin32Error = Marshal.GetLastWin32Error();
						__Error.WinIOError(lastWin32Error, fullPath);
					}
					do
					{
						if (0 != (wIN32_FIND_DATA.dwFileAttributes & 0x10))
						{
							if (wIN32_FIND_DATA.cFileName.Equals(".") || wIN32_FIND_DATA.cFileName.Equals(".."))
							{
								continue;
							}
							if (0 == (wIN32_FIND_DATA.dwFileAttributes & 0x400))
							{
								string fullPath2 = Path.InternalCombine(fullPath, wIN32_FIND_DATA.cFileName);
								string userPath2 = Path.InternalCombine(userPath, wIN32_FIND_DATA.cFileName);
								try
								{
									DeleteHelper(fullPath2, userPath2, recursive);
								}
								catch (Exception ex2)
								{
									if (ex == null)
									{
										ex = ex2;
									}
								}
								continue;
							}
							if (wIN32_FIND_DATA.dwReserved0 == -1610612733)
							{
								string mountPoint = Path.InternalCombine(fullPath, wIN32_FIND_DATA.cFileName + Path.DirectorySeparatorChar);
								if (!Win32Native.DeleteVolumeMountPoint(mountPoint))
								{
									lastWin32Error = Marshal.GetLastWin32Error();
									try
									{
										__Error.WinIOError(lastWin32Error, wIN32_FIND_DATA.cFileName);
									}
									catch (Exception ex3)
									{
										if (ex == null)
										{
											ex = ex3;
										}
									}
								}
							}
							string path = Path.InternalCombine(fullPath, wIN32_FIND_DATA.cFileName);
							if (Win32Native.RemoveDirectory(path))
							{
								continue;
							}
							lastWin32Error = Marshal.GetLastWin32Error();
							try
							{
								__Error.WinIOError(lastWin32Error, wIN32_FIND_DATA.cFileName);
							}
							catch (Exception ex4)
							{
								if (ex == null)
								{
									ex = ex4;
								}
							}
							continue;
						}
						string path2 = Path.InternalCombine(fullPath, wIN32_FIND_DATA.cFileName);
						if (Win32Native.DeleteFile(path2))
						{
							continue;
						}
						lastWin32Error = Marshal.GetLastWin32Error();
						try
						{
							__Error.WinIOError(lastWin32Error, wIN32_FIND_DATA.cFileName);
						}
						catch (Exception ex5)
						{
							if (ex == null)
							{
								ex = ex5;
							}
						}
					}
					while (Win32Native.FindNextFile(safeFindHandle, wIN32_FIND_DATA));
					lastWin32Error = Marshal.GetLastWin32Error();
				}
				if (ex != null)
				{
					throw ex;
				}
				if (lastWin32Error != 0 && lastWin32Error != 18)
				{
					__Error.WinIOError(lastWin32Error, userPath);
				}
			}
			if (!Win32Native.RemoveDirectory(fullPath))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 2)
				{
					lastWin32Error = 3;
				}
				if (lastWin32Error == 5)
				{
					throw new IOException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("UnauthorizedAccess_IODenied_Path"), userPath));
				}
				__Error.WinIOError(lastWin32Error, fullPath);
			}
		}

		private static SafeFileHandle OpenHandle(string path)
		{
			string fullPathInternal = Path.GetFullPathInternal(path);
			string pathRoot = Path.GetPathRoot(fullPathInternal);
			if (pathRoot == fullPathInternal && pathRoot[1] == Path.VolumeSeparatorChar)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_PathIsVolume"));
			}
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				GetDemandDir(fullPathInternal, thisDirOnly: true)
			}, checkForDuplicates: false, needFullPath: false).Demand();
			SafeFileHandle safeFileHandle = Win32Native.SafeCreateFile(fullPathInternal, 1073741824, FileShare.Write | FileShare.Delete, null, FileMode.Open, 33554432, Win32Native.NULL);
			if (safeFileHandle.IsInvalid)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				__Error.WinIOError(lastWin32Error, fullPathInternal);
			}
			return safeFileHandle;
		}
	}
}
