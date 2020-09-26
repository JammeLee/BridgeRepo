namespace System.Threading
{
	internal class _TimerCallback
	{
		private TimerCallback _timerCallback;

		private ExecutionContext _executionContext;

		private object _state;

		internal static ContextCallback _ccb = TimerCallback_Context;

		internal static void TimerCallback_Context(object state)
		{
			_TimerCallback timerCallback = (_TimerCallback)state;
			timerCallback._timerCallback(timerCallback._state);
		}

		internal _TimerCallback(TimerCallback timerCallback, object state, ref StackCrawlMark stackMark)
		{
			_timerCallback = timerCallback;
			_state = state;
			if (!ExecutionContext.IsFlowSuppressed())
			{
				_executionContext = ExecutionContext.Capture(ref stackMark);
				ExecutionContext.ClearSyncContext(_executionContext);
			}
		}

		internal static void PerformTimerCallback(object state)
		{
			_TimerCallback timerCallback = (_TimerCallback)state;
			if (timerCallback._executionContext == null)
			{
				TimerCallback timerCallback2 = timerCallback._timerCallback;
				timerCallback2(timerCallback._state);
			}
			else
			{
				ExecutionContext.Run(timerCallback._executionContext.CreateCopy(), _ccb, timerCallback);
			}
		}
	}
}
