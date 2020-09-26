using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum ParameterAttributes
	{
		None = 0x0,
		In = 0x1,
		Out = 0x2,
		Lcid = 0x4,
		Retval = 0x8,
		Optional = 0x10,
		ReservedMask = 0xF000,
		HasDefault = 0x1000,
		HasFieldMarshal = 0x2000,
		Reserved3 = 0x4000,
		Reserved4 = 0x8000
	}
}
