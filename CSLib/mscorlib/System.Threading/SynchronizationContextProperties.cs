namespace System.Threading
{
	[Flags]
	internal enum SynchronizationContextProperties
	{
		None = 0x0,
		RequireWaitNotification = 0x1
	}
}
