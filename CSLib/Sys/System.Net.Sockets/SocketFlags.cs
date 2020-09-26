namespace System.Net.Sockets
{
	[Flags]
	public enum SocketFlags
	{
		None = 0x0,
		OutOfBand = 0x1,
		Peek = 0x2,
		DontRoute = 0x4,
		MaxIOVectorLength = 0x10,
		Truncated = 0x100,
		ControlDataTruncated = 0x200,
		Broadcast = 0x400,
		Multicast = 0x800,
		Partial = 0x8000
	}
}
