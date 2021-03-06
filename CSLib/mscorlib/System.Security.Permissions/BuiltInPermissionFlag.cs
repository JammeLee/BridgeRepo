namespace System.Security.Permissions
{
	[Serializable]
	internal enum BuiltInPermissionFlag
	{
		EnvironmentPermission = 1,
		FileDialogPermission = 2,
		FileIOPermission = 4,
		IsolatedStorageFilePermission = 8,
		ReflectionPermission = 0x10,
		RegistryPermission = 0x20,
		SecurityPermission = 0x40,
		UIPermission = 0x80,
		PrincipalPermission = 0x100,
		PublisherIdentityPermission = 0x200,
		SiteIdentityPermission = 0x400,
		StrongNameIdentityPermission = 0x800,
		UrlIdentityPermission = 0x1000,
		ZoneIdentityPermission = 0x2000,
		KeyContainerPermission = 0x4000
	}
}
