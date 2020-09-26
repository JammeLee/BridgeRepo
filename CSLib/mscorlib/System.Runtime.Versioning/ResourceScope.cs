namespace System.Runtime.Versioning
{
	[Flags]
	public enum ResourceScope
	{
		None = 0x0,
		Machine = 0x1,
		Process = 0x2,
		AppDomain = 0x4,
		Library = 0x8,
		Private = 0x10,
		Assembly = 0x20
	}
}
