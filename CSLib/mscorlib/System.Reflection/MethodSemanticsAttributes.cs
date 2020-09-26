namespace System.Reflection
{
	[Serializable]
	[Flags]
	internal enum MethodSemanticsAttributes
	{
		Setter = 0x1,
		Getter = 0x2,
		Other = 0x4,
		AddOn = 0x8,
		RemoveOn = 0x10,
		Fire = 0x20
	}
}
