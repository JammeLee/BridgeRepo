namespace System.Security.AccessControl
{
	[Flags]
	public enum SecurityInfos
	{
		Owner = 0x1,
		Group = 0x2,
		DiscretionaryAcl = 0x4,
		SystemAcl = 0x8
	}
}
