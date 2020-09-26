using System.Security.Permissions;

namespace System.Threading
{
	internal static class ThreadPoolGlobals
	{
		public static uint tpQuantum = 2u;

		public static int tpWarmupCount = GetProcessorCount() * 2;

		public static bool tpHosted = ThreadPool.IsThreadPoolHosted();

		public static bool vmTpInitialized;

		public static ThreadPoolRequestQueue tpQueue = new ThreadPoolRequestQueue();

		[EnvironmentPermission(SecurityAction.Assert, Read = "NUMBER_OF_PROCESSORS")]
		internal static int GetProcessorCount()
		{
			return Environment.ProcessorCount;
		}
	}
}
