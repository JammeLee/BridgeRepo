using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible(true)]
	public enum MemberAttributes
	{
		Abstract = 1,
		Final = 2,
		Static = 3,
		Override = 4,
		Const = 5,
		New = 0x10,
		Overloaded = 0x100,
		Assembly = 0x1000,
		FamilyAndAssembly = 0x2000,
		Family = 12288,
		FamilyOrAssembly = 0x4000,
		Private = 20480,
		Public = 24576,
		AccessMask = 61440,
		ScopeMask = 0xF,
		VTableMask = 240
	}
}
