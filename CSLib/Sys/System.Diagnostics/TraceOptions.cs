namespace System.Diagnostics
{
	[Flags]
	public enum TraceOptions
	{
		None = 0x0,
		LogicalOperationStack = 0x1,
		DateTime = 0x2,
		Timestamp = 0x4,
		ProcessId = 0x8,
		ThreadId = 0x10,
		Callstack = 0x20
	}
}
