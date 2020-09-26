namespace System.Threading
{
	internal class _IOCompletionCallback
	{
		private IOCompletionCallback _ioCompletionCallback;

		private ExecutionContext _executionContext;

		private uint _errorCode;

		private uint _numBytes;

		private unsafe NativeOverlapped* _pOVERLAP;

		internal static ContextCallback _ccb = IOCompletionCallback_Context;

		internal unsafe _IOCompletionCallback(IOCompletionCallback ioCompletionCallback, ref StackCrawlMark stackMark)
		{
			_ioCompletionCallback = ioCompletionCallback;
			_executionContext = ExecutionContext.Capture(ref stackMark);
			ExecutionContext.ClearSyncContext(_executionContext);
		}

		internal unsafe static void IOCompletionCallback_Context(object state)
		{
			_IOCompletionCallback iOCompletionCallback = (_IOCompletionCallback)state;
			iOCompletionCallback._ioCompletionCallback(iOCompletionCallback._errorCode, iOCompletionCallback._numBytes, iOCompletionCallback._pOVERLAP);
		}

		internal unsafe static void PerformIOCompletionCallback(uint errorCode, uint numBytes, NativeOverlapped* pOVERLAP)
		{
			do
			{
				Overlapped overlapped = OverlappedData.GetOverlappedFromNative(pOVERLAP).m_overlapped;
				_IOCompletionCallback iocbHelper = overlapped.iocbHelper;
				if (iocbHelper == null || iocbHelper._executionContext == null || iocbHelper._executionContext.IsDefaultFTContext())
				{
					IOCompletionCallback userCallback = overlapped.UserCallback;
					userCallback(errorCode, numBytes, pOVERLAP);
				}
				else
				{
					iocbHelper._errorCode = errorCode;
					iocbHelper._numBytes = numBytes;
					iocbHelper._pOVERLAP = pOVERLAP;
					ExecutionContext.Run(iocbHelper._executionContext.CreateCopy(), _ccb, iocbHelper);
				}
				OverlappedData.CheckVMForIOPacket(out pOVERLAP, out errorCode, out numBytes);
			}
			while (pOVERLAP != null);
		}
	}
}
