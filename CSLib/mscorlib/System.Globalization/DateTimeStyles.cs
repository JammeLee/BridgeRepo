using System.Runtime.InteropServices;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum DateTimeStyles
	{
		None = 0x0,
		AllowLeadingWhite = 0x1,
		AllowTrailingWhite = 0x2,
		AllowInnerWhite = 0x4,
		AllowWhiteSpaces = 0x7,
		NoCurrentDateDefault = 0x8,
		AdjustToUniversal = 0x10,
		AssumeLocal = 0x20,
		AssumeUniversal = 0x40,
		RoundtripKind = 0x80
	}
}
