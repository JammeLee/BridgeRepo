namespace System.Threading
{
	internal delegate int WaitDelegate(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);
}
