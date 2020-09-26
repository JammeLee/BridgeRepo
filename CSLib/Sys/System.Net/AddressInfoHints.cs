namespace System.Net
{
	[Flags]
	internal enum AddressInfoHints
	{
		AI_PASSIVE = 0x1,
		AI_CANONNAME = 0x2,
		AI_NUMERICHOST = 0x4
	}
}
