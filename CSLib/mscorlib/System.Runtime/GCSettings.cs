using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

namespace System.Runtime
{
	public static class GCSettings
	{
		public static GCLatencyMode LatencyMode
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return (GCLatencyMode)GC.nativeGetGCLatencyMode();
			}
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
			set
			{
				if (value < GCLatencyMode.Batch || value > GCLatencyMode.LowLatency)
				{
					throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_Enum"));
				}
				GC.nativeSetGCLatencyMode((int)value);
			}
		}

		public static bool IsServerGC => GC.nativeIsServerGC();
	}
}
