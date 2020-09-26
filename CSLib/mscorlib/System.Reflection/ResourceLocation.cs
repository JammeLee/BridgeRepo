using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum ResourceLocation
	{
		Embedded = 0x1,
		ContainedInAnotherAssembly = 0x2,
		ContainedInManifestFile = 0x4
	}
}
