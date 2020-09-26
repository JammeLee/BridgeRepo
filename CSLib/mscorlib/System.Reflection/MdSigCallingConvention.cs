namespace System.Reflection
{
	[Serializable]
	[Flags]
	internal enum MdSigCallingConvention : byte
	{
		CallConvMask = 0xF,
		Default = 0x0,
		C = 0x1,
		StdCall = 0x2,
		ThisCall = 0x3,
		FastCall = 0x4,
		Vararg = 0x5,
		Field = 0x6,
		LoclaSig = 0x7,
		Property = 0x8,
		Unmgd = 0x9,
		GenericInst = 0xA,
		Generic = 0x10,
		HasThis = 0x20,
		ExplicitThis = 0x40
	}
}
