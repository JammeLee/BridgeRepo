using System.Runtime.InteropServices;

namespace System.Threading
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum ThreadState
	{
		Running = 0x0,
		StopRequested = 0x1,
		SuspendRequested = 0x2,
		Background = 0x4,
		Unstarted = 0x8,
		Stopped = 0x10,
		WaitSleepJoin = 0x20,
		Suspended = 0x40,
		AbortRequested = 0x80,
		Aborted = 0x100
	}
}
