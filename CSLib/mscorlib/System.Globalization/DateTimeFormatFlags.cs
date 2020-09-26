namespace System.Globalization
{
	[Flags]
	internal enum DateTimeFormatFlags
	{
		None = 0x0,
		UseGenitiveMonth = 0x1,
		UseLeapYearMonth = 0x2,
		UseSpacesInMonthNames = 0x4,
		UseHebrewRule = 0x8,
		UseSpacesInDayNames = 0x10,
		UseDigitPrefixInTokens = 0x20,
		NotInitialized = -1
	}
}
