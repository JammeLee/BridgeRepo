namespace System.Reflection
{
	[Serializable]
	internal enum MetadataColumnType
	{
		Module = 0,
		TypeRef = 1,
		TypeDef = 2,
		FieldPtr = 3,
		Field = 4,
		MethodPtr = 5,
		Method = 6,
		ParamPtr = 7,
		Param = 8,
		InterfaceImpl = 9,
		MemberRef = 10,
		Constant = 11,
		CustomAttribute = 12,
		FieldMarshal = 13,
		DeclSecurity = 14,
		ClassLayout = 0xF,
		FieldLayout = 0x10,
		StandAloneSig = 17,
		EventMap = 18,
		EventPtr = 19,
		Event = 20,
		PropertyMap = 21,
		PropertyPtr = 22,
		Property = 23,
		MethodSemantics = 24,
		MethodImpl = 25,
		ModuleRef = 26,
		TypeSpec = 27,
		ImplMap = 28,
		FieldRVA = 29,
		ENCLog = 30,
		ENCMap = 0x1F,
		Assembly = 0x20,
		AssemblyProcessor = 33,
		AssemblyOS = 34,
		AssemblyRef = 35,
		AssemblyRefProcessor = 36,
		AssemblyRefOS = 37,
		File = 38,
		ExportedType = 39,
		ManifestResource = 40,
		NestedClass = 41,
		GenericParam = 42,
		MethodSpec = 43,
		GenericParamConstraint = 44,
		TableIdMax = 0x3F,
		CodedToken = 0x40,
		TypeDefOrRef = 65,
		HasConstant = 66,
		HasCustomAttribute = 67,
		HasFieldMarshal = 68,
		HasDeclSecurity = 69,
		MemberRefParent = 70,
		HasSemantic = 71,
		MethodDefOrRef = 72,
		MemberForwarded = 73,
		Implementation = 74,
		CustomAttributeType = 75,
		ResolutionScope = 76,
		TypeOrMethodDef = 77,
		CodedTokenMax = 95,
		Short = 96,
		UShort = 97,
		Long = 98,
		ULong = 99,
		Byte = 100,
		StringHeap = 101,
		GuidHeap = 102,
		BlobHeap = 103
	}
}