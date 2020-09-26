namespace System.Net
{
	[Flags]
	public enum DecompressionMethods
	{
		None = 0x0,
		GZip = 0x1,
		Deflate = 0x2
	}
}
