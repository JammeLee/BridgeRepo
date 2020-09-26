namespace System.Reflection
{
	[Serializable]
	internal enum CorElementType : byte
	{
		End = 0,
		Void = 1,
		Boolean = 2,
		Char = 3,
		I1 = 4,
		U1 = 5,
		I2 = 6,
		U2 = 7,
		I4 = 8,
		U4 = 9,
		I8 = 10,
		U8 = 11,
		R4 = 12,
		R8 = 13,
		String = 14,
		Ptr = 0xF,
		ByRef = 0x10,
		ValueType = 17,
		Class = 18,
		Array = 20,
		TypedByRef = 22,
		I = 24,
		U = 25,
		FnPtr = 27,
		Object = 28,
		SzArray = 29,
		CModReqd = 0x1F,
		CModOpt = 0x20,
		Internal = 33,
		Modifier = 0x40,
		Sentinel = 65,
		Pinned = 69
	}
}
