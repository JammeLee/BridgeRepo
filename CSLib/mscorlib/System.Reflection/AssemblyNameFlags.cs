using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum AssemblyNameFlags
	{
		None = 0x0,
		PublicKey = 0x1,
		EnableJITcompileOptimizer = 0x4000,
		EnableJITcompileTracking = 0x8000,
		Retargetable = 0x100
	}
}
