namespace System.IO
{
	[Flags]
	public enum WatcherChangeTypes
	{
		Created = 0x1,
		Deleted = 0x2,
		Changed = 0x4,
		Renamed = 0x8,
		All = 0xF
	}
}
