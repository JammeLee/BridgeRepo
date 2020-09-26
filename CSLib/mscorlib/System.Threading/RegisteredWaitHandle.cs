using System.Runtime.InteropServices;

namespace System.Threading
{
	[ComVisible(true)]
	public sealed class RegisteredWaitHandle : MarshalByRefObject
	{
		private RegisteredWaitHandleSafe internalRegisteredWait;

		internal RegisteredWaitHandle()
		{
			internalRegisteredWait = new RegisteredWaitHandleSafe();
		}

		internal void SetHandle(IntPtr handle)
		{
			internalRegisteredWait.SetHandle(handle);
		}

		internal void SetWaitObject(WaitHandle waitObject)
		{
			internalRegisteredWait.SetWaitObject(waitObject);
		}

		[ComVisible(true)]
		public bool Unregister(WaitHandle waitObject)
		{
			return internalRegisteredWait.Unregister(waitObject);
		}
	}
}
