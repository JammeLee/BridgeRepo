namespace System.Net.Mail
{
	[Flags]
	internal enum MBKeyAccess : uint
	{
		Read = 0x1u,
		Write = 0x2u
	}
}
