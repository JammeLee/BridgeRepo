using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public sealed class DriveInfo : ISerializable
	{
		private const string NameField = "_name";

		private string _name;

		public string Name => _name;

		public DriveType DriveType => (DriveType)Win32Native.GetDriveType(Name);

		public string DriveFormat
		{
			get
			{
				StringBuilder volumeName = new StringBuilder(50);
				StringBuilder stringBuilder = new StringBuilder(50);
				int errorMode = Win32Native.SetErrorMode(1);
				try
				{
					if (!Win32Native.GetVolumeInformation(Name, volumeName, 50, out var _, out var _, out var _, stringBuilder, 50))
					{
						int num = Marshal.GetLastWin32Error();
						if (num == 13)
						{
							num = 15;
						}
						__Error.WinIODriveError(Name, num);
					}
				}
				finally
				{
					Win32Native.SetErrorMode(errorMode);
				}
				return stringBuilder.ToString();
			}
		}

		public bool IsReady => Directory.InternalExists(Name);

		public long AvailableFreeSpace
		{
			get
			{
				int errorMode = Win32Native.SetErrorMode(1);
				try
				{
					if (!Win32Native.GetDiskFreeSpaceEx(Name, out var freeBytesForUser, out var _, out var _))
					{
						__Error.WinIODriveError(Name);
						return freeBytesForUser;
					}
					return freeBytesForUser;
				}
				finally
				{
					Win32Native.SetErrorMode(errorMode);
				}
			}
		}

		public long TotalFreeSpace
		{
			get
			{
				int errorMode = Win32Native.SetErrorMode(1);
				try
				{
					if (!Win32Native.GetDiskFreeSpaceEx(Name, out var _, out var _, out var freeBytes))
					{
						__Error.WinIODriveError(Name);
						return freeBytes;
					}
					return freeBytes;
				}
				finally
				{
					Win32Native.SetErrorMode(errorMode);
				}
			}
		}

		public long TotalSize
		{
			get
			{
				int errorMode = Win32Native.SetErrorMode(1);
				try
				{
					if (!Win32Native.GetDiskFreeSpaceEx(Name, out var _, out var totalBytes, out var _))
					{
						__Error.WinIODriveError(Name);
						return totalBytes;
					}
					return totalBytes;
				}
				finally
				{
					Win32Native.SetErrorMode(errorMode);
				}
			}
		}

		public DirectoryInfo RootDirectory => new DirectoryInfo(Name);

		public string VolumeLabel
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(50);
				StringBuilder fileSystemName = new StringBuilder(50);
				int errorMode = Win32Native.SetErrorMode(1);
				try
				{
					if (!Win32Native.GetVolumeInformation(Name, stringBuilder, 50, out var _, out var _, out var _, fileSystemName, 50))
					{
						int num = Marshal.GetLastWin32Error();
						if (num == 13)
						{
							num = 15;
						}
						__Error.WinIODriveError(Name, num);
					}
				}
				finally
				{
					Win32Native.SetErrorMode(errorMode);
				}
				return stringBuilder.ToString();
			}
			set
			{
				string path = _name + '.';
				new FileIOPermission(FileIOPermissionAccess.Write, path).Demand();
				int errorMode = Win32Native.SetErrorMode(1);
				try
				{
					if (!Win32Native.SetVolumeLabel(Name, value))
					{
						int lastWin32Error = Marshal.GetLastWin32Error();
						if (lastWin32Error == 5)
						{
							throw new UnauthorizedAccessException(Environment.GetResourceString("InvalidOperation_SetVolumeLabelFailed"));
						}
						__Error.WinIODriveError(Name, lastWin32Error);
					}
				}
				finally
				{
					Win32Native.SetErrorMode(errorMode);
				}
			}
		}

		public DriveInfo(string driveName)
		{
			if (driveName == null)
			{
				throw new ArgumentNullException("driveName");
			}
			if (driveName.Length == 1)
			{
				_name = driveName + ":\\";
			}
			else
			{
				Path.CheckInvalidPathChars(driveName);
				_name = Path.GetPathRoot(driveName);
				if (_name == null || _name.Length == 0 || _name.StartsWith("\\\\", StringComparison.Ordinal))
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDriveLetterOrRootDir"));
				}
			}
			if (_name.Length == 2 && _name[1] == ':')
			{
				_name += "\\";
			}
			char c = driveName[0];
			if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z'))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDriveLetterOrRootDir"));
			}
			string path = _name + '.';
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
		}

		private DriveInfo(SerializationInfo info, StreamingContext context)
		{
			_name = (string)info.GetValue("_name", typeof(string));
			string path = _name + '.';
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
		}

		public static DriveInfo[] GetDrives()
		{
			string[] logicalDrives = Directory.GetLogicalDrives();
			DriveInfo[] array = new DriveInfo[logicalDrives.Length];
			for (int i = 0; i < logicalDrives.Length; i++)
			{
				array[i] = new DriveInfo(logicalDrives[i]);
			}
			return array;
		}

		public override string ToString()
		{
			return Name;
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("_name", _name, typeof(string));
		}
	}
}
