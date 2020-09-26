namespace System
{
	[Flags]
	public enum GenericUriParserOptions
	{
		Default = 0x0,
		GenericAuthority = 0x1,
		AllowEmptyAuthority = 0x2,
		NoUserInfo = 0x4,
		NoPort = 0x8,
		NoQuery = 0x10,
		NoFragment = 0x20,
		DontConvertPathBackslashes = 0x40,
		DontCompressPath = 0x80,
		DontUnescapePathDotsAndSlashes = 0x100,
		Idn = 0x200,
		IriParsing = 0x400
	}
}
