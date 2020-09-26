namespace System.Net.NetworkInformation
{
	[Flags]
	public enum NetworkInformationAccess
	{
		None = 0x0,
		Read = 0x1,
		Ping = 0x4
	}
}
