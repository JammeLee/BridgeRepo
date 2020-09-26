using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	[FileIOPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
	public abstract class FileSystemInfo : MarshalByRefObject, ISerializable
	{
		private const int ERROR_INVALID_PARAMETER = 87;

		internal const int ERROR_ACCESS_DENIED = 5;

		internal Win32Native.WIN32_FILE_ATTRIBUTE_DATA _data;

		internal int _dataInitialised = -1;

		protected string FullPath;

		protected string OriginalPath;

		public virtual string FullName
		{
			get
			{
				string path = ((!(this is DirectoryInfo)) ? FullPath : Directory.GetDemandDir(FullPath, thisDirOnly: true));
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
				return FullPath;
			}
		}

		public string Extension
		{
			get
			{
				int length = FullPath.Length;
				int num = length;
				while (--num >= 0)
				{
					char c = FullPath[num];
					if (c == '.')
					{
						return FullPath.Substring(num, length - num);
					}
					if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar)
					{
						break;
					}
				}
				return string.Empty;
			}
		}

		public abstract string Name
		{
			get;
		}

		public abstract bool Exists
		{
			get;
		}

		public DateTime CreationTime
		{
			get
			{
				return CreationTimeUtc.ToLocalTime();
			}
			set
			{
				CreationTimeUtc = value.ToUniversalTime();
			}
		}

		[ComVisible(false)]
		public DateTime CreationTimeUtc
		{
			get
			{
				if (_dataInitialised == -1)
				{
					_data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
					Refresh();
				}
				if (_dataInitialised != 0)
				{
					__Error.WinIOError(_dataInitialised, OriginalPath);
				}
				long fileTime = (long)(((ulong)_data.ftCreationTimeHigh << 32) | _data.ftCreationTimeLow);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (this is DirectoryInfo)
				{
					Directory.SetCreationTimeUtc(FullPath, value);
				}
				else
				{
					File.SetCreationTimeUtc(FullPath, value);
				}
				_dataInitialised = -1;
			}
		}

		public DateTime LastAccessTime
		{
			get
			{
				return LastAccessTimeUtc.ToLocalTime();
			}
			set
			{
				LastAccessTimeUtc = value.ToUniversalTime();
			}
		}

		[ComVisible(false)]
		public DateTime LastAccessTimeUtc
		{
			get
			{
				if (_dataInitialised == -1)
				{
					_data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
					Refresh();
				}
				if (_dataInitialised != 0)
				{
					__Error.WinIOError(_dataInitialised, OriginalPath);
				}
				long fileTime = (long)(((ulong)_data.ftLastAccessTimeHigh << 32) | _data.ftLastAccessTimeLow);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (this is DirectoryInfo)
				{
					Directory.SetLastAccessTimeUtc(FullPath, value);
				}
				else
				{
					File.SetLastAccessTimeUtc(FullPath, value);
				}
				_dataInitialised = -1;
			}
		}

		public DateTime LastWriteTime
		{
			get
			{
				return LastWriteTimeUtc.ToLocalTime();
			}
			set
			{
				LastWriteTimeUtc = value.ToUniversalTime();
			}
		}

		[ComVisible(false)]
		public DateTime LastWriteTimeUtc
		{
			get
			{
				if (_dataInitialised == -1)
				{
					_data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
					Refresh();
				}
				if (_dataInitialised != 0)
				{
					__Error.WinIOError(_dataInitialised, OriginalPath);
				}
				long fileTime = (long)(((ulong)_data.ftLastWriteTimeHigh << 32) | _data.ftLastWriteTimeLow);
				return DateTime.FromFileTimeUtc(fileTime);
			}
			set
			{
				if (this is DirectoryInfo)
				{
					Directory.SetLastWriteTimeUtc(FullPath, value);
				}
				else
				{
					File.SetLastWriteTimeUtc(FullPath, value);
				}
				_dataInitialised = -1;
			}
		}

		public FileAttributes Attributes
		{
			get
			{
				if (_dataInitialised == -1)
				{
					_data = default(Win32Native.WIN32_FILE_ATTRIBUTE_DATA);
					Refresh();
				}
				if (_dataInitialised != 0)
				{
					__Error.WinIOError(_dataInitialised, OriginalPath);
				}
				return (FileAttributes)_data.fileAttributes;
			}
			set
			{
				new FileIOPermission(FileIOPermissionAccess.Write, FullPath).Demand();
				if (!Win32Native.SetFileAttributes(FullPath, (int)value))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					switch (lastWin32Error)
					{
					case 87:
						throw new ArgumentException(Environment.GetResourceString("Arg_InvalidFileAttrs"));
					case 5:
						throw new ArgumentException(Environment.GetResourceString("UnauthorizedAccess_IODenied_NoPathName"));
					}
					__Error.WinIOError(lastWin32Error, OriginalPath);
				}
				_dataInitialised = -1;
			}
		}

		protected FileSystemInfo()
		{
		}

		protected FileSystemInfo(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			FullPath = Path.GetFullPathInternal(info.GetString("FullPath"));
			OriginalPath = info.GetString("OriginalPath");
			_dataInitialised = -1;
		}

		public abstract void Delete();

		public void Refresh()
		{
			_dataInitialised = File.FillAttributeInfo(FullPath, ref _data, tryagain: false, returnErrorOnNotFound: false);
		}

		[ComVisible(false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, FullPath).Demand();
			info.AddValue("OriginalPath", OriginalPath, typeof(string));
			info.AddValue("FullPath", FullPath, typeof(string));
		}
	}
}
