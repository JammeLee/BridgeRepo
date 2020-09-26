namespace System
{
	[Flags]
	public enum UriComponents
	{
		Scheme = 0x1,
		UserInfo = 0x2,
		Host = 0x4,
		Port = 0x8,
		Path = 0x10,
		Query = 0x20,
		Fragment = 0x40,
		StrongPort = 0x80,
		KeepDelimiter = 0x40000000,
		SerializationInfoString = int.MinValue,
		AbsoluteUri = 0x7F,
		HostAndPort = 0x84,
		StrongAuthority = 0x86,
		SchemeAndServer = 0xD,
		HttpRequestUrl = 0x3D,
		PathAndQuery = 0x30
	}
}
