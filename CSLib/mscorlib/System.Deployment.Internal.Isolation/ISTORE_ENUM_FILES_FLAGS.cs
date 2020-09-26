namespace System.Deployment.Internal.Isolation
{
	[Flags]
	internal enum ISTORE_ENUM_FILES_FLAGS
	{
		ISTORE_ENUM_FILES_FLAG_INCLUDE_INSTALLED_FILES = 0x1,
		ISTORE_ENUM_FILES_FLAG_INCLUDE_MISSING_FILES = 0x2
	}
}
