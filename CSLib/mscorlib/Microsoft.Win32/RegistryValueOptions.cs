using System;

namespace Microsoft.Win32
{
	[Flags]
	public enum RegistryValueOptions
	{
		None = 0x0,
		DoNotExpandEnvironmentNames = 0x1
	}
}
