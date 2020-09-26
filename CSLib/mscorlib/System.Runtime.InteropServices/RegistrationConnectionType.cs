namespace System.Runtime.InteropServices
{
	[Flags]
	public enum RegistrationConnectionType
	{
		SingleUse = 0x0,
		MultipleUse = 0x1,
		MultiSeparate = 0x2,
		Suspended = 0x4,
		Surrogate = 0x8
	}
}
