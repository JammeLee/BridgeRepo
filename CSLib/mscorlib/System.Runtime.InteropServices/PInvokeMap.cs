namespace System.Runtime.InteropServices
{
	[Serializable]
	internal enum PInvokeMap
	{
		NoMangle = 1,
		CharSetMask = 6,
		CharSetNotSpec = 0,
		CharSetAnsi = 2,
		CharSetUnicode = 4,
		CharSetAuto = 6,
		PinvokeOLE = 0x20,
		SupportsLastError = 0x40,
		BestFitMask = 48,
		BestFitEnabled = 0x10,
		BestFitDisabled = 0x20,
		BestFitUseAsm = 48,
		ThrowOnUnmappableCharMask = 12288,
		ThrowOnUnmappableCharEnabled = 0x1000,
		ThrowOnUnmappableCharDisabled = 0x2000,
		ThrowOnUnmappableCharUseAsm = 12288,
		CallConvMask = 1792,
		CallConvWinapi = 0x100,
		CallConvCdecl = 0x200,
		CallConvStdcall = 768,
		CallConvThiscall = 0x400,
		CallConvFastcall = 1280
	}
}
