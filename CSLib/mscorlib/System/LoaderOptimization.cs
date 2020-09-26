using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public enum LoaderOptimization
	{
		NotSpecified = 0,
		SingleDomain = 1,
		MultiDomain = 2,
		MultiDomainHost = 3,
		[Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		DomainMask = 3,
		[Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		DisallowBindings = 4
	}
}
