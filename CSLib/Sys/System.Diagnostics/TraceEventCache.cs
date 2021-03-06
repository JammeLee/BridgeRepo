using System.Collections;
using System.Globalization;
using System.Security.Permissions;
using System.Threading;

namespace System.Diagnostics
{
	public class TraceEventCache
	{
		private static int processId;

		private static string processName;

		private long timeStamp = -1L;

		private DateTime dateTime = DateTime.MinValue;

		private string stackTrace;

		internal Guid ActivityId => Trace.CorrelationManager.ActivityId;

		public string Callstack
		{
			get
			{
				if (stackTrace == null)
				{
					stackTrace = Environment.StackTrace;
				}
				else
				{
					new EnvironmentPermission(PermissionState.Unrestricted).Demand();
				}
				return stackTrace;
			}
		}

		public Stack LogicalOperationStack => Trace.CorrelationManager.LogicalOperationStack;

		public DateTime DateTime
		{
			get
			{
				if (dateTime == DateTime.MinValue)
				{
					dateTime = DateTime.UtcNow;
				}
				return dateTime;
			}
		}

		public int ProcessId => GetProcessId();

		public string ThreadId => GetThreadId().ToString(CultureInfo.InvariantCulture);

		public long Timestamp
		{
			get
			{
				if (timeStamp == -1)
				{
					timeStamp = Stopwatch.GetTimestamp();
				}
				return timeStamp;
			}
		}

		internal void Clear()
		{
			timeStamp = -1L;
			dateTime = DateTime.MinValue;
			stackTrace = null;
		}

		private static void InitProcessInfo()
		{
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			if (processName == null)
			{
				Process currentProcess = Process.GetCurrentProcess();
				try
				{
					processId = currentProcess.Id;
					processName = currentProcess.ProcessName;
				}
				finally
				{
					currentProcess.Dispose();
				}
			}
		}

		internal static int GetProcessId()
		{
			InitProcessInfo();
			return processId;
		}

		internal static string GetProcessName()
		{
			InitProcessInfo();
			return processName;
		}

		internal static int GetThreadId()
		{
			return Thread.CurrentThread.ManagedThreadId;
		}
	}
}
