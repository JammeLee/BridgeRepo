using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum TypeAttributes
	{
		VisibilityMask = 0x7,
		NotPublic = 0x0,
		Public = 0x1,
		NestedPublic = 0x2,
		NestedPrivate = 0x3,
		NestedFamily = 0x4,
		NestedAssembly = 0x5,
		NestedFamANDAssem = 0x6,
		NestedFamORAssem = 0x7,
		LayoutMask = 0x18,
		AutoLayout = 0x0,
		SequentialLayout = 0x8,
		ExplicitLayout = 0x10,
		ClassSemanticsMask = 0x20,
		Class = 0x0,
		Interface = 0x20,
		Abstract = 0x80,
		Sealed = 0x100,
		SpecialName = 0x400,
		Import = 0x1000,
		Serializable = 0x2000,
		StringFormatMask = 0x30000,
		AnsiClass = 0x0,
		UnicodeClass = 0x10000,
		AutoClass = 0x20000,
		CustomFormatClass = 0x30000,
		CustomFormatMask = 0xC00000,
		BeforeFieldInit = 0x100000,
		ReservedMask = 0x40800,
		RTSpecialName = 0x800,
		HasSecurity = 0x40000
	}
}
