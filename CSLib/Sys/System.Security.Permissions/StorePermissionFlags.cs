namespace System.Security.Permissions
{
	[Serializable]
	[Flags]
	public enum StorePermissionFlags
	{
		NoFlags = 0x0,
		CreateStore = 0x1,
		DeleteStore = 0x2,
		EnumerateStores = 0x4,
		OpenStore = 0x10,
		AddToStore = 0x20,
		RemoveFromStore = 0x40,
		EnumerateCertificates = 0x80,
		AllFlags = 0xF7
	}
}
