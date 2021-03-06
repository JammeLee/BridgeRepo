using System.Security.Permissions;

namespace System.IO
{
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	internal static class Direct
	{
		public const int FILE_ACTION_ADDED = 1;

		public const int FILE_ACTION_REMOVED = 2;

		public const int FILE_ACTION_MODIFIED = 3;

		public const int FILE_ACTION_RENAMED_OLD_NAME = 4;

		public const int FILE_ACTION_RENAMED_NEW_NAME = 5;

		public const int FILE_NOTIFY_CHANGE_FILE_NAME = 1;

		public const int FILE_NOTIFY_CHANGE_DIR_NAME = 2;

		public const int FILE_NOTIFY_CHANGE_NAME = 3;

		public const int FILE_NOTIFY_CHANGE_ATTRIBUTES = 4;

		public const int FILE_NOTIFY_CHANGE_SIZE = 8;

		public const int FILE_NOTIFY_CHANGE_LAST_WRITE = 16;

		public const int FILE_NOTIFY_CHANGE_LAST_ACCESS = 32;

		public const int FILE_NOTIFY_CHANGE_CREATION = 64;

		public const int FILE_NOTIFY_CHANGE_SECURITY = 256;
	}
}
