namespace System.Text.RegularExpressions
{
	[Flags]
	public enum RegexOptions
	{
		None = 0x0,
		IgnoreCase = 0x1,
		Multiline = 0x2,
		ExplicitCapture = 0x4,
		Compiled = 0x8,
		Singleline = 0x10,
		IgnorePatternWhitespace = 0x20,
		RightToLeft = 0x40,
		ECMAScript = 0x100,
		CultureInvariant = 0x200
	}
}
