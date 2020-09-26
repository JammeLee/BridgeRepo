namespace System.Runtime.InteropServices
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum TypeLibImporterFlags
	{
		None = 0x0,
		PrimaryInteropAssembly = 0x1,
		UnsafeInterfaces = 0x2,
		SafeArrayAsSystemArray = 0x4,
		TransformDispRetVals = 0x8,
		PreventClassMembers = 0x10,
		SerializableValueClasses = 0x20,
		ImportAsX86 = 0x100,
		ImportAsX64 = 0x200,
		ImportAsItanium = 0x400,
		ImportAsAgnostic = 0x800,
		ReflectionOnlyLoading = 0x1000
	}
}
