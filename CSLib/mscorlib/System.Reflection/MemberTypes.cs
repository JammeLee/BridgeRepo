using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum MemberTypes
	{
		Constructor = 0x1,
		Event = 0x2,
		Field = 0x4,
		Method = 0x8,
		Property = 0x10,
		TypeInfo = 0x20,
		Custom = 0x40,
		NestedType = 0x80,
		All = 0xBF
	}
}
