using System.IO;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.Security.AccessControl
{
	public sealed class FileSecurity : FileSystemSecurity
	{
		public FileSecurity()
			: base(isContainer: false)
		{
		}

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		public FileSecurity(string fileName, AccessControlSections includeSections)
			: base(isContainer: false, fileName, includeSections, isDirectory: false)
		{
			string fullPathInternal = Path.GetFullPathInternal(fileName);
			new FileIOPermission(FileIOPermissionAccess.NoAccess, AccessControlActions.View, fullPathInternal).Demand();
		}

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		internal FileSecurity(SafeFileHandle handle, string fullPath, AccessControlSections includeSections)
			: base(isContainer: false, handle, includeSections, isDirectory: false)
		{
			if (fullPath != null)
			{
				new FileIOPermission(FileIOPermissionAccess.NoAccess, AccessControlActions.View, fullPath).Demand();
			}
			else
			{
				new FileIOPermission(PermissionState.Unrestricted).Demand();
			}
		}
	}
}
