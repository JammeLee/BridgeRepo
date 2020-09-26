using System.Runtime.CompilerServices;

namespace System.Threading
{
	internal sealed class ThreadPoolRequestQueue
	{
		private _ThreadPoolWaitCallback tpHead;

		private _ThreadPoolWaitCallback tpTail;

		private object tpSync;

		private uint tpCount;

		public ThreadPoolRequestQueue()
		{
			tpSync = new object();
		}

		public uint EnQueue(_ThreadPoolWaitCallback tpcallBack)
		{
			uint result = 0u;
			bool flag = false;
			bool flag2 = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					Monitor.Enter(tpSync);
					flag = true;
				}
				catch (Exception)
				{
				}
				if (flag)
				{
					if (tpCount == 0)
					{
						flag2 = ThreadPool.SetAppDomainRequestActive();
					}
					tpCount++;
					result = tpCount;
					if (tpHead == null)
					{
						tpHead = tpcallBack;
						tpTail = tpcallBack;
					}
					else
					{
						tpTail._next = tpcallBack;
						tpTail = tpcallBack;
					}
					Monitor.Exit(tpSync);
					if (flag2)
					{
						ThreadPool.SetNativeTpEvent();
					}
				}
			}
			return result;
		}

		public _ThreadPoolWaitCallback DeQueue()
		{
			bool flag = false;
			_ThreadPoolWaitCallback result = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
			}
			finally
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					Monitor.Enter(tpSync);
					flag = true;
				}
				catch (Exception)
				{
				}
				if (flag)
				{
					_ThreadPoolWaitCallback threadPoolWaitCallback = tpHead;
					if (threadPoolWaitCallback != null)
					{
						result = threadPoolWaitCallback;
						tpHead = threadPoolWaitCallback._next;
						tpCount--;
						if (tpCount == 0)
						{
							tpTail = null;
							ThreadPool.ClearAppDomainRequestActive();
						}
					}
					Monitor.Exit(tpSync);
				}
			}
			return result;
		}

		public uint GetQueueCount()
		{
			return tpCount;
		}
	}
}
