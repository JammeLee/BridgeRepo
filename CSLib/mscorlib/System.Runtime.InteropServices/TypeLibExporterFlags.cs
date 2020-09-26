namespace System.Runtime.InteropServices
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum TypeLibExporterFlags
	{
		None = 0x0,
		OnlyReferenceRegistered = 0x1,
		CallerResolvedReferences = 0x2,
		OldNames = 0x4,
		ExportAs32Bit = 0x10,
		ExportAs64Bit = 0x20
	}
}
