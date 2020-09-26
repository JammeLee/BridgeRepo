namespace System.Security.AccessControl
{
	[Flags]
	public enum CryptoKeyRights
	{
		ReadData = 0x1,
		WriteData = 0x2,
		ReadExtendedAttributes = 0x8,
		WriteExtendedAttributes = 0x10,
		ReadAttributes = 0x80,
		WriteAttributes = 0x100,
		Delete = 0x10000,
		ReadPermissions = 0x20000,
		ChangePermissions = 0x40000,
		TakeOwnership = 0x80000,
		Synchronize = 0x100000,
		FullControl = 0x1F019B,
		GenericAll = 0x10000000,
		GenericExecute = 0x20000000,
		GenericWrite = 0x40000000,
		GenericRead = int.MinValue
	}
}
