namespace System.Net
{
	[Flags]
	internal enum CloseExState
	{
		Normal = 0x0,
		Abort = 0x1,
		Silent = 0x2
	}
}
