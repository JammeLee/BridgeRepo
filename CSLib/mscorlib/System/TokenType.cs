namespace System
{
	internal enum TokenType
	{
		NumberToken = 1,
		YearNumberToken = 2,
		Am = 3,
		Pm = 4,
		MonthToken = 5,
		EndOfString = 6,
		DayOfWeekToken = 7,
		TimeZoneToken = 8,
		EraToken = 9,
		DateWordToken = 10,
		UnknownToken = 11,
		HebrewNumber = 12,
		JapaneseEraToken = 13,
		TEraToken = 14,
		IgnorableSymbol = 0xF,
		SEP_Unk = 0x100,
		SEP_End = 0x200,
		SEP_Space = 768,
		SEP_Am = 0x400,
		SEP_Pm = 1280,
		SEP_Date = 1536,
		SEP_Time = 1792,
		SEP_YearSuff = 0x800,
		SEP_MonthSuff = 2304,
		SEP_DaySuff = 2560,
		SEP_HourSuff = 2816,
		SEP_MinuteSuff = 3072,
		SEP_SecondSuff = 3328,
		SEP_LocalTimeMark = 3584,
		SEP_DateOrOffset = 3840,
		RegularTokenMask = 0xFF,
		SeparatorTokenMask = 65280
	}
}
