namespace System.Net
{
	[Flags]
	internal enum ThreadKinds
	{
		Unknown = 0x0,
		User = 0x1,
		System = 0x2,
		Sync = 0x4,
		Async = 0x8,
		Timer = 0x10,
		CompletionPort = 0x20,
		Worker = 0x40,
		Finalization = 0x80,
		Other = 0x100,
		OwnerMask = 0x3,
		SyncMask = 0xC,
		SourceMask = 0x1F0,
		SafeSources = 0x160,
		ThreadPool = 0x60
	}
}
