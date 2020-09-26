namespace System.Threading
{
	internal class _ThreadPoolWaitOrTimerCallback
	{
		private WaitOrTimerCallback _waitOrTimerCallback;

		private ExecutionContext _executionContext;

		private object _state;

		private static ContextCallback _ccbt = WaitOrTimerCallback_Context_t;

		private static ContextCallback _ccbf = WaitOrTimerCallback_Context_f;

		internal _ThreadPoolWaitOrTimerCallback(WaitOrTimerCallback waitOrTimerCallback, object state, bool compressStack, ref StackCrawlMark stackMark)
		{
			_waitOrTimerCallback = waitOrTimerCallback;
			_state = state;
			if (compressStack && !ExecutionContext.IsFlowSuppressed())
			{
				_executionContext = ExecutionContext.Capture(ref stackMark);
				ExecutionContext.ClearSyncContext(_executionContext);
			}
		}

		private static void WaitOrTimerCallback_Context_t(object state)
		{
			WaitOrTimerCallback_Context(state, timedOut: true);
		}

		private static void WaitOrTimerCallback_Context_f(object state)
		{
			WaitOrTimerCallback_Context(state, timedOut: false);
		}

		private static void WaitOrTimerCallback_Context(object state, bool timedOut)
		{
			_ThreadPoolWaitOrTimerCallback threadPoolWaitOrTimerCallback = (_ThreadPoolWaitOrTimerCallback)state;
			threadPoolWaitOrTimerCallback._waitOrTimerCallback(threadPoolWaitOrTimerCallback._state, timedOut);
		}

		internal static void PerformWaitOrTimerCallback(object state, bool timedOut)
		{
			_ThreadPoolWaitOrTimerCallback threadPoolWaitOrTimerCallback = (_ThreadPoolWaitOrTimerCallback)state;
			if (threadPoolWaitOrTimerCallback._executionContext == null)
			{
				WaitOrTimerCallback waitOrTimerCallback = threadPoolWaitOrTimerCallback._waitOrTimerCallback;
				waitOrTimerCallback(threadPoolWaitOrTimerCallback._state, timedOut);
			}
			else if (timedOut)
			{
				ExecutionContext.Run(threadPoolWaitOrTimerCallback._executionContext.CreateCopy(), _ccbt, threadPoolWaitOrTimerCallback);
			}
			else
			{
				ExecutionContext.Run(threadPoolWaitOrTimerCallback._executionContext.CreateCopy(), _ccbf, threadPoolWaitOrTimerCallback);
			}
		}
	}
}
