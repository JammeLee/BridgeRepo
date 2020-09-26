namespace System.Reflection
{
	[Serializable]
	[Flags]
	internal enum DeclSecurityAttributes
	{
		ActionMask = 0x1F,
		ActionNil = 0x0,
		Request = 0x1,
		Demand = 0x2,
		Assert = 0x3,
		Deny = 0x4,
		PermitOnly = 0x5,
		LinktimeCheck = 0x6,
		InheritanceCheck = 0x7,
		RequestMinimum = 0x8,
		RequestOptional = 0x9,
		RequestRefuse = 0xA,
		PrejitGrant = 0xB,
		PrejitDenied = 0xC,
		NonCasDemand = 0xD,
		NonCasLinkDemand = 0xE,
		NonCasInheritance = 0xF,
		MaximumValue = 0xF
	}
}
