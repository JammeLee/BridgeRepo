namespace System.Reflection
{
	[Serializable]
	internal enum CorCallingConvention : byte
	{
		Default = 0,
		Vararg = 5,
		Field = 6,
		LocalSig = 7,
		Property = 8,
		Unmanaged = 9,
		GenericInstance = 10
	}
}
