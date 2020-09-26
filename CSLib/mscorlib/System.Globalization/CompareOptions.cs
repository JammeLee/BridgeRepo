using System.Runtime.InteropServices;

namespace System.Globalization
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum CompareOptions
	{
		None = 0x0,
		IgnoreCase = 0x1,
		IgnoreNonSpace = 0x2,
		IgnoreSymbols = 0x4,
		IgnoreKanaType = 0x8,
		IgnoreWidth = 0x10,
		OrdinalIgnoreCase = 0x10000000,
		StringSort = 0x20000000,
		Ordinal = 0x40000000
	}
}
