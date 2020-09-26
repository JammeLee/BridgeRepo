namespace System.Net
{
	[Flags]
	internal enum FtpMethodFlags
	{
		None = 0x0,
		IsDownload = 0x1,
		IsUpload = 0x2,
		TakesParameter = 0x4,
		MayTakeParameter = 0x8,
		DoesNotTakeParameter = 0x10,
		ParameterIsDirectory = 0x20,
		ShouldParseForResponseUri = 0x40,
		HasHttpCommand = 0x80
	}
}
