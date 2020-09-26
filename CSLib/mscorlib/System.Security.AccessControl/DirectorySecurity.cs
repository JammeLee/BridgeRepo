using System.IO;
using System.Security.Permissions;

namespace System.Security.AccessControl
{
	public sealed class DirectorySecurity : FileSystemSecurity
	{
		public DirectorySecurity()
			: base(isContainer: true)
		{
		}

		[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
		public DirectorySecurity(string name, AccessControlSections includeSections)
			: base(isContainer: true, name, includeSections, isDirectory: true)
		{
			string fullPathInternal = Path.GetFullPathInternal(name);
			new FileIOPermission(FileIOPermissionAccess.NoAccess, AccessControlActions.View, fullPathInternal).Demand();
		}
	}
}
