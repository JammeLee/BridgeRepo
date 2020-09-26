namespace System.Net.Sockets
{
	[Flags]
	public enum TransmitFileOptions
	{
		UseDefaultWorkerThread = 0x0,
		Disconnect = 0x1,
		ReuseSocket = 0x2,
		WriteBehind = 0x4,
		UseSystemThread = 0x10,
		UseKernelApc = 0x20
	}
}
