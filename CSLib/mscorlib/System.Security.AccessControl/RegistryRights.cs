namespace System.Security.AccessControl
{
	[Flags]
	public enum RegistryRights
	{
		QueryValues = 0x1,
		SetValue = 0x2,
		CreateSubKey = 0x4,
		EnumerateSubKeys = 0x8,
		Notify = 0x10,
		CreateLink = 0x20,
		ExecuteKey = 0x20019,
		ReadKey = 0x20019,
		WriteKey = 0x20006,
		Delete = 0x10000,
		ReadPermissions = 0x20000,
		ChangePermissions = 0x40000,
		TakeOwnership = 0x80000,
		FullControl = 0xF003F
	}
}
