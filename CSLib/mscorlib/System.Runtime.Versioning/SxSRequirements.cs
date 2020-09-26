namespace System.Runtime.Versioning
{
	[Flags]
	internal enum SxSRequirements
	{
		None = 0x0,
		AppDomainID = 0x1,
		ProcessID = 0x2,
		AssemblyName = 0x4,
		TypeName = 0x8
	}
}
