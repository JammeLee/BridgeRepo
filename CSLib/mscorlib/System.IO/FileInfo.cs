using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public sealed class FileInfo : FileSystemInfo
	{
		private string _name;

		public override string Name => _name;

		public long Length
		{
			get
			{
				if (_dataInitialised == -1)
				{
					Refresh();
				}
				if (_dataInitialised != 0)
				{
					__Error.WinIOError(_dataInitialised, OriginalPath);
				}
				if (((uint)_data.fileAttributes & 0x10u) != 0)
				{
					__Error.WinIOError(2, OriginalPath);
				}
				return ((long)_data.fileSizeHigh << 32) | (_data.fileSizeLow & 0xFFFFFFFFu);
			}
		}

		public string DirectoryName
		{
			get
			{
				string directoryName = Path.GetDirectoryName(FullPath);
				if (directoryName != null)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[1]
					{
						directoryName
					}, checkForDuplicates: false, needFullPath: false).Demand();
				}
				return directoryName;
			}
		}

		public DirectoryInfo Directory
		{
			get
			{
				string directoryName = DirectoryName;
				if (directoryName == null)
				{
					return null;
				}
				return new DirectoryInfo(directoryName);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return (base.Attributes & FileAttributes.ReadOnly) != 0;
			}
			set
			{
				if (value)
				{
					base.Attributes |= FileAttributes.ReadOnly;
				}
				else
				{
					base.Attributes &= ~FileAttributes.ReadOnly;
				}
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
					return (_data.fileAttributes & 0x10) == 0;
				}
				catch
				{
					return false;
				}
			}
		}

		public FileInfo(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			OriginalPath = fileName;
			string fullPathInternal = Path.GetFullPathInternal(fileName);
			new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			_name = Path.GetFileName(fileName);
			FullPath = fullPathInternal;
		}

		private FileInfo(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			new FileIOPermission(FileIOPermissionAccess.Read, new string[1]
			{
				FullPath
			}, checkForDuplicates: false, needFullPath: false).Demand();
			_name = Path.GetFileName(OriginalPath);
		}

		internal FileInfo(string fullPath, bool ignoreThis)
		{
			_name = Path.GetFileName(fullPath);
			OriginalPath = _name;
			FullPath = fullPath;
		}

		public FileSecurity GetAccessControl()
		{
			return File.GetAccessControl(FullPath, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public FileSecurity GetAccessControl(AccessControlSections includeSections)
		{
			return File.GetAccessControl(FullPath, includeSections);
		}

		public void SetAccessControl(FileSecurity fileSecurity)
		{
			File.SetAccessControl(FullPath, fileSecurity);
		}

		public StreamReader OpenText()
		{
			return new StreamReader(FullPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024);
		}

		public StreamWriter CreateText()
		{
			return new StreamWriter(FullPath, append: false);
		}

		public StreamWriter AppendText()
		{
			return new StreamWriter(FullPath, append: true);
		}

		public FileInfo CopyTo(string destFileName)
		{
			return CopyTo(destFileName, overwrite: false);
		}

		public FileInfo CopyTo(string destFileName, bool overwrite)
		{
			destFileName = File.InternalCopy(FullPath, destFileName, overwrite);
			return new FileInfo(destFileName, ignoreThis: false);
		}

		public FileStream Create()
		{
			return File.Create(FullPath);
		}

		public override void Delete()
		{
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				FullPath
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (Environment.IsWin9X() && System.IO.Directory.InternalExists(FullPath))
			{
				throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("UnauthorizedAccess_IODenied_Path"), OriginalPath));
			}
			if (!Win32Native.DeleteFile(FullPath))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 2)
				{
					__Error.WinIOError(lastWin32Error, OriginalPath);
				}
			}
		}

		[ComVisible(false)]
		public void Decrypt()
		{
			File.Decrypt(FullPath);
		}

		[ComVisible(false)]
		public void Encrypt()
		{
			File.Encrypt(FullPath);
		}

		public FileStream Open(FileMode mode)
		{
			return Open(mode, FileAccess.ReadWrite, FileShare.None);
		}

		public FileStream Open(FileMode mode, FileAccess access)
		{
			return Open(mode, access, FileShare.None);
		}

		public FileStream Open(FileMode mode, FileAccess access, FileShare share)
		{
			return new FileStream(FullPath, mode, access, share);
		}

		public FileStream OpenRead()
		{
			return new FileStream(FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public FileStream OpenWrite()
		{
			return new FileStream(FullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}

		public void MoveTo(string destFileName)
		{
			if (destFileName == null)
			{
				throw new ArgumentNullException("destFileName");
			}
			if (destFileName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destFileName");
			}
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, new string[1]
			{
				FullPath
			}, checkForDuplicates: false, needFullPath: false).Demand();
			string fullPathInternal = Path.GetFullPathInternal(destFileName);
			new FileIOPermission(FileIOPermissionAccess.Write, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			if (!Win32Native.MoveFile(FullPath, fullPathInternal))
			{
				__Error.WinIOError();
			}
			FullPath = fullPathInternal;
			OriginalPath = destFileName;
			_name = Path.GetFileName(fullPathInternal);
			_dataInitialised = -1;
		}

		[ComVisible(false)]
		public FileInfo Replace(string destinationFileName, string destinationBackupFileName)
		{
			return Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors: false);
		}

		[ComVisible(false)]
		public FileInfo Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
		{
			File.Replace(FullPath, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
			return new FileInfo(destinationFileName);
		}

		public override string ToString()
		{
			return OriginalPath;
		}
	}
}
