namespace System.Reflection
{
	[Serializable]
	internal enum MetadataTokenType
	{
		Module = 0,
		TypeRef = 0x1000000,
		TypeDef = 0x2000000,
		FieldDef = 0x4000000,
		MethodDef = 100663296,
		ParamDef = 0x8000000,
		InterfaceImpl = 150994944,
		MemberRef = 167772160,
		CustomAttribute = 201326592,
		Permission = 234881024,
		Signature = 285212672,
		Event = 335544320,
		Property = 385875968,
		ModuleRef = 436207616,
		TypeSpec = 452984832,
		Assembly = 0x20000000,
		AssemblyRef = 587202560,
		File = 637534208,
		ExportedType = 654311424,
		ManifestResource = 671088640,
		GenericPar = 704643072,
		MethodSpec = 721420288,
		String = 1879048192,
		Name = 1895825408,
		BaseType = 1912602624,
		Invalid = int.MaxValue
	}
}
