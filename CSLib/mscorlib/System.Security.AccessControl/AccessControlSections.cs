namespace System.Security.AccessControl
{
	[Flags]
	public enum AccessControlSections
	{
		None = 0x0,
		Audit = 0x1,
		Access = 0x2,
		Owner = 0x4,
		Group = 0x8,
		All = 0xF
	}
}
