using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum PortableExecutableKinds
	{
		NotAPortableExecutableImage = 0x0,
		ILOnly = 0x1,
		Required32Bit = 0x2,
		PE32Plus = 0x4,
		Unmanaged32Bit = 0x8
	}
}
