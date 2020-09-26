namespace System.Net
{
	[Flags]
	internal enum Alg
	{
		Any = 0x0,
		ClassSignture = 0x2000,
		ClassEncrypt = 0x6000,
		ClassHash = 0x8000,
		ClassKeyXch = 0xA000,
		TypeRSA = 0x400,
		TypeBlock = 0x600,
		TypeStream = 0x800,
		TypeDH = 0xA00,
		NameDES = 0x1,
		NameRC2 = 0x2,
		Name3DES = 0x3,
		NameAES_128 = 0xE,
		NameAES_192 = 0xF,
		NameAES_256 = 0x10,
		NameAES = 0x11,
		NameRC4 = 0x1,
		NameMD5 = 0x3,
		NameSHA = 0x4,
		NameDH_Ephem = 0x2
	}
}
