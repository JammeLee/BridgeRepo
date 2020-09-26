using System.Runtime.InteropServices;

namespace System
{
	[Flags]
	[ComVisible(true)]
	public enum AppDomainManagerInitializationOptions
	{
		None = 0x0,
		RegisterWithHost = 0x1
	}
}
