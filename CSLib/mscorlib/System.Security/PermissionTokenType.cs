namespace System.Security
{
	[Flags]
	internal enum PermissionTokenType
	{
		Normal = 0x1,
		IUnrestricted = 0x2,
		DontKnow = 0x4,
		BuiltIn = 0x8
	}
}
