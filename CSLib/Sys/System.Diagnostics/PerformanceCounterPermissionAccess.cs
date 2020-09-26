namespace System.Diagnostics
{
	[Flags]
	public enum PerformanceCounterPermissionAccess
	{
		[Obsolete("This member has been deprecated.  Use System.Diagnostics.PerformanceCounter.PerformanceCounterPermissionAccess.Read instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		Browse = 0x1,
		[Obsolete("This member has been deprecated.  Use System.Diagnostics.PerformanceCounter.PerformanceCounterPermissionAccess.Write instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		Instrument = 0x3,
		None = 0x0,
		Read = 0x1,
		Write = 0x2,
		Administer = 0x7
	}
}
