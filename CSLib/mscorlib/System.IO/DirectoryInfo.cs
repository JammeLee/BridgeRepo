using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public sealed class DirectoryInfo : FileSystemInfo
	{
		private string[] demandDir;

		public override string Name
		{
			get
			{
				string text = FullPath;
				if (text.Length > 3)
				{
					if (text.EndsWith(Path.DirectorySeparatorChar))
					{
						text = FullPath.Substring(0, FullPath.Length - 1);
					}
					return Path.GetFileName(text);
				}
				return FullPath;
			}
		}

		public DirectoryInfo Parent
		{
			get
			{
				string text = FullPath;
				if (text.Length > 3 && text.EndsWith(Path.DirectorySeparatorChar))
				{
					text = FullPath.Substring(0, FullPath.Length - 1);
				}
				string directoryName = Path.GetDirectoryName(text);
				if (directoryName == null)
				{
					return null;
				}
				DirectoryInfo directoryInfo = new DirectoryInfo(directoryName, junk: false);
				new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, directoryInfo.demandDir, checkForDuplicates: false, needFullPath: false).Demand();
				return directoryInfo;
			}
		}

		public override bool Exists
		{
			get
			{
				try
				{
					if (_dataInitialised == -1)
					{
						Refresh();
					}
					if (_dataInitialised != 0)
					{
						return false;
					}
					return _data.fileAttributes != -1 && (_data.fileAttributes & 0x10) != 0;
				}
				catch
				{
					return false;
				}
			}
		}

		public DirectoryInfo Root
		{
			get
			{
				int rootLength = Path.GetRootLength(FullPath);
				string text = FullPath.Substring(0, rootLength);
				string text2 = Directory.GetDemandDir(text, thisDirOnly: true);
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[1]
				{
					text2
				}, checkForDuplicates: false, needFullPath: false).Demand();
				return new DirectoryInfo(text);
			}
		}

		public DirectoryInfo(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (path.Length == 2 && path[1] == ':')
			{
				OriginalPath = ".";
			}
			else
			{
				OriginalPath = path;
			}
			string fullPathInternal = Path.GetFullPathInternal(path);
			demandDir = new string[1]
			{
				Directory.GetDemandDir(fullPathInternal, thisDirOnly: true)
			};
			new FileIOPermission(FileIOPermissionAccess.Read, demandDir, checkForDuplicates: false, needFullPath: false).Demand();
			FullPath = fullPathInternal;
		}

		internal DirectoryInfo(string fullPath, bool junk)
		{
			OriginalPath = Path.GetFileName(fullPath);
			FullPath = fullPath;
			demandDir = new string[1]
			{
				Directory.GetDemandDir(fullPath, thisDirOnly: true)
			};
		}

		private DirectoryInfo(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			demandDir = new string[1]
			{
				Directory.GetDemandDir(FullPath, thisDirOnly: true)
			};
			new FileIOPermission(FileIOPermissionAccess.Read, demandDir, checkForDuplicates: false, needFullPath: false).Demand();
		}

		public DirectoryInfo CreateSubdirectory(string path)
		{
			return CreateSubdirectory(path, null);
		}

		public DirectoryInfo CreateSubdirectory(string path, DirectorySecurity directorySecurity)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			string path2 = Path.InternalCombine(FullPath, path);
			string fullPathInternal = Path.GetFullPathInternal(path2);
			if (string.Compare(FullPath, 0, fullPathInternal, 0, FullPath.Length, StringComparison.OrdinalIgnoreCase) != 0)
			{
				string displayablePath = __Error.GetDisplayablePath(OriginalPath, isInvalidPath: false);
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidSubPath"), path, displayablePath));
			}
			string text = Directory.GetDemandDir(fullPathInternal, thisDirOnly: true);
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				text
			}, checkForDuplicates: false, needFullPath: false).Demand();
			Directory.InternalCreateDirectory(fullPathInternal, path, directorySecurity);
			return new DirectoryInfo(fullPathInternal);
		}

		public void Create()
		{
			Directory.InternalCreateDirectory(FullPath, OriginalPath, null);
		}

		public void Create(DirectorySecurity directorySecurity)
		{
			Directory.InternalCreateDirectory(FullPath, OriginalPath, directorySecurity);
		}

		public DirectorySecurity GetAccessControl()
		{
			return Directory.GetAccessControl(FullPath, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
		{
			return Directory.GetAccessControl(FullPath, includeSections);
		}

		public void SetAccessControl(DirectorySecurity directorySecurity)
		{
			Directory.SetAccessControl(FullPath, directorySecurity);
		}

		public FileInfo[] GetFiles(string searchPattern)
		{
			return GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
		}

		private string FixupFileDirFullPath(string fileDirUserPath)
		{
			if (OriginalPath.Length == 0)
			{
				return Path.InternalCombine(FullPath, fileDirUserPath);
			}
			if (OriginalPath.EndsWith(Path.DirectorySeparatorChar) || OriginalPath.EndsWith(Path.AltDirectorySeparatorChar))
			{
				return Path.InternalCombine(FullPath, fileDirUserPath.Substring(OriginalPath.Length));
			}
			return Path.InternalCombine(FullPath, fileDirUserPath.Substring(OriginalPath.Length + 1));
		}

		public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
		{
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			string[] array = Directory.InternalGetFileDirectoryNames(FullPath, OriginalPath, searchPattern, includeFiles: true, includeDirs: false, searchOption);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = FixupFileDirFullPath(array[i]);
			}
			if (array.Length != 0)
			{
				new FileIOPermission(FileIOPermissionAccess.Read, array, checkForDuplicates: false, needFullPath: false).Demand();
			}
			FileInfo[] array2 = new FileInfo[array.Length];
			for (int j = 0; j < array.Length; j++)
			{
				array2[j] = new FileInfo(array[j], ignoreThis: false);
			}
			return array2;
		}

		public FileInfo[] GetFiles()
		{
			return GetFiles("*");
		}

		public DirectoryInfo[] GetDirectories()
		{
			return GetDirectories("*");
		}

		public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
		{
			return GetFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
		}

		private FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			string[] array = Directory.InternalGetFileDirectoryNames(FullPath, OriginalPath, searchPattern, includeFiles: false, includeDirs: true, searchOption);
			string[] array2 = Directory.InternalGetFileDirectoryNames(FullPath, OriginalPath, searchPattern, includeFiles: true, includeDirs: false, searchOption);
			FileSystemInfo[] array3 = new FileSystemInfo[array.Length + array2.Length];
			string[] array4 = new string[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = FixupFileDirFullPath(array[i]);
				array4[i] = array[i] + "\\.";
			}
			if (array.Length != 0)
			{
				new FileIOPermission(FileIOPermissionAccess.Read, array4, checkForDuplicates: false, needFullPath: false).Demand();
			}
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = FixupFileDirFullPath(array2[j]);
			}
			if (array2.Length != 0)
			{
				new FileIOPermission(FileIOPermissionAccess.Read, array2, checkForDuplicates: false, needFullPath: false).Demand();
			}
			int num = 0;
			for (int k = 0; k < array.Length; k++)
			{
				array3[num++] = new DirectoryInfo(array[k], junk: false);
			}
			for (int l = 0; l < array2.Length; l++)
			{
				array3[num++] = new FileInfo(array2[l], ignoreThis: false);
			}
			return array3;
		}

		public FileSystemInfo[] GetFileSystemInfos()
		{
			return GetFileSystemInfos("*");
		}

		public DirectoryInfo[] GetDirectories(string searchPattern)
		{
			return GetDirectories(searchPattern, SearchOption.TopDirectoryOnly);
		}

		public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
		{
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			string[] array = Directory.InternalGetFileDirectoryNames(FullPath, OriginalPath, searchPattern, includeFiles: false, includeDirs: true, searchOption);
			string[] array2 = new string[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = FixupFileDirFullPath(array[i]);
				array2[i] = array[i] + "\\.";
			}
			if (array.Length != 0)
			{
				new FileIOPermission(FileIOPermissionAccess.Read, array2, checkForDuplicates: false, needFullPath: false).Demand();
			}
			DirectoryInfo[] array3 = new DirectoryInfo[array.Length];
			for (int j = 0; j < array.Length; j++)
			{
				array3[j] = new DirectoryInfo(array[j], junk: false);
			}
			return array3;
		}

		public void MoveTo(string destDirName)
		{
			if (destDirName == null)
			{
				throw new ArgumentNullException("destDirName");
			}
			if (destDirName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destDirName");
			}
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, demandDir, checkForDuplicates: false, needFullPath: false).Demand();
			string text = Path.GetFullPathInternal(destDirName);
			if (!text.EndsWith(Path.DirectorySeparatorChar))
			{
				text += Path.DirectorySeparatorChar;
			}
			string path = text + '.';
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, path).Demand();
			string text2 = ((!FullPath.EndsWith(Path.DirectorySeparatorChar)) ? (FullPath + Path.DirectorySeparatorChar) : FullPath);
			if (CultureInfo.InvariantCulture.CompareInfo.Compare(text2, text, CompareOptions.IgnoreCase) == 0)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustBeDifferent"));
			}
			string pathRoot = Path.GetPathRoot(text2);
			string pathRoot2 = Path.GetPathRoot(text);
			if (CultureInfo.InvariantCulture.CompareInfo.Compare(pathRoot, pathRoot2, CompareOptions.IgnoreCase) != 0)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustHaveSameRoot"));
			}
			if (Environment.IsWin9X() && !Directory.InternalExists(FullPath))
			{
				throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.PathNotFound_Path"), destDirName));
			}
			if (!Win32Native.MoveFile(FullPath, destDirName))
			{
				int num = Marshal.GetLastWin32Error();
				if (num == 2)
				{
					num = 3;
					__Error.WinIOError(num, OriginalPath);
				}
				if (num == 5)
				{
					throw new IOException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("UnauthorizedAccess_IODenied_Path"), OriginalPath));
				}
				__Error.WinIOError(num, string.Empty);
			}
			FullPath = text;
			OriginalPath = destDirName;
			demandDir = new string[1]
			{
				Directory.GetDemandDir(FullPath, thisDirOnly: true)
			};
			_dataInitialised = -1;
		}

		public override void Delete()
		{
			Directory.Delete(FullPath, OriginalPath, recursive: false);
		}

		public void Delete(bool recursive)
		{
			Directory.Delete(FullPath, OriginalPath, recursive);
		}

		public override string ToString()
		{
			return OriginalPath;
		}
	}
}
