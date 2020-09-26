namespace System.Net.Sockets
{
	[Flags]
	public enum SocketInformationOptions
	{
		NonBlocking = 0x1,
		Connected = 0x2,
		Listening = 0x4,
		UseOnlyOverlappedIO = 0x8
	}
}
