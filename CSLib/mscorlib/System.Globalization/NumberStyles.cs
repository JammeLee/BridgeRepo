using System.Runtime.InteropServices;

namespace System.Globalization
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum NumberStyles
	{
		None = 0x0,
		AllowLeadingWhite = 0x1,
		AllowTrailingWhite = 0x2,
		AllowLeadingSign = 0x4,
		AllowTrailingSign = 0x8,
		AllowParentheses = 0x10,
		AllowDecimalPoint = 0x20,
		AllowThousands = 0x40,
		AllowExponent = 0x80,
		AllowCurrencySymbol = 0x100,
		AllowHexSpecifier = 0x200,
		Integer = 0x7,
		HexNumber = 0x203,
		Number = 0x6F,
		Float = 0xA7,
		Currency = 0x17F,
		Any = 0x1FF
	}
}
