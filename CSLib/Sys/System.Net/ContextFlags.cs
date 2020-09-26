namespace System.Net
{
	[Flags]
	internal enum ContextFlags
	{
		Zero = 0x0,
		Delegate = 0x1,
		MutualAuth = 0x2,
		ReplayDetect = 0x4,
		SequenceDetect = 0x8,
		Confidentiality = 0x10,
		UseSessionKey = 0x20,
		AllocateMemory = 0x100,
		Connection = 0x800,
		InitExtendedError = 0x4000,
		AcceptExtendedError = 0x8000,
		InitStream = 0x8000,
		AcceptStream = 0x10000,
		InitIntegrity = 0x10000,
		AcceptIntegrity = 0x20000,
		InitManualCredValidation = 0x80000,
		InitUseSuppliedCreds = 0x80,
		InitIdentify = 0x20000,
		AcceptIdentify = 0x80000,
		ProxyBindings = 0x4000000,
		AllowMissingBindings = 0x10000000
	}
}
