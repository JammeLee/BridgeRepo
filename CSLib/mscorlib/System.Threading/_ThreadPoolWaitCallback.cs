namespace System.Threading
{
	internal class _ThreadPoolWaitCallback
	{
		private WaitCallback _waitCallback;

		private ExecutionContext _executionContext;

		private object _state;

		protected internal _ThreadPoolWaitCallback _next;

		internal static ContextCallback _ccb = WaitCallback_Context;

		internal static void WaitCallback_Context(object state)
		{
			_ThreadPoolWaitCallback threadPoolWaitCallback = (_ThreadPoolWaitCallback)state;
			threadPoolWaitCallback._waitCallback(threadPoolWaitCallback._state);
		}

		internal _ThreadPoolWaitCallback(WaitCallback waitCallback, object state, bool compressStack, ref StackCrawlMark stackMark)
		{
			_waitCallback = waitCallback;
			_state = state;
			if (compressStack && !ExecutionContext.IsFlowSuppressed())
			{
				_executionContext = ExecutionContext.Capture(ref stackMark);
				ExecutionContext.ClearSyncContext(_executionContext);
			}
		}

		internal static void PerformWaitCallback(object state)
		{
			int num = 0;
			_ThreadPoolWaitCallback threadPoolWaitCallback = null;
			int tickCount = Environment.TickCount;
			do
			{
				threadPoolWaitCallback = ThreadPoolGlobals.tpQueue.DeQueue();
				if (threadPoolWaitCallback == null)
				{
					break;
				}
				ThreadPool.CompleteThreadPoolRequest(ThreadPoolGlobals.tpQueue.GetQueueCount());
				PerformWaitCallbackInternal(threadPoolWaitCallback);
				int tickCount2 = Environment.TickCount;
				num = tickCount2 - tickCount;
			}
			while (num <= ThreadPoolGlobals.tpQuantum || !ThreadPool.ShouldReturnToVm());
		}

		internal static void PerformWaitCallbackInternal(_ThreadPoolWaitCallback tpWaitCallBack)
		{
			if (tpWaitCallBack._executionContext == null)
			{
				WaitCallback waitCallback = tpWaitCallBack._waitCallback;
				waitCallback(tpWaitCallBack._state);
			}
			else
			{
				ExecutionContext.Run(tpWaitCallBack._executionContext, _ccb, tpWaitCallBack);
			}
		}
	}
}
